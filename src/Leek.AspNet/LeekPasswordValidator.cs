// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using Leek.Core;
using Leek.Core.Providers;
using Leek.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Leek.AspNet;

/// <summary>
/// LeekPasswordValidator is an ASP.NET Identity password validator that checks if a password has been found in a breach.
/// </summary>
/// <typeparam name="TUser">The applications user type, passed to the UserManager.</typeparam>
/// <param name="auditor">An instance of IAuditor to perform breach checks.</param>
/// <param name="options">Configuration options for the LeekPasswordValidator, including connection settings.</param>
public class LeekPasswordValidator<TUser>(IAuditor auditor, IOptions<LeekPasswordValidatorOptions> options, ILogger<LeekPasswordValidator<TUser>> logger) : IPasswordValidator<TUser> where TUser : class
{
    /// <inheritdoc />
    public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
    {
        logger.LogInformation("LeekPasswordValidator: Validating password for user {UserType}: {Password}", user?.GetType().Name ?? "unknown", password);

        LeekSearchResponse response = await auditor.SearchBreaches(options.Value.Connections, new LeekSearchRequest(password));
        if (response.IsBreached)
        {
            logger.LogCritical("LeekPasswordValidator: Password breach found for user ");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordBreach",
                Description = "The password has been found in a breach and is not allowed."
            });
        }

        return IdentityResult.Success;
    }
}

/// <summary>
/// Options for configuring the LeekPasswordValidator.
/// </summary>
public class LeekPasswordValidatorOptions
{
    /// <summary>
    /// An array of connection contexts that the validator will use to check for breaches.
    /// </summary>
    public required ConnectionContext[] Connections { get; set; }
}
