// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Leek.Services;

/// <summary>
/// Builder class for configuring Leek services.
/// </summary>
/// <param name="services"></param>
public class LeekBuilder(IServiceCollection services)
{
    /// <summary>
    /// Gets the service collection used to configure Leek services.
    /// </summary>
    public IServiceCollection Services { get; } = services;
}

/// <summary>
/// Provides extension methods for configuring Leek services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers the bare minimum services required for Leek to function.
    /// </summary>
    /// <param name="services">Service collection to add services to.</param>
    /// <returns>A <see cref="LeekBuilder"/> instance for further configuration.</returns>
    public static LeekBuilder AddLeekServices(this IServiceCollection services)
    {
        LeekBuilder builder = new(services);
        services.AddTransient<IAuditor, DefaultAuditor>();
        return builder;
    }

    /// <summary>
    /// Adds a database provider to the Leek service collection.
    /// </summary>
    /// <param name="builder">The <see cref="LeekBuilder"/> instance to configure.</param>
    /// <returns>The same <see cref="LeekBuilder"/> instance for method chaining.</returns>
    public static LeekBuilder AddDatabaseProvider(this LeekBuilder builder)
    {
        builder.Services.AddTransient<IDataProvider, DatabaseProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a file store provider to the Leek service collection.
    /// </summary>
    /// <param name="builder">The <see cref="LeekBuilder"/> instance to configure.</param>
    /// <returns>The same <see cref="LeekBuilder"/> instance for method chaining.</returns>
    public static LeekBuilder AddFileStoreProvider(this LeekBuilder builder)
    {
        builder.Services.AddTransient<IDataProvider, FileStoreDataProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a Have I Been Pwned (HIBP) provider to the Leek service collection.
    /// </summary>
    /// <param name="builder">The <see cref="LeekBuilder"/> instance to configure.</param>
    /// <returns>The same <see cref="LeekBuilder"/> instance for method chaining.</returns>
    public static LeekBuilder AddHIBPProvider(this LeekBuilder builder)
    {
        builder.Services.AddTransient<IDataProvider, HIBPProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a wordlist reader to the Leek service collection.
    /// </summary>
    /// <param name="builder">The <see cref="LeekBuilder"/> instance to configure.</param>
    /// <returns>The same <see cref="LeekBuilder"/> instance for method chaining.</returns>
    public static LeekBuilder AddWordlistReader(this LeekBuilder builder)
    {
        builder.Services.AddTransient<IWordlistReader, DefaultWordlistReader>();
        return builder;
    }

    /// <summary>
    /// Adds a wordlist provider to the Leek service collection.
    /// </summary>
    /// <param name="builder">The <see cref="LeekBuilder"/> instance to configure.</param>
    /// <returns>The same <see cref="LeekBuilder"/> instance for method chaining.</returns>
    public static LeekBuilder AddWordlistProvider(this LeekBuilder builder)
    {
        builder.Services.AddTransient<IDataProvider, WordlistProvider>();
        return builder;
    }

    /// <summary>
    /// Adds a set of default services to the Leek service collection.
    /// This includes database, file store, HIBP, wordlist reader, and wordlist provider.
    /// </summary>
    /// <param name="builder">The <see cref="LeekBuilder"/> instance to configure.</param>
    /// <returns>The same <see cref="LeekBuilder"/> instance for method chaining.</returns>
    public static LeekBuilder AddDefaultServices(this LeekBuilder builder)
    {
        builder.AddDatabaseProvider()
               .AddFileStoreProvider()
               .AddHIBPProvider()
               .AddWordlistReader()
               .AddWordlistProvider();
        return builder;
    }
}
