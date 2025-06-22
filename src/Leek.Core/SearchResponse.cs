// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
namespace Leek.Core;

/// <summary>
/// Represents the response from a search operation in Leek.
/// </summary>
/// <param name="IsBreached">Indicates whether the secret was found</param>
/// <param name="Message">A message providing additional information about the search result</param>
/// <param name="Location">Where the secret was found, if applicable</param>
public record struct LeekSearchResponse(bool IsBreached, string? Message, string? Location = null);
