// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Leek.Core.Services;
using Leek.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Leek.Tests;

public class WordlistTests(AuditFixture fixture) : IClassFixture<AuditFixture>
{
    /// <summary>
    /// Ensure that a known breached password is detected as breached.
    /// </summary>
    [Fact]
    public async Task EnsureIsBreached()
    {
        LeekSearchResponse response = await fixture.Service.SearchBreaches([fixture.WordlistConnection], new LeekSearchRequest("test"));

        Assert.True(response.IsBreached, "Expected a match in the wordlist.");
    }

    /// <summary>
    /// Ensure that a word not in the wordlist is not considered breached.
    /// </summary>
    [Fact]
    public async Task EnsureIsNotBreached()
    {
        LeekSearchResponse response = await fixture.Service.SearchBreaches([fixture.WordlistConnection], new LeekSearchRequest("test-not-in-wordlist"));

        Assert.False(response.IsBreached, "Expected no match in wordlist.");
    }

    /// <summary>
    /// Ensure that the wordlist can be iterated over.
    /// This is important for the wordlist reader to function correctly, and to ensure overhead is minimal when reading large files
    /// </summary>
    [Fact]
    public async Task EnsureIteration()
    {
        IWordlistReader reader = fixture.ServiceProvider.GetRequiredService<IWordlistReader>();

        int count = 0;

        await foreach (string word in reader.ReadLinesFromFileAsync(fixture.WordlistConnection.ConnectionString))
        {
            count++;
            Assert.Contains(word, AuditFixture.Wordlist);
        }

        Assert.Equal(AuditFixture.Wordlist.Length, count);
    }
}
