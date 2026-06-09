using BT.Domain.Features.IAM.Users.Entities;
using BT.Domain.Features.IAM.Users.Enums;
using BT.Infrastructure.Configuration;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private static readonly Guid EmployeeAlexId = new("0194f800-0000-7000-8000-000000000001");
    private static readonly Guid EmployeeAllanId = new("0194f800-0000-7000-8000-000000000002");
    private static readonly Guid EmployeeLauraId = new("0194f800-0000-7000-8000-000000000003");

    private static readonly DevelopmentEmployeeUser[] DevelopmentEmployeeUsers =
    [
        new(EmployeeAlexId, "aamodhiambo@gmail.com", "Alex", "Odhiambo", "14042262", "+254798980115", IsBootstrapAdmin: true),
        new(EmployeeAllanId, "allan.alex0803@gmail.com", "Allan", "Alex", "67532424", "+254700057578", IsBootstrapAdmin: false),
        new(EmployeeLauraId, "omitolaura469@gmail.com", "Laura", "Omito", "76945774", "+254719423686", IsBootstrapAdmin: false),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return;
        }

        try
        {
            await SeedRolesAsync().ConfigureAwait(false);
            await SeedDevelopmentEmployeeUsersAsync(cancellationToken).ConfigureAwait(false);
            await RemoveStaleDevelopmentUsersAsync(cancellationToken).ConfigureAwait(false);
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

    private async Task SeedDevelopmentEmployeeUsersAsync(CancellationToken cancellationToken)
    {
        foreach (var employeeUser in DevelopmentEmployeeUsers)
        {
            var user = await userManager.Users
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    existing => existing.EmployeeId == employeeUser.EmployeeId || existing.Email == employeeUser.Email,
                    cancellationToken)
                .ConfigureAwait(false);

            if (user is null)
            {
                user = AppUser.CreateForEmployee(
                    _settings.TenantId,
                    employeeUser.EmployeeId,
                    employeeUser.Email,
                    employeeUser.FirstName,
                    employeeUser.LastName,
                    employeeUser.Email,
                    employeeUser.PhoneNumber,
                    employeeUser.NationalId,
                    "DevelopmentSeed");

                ApplyDevelopmentUserState(user, employeeUser);

                var createResult = await userManager.CreateAsync(user, _settings.AdminPassword).ConfigureAwait(false);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to seed development user '{employeeUser.Email}': {FormatErrors(createResult)}");
                }
            }
            else
            {
                ApplyDevelopmentUserState(user, employeeUser);

                var updateResult = await userManager.UpdateAsync(user).ConfigureAwait(false);
                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to update development user '{employeeUser.Email}': {FormatErrors(updateResult)}");
                }
            }

            if (employeeUser.IsBootstrapAdmin)
            {
                await EnsureAdminRoleAsync(user).ConfigureAwait(false);
                ServiceLogDefinitions.LogDevelopmentAdminSeeded(logger, user.Email ?? employeeUser.Email);
            }
        }
    }

    private static void ApplyDevelopmentUserState(AppUser user, DevelopmentEmployeeUser employeeUser)
    {
        user.TenantId = new Guid("0194f700-0000-7000-8000-000000000001");
        user.EmployeeId = employeeUser.EmployeeId;
        user.CustomerId = null;
        user.UserName = employeeUser.Email;
        user.Email = employeeUser.Email;
        user.FirstName = employeeUser.FirstName;
        user.LastName = employeeUser.LastName;
        user.NationalId = employeeUser.NationalId;
        user.PhoneNumber = employeeUser.PhoneNumber;
        user.EmailConfirmed = true;
        user.PhoneNumberConfirmed = true;
        user.IsActive = employeeUser.IsBootstrapAdmin;
        user.RequirePasswordChange = !employeeUser.IsBootstrapAdmin;
        user.ActivatedAt = employeeUser.IsBootstrapAdmin ? DateTimeOffset.UtcNow : null;
        user.ActivatedBy = employeeUser.IsBootstrapAdmin ? "DevelopmentSeed" : null;
        user.DeactivatedAt = employeeUser.IsBootstrapAdmin ? null : DateTimeOffset.UtcNow;
        user.DeactivatedBy = employeeUser.IsBootstrapAdmin ? null : "DevelopmentSeed";
        user.DeactivationReason = employeeUser.IsBootstrapAdmin ? null : "Seeded inactive so Grant Access can be tested.";
        user.IsDeleted = false;
        user.DeletedAt = null;
        user.DeletedBy = null;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.UpdatedBy = "DevelopmentSeed";
    }

    private async Task EnsureAdminRoleAsync(AppUser user)
    {
        var adminRole = Roles.SysAdmin.ToDisplayString();
        if (await userManager.IsInRoleAsync(user, adminRole).ConfigureAwait(false))
        {
            return;
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, adminRole).ConfigureAwait(false);
        if (!addRoleResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to assign development admin role: {FormatErrors(addRoleResult)}");
        }
    }

    private async Task RemoveStaleDevelopmentUsersAsync(CancellationToken cancellationToken)
    {
        var retainedEmails = DevelopmentEmployeeUsers
            .Select(static user => user.Email)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var retainedEmployeeIds = DevelopmentEmployeeUsers
            .Select(static user => user.EmployeeId)
            .ToHashSet();

        var staleUsers = await userManager.Users
            .IgnoreQueryFilters()
            .Where(user =>
                (user.Email != null && !retainedEmails.Contains(user.Email) && user.Email.EndsWith("@basetemplate.local")) ||
                user.Email == "admin@basetemplate.local" ||
                (user.EmployeeId.HasValue && !retainedEmployeeIds.Contains(user.EmployeeId.Value)))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var staleUser in staleUsers)
        {
            var deleteResult = await userManager.DeleteAsync(staleUser).ConfigureAwait(false);
            if (!deleteResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to remove stale development user '{staleUser.Email}': {FormatErrors(deleteResult)}");
            }
        }
    }

    private static string FormatErrors(IdentityResult result)
        => string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));

    private sealed record DevelopmentEmployeeUser(
        Guid EmployeeId,
        string Email,
        string FirstName,
        string LastName,
        string NationalId,
        string PhoneNumber,
        bool IsBootstrapAdmin);
}
