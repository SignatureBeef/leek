// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using System.Security.Cryptography;
using System.Text;

namespace Leek.Updater;

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms

/// <summary>
/// SecLists is a provider that updates Leek with wordlists from the SecLists repository.
/// </summary>
/// <param name="wordlistReader"></param>
public class SecLists(IWordlistReader wordlistReader) : IUpdateProvider
{
    const String BaseRepositoryUrl = "https://raw.githubusercontent.com/danielmiessler/SecLists/refs/heads/master/";

    public List<string> Files { get; } =
    [
        "Passwords/Leaked-Databases/rockyou-75.txt",
    ];

    public virtual async Task UpdateIntoAsync(ProviderConnection[] connections)
    {
        Task[] tasks = [.. Files.Select(file => UpdateIntoAsync(connections, file))];
        Console.WriteLine($"Processing {tasks.Length} files for updates...");
        try
        {
            await Task.WhenAll(tasks);
            Console.WriteLine("All files processed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while processing files: {ex.Message}");
        }
    }

    public async Task UpdateIntoAsync(ProviderConnection[] connections, string fileName)
    {
        string fileUrl = $"{BaseRepositoryUrl}{fileName}";

        List<string> batch = [];
        await foreach (string line in wordlistReader.ReadLinesFromUriAsync(new Uri(fileUrl)))
        {
            batch.Add(line);
            if (batch.Count >= 1000)
            {
                Console.WriteLine($"Processing batch of {batch.Count} items from {fileName}");
                await InsertBatchAsync(connections, batch);
            }
        }

        if (batch.Count > 0)
        {
            Console.WriteLine($"Processing final batch of {batch.Count} items from {fileName}");
            await InsertBatchAsync(connections, batch);
        }
    }

    static async Task InsertBatchAsync(ProviderConnection[] connections, List<string> batch)
    {
        HashEntity[] insertRequests = [.. batch.Select(line => new HashEntity
        {
            Type = ESecretType.SHA1,
            Value = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(line))),
            KnownBreachCount = 0 // TODO: Get star count? or arbitrary value?
        })];

        Task[] tasks = [.. connections
            .Where(connection => connection.Provider is IDataWriteProvider)
            .Select(connection => (connection.Provider as IDataWriteProvider)!.AddAsync(connection.Connection, insertRequests))];
        await Task.WhenAll(tasks);

        batch.Clear();
    }
}