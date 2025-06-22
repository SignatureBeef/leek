// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;

namespace Leek.Core.Services;

/// <summary>
/// Interface for data read providers.
/// </summary>
public interface IDataReadProvider
{
    /// <summary>Allows iteration over all hashes in the provider.</summary>
    /// <param name="connection">The connection context to use for reading data.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of <see cref="HashEntity"/> representing the hashes in the provider.</returns>
    IAsyncEnumerable<HashEntity> GetHashesAsync(ConnectionContext connection, CancellationToken cancellationToken = default);

    // TODO: this was thought mainly for progress tracking, but isn't feasible with some providers (e.g. file/dir without indexing)
    // Task<long> GetHashCountAsync(ConnectionContext connection, CancellationToken cancellationToken = default);
}

