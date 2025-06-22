// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
namespace Leek.Core.Providers;

/// <summary>
/// Enumeration of supported database connection providers.
/// </summary>
public enum EConnectionProvider
{
    Unknown = 0,
    SQLite,
    MySQL,
    MSSQL,
    Postgres,
}
