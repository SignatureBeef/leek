// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Extensions;
using Leek.Core.Providers;
using Leek.Core.Services;

namespace Leek.CLI;

internal static class SharedCommandOptions
{
    static ConnectionContext ParseFromMaybeUri(string uri)
    {
        string provider = uri;
        string connectionString = "";

        // does it contain :// ?
        int index = uri.IndexOf("://", StringComparison.OrdinalIgnoreCase);
        if (index > 0)
        {            // it does, so we can assume it's a provider://connectionString format
            provider = uri[..index].ToLowerInvariant();
            connectionString = uri[(index + 3)..];
        }

        return new(provider, connectionString);
    }

    public static ConnectionContext[] CreateConnections(IEnumerable<IDataProvider> registeredProviders, string[] requestedProviders)
    {
        if (requestedProviders.Length == 0)
        {
            // No providers specified, return all registered providers with their default connections
            return [.. registeredProviders
                .Select(provider => provider.CreateDefaultConnection())
                .Where(connection => connection != null)
                .Cast<ConnectionContext>()];
        }

        return [.. requestedProviders
            .Select(provider => ParseFromMaybeUri(provider))
            .Where(connection => registeredProviders.Any(p => p.SupportsConnection(connection)))];
    }

    public static ProviderConnection[] CreateProviderConnections(IEnumerable<IDataProvider> registeredProviders, string[] requestedProviders)
    {
        ConnectionContext[] connections = CreateConnections(registeredProviders, requestedProviders);
        return connections.AsProviderConnections(registeredProviders);
    }
}
