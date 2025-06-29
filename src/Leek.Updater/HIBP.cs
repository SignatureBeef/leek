// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Leek.Updater;

/// <summary>
/// Provides an implementation for updating breaches from haveibeenpwned.com.
/// </summary>
public class HIBP(ILogger<HIBP> logger) : IUpdateProvider
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
        logger.LogInformation("🔍 Updating breaches from haveibeenpwned.com for {Max} hash prefixes...", Max);

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

                    Task[] tasks = [.. connections
                        .Where(item => item.Provider is IDataWriteProvider)
                        .Select(item => (item.Provider as IDataWriteProvider)!.AddAsync(item.Connection, data))];

                    DateTime msDataProvidersStarted = DateTime.UtcNow;

                    await Task.WhenAll(tasks);

                    double msDataProvidersProcessed = Math.Round((DateTime.UtcNow - msDataProvidersStarted).TotalMilliseconds, 2);

                    logger.LogInformation("[{Hash}] #{Count} {Progress:P2}% rem:{Remaining} days:{Days} hps:{HashesPerSecond} p:{Providers} h:{HttpReq}ms dp:{DataProviders}ms",
                        hash, data.Length, progress, remaining, ts.TotalDays, hashesPerSecond, tasks.Length, msHttpReq, msDataProvidersProcessed);

                    totalHashes += data.LongLength;
                }
                else
                {
                    logger.LogWarning("Failed to fetch data for hash prefix: {Hash}, Status Code: {StatusCode}", hash, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching data for hash prefix {Hash}", hash);
                if (ex.InnerException != null)
                {
                    logger.LogError(ex.InnerException, "Inner exception while fetching data for hash prefix {Hash}", hash);
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
