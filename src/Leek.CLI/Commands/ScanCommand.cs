
// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Leek.CLI.Commands;

public class ScanCommand : Command
{
    public ScanCommand() : base("scan", "Scan a target for breaches.")
    {
        AddOption(Target);
        AddOption(Provider);
    }

    static readonly Option<string[]?> Target = new(
         aliases: ["--target", "-t"],
         description: "The target to scan (e.g., wordpress://, etc.).")
    {
        IsRequired = true,
        AllowMultipleArgumentsPerToken = true,
    };

    static readonly Option<string[]?> Provider = new(
         aliases: ["--provider", "-p"],
         description: "The provider to use (e.g., wordpress://, etc.).")
    {
        IsRequired = false, // defaults will be to all providers
        AllowMultipleArgumentsPerToken = true,
    };
}

public class ScanCommandHandler(IEnumerable<IDataProvider> dataProviders, ILogger<ScanCommandHandler> logger) : ICommandHandler
{
    public string[]? Target { get; set; }
    public string[]? Provider { get; set; }

    public int Invoke(InvocationContext context) => throw new NotImplementedException();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        ProviderConnection[] targets = SharedCommandOptions.CreateProviderConnections(dataProviders, Target ?? []);
        ProviderConnection[] providers = SharedCommandOptions.CreateProviderConnections(dataProviders, Provider ?? []);

        IDataScanProvider[] scanProviders = [.. targets
            .Where(x => x.Provider is IDataScanProvider)
            .Select(x => (IDataScanProvider)x.Provider)
            .Distinct()];

        if (scanProviders.Length == 0)
        {
            logger.LogWarning("No scan providers found for the specified connections.");
            return 1;
        }

        IDataSearchProvider[] searchProviders = [.. providers
            .Where(x => x.Provider is IDataSearchProvider)
            .Select(x => (IDataSearchProvider)x.Provider)
            .Distinct()];

        if (searchProviders.Length == 0)
        {
            logger.LogWarning("No search providers found for the specified connections.");
            return 1;
        }

        logger.LogInformation("Scanning {Count} targets with {ProviderCount} providers...", targets.Length, scanProviders.Length);

        (IDataScanProvider Target, ConnectionContext[] Datasources)[] scanTargets = [.. scanProviders
            .Select(provider => (
                Target: provider,
                Datasources: targets
                                .Where(x => x.Provider == provider)
                                .Select(x => x.Connection)
                                .ToArray()
            ))
            .Where(x => x.Datasources.Length > 0)];

        ConnectionContext[] searchConnections = [.. providers
            .Where(x => x.Provider is IDataSearchProvider)
            .Select(x => x.Connection)];
        Task<LeekScanResult[]>[] scanTasks = [.. scanTargets
            .Select(x => x.Target.ScanAsync(x.Datasources, searchConnections, context.GetCancellationToken()).ContinueWith(r => {
                if (!r.IsFaulted)
                {
                    logger.LogInformation("Scan completed for {Provider} with {Count} results.", x.Target.GetType().Name, r.Result.Length);

                    foreach (LeekScanResult result in r.Result)
                    {
                        if (result.Breach)
                            logger.LogCritical("Scan resulted in breach: {Message}", result.Message);
                        else
                          logger.LogInformation("Scan result: {Message}", result.Message);
                    }
                }
                else
                {
                    logger.LogError(r.Exception, "Error during scan for {Provider}.", x.Target.GetType().Name);
                }
                return r.Result;
            }))];

        await Task.WhenAll(scanTasks);

        return 0;
    }
}