// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Security.Cryptography;
using Leek.Core.Providers;
using Leek.Core.Services;

namespace Leek.Core.Extensions;

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms

public static class GeneralExtensions
{
    /// <summary>
    /// Converts an array of <see cref="ConnectionContext"/> to an array of <see cref="ProviderConnection"/> using the provided data providers.
    /// </summary>
    /// <param name="connections">The array of <see cref="ConnectionContext"/> to convert.</param>
    /// <param name="providers">The collection of <see cref="IDataProvider"/> to use for conversion.</param>
    /// <returns>An array of <see cref="ProviderConnection"/> where each connection is associated with a provider that supports it.</returns>
    public static ProviderConnection[] AsProviderConnections(this ConnectionContext[] connections, IEnumerable<IDataProvider> providers)
        => [.. connections
               .Select(connection => new ProviderConnection(
                   Provider: providers.FirstOrDefault(p => p.SupportsConnection(connection))!,
                   Connection: connection
               ))
               .Where(item => item.Provider != null)];

    /// <summary>
    /// Converts the request's secret to the specified target type without changing the original request.
    /// </summary>
    /// <param name="request">The request containing the secret to convert.</param>
    /// <param name="target">The target secret type to convert the secret into.</param>
    /// <returns>A new <see cref="LeekSearchRequest"/> with the converted secret and the specified target type.</returns>   
    public static LeekSearchRequest As(this LeekSearchRequest request, ESecretType target)
    {
        if (request.SecretType == target)
            return request; // No change needed
        return request.HashAs(target);
    }

    /// <summary>
    /// Hashes the secret in the request to the specified target type.
    /// </summary>
    /// <param name="request">The request containing the secret to hash.</param>
    /// <param name="target">The target secret type to hash the secret into.</param>
    /// <returns>A new <see cref="LeekSearchRequest"/> with the hashed secret and the specified target type.</returns>
    /// <exception cref="NotSupportedException"></exception>
    public static LeekSearchRequest HashAs(this LeekSearchRequest request, ESecretType target)
    {
        return target switch
        {
            ESecretType.SHA1 => new(SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(request.Secret)).ToHexString(), target),
            _ => throw new NotSupportedException($"Secret type '{target}' is not supported for hashing.")
        };
    }

    /// <summary>
    /// Converts a byte array to a lowercase hexadecimal string.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>A lowercase hexadecimal string representation of the byte array.</returns>
    private static string ToHexString(this byte[] bytes) => Convert.ToHexStringLower(bytes);
}
