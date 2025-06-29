// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using EFCore.BulkExtensions;
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using Leek.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Leek.Services;

public class DatabaseProvider(ILogger<DatabaseProvider> logger) : IDataProvider, IDataReadProvider, IDataWriteProvider, IDataSearchProvider
{
    public bool SupportsConnection(ConnectionContext connection) =>
         connection.Provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase) ||
         connection.Provider.Equals("mssql", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public ConnectionContext? CreateDefaultConnection() => new ConnectionContext
    {
        Provider = "sqlite",
        ConnectionString = "Data Source=leek.db"
    };

    public async Task<bool> Search(ConnectionContext connection, LeekSearchRequest request, CancellationToken cancellationToken = default)
    {
        using LeekDbContext context = CreateDbContext(connection);

        // determine if the database is initialized, and if the schema is up to date
        await context.Database.EnsureCreatedAsync(cancellationToken);

        Hash[] results = await context.Hashes
            .Where(x => x.Type == request.SecretType && x.Value.ToLower() == request.Secret.ToLower())
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        return results.Length > 0;
    }

    public virtual LeekDbContext CreateDbContext(ConnectionContext connection)
    {
        DbContextOptionsBuilder<LeekDbContext> builder = new();

        switch (connection.Provider.ToLowerInvariant())
        {
            case "sqlite":
                if (String.IsNullOrWhiteSpace(connection.ConnectionString))
                {
                    logger.LogInformation($"[{nameof(DatabaseProvider)}] No connection string provided, using default 'leek.db'.");
                    connection.ConnectionString = "Data Source=leek.db";
                }
                builder.UseSqlite(connection.ConnectionString);
                break;
            case "mssql":
                if (String.IsNullOrWhiteSpace(connection.ConnectionString))
                {
                    logger.LogInformation($"[{nameof(DatabaseProvider)}] No connection string provided, using default `localhost` instance with database 'leek'.");
                    connection.ConnectionString = "Server=localhost;Database=leek;User Id=sa;Password=DoNotUseThisPassword123!;Encrypt=False";
                }
                builder.UseSqlServer(connection.ConnectionString);
                break;
            default:
                throw new NotSupportedException($"Provider '{connection.Provider}' is not supported.");
        }

        return new LeekDbContext(builder.Options);
    }

    public Task AddAsync(ConnectionContext connection, HashEntity[] items, CancellationToken cancellationToken = default)
        => AddWithRetryAsync(connection, items, cancellationToken);

    async Task AddWithRetryAsync(ConnectionContext connection, HashEntity[] items, CancellationToken cancellationToken = default)
    {
        try
        {
            await BulkAddAsync(connection, items, cancellationToken);
        }
        catch
        {
            logger.LogWarning("Bulk add failed, falling back to slow add method.");
            await Task.Delay(10, cancellationToken);
            await AddSlowAsync(connection, items, cancellationToken);
        }
    }

    async Task BulkAddAsync(ConnectionContext connection, HashEntity[] items, CancellationToken cancellationToken = default)
    {
        using LeekDbContext context = CreateDbContext(connection);
        await context.Database.EnsureCreatedAsync(cancellationToken);

        Hash[] hashes = [.. items
            .Select(i => new Hash
            {
                Type = i.Type,
                Value = i.Value,
                ForeignBreachCount = i.KnownBreachCount,
                LocalBreachCount = 0,
                CreatedAt = DateTime.UtcNow
            })];

        await context.BulkInsertOrUpdateAsync(hashes, cancellationToken: cancellationToken);

        await context.BulkSaveChangesAsync(cancellationToken: cancellationToken);
    }

    public async Task AddSlowAsync(ConnectionContext connection, HashEntity[] items, CancellationToken cancellationToken = default)
    {
        using LeekDbContext context = CreateDbContext(connection);

        // determine if the database is initialized, and if the schema is up to date
        await context.Database.EnsureCreatedAsync(cancellationToken);

        foreach (HashEntity item in items)
        {
            Hash? existing = await context.Hashes
                .FirstOrDefaultAsync(x => x.Type == item.Type && x.Value == item.Value, cancellationToken);

            if (existing == null)
            {
                Hash hash = new()
                {
                    Type = item.Type,
                    Value = item.Value,
                    ForeignBreachCount = item.KnownBreachCount,
                    LocalBreachCount = 0,
                    CreatedAt = DateTime.UtcNow,
                };
                context.Hashes.Add(hash);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async IAsyncEnumerable<HashEntity> GetHashesAsync(ConnectionContext connection, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using LeekDbContext context = CreateDbContext(connection);

        // determine if the database is initialized, and if the schema is up to date
        await context.Database.EnsureCreatedAsync(cancellationToken);

        // Stream results as HashEntity
        await foreach (Hash? hash in context.Hashes.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return new HashEntity
            {
                Type = hash.Type,
                Value = hash.Value,
                KnownBreachCount = hash.LocalBreachCount + hash.ForeignBreachCount,
            };
        }
    }
}

public static class DatabaseProviderExtensions
{
    public static ConnectionBuilder WithMsSqlServer(this ConnectionBuilder builder)
        => builder.WithProvider("mssql");

    public static ConnectionBuilder WithMsSqlServer(this ConnectionBuilder builder, string connectionString)
        => builder.WithProvider("mssql").WithConnectionString(connectionString);


    public static ConnectionBuilder WithSqlite(this ConnectionBuilder builder)
        => builder.WithProvider("sqlite");

    public static ConnectionBuilder WithSqlite(this ConnectionBuilder builder, string connectionString)
        => builder.WithProvider("sqlite").WithConnectionString(connectionString);
}
