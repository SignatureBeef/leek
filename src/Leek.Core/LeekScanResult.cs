// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
namespace Leek.Core;

/// <summary>
/// Represents the result of a scan operation.
/// </summary>
/// <param name="Breach">Indicates if a breach was found.</param>
/// <param name="Message">Message providing details about the scan result.</param>
public record struct LeekScanResult(bool Breach, string Message);
