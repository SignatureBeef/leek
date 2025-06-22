// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
namespace Leek.Core;

/// <summary>
/// Enumeration representing different types of secrets.
/// </summary>
public enum ESecretType : int
{
    /// <summary>
    /// Represents a plain text secret, such as a password.
    /// This is the default type and should be used for regular secrets.
    /// It is not hashed and is stored as-is.
    /// Use this type for sensitive information that needs to be checked against breaches.
    /// This valid will be excluded from transmitting, and instead is transformed into each supported hash instead.
    /// </summary>
    Secret = 0,

    /// <summary>
    /// Represents a SHA1 hash of a secret, most common option for storing breached secrets.
    SHA1 = 1,

    // NTLM = 2, TODO
}
