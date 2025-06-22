// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace Leek.Services;

/// <summary>
/// Represents the database context for leek
/// </summary>
public class LeekDbContext(DbContextOptions<LeekDbContext> options) : DbContext(options)
{
    public DbSet<Hash> Hashes => Set<Hash>();
}
