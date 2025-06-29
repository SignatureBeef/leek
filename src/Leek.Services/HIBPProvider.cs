// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Leek.Core.Extensions;
using Leek.Core.Providers;
using Leek.Core.Services;

namespace Leek.Services;

public class HIBPProvider : IDataProvider, IDataSearchProvider
{
    public async Task<bool> Search(ConnectionContext connection, LeekSearchRequest request, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔍 Searching breaches for secret: {request.Secret} of type: {request.SecretType} via haveibeenpwned.com");

        const String BaseUrl = "https://api.pwnedpasswords.com/range/";

        string sha1 = request.As(ESecretType.SHA1).Secret;
        string prefix = sha1[..5];
        string ending = sha1[5..] + ":"; // The API expects the hash to be in the format "hash:num"

        using HttpClient client = new();

        client.DefaultRequestHeaders.Clear();
        string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        client.DefaultRequestHeaders.Add("User-Agent", "Leek.Services/" + appVersion);

        string url = $"{BaseUrl}{prefix}";

        HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return content.Contains(ending, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public bool SupportsConnection(ConnectionContext connection)
        => connection.Provider.Equals("hibp", StringComparison.OrdinalIgnoreCase) ||
           connection.Provider.Equals("haveibeenpwned", StringComparison.OrdinalIgnoreCase);
}

public static class HIBPProviderExtensions
{
    public static ConnectionBuilder WithHIBPProvider(this ConnectionBuilder builder)
    {
        return builder.WithProvider("hibp")
                      .WithConnectionString("https://api.pwnedpasswords.com/range/");
    }
}
