// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
namespace Leek.Core.Providers;

/// <summary>
/// Represents a connection context for a data provider.
/// </summary>
/// <param name="Provider">The name of the provider.</param>
/// <param name="ConnectionString"> The connection string for the provider.</param>
public record struct ConnectionContext(string Provider, string ConnectionString);
