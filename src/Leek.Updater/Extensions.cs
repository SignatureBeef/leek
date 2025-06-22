// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Services;

namespace Leek.Updater;

/// <summary>
/// Extension methods for configuring the update service in a <see cref="LeekBuilder"/>.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers the default update service and its providers in the service collection.
    /// </summary>
    /// <param name="builder">Service collection to add the update service to.</param>
    /// <returns>The updated <see cref="LeekBuilder"/> instance with the default update service registered.</returns>
    public static LeekBuilder AddUpdateService(this LeekBuilder builder)
    {
        builder.Services.AddDefaultUpdateService();
        return builder;
    }
}
