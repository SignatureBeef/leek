// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;

namespace Leek.Core.Services;

/// <summary>
/// Defines a service that handles one or more <see cref="IUpdateProvider"/> instances
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Triggers providers to update data into the specified connections.
    /// </summary>
    /// <param name="connections">An array of provider connections to update.</param>
    Task UpdateAsync(ProviderConnection[] connections);
}
