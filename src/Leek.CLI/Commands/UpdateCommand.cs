// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;
using Leek.Core.Services;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Leek.CLI.Commands;

public class UpdateCommand : Command
{
    public UpdateCommand() : base("update", "Updates the desired provider with hashes from trusted authorities.")
    {
        AddOption(Provider);
        AddOption(ConnectionString);
    }

    static readonly Option<string?> Provider = new(
         aliases: ["--provider", "-p"],
         description: "The provider to use (e.g., sqlite, mssql, etc.).")
    {
        IsRequired = true
    };

    static readonly Option<string?> ConnectionString = new(
        aliases: ["--connection-string", "-c", "-cs"],
        description: "The connection string or path to the provider to update.")
    {
        IsRequired = true // can default but may lead to user confusion if not specified
    };
}

public class UpdateCommandHandler(IAuditor auditor, IUpdateService updateService, IEnumerable<IDataProvider> dataProviders) : ICommandHandler
{
    public string? Provider { get; set; } = "";
    public string? ConnectionString { get; set; } = "";

    public int Invoke(InvocationContext context) => throw new NotImplementedException();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        Console.WriteLine($"Updating {Provider} database with using auditor: {auditor.GetType().Name}");

        ProviderConnection[] connectionProviders = SharedCommandOptions.CreateProviderConnections(Provider, ConnectionString, dataProviders);

        await updateService.UpdateAsync(connectionProviders);

        Console.WriteLine("Update completed successfully.");

        return 0;
    }
}
