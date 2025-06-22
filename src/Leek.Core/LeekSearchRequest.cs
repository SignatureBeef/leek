// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using System.ComponentModel.DataAnnotations;

namespace Leek.Core;

/// <summary>
/// Represents a request to search for a secret in the Leek database.
/// </summary>
/// <param name="Secret">The secret to search for, such as a password or hash.</param>
/// <param name="SecretType">
/// The type of the secret, which can be a password, hash, or other types defined in <see cref="ESecretType"/>.
/// Defaults to <see cref="ESecretType.Secret"/>
/// </param>
public record struct LeekSearchRequest([Required] string Secret, ESecretType SecretType = ESecretType.Secret);
