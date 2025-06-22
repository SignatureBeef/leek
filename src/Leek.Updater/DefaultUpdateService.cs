// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core.Providers;
using Leek.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Leek.Updater;

public class DefaultUpdateService(IEnumerable<IUpdateProvider> authorities) : IUpdateService
{
    public async Task UpdateAsync(ProviderConnection[] connections)
    {
        var authorityList = authorities.ToList();
        if (authorityList.Count == 0)
        {
            Console.WriteLine("No update authorities configured. Please add at least one authority to the service.");
            return;
        }

        Task[] tasks = authorityList.Select(authority => authority.UpdateIntoAsync(connections)).ToArray();
        Console.WriteLine($"Processing {tasks.Length} authorities for updates...");
        try
        {
            await Task.WhenAll(tasks);
            Console.WriteLine("All authorities processed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while processing authorities: {ex.Message}");
            // Handle exceptions as needed, e.g., log them or rethrow
        }
        Console.WriteLine($"Update completed");
    }
}

/// <summary>
/// Extension methods for configuring the update service in an <see cref="IServiceCollection"/>.
/// </summary>
public static class UpdateServiceExtensions
{
    /// <summary>
    /// Registers the default update service and its providers in the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the update service to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the default update service registered.</returns>
    public static IServiceCollection AddDefaultUpdateService(this IServiceCollection services)
    {
        services.AddTransient<IUpdateService, DefaultUpdateService>();
        services.AddTransient<IUpdateProvider, HIBP>();
        services.AddTransient<IUpdateProvider, SecLists>();
        return services;
    }
}