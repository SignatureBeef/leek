// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;
using Leek.Core.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Leek.CLI.Commands;

public class UpdateCommand : Command
{
    public UpdateCommand() : base("update", "Updates the desired provider with hashes from trusted authorities.")
    {
        AddOption(Provider);
    }

    static readonly Option<string?> Provider = new(
         aliases: ["--provider", "-p"],
         description: "The provider to use (e.g., sqlite://, mssql://, etc.).")
    {
        IsRequired = true,
        AllowMultipleArgumentsPerToken = false, // only one provider can be specified
    };
}

public class UpdateCommandHandler(IUpdateService updateService, IEnumerable<IDataProvider> dataProviders, ILogger<UpdateCommandHandler> logger) : ICommandHandler
{
    public string[]? Provider { get; set; }

    public int Invoke(InvocationContext context) => throw new NotImplementedException();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        ProviderConnection[] connectionProviders = SharedCommandOptions.CreateProviderConnections(dataProviders, Provider ?? []);

        if (connectionProviders.Length == 0)
        {
            logger.LogError("No valid providers specified. Please check your input.");
            return 1;
        }

        logger.LogInformation("Starting update for {ProviderCount} provider(s).", connectionProviders.Length);

        await updateService.UpdateAsync(connectionProviders);

        logger.LogInformation("Update completed successfully for {ProviderCount} provider(s).", connectionProviders.Length);

        return 0;
    }
}
