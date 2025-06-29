// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
namespace Leek.Core.Providers;

/// <summary>
/// A builder for creating <see cref="ConnectionContext"/> instances.
/// </summary>
public class ConnectionBuilder
{
    private string _provider = string.Empty;
    private string _connectionString = string.Empty;

    /// <summary>
    /// Sets the provider name.
    /// </summary>
    /// <param name="provider">The provider name.</param>
    /// <returns>The builder instance.</returns>
    public ConnectionBuilder WithProvider(string provider)
    {
        _provider = provider;
        return this;
    }

    /// <summary>
    /// Sets the connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>The builder instance.</returns>
    public ConnectionBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    /// <summary>
    /// Builds the connection context.
    /// </summary>
    /// <returns>A new instance of <see cref="ConnectionContext"/>.</returns>
    public ConnectionContext Build()
        => new(_provider, _connectionString);
}
