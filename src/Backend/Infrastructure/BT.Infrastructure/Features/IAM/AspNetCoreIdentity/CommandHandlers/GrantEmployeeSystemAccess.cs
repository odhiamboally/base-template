using BT.Application.Features.IAM.Users.Commands;
using BT.Domain.Features.HR.Employees.Entities;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class GrantEmployeeSystemAccess(UserManager<AppUser> userManager, ILogger<GrantEmployeeSystemAccess> logger)
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

            if (user is null)
            {
                return AppResponse.Failure<bool>("No user account found for this employee.");
            }

            if (user.IsActive)
            {
                return AppResponse.Failure<bool>("This employee already has system access.");
            }

            user.GrantAccess(command.GrantedBy, command.Roles);

            var updateResult = await userManager.UpdateAsync(user).ConfigureAwait(false);
            if (!updateResult.Succeeded)
                return AppResponse.Failure<bool>(updateResult.Errors.First().Description);

            if (command.Roles.Any())
            {
                var roleResult = await userManager.AddToRolesAsync(user, command.Roles).ConfigureAwait(false);
                if (!roleResult.Succeeded)
                    return AppResponse.Failure<bool>(roleResult.Errors.First().Description);
            }

            _ = await userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
            if (logger.IsEnabled(LogLevel.Information))
            {
                ServiceLogDefinitions.LogEmployeeSystemAccessGranted(logger, employeeId, command.GrantedBy);
            }

            return AppResponse.Success("System access granted. Activation email sent.", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogGrantEmployeeSystemAccessError(logger, employeeId, ex);
            throw;
        }
    }
}
