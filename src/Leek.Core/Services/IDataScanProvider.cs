// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;

namespace Leek.Core.Services;

public interface IDataScanProvider
{
    /// <summary>
    /// Scans the provided connections for known bad hashes.
    /// </summary>
    /// <param name="connection">Array of connections to scan.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>List of potential breaches found in the connections.</returns>
    Task<LeekScanResult[]> ScanAsync(ConnectionContext[] targets, ConnectionContext[] datasources, CancellationToken cancellationToken = default);
}
