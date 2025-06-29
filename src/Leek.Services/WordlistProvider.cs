// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;

namespace Leek.Services;

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms

/// <summary>
/// Provides wordlist-based data searching and reading capabilities.
/// </summary>
/// <param name="wordlistReader">The wordlist reader service used to read lines from wordlists.</param>
public class WordlistProvider(IWordlistReader wordlistReader) : IDataProvider, IDataSearchProvider, IDataReadProvider
{
    public bool SupportsConnection(ConnectionContext connection) => connection.Provider.Equals("wordlist", StringComparison.OrdinalIgnoreCase);

    public async Task<bool> Search(ConnectionContext connection, LeekSearchRequest request, CancellationToken cancellationToken = default)
    {
        bool fileExists = File.Exists(connection.ConnectionString);
        if (!fileExists)
            throw new FileNotFoundException($"Wordlist file not found: {connection.ConnectionString}");

        if (request.SecretType != ESecretType.SHA1)
            throw new NotSupportedException($"Wordlist provider only supports SHA1 hashes, but received {request.SecretType}.");

        await foreach (string line in wordlistReader.ReadLinesFromFileAsync(connection.ConnectionString, cancellationToken))
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            byte[] bytes = SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(line));
            string sha1 = Convert.ToHexString(bytes);

            if (sha1.Equals(request.Secret, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public async IAsyncEnumerable<HashEntity> GetHashesAsync(ConnectionContext connection, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        bool fileExists = File.Exists(connection.ConnectionString);
        if (!fileExists)
        {
            throw new FileNotFoundException($"Wordlist file not found: {connection.ConnectionString}");
        }

        await foreach (string line in wordlistReader.ReadLinesFromFileAsync(connection.ConnectionString, cancellationToken))
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            byte[] bytes = SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(line));
            string sha1 = Convert.ToHexString(bytes);
            yield return new HashEntity
            {
                Value = sha1,
                Type = ESecretType.SHA1,
            };
        }
    }
}

public static class WordlistProviderExtensions
{
    public static ConnectionBuilder WithWordlistProvider(this ConnectionBuilder builder, string wordlistFilePath)
    {
        return builder.WithProvider("wordlist")
                      .WithConnectionString(wordlistFilePath);
    }
}
