// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Leek.Services;

public class FileStoreDataProvider(ILogger<FileStoreDataProvider> logger) : IDataProvider, IDataReadProvider, IDataWriteProvider, IDataSearchProvider
{
    public async Task AddAsync(ConnectionContext connection, HashEntity[] items, CancellationToken cancellationToken = default)
    {
        if (String.IsNullOrWhiteSpace(connection.ConnectionString))
        {
            logger.LogInformation($"[{nameof(FileStoreDataProvider)}] No connection string provided, using default 'filestore' directory.");
            connection.ConnectionString = "filestore";
        }

        if (!Directory.Exists(connection.ConnectionString)) Directory.CreateDirectory(connection.ConnectionString);

        var secretTypes = items
            .Select(item => item.Type)
            .Distinct()
            .Select(secretType => new
            {
                secretType,
                hashes = items.Where(x => x.Type == secretType).ToArray(),
            })
            .ToArray();

        foreach (var secretType in secretTypes)
        {
            var hashGroups = secretType.hashes
                .Select(x => x.Value[..5].ToLower())
                .Distinct()
                .Select(hashGroup => new
                {
                    secretType.secretType,
                    hashGroup,
                    hashes = secretType.hashes.Where(x => x.Value[..5].ToLower() == hashGroup).ToArray()
                });

            foreach (var hashGroup in hashGroups)
            {
                string sub = hashGroup.hashGroup[..3];
                string secretTypeFolder = GetSecretFolder(connection, hashGroup.secretType, hashGroup.hashGroup);
                string hashGroupFile = GetHashFile(secretTypeFolder, hashGroup.hashGroup);

                Directory.CreateDirectory(secretTypeFolder);
                await File.WriteAllLinesAsync(hashGroupFile, hashGroup.hashes.Select(x => $"{x.Value.ToLower()}:{x.KnownBreachCount}"), cancellationToken);
            }
        }
    }

    static string GetSecretFolder(ConnectionContext connection, ESecretType type, string hash) =>
        Path.Combine(connection.ConnectionString, type.ToString().ToLower(), hash[..3].ToLower());

    static string GetHashFile(string secretFolder, string hash) => Path.Combine(secretFolder, hash[..5].ToLower() + ".txt");

    public async Task<bool> Search(ConnectionContext connection, LeekSearchRequest request, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(connection.ConnectionString)) Directory.CreateDirectory(connection.ConnectionString);

        string directory = GetSecretFolder(connection, request.SecretType, request.Secret);
        if (!Directory.Exists(directory))
            return false;

        string hashGroupFile = GetHashFile(directory, request.Secret);
        if (!File.Exists(hashGroupFile))
            return false;

        string lowered = request.Secret.ToLower();

        // read line by line
        using FileStream fs = File.OpenRead(hashGroupFile);
        using var sr = new StreamReader(fs);
        while (!sr.EndOfStream)
        {
            string? line = await sr.ReadLineAsync(cancellationToken);
            if (line?.StartsWith(lowered + ':') == true)
                return true;
        }

        return false;
    }

    public bool SupportsConnection(ConnectionContext connection)
    {
        return connection.Provider.Equals("directory", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public ConnectionContext? CreateDefaultConnection() => new ConnectionContext
    {
        Provider = "directory",
        ConnectionString = "filestore"
    };

    // public async Task<long> GetHashCountAsync(ConnectionContext connection, CancellationToken cancellationToken = default)
    // {
    //     if (!Directory.Exists(connection.ConnectionString))
    //         return 0;

    //     //long count = 0;
    //     //foreach (var file in Directory.EnumerateFiles(connection.ConnectionString, "*", SearchOption.AllDirectories))
    //     //{
    //     //    count += (new FileInfo(file)).
    //     //    cancellationToken.ThrowIfCancellationRequested();
    //     //    await Task.Yield();
    //     //}

    //     //return count;


    //     var files = Directory.EnumerateFiles(connection.ConnectionString, "*", SearchOption.AllDirectories);

    //     var semaphore = new SemaphoreSlim(4);
    //     var tasks = files.Select(async file =>
    //     {
    //         await semaphore.WaitAsync(cancellationToken);
    //         try
    //         {
    //             long lineCount = 0;
    //             using var reader = File.OpenText(file);
    //             while (await reader.ReadLineAsync() != null)
    //             {
    //                 lineCount++;
    //             }
    //             return lineCount;
    //         }
    //         catch
    //         {
    //             // Log or skip unreadable files
    //             return 0;
    //         }
    //         finally
    //         {
    //             semaphore.Release();
    //         }
    //     });

    //     var results = await Task.WhenAll(tasks);
    //     return results.Sum();
    // }

    public async IAsyncEnumerable<HashEntity> GetHashesAsync(ConnectionContext connection, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(connection.ConnectionString))
            yield break;

        IEnumerable<string> secretTypeDirs = Directory.EnumerateDirectories(connection.ConnectionString);

        foreach (string secretTypeDir in secretTypeDirs)
        {
            // Try to parse the secret type folder name into ESecretType
            if (!Enum.TryParse<ESecretType>(Path.GetFileName(secretTypeDir), ignoreCase: true, out ESecretType secretType))
                continue;

            IEnumerable<string> prefixDirs = Directory.EnumerateDirectories(secretTypeDir);
            foreach (string prefixDir in prefixDirs)
            {
                IEnumerable<string> files = Directory.EnumerateFiles(prefixDir, "*.txt");
                foreach (string file in files)
                {
                    using FileStream stream = File.OpenRead(file);
                    using var reader = new StreamReader(stream);
                    while (!reader.EndOfStream)
                    {
                        string? line = await reader.ReadLineAsync(cancellationToken);
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] parts = line.Split(':', 2);
                        if (parts.Length != 2) continue;

                        yield return new HashEntity
                        {
                            Type = secretType,
                            Value = parts[0],
                            KnownBreachCount = int.TryParse(parts[1], out int count) ? count : 0,
                        };
                    }
                }
            }
        }
    }
}

public static class FileStoreDataProviderExtensions
{
    public static ConnectionBuilder WithFileStore(this ConnectionBuilder builder, string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));

        return builder.WithProvider("directory").WithConnectionString(directoryPath);
    }
}
