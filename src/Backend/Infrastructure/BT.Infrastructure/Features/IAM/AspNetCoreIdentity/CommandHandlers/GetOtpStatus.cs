using BT.Application.Features.IAM.Commands;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class GetOtpStatus(UserManager<AppUser> userManager, ILogger<GetOtpStatus> logger)
    : IRequestHandler<GetOtpStatusCommand, AppResponse<OtpStatusResponse>>
{
    public async Task<AppResponse<OtpStatusResponse>> Handle(GetOtpStatusCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userManager.FindByIdAsync(command.UserId).ConfigureAwait(false);
            if (user == null)
            {
                return AppResponse.Failure<OtpStatusResponse>("User not found.");
            }

            var isEnabled = await userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false);

            var response = new OtpStatusResponse(
                IsConfigured: isEnabled,
                IsEnabled: isEnabled,
                ProviderName: "Authenticator",
                DisplayName: "Authenticator App"
            );

            return AppResponse.Success("OTP status retrieved", response);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogGetOtpStatusError(logger, command.UserId, ex);
            throw;
        }
    }
}
