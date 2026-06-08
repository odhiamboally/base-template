using BT.Domain.Features.IAM.Users.Entities;
using BT.Domain.Features.IAM.Users.Enums;
using BT.Infrastructure.Configuration;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BT.Infrastructure.Features.IAM.Users.Seeding;

internal sealed class DevelopmentIdentitySeeder(
    RoleManager<AppRole> roleManager,
    UserManager<AppUser> userManager,
    IOptions<DevelopmentSeedSettings> options,
    ILogger<DevelopmentIdentitySeeder> logger)
{
    private readonly DevelopmentSeedSettings _settings = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return;
        }

        try
        {
            await SeedRolesAsync().ConfigureAwait(false);
            await SeedAdminUserAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogDevelopmentIdentitySeedError(logger, ex);
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        foreach (Roles role in Enum.GetValues<Roles>())
        {
            var roleName = role.ToDisplayString();

            if (await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new AppRole { Name = roleName }).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed role '{roleName}': {FormatErrors(result)}");
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var admin = await userManager.FindByEmailAsync(_settings.AdminEmail).ConfigureAwait(false);
        if (admin is null)
        {
            admin = AppUser.CreateSystemUser(
                _settings.TenantId,
                _settings.AdminUserName,
                _settings.AdminEmail,
                "DevelopmentSeed");

            admin.FirstName = "Template";
            admin.LastName = "Admin";
            admin.EmailConfirmed = true;
            admin.PhoneNumberConfirmed = true;
            admin.IsActive = true;
            admin.ActivatedAt = DateTimeOffset.UtcNow;
            admin.ActivatedBy = "DevelopmentSeed";
            admin.RequirePasswordChange = false;
            admin.PasswordLastChanged = DateTimeOffset.UtcNow;

            var createResult = await userManager.CreateAsync(admin, _settings.AdminPassword).ConfigureAwait(false);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed development admin user: {FormatErrors(createResult)}");
            }
        }

        var adminRole = Roles.SysAdmin.ToDisplayString();
        if (!await userManager.IsInRoleAsync(admin, adminRole).ConfigureAwait(false))
        {
            var addRoleResult = await userManager.AddToRoleAsync(admin, adminRole).ConfigureAwait(false);
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to assign development admin role: {FormatErrors(addRoleResult)}");
            }
        }

        ServiceLogDefinitions.LogDevelopmentAdminSeeded(logger, admin.Email ?? _settings.AdminEmail);
    }

    private static string FormatErrors(IdentityResult result)
        => string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
}
