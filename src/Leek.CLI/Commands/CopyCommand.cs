// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Leek.CLI.Commands;

public class CopyCommand : Command
{
    public CopyCommand() : base("copy", "Copy elements from one provider to another.")
    {
        AddOption(FromProvider);
        AddOption(ToProvider);
    }

    static readonly Option<string[]?> FromProvider = new(
         aliases: ["--from-provider", "-fp"],
         description: "The provider pull the data from (e.g., sqlite://, mssql://, etc.).")
    {
        IsRequired = true,
        AllowMultipleArgumentsPerToken = true, // allows multiple providers to be specified
    };

    static readonly Option<string[]?> ToProvider = new(
         aliases: ["--to-provider", "-tp"],
         description: "The provider send the data to (e.g., sqlite://, mssql://, etc.).")
    {
        IsRequired = true,
        AllowMultipleArgumentsPerToken = true, // allows multiple providers to be specified
    };

    enum Variant
    {
        From,
        To
    }
}

public class CopyCommandHandler(IEnumerable<IDataProvider> dataProviders, ILogger<CopyCommandHandler> logger) : ICommandHandler
{
    public string[]? FromProvider { get; set; }

    public string[]? ToProvider { get; set; }

    public int Invoke(InvocationContext context) => throw new NotImplementedException();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        ProviderConnection[] fromConnectionProviders = SharedCommandOptions.CreateProviderConnections(dataProviders, FromProvider ?? []);
        ProviderConnection[] toConnectionProviders = SharedCommandOptions.CreateProviderConnections(dataProviders, ToProvider ?? []);

        if (fromConnectionProviders.Length == 0)
        {
            logger.LogError("No valid 'from' providers specified. Please check your input.");
            return 1;
        }
        if (toConnectionProviders.Length == 0)
        {
            logger.LogError("No valid 'to' providers specified. Please check your input.");
            return 1;
        }

        logger.LogInformation("Copying from {FromCount} provider(s) into {ToCount} provider(s).", fromConnectionProviders.Length, toConnectionProviders.Length);

        List<HashEntity> queue = [];
        DateTime started = DateTime.UtcNow;
        long processed = 0, count = 0;

        foreach (ProviderConnection from in fromConnectionProviders)
        {
            //var numHashes = await from.Provider.GetHashCountAsync(from.Connection);  file provider needs some work on this...needs a full scan anyway

            count = 0;

            if (from.Provider is not IDataReadProvider fromProvider)
            {
                logger.LogWarning("Skipping {Provider} as it does not support reading.", from.Connection.Provider);
                continue;
            }

            // read hashes in batches
            await foreach (HashEntity item in fromProvider.GetHashesAsync(from.Connection))
            {
                queue.Add(item);

                if (queue.Count >= 1000 * 20)
                {
                    HashEntity[] batch = [.. queue];
                    queue.Clear();
                    IEnumerable<Task> tasks = toConnectionProviders
                        .Where(to => to.Provider is IDataWriteProvider)
                        .Select(to => (to.Provider as IDataWriteProvider)!.AddAsync(to.Connection, batch));
                    await Task.WhenAll(tasks);

                    processed += batch.Length;
                    count += batch.Length;

                    TimeSpan taken = DateTime.UtcNow - started;
                    double hashesPerSecond = Math.Round(processed / taken.TotalSeconds, 2);

                    string to = String.Join(",", toConnectionProviders.Select(x => x.Connection.Provider).Distinct());
                    logger.LogInformation("Copied batch of {BatchSize} hashes @{HashesPerSecond}hps from {FromProvider} into {ToProviders}.",
                        batch.Length, hashesPerSecond, from.Connection.Provider, to);
                }
            }
        }

        if (queue.Count != 0)
        {
            HashEntity[] batch = [.. queue];
            queue.Clear();
            IEnumerable<Task> tasks = toConnectionProviders
                .Where(to => to.Provider is IDataWriteProvider)
                .Select(to => (to.Provider as IDataWriteProvider)!.AddAsync(to.Connection, batch));
            await Task.WhenAll(tasks);
            logger.LogInformation("Copied last batch of {BatchSize} hashes from {FromProvider} into {ToProviders}.",
                batch.Length, fromConnectionProviders.First().Connection.Provider, String.Join(",", toConnectionProviders.Select(x => x.Connection.Provider).Distinct()));
        }

        return 0;
    }
}