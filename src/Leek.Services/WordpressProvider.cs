// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Text;
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Leek.Services;

// public class DatabaseScanProvider(ILogger<DatabaseScanProvider> logger, DatabaseProvider databaseProvider, IAuditor auditor) : IDataScanProvider
// {
//     public ConnectionContext? CreateDefaultConnection() => null; // we can't assume one.

//     public bool SupportsConnection(ConnectionContext connection) => ToDatabaseConnection(connection) is not null;

//     static ConnectionContext? ToDatabaseConnection(ConnectionContext connection)
//     {
//         if (connection.Provider.StartsWith("query:", StringComparison.OrdinalIgnoreCase))
//         {
//             string provider = connection.Provider[6..]; // remove "query:"
//             return connection.AsProvider(provider);
//         }
//         return null;
//     }

//     public Task<LeekScanResult[]> ScanAsync(ConnectionContext[] targets, ConnectionContext[] datasources, CancellationToken cancellationToken = default)
//     {
//         throw new NotImplementedException("DatabaseScanProvider is not implemented yet.");
//     }
// }

// public class WordpressAdapter(DatabaseScanProvider databaseScanProvider, ILogger<WordpressAdapter> logger) : IDataAdapter
// {
//     static readonly ConnectionContext DefaultConnection = new("wordpress:database", "Server=localhost;Database=leek_wordpress;User Id=changemeuser;Password=BeSureToChangeThisPassword123!");
//     public ConnectionContext? CreateDefaultConnection() => databaseScanProvider.CreateDefaultConnection() ?? DefaultConnection;

//     public bool SupportsConnection(ConnectionContext connection) => databaseScanProvider.SupportsConnection(connection.AsProvider("mysql"));

//     public Task<LeekScanResult[]> ScanAsync(ConnectionContext[] targets, ConnectionContext[] datasources, CancellationToken cancellationToken = default)
//     {
//         logger.LogInformation("Scanning Wordpress database via adapter...");

//         // we can only scan databases, so we need to convert the targets to database connections
//         ConnectionContext[] dbTargets = [.. targets.Select(t => t.AsProvider("mysql"))];

//         return databaseScanProvider.ScanAsync(dbTargets, datasources, cancellationToken);
//     }
// }

public class WordpressProvider(ILogger<WordpressProvider> logger, DatabaseProvider databaseProvider, IAuditor auditor) : IDataProvider, IDataScanProvider
{
    static readonly ConnectionContext DefaultConnection = new("wordpress:database", "Server=localhost;Database=leek_wordpress;User Id=changemeuser;Password=BeSureToChangeThisPassword123!");

    public ConnectionContext? CreateDefaultConnection() => DefaultConnection;

    public bool SupportsConnection(ConnectionContext connection)
    {
        bool supported = false;
        if (connection.Provider.Equals("wordpress:database", StringComparison.OrdinalIgnoreCase))
        {
            ConnectionContext mysql = new("mysql", connection.ConnectionString);
            supported = databaseProvider.SupportsConnection(mysql);
        }

        return supported;
    }

    public async Task<LeekScanResult[]> ScanAsync(ConnectionContext[] targets, ConnectionContext[] datasources, CancellationToken cancellationToken = default)
    {
        Task<LeekScanResult[]>[] tasks = [.. targets
            .Select(c => c.Provider switch
            {
                "wordpress:database" => ScanDatabaseAsync(c, datasources, cancellationToken),
                // "wordpress:sftp" => ScanSftpAsync(c, cancellationToken),
                _ => throw new NotSupportedException($"Unsupported Wordpress provider: {c.Provider}")
            })];

        LeekScanResult[][] results = await Task.WhenAll(tasks);

        return [.. results.SelectMany(x => x)];
    }

    public async Task<LeekScanResult[]> ScanDatabaseAsync(ConnectionContext target, ConnectionContext[] datasources, CancellationToken cancellationToken = default)
    {
        target = new("mysql", target.ConnectionString);

        List<LeekScanResult> results = [];

        // we dont have passwords in raw format (at least from hibp), so we cant convert to wp format and scan as easily as one would prefer.
        // until that may happen, we scan for some various things we can assume users may do, such as using their username as a password.
        // however, other providers such as existing databases or wordlists may contain raw passwords that can be compared instead

        // TODO: implement ability to determine what connections support raw passwords
        // if(supports raw passwords)
        //     results.AddRange(await ScanPasswordsAsync(connection, cancellationToken));

        logger.LogInformation("Scanning Wordpress database users sharing the same password...");
        results.AddRange(await ScanUsernamesAsync(target, datasources, cancellationToken));

        return [.. results];
    }

