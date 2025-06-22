// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
namespace Leek.Core;

/// <summary>
/// Represents a hash entity with its type, value, and known breach count.
/// </summary>
/// <param name="Type">The type of the secret (e.g. secret, sha1).
/// <param name="Value">The hash value of the secret.</param>
/// <param name="KnownBreachCount">The number of known breaches associated with this hash.</param>
public record struct HashEntity(ESecretType Type, string Value, int KnownBreachCount);
