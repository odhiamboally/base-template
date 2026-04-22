using BT.Application.Features.Auth.Commands;
using BT.Domain.Entities;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Features.Auth.AspNetCoreIdentity.CommandHandlers;


internal sealed class GrantEmployeeSystemAccess(UserManager<AppUser> userManager, ILogger<GrantEmployeeSystemAccess> logger)
    : IRequestHandler<GrantEmployeeSystemAccessCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(GrantEmployeeSystemAccessCommand command, CancellationToken ct)
    {
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

            // Domain behaviour — raises event, sets timestamps
            user.GrantAccess(command.GrantedBy, command.Roles);

            var updateResult = await userManager.UpdateAsync(user).ConfigureAwait(false);
            if (!updateResult.Succeeded)
                return AppResponse.Failure<bool>(updateResult.Errors.First().Description);

            if (command.Roles.Count > 0)
            {
                var roleResult = await userManager.AddToRolesAsync(user, command.Roles).ConfigureAwait(false);
                if (!roleResult.Succeeded)
                    return AppResponse.Failure<bool>(roleResult.Errors.First().Description);
            }

            // Generate activation token → publish event → email sent by handler
            var token = await userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
            // TODO: Raise AppUserActivationEmailRequestedEvent(user.Email, token)

            if (logger.IsEnabled(LogLevel.Information))
            {
                ServiceLogDefinitions.LogEmployeeSystemAccessGranted(logger, command.EmployeeId.ToString(), command.GrantedBy);
            }

            return AppResponse.Success("System access granted. Activation email sent.", true);
        }
        catch (Exception)
        {
            throw;
        }
        
    }
}