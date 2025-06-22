// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;
using Leek.Core.Services;
using Leek.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Leek.Tests.Fixtures;

public class AuditFixture : IDisposable, IAsyncDisposable
{
    public ServiceProvider ServiceProvider { get; private set; }

    public IAuditor Service { get; }

    public ConnectionContext WordlistConnection => new("wordlist", "example-wordlist.txt");

    public static readonly string[] Wordlist = [
        "password",
        "123456",
        "letmein",
        "qwerty",
        "test"
    ];

    public AuditFixture()
    {
        File.WriteAllLines(WordlistConnection.ConnectionString, Wordlist);

        ServiceCollection services = new();
        services.AddLeekServices()
            .AddWordlistProvider()
            .AddWordlistReader();
        ServiceProvider = services.BuildServiceProvider();

        Service = ServiceProvider.GetRequiredService<IAuditor>();
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await ServiceProvider.DisposeAsync();
    }
}
