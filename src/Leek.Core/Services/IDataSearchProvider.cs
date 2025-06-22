// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;

namespace Leek.Core.Services;

/// <summary>
/// Interface for search providers.
/// </summary>
public interface IDataSearchProvider
{
    /// <summary>
    /// Searches for breaches across multiple connections based on the provided request.
    /// </summary>
    /// <param name="connection">A connection  to search through.</param>
    /// <param name="request">The search request containing the criteria for the search.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>True if any breaches are found, otherwise false.</returns>
    Task<bool> Search(ConnectionContext connection, LeekSearchRequest request, CancellationToken cancellationToken = default);
}
