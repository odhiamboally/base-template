using BT.Application.Contracts.Interfaces.Services;
using BT.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Globalization;
using BT.Infrastructure.Logging;

namespace BT.Infrastructure.Contracts.Implementations.Services; 

internal sealed class ClaimsService(
    UserManager<AppUser> userManager,
    RoleManager<AppUser> roleManager, ILogger<ClaimsService> logger) : IClaimsService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<AppUser> _roleManager = roleManager;
    private readonly ILogger<ClaimsService> _logger = logger;

    public async Task<bool> AddUserClaimAsync(AppUser user, Claim claim)
    {
        try
        {
            var result = await _userManager.AddClaimAsync(user, claim).ConfigureAwait(false);
            if (result.Succeeded)
            {
                ServiceLogDefinitions.LogClaimAdded(_logger, claim.Type, claim.Value, user.Id);

                return true;
            }

            ServiceLogDefinitions.LogFailedToAddClaim(_logger, claim.Type, claim.Value, user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));

            return false;
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogErrorAddingClaim(_logger, user.Id, ex);
            throw;
        }
    }

    public async Task<List<Claim>> GetUserClaimsAsync(AppUser user)
    {
        try
        {
            var claims = new List<Claim>
            {
                // Standard Identity
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),

                new("tenant_id", user.TenantId.ToString()),
                new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            };

            if (user.EmployeeId.HasValue)
                claims.Add(new("employee_id", user.EmployeeId.Value.ToString()));

            if (user.CustomerId.HasValue)
                claims.Add(new("member_id", user.CustomerId.Value.ToString()));

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            foreach (var roleName in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));

                // 2. DYNAMICALLY fetch permissions linked to this role from the DB
                var role = await _roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role).ConfigureAwait(false);

                    // Permissions are stored as claims with type "permission"
                    var permissions = roleClaims.Where(c => c.Type == "permission");
                    claims.AddRange(permissions);
                }
            }

            //claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            return claims;
        }
        catch (Exception)
        {
            throw;
        }

    }

    public async Task<List<Claim>> GetUserClaimsCombinedAsync(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),

            new("tenant_id", user.TenantId.ToString()),
            new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        };


        // Get permissions from the User's ROLES
        var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role).ConfigureAwait(false);
                claims.AddRange(roleClaims.Where(c => c.Type == "permission"));
            }
        }

        // Get permissions assigned DIRECTLY to the USER (the "Permission 5" case)
        var directClaims = await _userManager.GetClaimsAsync(user).ConfigureAwait(false);
        claims.AddRange(directClaims.Where(c => c.Type == "permission"));

        // Return a distinct list (so Permission 1 isn't added twice if it's in both)
        return [.. claims.GroupBy(c => c.Value).Select(g => g.First())];
    }

    public async Task<bool> RemoveUserClaimAsync(AppUser user, Claim claim)
    {
        try
        {
            var result = await _userManager.RemoveClaimAsync(user, claim).ConfigureAwait(false);
            if (result.Succeeded)
            {
                ServiceLogDefinitions.LogClaimRemoved(_logger, claim.Type, claim.Value, user.Id);

                return true;
            }

            ServiceLogDefinitions.LogFailedToRemoveClaim(_logger, claim.Type, claim.Value, user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));

            return false;
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogErrorRemovingClaim(_logger, user.Id, ex);
            throw;
        }
    }

    public async Task<bool> UpdateUserClaimAsync(AppUser user, Claim existingClaim, Claim newClaim)
    {
        try
        {
            // Remove old claim and add new one
            var removeResult = await _userManager.RemoveClaimAsync(user, existingClaim).ConfigureAwait(false);
            if (!removeResult.Succeeded)
            {
                ServiceLogDefinitions.LogFailedToRemoveExistingClaim(_logger, user.Id);

                return false;
            }

            var addResult = await _userManager.AddClaimAsync(user, newClaim).ConfigureAwait(false);
            if (!addResult.Succeeded)
            {
                // Try to rollback by adding the old claim back
                await _userManager.AddClaimAsync(user, existingClaim).ConfigureAwait(false);

                ServiceLogDefinitions.LogFailedToAddNewClaimRolledBack(_logger, user.Id);

                return false;
            }

            ServiceLogDefinitions.LogUpdatedClaim(_logger, user.Id, $"{existingClaim.Type}:{existingClaim.Value}", $"{newClaim.Type}:{newClaim.Value}");

            return true;
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogErrorUpdatingClaim(_logger, user.Id, ex);
            throw;
        }
    }


}

