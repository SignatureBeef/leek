// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;

namespace Leek.Core.Services;

/// <summary>
/// A provider that can update data into the leek system.
/// </summary>
public interface IUpdateProvider
{
    /// <summary>
    /// Updates data into the specified connections.
    /// </summary>
    /// <param name="connections">The connections to update data into.</param>
    Task UpdateIntoAsync(ProviderConnection[] connections);
}

