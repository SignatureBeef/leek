// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Services;

namespace Leek.Core.Providers;

/// <summary>
/// Represents a connection between a data provider and a connection context.
/// </summary>
/// <param name="Provider">The data provider instance.</param>
/// <param name="Connection">The connection context for the provider.</param>
public record struct ProviderConnection(IDataProvider Provider, ConnectionContext Connection);
