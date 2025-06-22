// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
namespace Leek.Core.Services;

/// <summary>
/// Interface for reading wordlists from various sources, including local files and remote URIs.
/// </summary>
public interface IWordlistReader
{
    /// <summary>
    /// Reads lines from a URI asynchronously, supporting both local files and remote files over HTTPS.
    /// </summary>
    /// <param name="uri">The URI to read lines from. This can be a local file path or a remote file URL.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <exception cref="NotSupportedException">Thrown if the URI scheme is HTTP, as only HTTPS is supported for remote files.</exception>
    /// <returns>Each line string at a time, if any.</returns>
    IAsyncEnumerable<string> ReadLinesFromUriAsync(Uri uri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads lines from a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream instance</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>Each line string at a time, if any.</returns>
    IAsyncEnumerable<string> FromStreamAsync(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads lines from a local file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>Each line string at a time, if any.</returns>
    IAsyncEnumerable<string> ReadLinesFromFileAsync(string filePath, CancellationToken cancellationToken = default);
}
