// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;

namespace Leek.Core.Services;

/// <summary>
/// Interface for data write providers.
/// </summary>
public interface IDataWriteProvider
{
    /// <summary>
    /// Bulk add an array of hash entities to the specified connection asynchronously.
    /// </summary>
    /// <param name="connection">The connection context to which the items will be added.</param>
    /// <param name="items">The array of hash entities to add.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    Task AddAsync(ConnectionContext connection, HashEntity[] items, CancellationToken cancellationToken = default);
}
