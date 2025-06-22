// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leek.Services.Models;

[Table("Hashes")]
[PrimaryKey(nameof(Type), nameof(Value))]
public class Hash
{
    public required ESecretType Type { get; set; }
    public required string Value { get; set; }

    /// <summary>
    /// The number of breaches this hash has been found in online.
    /// </summary>
    public required int ForeignBreachCount { get; set; } = 0;

    /// <summary>
    /// The number of breaches this hash has been found locally through leek.
    /// </summary>
    public required int LocalBreachCount { get; set; } = 0;

    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
