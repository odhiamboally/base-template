using BT.Application.Features.IAM.Users.Commands;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.SharedKernel.Dtos.Common;
using BT.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BT.UI.Blazor.Features.IAM.Users.Contracts.Implementations;

internal sealed class AuthService(ISender sender, ITokenStorage storage) : IAuthService
{
    public async Task<AppResponse<CurrentUserResponse>> GetCurrentUserAsync()
    {
        var (token, _) = await storage.GetAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(token))
        {
            return AppResponse.Failure<CurrentUserResponse>("Not authenticated");
        }

        var response = await sender.Send(new GetCurrentUserCommand()).ConfigureAwait(false);
        return response;

    }

    public async Task<AppResponse<LoginResponse>> LoginAsync(LoginRequest loginRequest)
    {
        var response = await sender.Send(new LoginCommand(loginRequest)).ConfigureAwait(false);

        if (response.Successful)
        {
            await storage.SaveAsync(response.Data?.Token, response.Data?.RefreshToken).ConfigureAwait(false);
        }

        return response;
    }

    public async Task<AppResponse<bool>> LogoutAsync()
    {
        var response = await sender.Send(new LogoutCommand()).ConfigureAwait(false);
        await storage.ClearAsync().ConfigureAwait(false);
        return response;
    }
}
