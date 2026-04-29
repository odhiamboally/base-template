using BT.Application.Features.IAM.Commands;
using BT.Domain.IAM.Entities;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class VerifyPassword(
    UserManager<AppUser> userManager,
    ILogger<VerifyPassword> logger) : IRequestHandler<VerifyPasswordCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(VerifyPasswordCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        try
        {
            var user = await userManager.FindByIdAsync(request.UserId).ConfigureAwait(false);
            var userByEmail = await userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);

            if (user == null && userByEmail != null)
            {
                user = userByEmail;
            }

            if (user == null)
            {
                return AppResponse.Failure<bool>("User not found");
            }

            if (await userManager.IsLockedOutAsync(user).ConfigureAwait(false))
            {
                return AppResponse.Failure<bool>("Account is locked");
            }

            var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password).ConfigureAwait(false);
            if (!isPasswordValid)
            {
                await userManager.AccessFailedAsync(user).ConfigureAwait(false);

                return await userManager.IsLockedOutAsync(user).ConfigureAwait(false)
                    ? AppResponse.Failure<bool>("Account locked due to too many failed attempts")
                    : AppResponse.Failure<bool>("Invalid password");
            }

            await userManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);

            return AppResponse.Success("Password verified successfully", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogErrorVerifyingPassword(logger, ex);
            throw;
        }
    }
}
