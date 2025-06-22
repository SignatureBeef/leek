// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Runtime.CompilerServices;
using Leek.Core.Services;

namespace Leek.Services;

/// <summary>
/// Default implementation of <see cref="IWordlistReader"/> that reads wordlists from local files or remote URIs.
/// Supports reading from local files, downloading remote files over HTTPS, and reading from streams.
/// </summary>
public class DefaultWordlistReader : IWordlistReader
{
    /// <inheritdoc/>
    public IAsyncEnumerable<string> ReadLinesFromUriAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        if (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException("HTTP scheme is not supported. Please use HTTPS for remote files.");

        bool isRemote = uri.IsAbsoluteUri && uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        if (isRemote)
            return DownloadAsync(uri.ToString(), cancellationToken);

        return ReadLinesFromFileAsync(uri.LocalPath, cancellationToken);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> ReadLinesFromFileAsync(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using FileStream stream = File.OpenRead(filePath);
        await foreach (string line in FromStreamAsync(stream, cancellationToken))
        {
            yield return line;
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> DownloadAsync(string fileUrl, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        using HttpResponseMessage response = await client.GetAsync(fileUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await foreach (string line in FromStreamAsync(stream, cancellationToken))
        {
            yield return line;
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> FromStreamAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using StreamReader reader = new(stream);
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            yield return line.Trim();
        }
    }
}