    // Task<LeekScanResult[]> ScanPasswordsAsync(ConnectionContext target, ConnectionContext[] datasources, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(Array.Empty<LeekScanResult>());
    // }

    /// <summary>
    /// Determines if there are accounts with usernames as passwords, and if so, determines if breached passwords/usernames are used.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async Task<LeekScanResult[]> ScanUsernamesAsync(ConnectionContext target, ConnectionContext[] datasources, CancellationToken cancellationToken)
    {
        List<LeekScanResult> results = [];
        using LeekDbContext ctx = databaseProvider.CreateDbContext(target);

        // await ctx.Database.EnsureCreatedAsync(cancellationToken);

        // Get all active user logins + their hashes
        WordpressLogin[] logins = await ctx.Database
            .SqlQuery<WordpressLogin>($"SELECT user_login, user_pass FROM wp_users WHERE user_status = 0")
            .ToArrayAsync(cancellationToken);

        if (logins.Length > 0)
        {
            // find which login names are used as passwords
            WordpressLogin[] userSameAsPassword = [.. logins.Where(login => IsUsernamePassword(login.user_login, login.user_pass) == true)];

            if (userSameAsPassword.Length > 0)
            {
                logger.LogInformation("Found {Count} user logins that use their username as password.", userSameAsPassword.Length);
                results.Add(new LeekScanResult
                {
                    Breach = false,
                    Message = $"Found {userSameAsPassword.Length} user logins that use their username as password.",
                });

                // for each user, check with the auditor if the username is breached

                Task<(LeekSearchRequest Request, LeekSearchResponse Response)>[] tasks = [.. userSameAsPassword
                    .Select(async login =>
                    {
                        // check if the username is breached
                        LeekSearchRequest request = new(login.user_login, ESecretType.Secret);
                        return (request, await auditor.SearchBreaches(datasources, request));
                    })];

                (LeekSearchRequest Request, LeekSearchResponse Response)[] completedTasks = await Task.WhenAll(tasks);

                bool breached = false;
                foreach (var (Request, Response) in completedTasks)
                {
                    if (Response.IsBreached)
                    {
                        breached = true;
                        logger.LogCritical("User login '{Login}' is breached!", Request.Secret);
                        results.Add(new LeekScanResult
                        {
                            Breach = true,
                            Message = $"User login '{Request.Secret}' is breached! It is used as a password.",
                        });
                    }
                }

                if (!breached)
                {
                    logger.LogInformation("No user logins found that are breached.");
                    results.Add(new LeekScanResult
                    {
                        Breach = false,
                        Message = "No user logins found that are breached.",
                    });
                }
            }
            else logger.LogInformation("No user logins found that use their username as password.");
        }
        else logger.LogInformation("No user logins found in the Wordpress database.");

        return [.. results];
    }

    record class WordpressLogin(string user_login, string user_pass);

    // public Task<LeekScanResult[]> ScanSftpAsync(ConnectionContext connection, CancellationToken cancellationToken = default)
    // {
    //     // Implement the logic to scan Wordpress via SFTP for known bad hashes
    //     throw new NotImplementedException("Wordpress SFTP scanning is not yet implemented.");
    // }

    public bool? IsUsernamePassword(string username, string hash)
    {
        // https://github.com/WordPress/WordPress/blob/fde5ddd334058c56b2d537f093abbc7a6f69735d/wp-includes/pluggable.php#L2740
        if (hash.Length <= 4096 && hash.StartsWith("$wp"))
        {
            byte[] hmac;
            using (var hasher = new System.Security.Cryptography.HMACSHA384(Encoding.UTF8.GetBytes("wp-sha384")))
                hmac = hasher.ComputeHash(Encoding.UTF8.GetBytes(username));

            string b64 = Convert.ToBase64String(hmac);

            return BCrypt.Net.BCrypt.Verify(b64, hash.Substring(3), hashType: BCrypt.Net.HashType.SHA384);
        }

        // TODO: consider legacy methods such as MD5, phpass

        logger.LogWarning("Unsupported Wordpress hash format: {Hash}", hash.Substring(3));

        return null;
    }
}
