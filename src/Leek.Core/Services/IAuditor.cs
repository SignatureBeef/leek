// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;

namespace Leek.Core.Services;

/// <summary>
/// Leek interface for auditing secrets and hashes.
/// </summary>
public interface IAuditor
{
    /// <summary>
    /// Searches for breaches across multiple connections based on the provided request.
    /// </summary>
    /// <param name="connections">An array of connection contexts to search through.</param>
    /// <param name="request">The search request containing the criteria for the search.</param>
    /// <returns>True if any breaches are found, otherwise false.</returns>
    Task<LeekSearchResponse> SearchBreaches(ConnectionContext[] connections, LeekSearchRequest request);
}
