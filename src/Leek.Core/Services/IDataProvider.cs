// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;

namespace Leek.Core.Services;

/// <summary>
/// Root contract for all data providers.
/// </summary>
public interface IDataProvider
{
    /// <summary>
    /// Checks if the provider supports the given connection context.
    /// </summary>
    /// <param name="connection">The connection context to check.</param>
    /// <returns>True if the provider supports the connection, otherwise false.</returns>
    bool SupportsConnection(ConnectionContext connection);
}
