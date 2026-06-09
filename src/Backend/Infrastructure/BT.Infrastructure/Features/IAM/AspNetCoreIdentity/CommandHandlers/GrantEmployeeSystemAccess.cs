using BT.Application.Features.IAM.Users.Commands;
using BT.Application.Features.Shared.Notifications.Contracts.Interfaces;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Infrastructure.Configuration;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Notifications.Dtos;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class GrantEmployeeSystemAccess(
    UserManager<AppUser> userManager,
    IHrUnitOfWork hrUnitOfWork,
    IOptions<IamProvisioningSettings> provisioningOptions,
    IEmailService emailService,
    ILogger<GrantEmployeeSystemAccess> logger)
    : IRequestHandler<GrantEmployeeSystemAccessCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(GrantEmployeeSystemAccessCommand command, CancellationToken ct)
    {
        var employeeId = command.EmployeeId.ToString();

        try
        {
            var user = await userManager.Users
                .SingleOrDefaultAsync(u => u.EmployeeId == command.EmployeeId, ct)
                .ConfigureAwait(false);

            var createdNewUser = false;
            if (user is null)
            {
                var employee = await hrUnitOfWork.EmployeeRepository
                    .FindByCondition(existing => existing.Id == command.EmployeeId)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(ct)
                    .ConfigureAwait(false);

                if (employee is null)
                {
                    return AppResponse.Failure<bool>("Employee record was not found.");
                }

                user = AppUser.CreateForEmployee(
                    Guid.Empty,
                    employee.Id,
                    employee.Email,
                    employee.FirstName,
                    employee.LastName,
                    employee.Email,
                    employee.PhoneNumber,
                    employee.IdNumber,
                    command.GrantedBy);

                user.EmailConfirmed = true;

                var temporaryPassword = provisioningOptions.Value.TemporaryPassword;
                if (string.IsNullOrWhiteSpace(temporaryPassword))
                {
                    return AppResponse.Failure<bool>(
                        "IAM provisioning is not configured. Set IamProvisioning:TemporaryPassword via user secrets, environment variables, or Key Vault.");
                }

                var createResult = await userManager.CreateAsync(user, temporaryPassword).ConfigureAwait(false);
                if (!createResult.Succeeded)
                {
                    return AppResponse.Failure<bool>(createResult.Errors.First().Description);
                }

                createdNewUser = true;
            }

            if (user.IsActive)
            {
                return AppResponse.Failure<bool>(
                    "This employee is already linked to an active IAM account. Use Manage Roles or Revoke Access instead.");
            }

            user.GrantAccess(command.GrantedBy, command.Roles);

            var updateResult = await userManager.UpdateAsync(user).ConfigureAwait(false);
            if (!updateResult.Succeeded)
                return AppResponse.Failure<bool>(updateResult.Errors.First().Description);

            if (command.Roles.Any())
            {
                var existingRoles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
                var rolesToAdd = command.Roles
                    .Except(existingRoles, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (rolesToAdd.Length > 0)
                {
                    var roleResult = await userManager.AddToRolesAsync(user, rolesToAdd).ConfigureAwait(false);
                    if (!roleResult.Succeeded)
                        return AppResponse.Failure<bool>(roleResult.Errors.First().Description);
                }
            }

            var emailResponse = await SendActivationEmailAsync(user, provisioningOptions.Value, createdNewUser, ct)
                .ConfigureAwait(false);
            if (logger.IsEnabled(LogLevel.Information))
            {
                ServiceLogDefinitions.LogEmployeeSystemAccessGranted(logger, employeeId, command.GrantedBy);
            }

            var message = emailResponse.Successful
                ? createdNewUser
                    ? "IAM account created, linked to the employee, activated, roles assigned, and activation email sent."
                    : "Existing IAM account reactivated, roles assigned, and activation email sent."
                : createdNewUser
                    ? $"IAM account created, linked to the employee, activated, and roles assigned; however, the activation email could not be sent: {emailResponse.Message}"
                    : $"Existing IAM account reactivated and roles assigned; however, the activation email could not be sent: {emailResponse.Message}";

            return AppResponse.Success(message, true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogGrantEmployeeSystemAccessError(logger, employeeId, ex);
            throw;
        }
    }

    private Task<AppResponse<SendEmailResponse>> SendActivationEmailAsync(
        AppUser user,
        IamProvisioningSettings settings,
        bool createdNewUser,
        CancellationToken ct)
    {
        var displayName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = user.UserName ?? user.Email ?? "there";
        }

        var subject = createdNewUser
            ? "Your Base Template account is ready"
            : "Your Base Template access has been reactivated";

        var body = $"""
            <p>Hello {System.Net.WebUtility.HtmlEncode(displayName)},</p>
            <p>Your system access has been enabled.</p>
            <p><strong>Username:</strong> {System.Net.WebUtility.HtmlEncode(user.UserName ?? user.Email ?? string.Empty)}</p>
            <p><strong>Temporary password:</strong> {System.Net.WebUtility.HtmlEncode(settings.TemporaryPassword)}</p>
            <p>Please sign in at <a href="{System.Net.WebUtility.HtmlEncode(settings.SignInUrl)}">{System.Net.WebUtility.HtmlEncode(settings.SignInUrl)}</a> and change your password immediately.</p>
            <p>If you did not expect this access, contact your system administrator.</p>
            """;

        return emailService.SendEmailAsync(new SendEmailRequest
        {
            To = user.Email,
            Subject = subject,
            Body = body
        }, ct);
    }
}
