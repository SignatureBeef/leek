// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Leek.CLI.Commands;

public class CopyCommand : Command
{
    public CopyCommand() : base("copy", "Copy elements from one provider to another.")
    {
        AddOption(FromProvider);
        AddOption(FromConnectionString);
        AddOption(ToProvider);
        AddOption(ToConnectionString);
    }

    static readonly Option<string?> FromProvider = new(
         aliases: ["--from-provider", "-fp"],
         description: "The provider pull the data from (e.g., sqlite, mssql, etc.).")
    {
        IsRequired = true
    };

    static readonly Option<string?> FromConnectionString = new(
        aliases: ["--from-connection-string", "--from-connection", "-fc", "-fcs"],
        description: "The connection string or path to the data source.")
    {
        IsRequired = true
    };

    static readonly Option<string?> ToProvider = new(
         aliases: ["--to-provider", "-tp"],
         description: "The provider send the data to (e.g., sqlite, mssql, etc.).")
    {
        IsRequired = true
    };

    static readonly Option<string?> ToConnectionString = new(
        aliases: ["--to-connection-string", "--to-connection", "-tc", "-tcs"],
        description: "The connection string or path to the destination.")
    {
        IsRequired = true
    };

    enum Variant
    {
        From,
        To
    }
}

public class CopyCommandHandler(IEnumerable<IDataProvider> dataProviders) : ICommandHandler
{
    public string? FromProvider { get; set; } = "";
    public string? FromConnectionString { get; set; } = "";

    public string? ToProvider { get; set; } = "";
    public string? ToConnectionString { get; set; } = "";

    public int Invoke(InvocationContext context) => throw new NotImplementedException();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        ProviderConnection[] fromConnectionProviders = SharedCommandOptions.CreateProviderConnections(FromProvider, FromConnectionString, dataProviders);
        ProviderConnection[] toConnectionProviders = SharedCommandOptions.CreateProviderConnections(ToProvider, ToConnectionString, dataProviders);

        Console.WriteLine($"Copying from {fromConnectionProviders.Length} provider into {toConnectionProviders.Length} others.");

        List<HashEntity> queue = [];
        DateTime started = DateTime.UtcNow;
        long processed = 0, count = 0;

        foreach (ProviderConnection from in fromConnectionProviders)
        {
            //var numHashes = await from.Provider.GetHashCountAsync(from.Connection);  file provider needs some work on this...needs a full scan anyway

            count = 0;

            if (from.Provider is not IDataReadProvider fromProvider)
            {
                Console.WriteLine($"Skipping {from.Connection.Provider} as it does not support writing.");
                continue;
            }

            // read hashes in batches
            await foreach (HashEntity item in fromProvider.GetHashesAsync(from.Connection))
            {
                queue.Add(item);

                if (queue.Count >= 1000 * 20)
                {
                    HashEntity[] batch = queue.ToArray();
                    queue.Clear();
                    IEnumerable<Task> tasks = toConnectionProviders
                        .Where(to => to.Provider is IDataWriteProvider)
                        .Select(to => (to.Provider as IDataWriteProvider)!.AddAsync(to.Connection, batch));
                    await Task.WhenAll(tasks);

                    processed += batch.Length;
                    count += batch.Length;

                    TimeSpan taken = DateTime.UtcNow - started;
                    double hashesPerSecond = Math.Round(processed / taken.TotalSeconds, 2);

                    //var progress = Math.Round(++count / (double)numHashes * 100, 2);
                    string to = String.Join(",", toConnectionProviders.Select(x => x.Connection.Provider).Distinct());
                    Console.WriteLine($"Copied batch of {batch.Length} @{hashesPerSecond}hps from {from.Connection.Provider} into {to}");
                    //Console.WriteLine($"Copied batch of {batch.Length} @{hashesPerSecond}hps from {from.Connection.Provider} into {to} {progress}% t:{numHashes}");
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
            Console.WriteLine($"Copied last batch of {batch.Length}");
        }

        //foreach (var to in toConnectionProviders)
        //{
        //    await CopyToProviderAsync(from, to);
        //}

        return 0;
    }
}