// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Extensions;
using Leek.Core.Providers;
using Leek.Core.Services;

namespace Leek.CLI;

internal static class SharedCommandOptions
{
    public static ConnectionContext Create(string? provider, string? connectionString)
    {
        // lets default to sqlite if no provider is specified
        if (string.IsNullOrWhiteSpace(provider))
            provider = "sqlite";

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = provider switch
            {
                "sqlite" => "Data Source=leek.db",
                "mssql" => "Server=localhost;Database=leek;User Id=sa;Password=DoNotUseThisPassword123!;Encrypt=False",
                "directory" => "filestore",
                "hibp" or "haveibeenpwned" => "",
                _ => throw new ArgumentException($"Unsupported provider: {provider}")
            };
        }

        return new ConnectionContext
        {
            Provider = provider,
            ConnectionString = connectionString
        };
    }

    public static ConnectionContext[] CreateConnectionContextsOrDefaults(string? provider, string? connectionString)
    => string.IsNullOrWhiteSpace(provider)
            ? [
                Create("mssql", connectionString),
                Create("sqlite", connectionString),
                Create("directory", connectionString),
                Create("hibp", connectionString),
            ]
            : [Create(provider, connectionString)];

    public static ProviderConnection[] CreateProviderConnections(string? provider, string? connectionString, IEnumerable<IDataProvider> providers)
    {
        ConnectionContext[] connectionContexts = CreateConnectionContextsOrDefaults(provider, connectionString);

        ProviderConnection[] connectionProviders = connectionContexts.AsProviderConnections(providers);
        if (connectionProviders.Length == 0)
        {
            // Console.WriteLine($"❗ No providers found for connections.");
            // return false; // No providers to search
            throw new InvalidOperationException("No providers found for connections.");
        }

        return connectionProviders;
    }
}
