// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using System.Diagnostics;
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using Leek.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace Leek.Services;

public class DefaultAuditor(IEnumerable<IDataProvider> providers, ILogger<DefaultAuditor> logger) : IAuditor
{
    public async Task<LeekSearchResponse> SearchBreaches(ConnectionContext[] connections, LeekSearchRequest request)
    {
        logger.LogInformation("🔍 Searching breaches for secret: {Secret} of type: {SecretType}", request.Secret, request.SecretType);

        CancellationTokenSource cts = new();

        List<Task<LeekSearchResponse>> tasks = request.SecretType != ESecretType.Secret
            ? [
                SearchHashAsync(connections, request, cts.Token)
              ]
              : [.. Enum.GetValues<ESecretType>()
                .Where(x => x != ESecretType.Secret) // Exclude the plain text secret type
                .Select(secretType => SearchHashAsync(connections, new LeekSearchRequest(request.Secret, secretType), cts.Token))
            ];

        while (tasks.Count != 0)
        {
            Task<LeekSearchResponse> completed = await Task.WhenAny(tasks);
            tasks.Remove(completed);

            LeekSearchResponse result = await completed;
            if (result.IsBreached)
            {
                cts.Cancel(); // Cancel the others
                return result;
            }
        }

        return new LeekSearchResponse(false, $"✅ No breaches found for secret: {request.Secret} ({request.SecretType})", null);
    }

    async Task<LeekSearchResponse> SearchHashAsync(ConnectionContext[] connections, LeekSearchRequest request, CancellationToken cancellationToken)
    {
        LeekSearchRequest hashed = request.HashAs(request.SecretType);
        logger.LogInformation("👀 Searching {SecretType} hash: {Secret} ({RequestSecretType})", hashed.SecretType, hashed.Secret, request.SecretType);

        var stopwatch = Stopwatch.StartNew();

        ProviderConnection[] connectionProviders = connections.AsProviderConnections(providers);
        if (connectionProviders.Length == 0)
            return new(false, "❗ No providers found for connections.", null);

        (IDataSearchProvider? Provider, ConnectionContext Connection)[] searchProviders = [.. connectionProviders
            .Select(ctx => (Provider: ctx.Provider as IDataSearchProvider, ctx.Connection))];

        if (searchProviders.Any(x => x.Provider == null))
            return new(false, "❗ No search providers found for connections.", null);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        CancellationToken internalToken = linkedCts.Token;

        var tasks = searchProviders.Select(async tuple =>
        {
            try
            {
                bool isBreached = await tuple.Provider!.Search(tuple.Connection, hashed, internalToken);
                return (IsBreached: isBreached, tuple.Connection);
            }
            catch
            {
                linkedCts.Cancel(); // Cancel all others on any error
                throw;
            }
        }).ToList();

        try
        {
            while (tasks.Count > 0)
            {
                Task<(bool IsBreached, ConnectionContext Connection)> completed = await Task.WhenAny(tasks);
                tasks.Remove(completed);

                // if (completed.IsFaulted && completed.Exception?.InnerException is not TaskCanceledException)
                //     return new(false, $"❗ Error searching for {hashed.SecretType} hash: {hashed.Secret} ({request.SecretType})\nError: {completed.Exception?.InnerException}", null);

                // propogate exceptions
                if (completed.IsFaulted)
                {
                    Exception? innerException = completed.Exception?.InnerException;
                    if (innerException is not null && innerException is not TaskCanceledException)
                        throw new Exception($"❗ Error searching for {hashed.SecretType} hash: {hashed.Secret} ({request.SecretType})", innerException);
                }

                if (completed.IsCompletedSuccessfully && completed.Result.IsBreached)
                {
                    linkedCts.Cancel(); // Found a match, cancel all others
                    stopwatch.Stop();
                    return new(true, $"🚨 Breach found for hash: {hashed.Secret} ({request.SecretType}) (in {stopwatch.ElapsedMilliseconds}ms)", completed.Result.Connection.Provider);
                }
            }
        }
        catch (OperationCanceledException)
        {
            return new(false, $"⏹️ Search cancelled for: {hashed.Secret} ({hashed.SecretType})", null);
        }

        stopwatch.Stop();
        return new(false, $"✅ No breach found for hash: {hashed.Secret} ({request.SecretType}) (in {stopwatch.ElapsedMilliseconds}ms)", null);
    }
}
