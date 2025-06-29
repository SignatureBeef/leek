// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Leek.CLI.Commands;

public class CheckCommand : Command
{
    public CheckCommand() : base("check", "Check a secret against known bad hashes.")
    {
        AddOption(Type);
        AddArgument(Secret);
        AddOption(Provider);
    }

    static readonly Argument<string> Secret = new(
        name: "secret",
        description: "The secret to check (can be quoted or escaped)")
    {
        Arity = ArgumentArity.ExactlyOne
    };

    static readonly Option<ESecretType?> Type = new(
         aliases: ["--type", "-t"],
         description: "Specify the type of secret (e.g., secret, sha1).",
         parseArgument: result =>
         {
             string? token = result.Tokens.SingleOrDefault()?.Value;

             if (string.IsNullOrWhiteSpace(token))
                 return null;

             if (Enum.TryParse<ESecretType>(token, ignoreCase: true, out ESecretType value))
                 return value;

             string valid = string.Join(", ", Enum.GetNames<ESecretType>());
             result.ErrorMessage = $"Invalid secret type: '{token}'. Valid values: {valid}.";
             return null;
         }
    )
    {
        IsRequired = false
    };

    static readonly Option<string[]?> Provider = new(
         aliases: ["--provider", "-p"],
         description: "The provider to use (e.g., sqlite://, mssql://, etc.).")
    {
        IsRequired = false, // defaults will be to all providers
        AllowMultipleArgumentsPerToken = true,
    };
}

public class CheckCommandHandler(IAuditor auditor, IEnumerable<IDataProvider> dataProviders, ILogger<CheckCommandHandler> logger) : ICommandHandler
{
    public ESecretType? Type { get; set; }
    public string Secret { get; set; } = "";
    public string[]? Provider { get; set; }

    public int Invoke(InvocationContext context) => throw new NotImplementedException();

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        ConnectionContext[] connectionContexts = SharedCommandOptions.CreateConnections(dataProviders, Provider ?? []);

        if (connectionContexts.Length == 0)
        {
            logger.LogError("❗ No providers found for the specified connections.");
            return -1; // Indicate no providers found
        }

        LeekSearchResponse response = await auditor.SearchBreaches(connectionContexts, new LeekSearchRequest(Secret, Type ?? ESecretType.Secret));

        if (response.IsBreached)
        {
            string foundIn = String.IsNullOrWhiteSpace(response.Location)
                ? ""
                : $" in {response.Location}";
            logger.LogCritical($"🚨 Breach found{foundIn}! The secret '{Secret}' is compromised, searched {connectionContexts.Length} providers.");
            return 1; // Indicate breach found
        }
        else
        {
            logger.LogInformation($"✅ No breaches found for the secret '{Secret}', searched {connectionContexts.Length} providers.");
        }

        return 0;
    }
}