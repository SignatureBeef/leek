// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using System.Diagnostics;

namespace Leek.Updater;

/// <summary>
/// Provides an implementation for updating breaches from haveibeenpwned.com.
/// </summary>
public class HIBP : IUpdateProvider
{
    static IEnumerable<string> GenerateHashPrefixes()
    {
        for (int i = 0; i <= Max; i++)
            yield return i.ToString("X5");
    }

    const int Max = 0xFFFFF;

    //static string[] GetHashPrefixes() => [.. (new int[Max]).Select(x => x.ToString("X5"))];

    public async Task UpdateIntoAsync(ProviderConnection[] connections)
    {
        Console.WriteLine($"🔍 Updating breaches from haveibeenpwned.com for {Max} hash prefixes...");

        using HttpClient client = new();
        int count = 0;
        var sw = Stopwatch.StartNew();
        long totalHashes = 0;

        var timeSinceLastApi = Stopwatch.StartNew();
        foreach (string hash in GenerateHashPrefixes())
        // var hash = "a94a8";
        {
            float progress = ++count / (float)Max;
            int remaining = Max - count;
            double eta = sw.Elapsed.TotalSeconds / progress * (1 - progress); // or: sw.Elapsed.TotalSeconds * remaining / count
            var ts = TimeSpan.FromSeconds(eta);

            double hashesPerSecond = Math.Round(totalHashes / sw.Elapsed.TotalSeconds, 2);


            string url = $"https://api.pwnedpasswords.com/range/{hash}";
            try
            {
                DateTime started = DateTime.UtcNow;

                // add common headers
                client.DefaultRequestHeaders.Clear();
                string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
                client.DefaultRequestHeaders.Add("User-Agent", "Leek.Updater/" + appVersion);

                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    // Console.WriteLine($"Successfully fetched data for hash prefix: {hash}");
                    // Console.WriteLine($"Content: {content[..Math.Min(100, content.Length)]}..."); // Display first 100 characters

                    // Console.WriteLine($"Response Headers for {hash}:");
                    // foreach (var header in response.Headers)
                    // {
                    //     Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
                    // }

                    double msHttpReq = Math.Round((DateTime.UtcNow - started).TotalMilliseconds, 2);

                    HashEntity[] data = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(line => line.Split(':'))
                                                   .Where(parts => parts.Length == 2)
                                                   .Select(parts => new HashEntity
                                                   {
                                                       Type = ESecretType.SHA1,
                                                       Value = $"{hash}{parts[0].Trim()}",
                                                       KnownBreachCount = int.Parse(parts[1].Trim())
                                                   })
                                                   .ToArray();

                    Task[] tasks = connections
                        .Where(item => item.Provider is IDataWriteProvider)
                        .Select(item => (item.Provider as IDataWriteProvider)!.AddAsync(item.Connection, data))
                        .ToArray();

                    DateTime msDataProvidersStarted = DateTime.UtcNow;

                    //await Task.WhenAll(tasks);
                    // Console.WriteLine($"Inserted {data.Length} records for hash prefix: {hash} into {tasks.Length} providers.");

                    //_ = Task.WhenAll(tasks).ConfigureAwait(false);
                    await Task.WhenAll(tasks);

                    double msDataProvidersProcessed = Math.Round((DateTime.UtcNow - msDataProvidersStarted).TotalMilliseconds, 2);

                    Console.WriteLine($"[{hash}] #{data.Length} {progress * 100:F2}% rem:{remaining} days:{ts.TotalDays} hps:{hashesPerSecond} p:{tasks.Length} h:{msHttpReq}ms dp:{msDataProvidersProcessed}ms");

                    totalHashes += data.LongLength;
                }
                else
                {
                    Console.WriteLine($"Failed to fetch data for hash prefix: {hash}, Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data for hash prefix {hash}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Error fetching data for hash prefix {hash}: {ex.InnerException.Message}");
                }
            }

            // TODO: replace with a decent strategy

            int delay = 50 - (int)timeSinceLastApi.ElapsedMilliseconds;

            // avoid hitting the API too fast
            // TODO: review api rate limits and adjust accordingly
            if (delay > 0)
                await Task.Delay(delay);

            timeSinceLastApi.Restart();
        }
    }
}
