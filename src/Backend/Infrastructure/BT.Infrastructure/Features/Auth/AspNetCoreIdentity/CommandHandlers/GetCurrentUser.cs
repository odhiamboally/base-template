using BT.Application.Extensions;
using BT.Application.Features.Auth.Commands;
using BT.Application.Mappings;
using BT.Domain.Entities;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BT.Infrastructure.Features.Auth.AspNetCoreIdentity.Handlers;

internal sealed class GetCurrentUser(
    IHttpContextAccessor httpContextAccessor,
    UserManager<AppUser> userManager,
    ILogger<GetCurrentUser> logger) : IRequestHandler<GetCurrentUserCommand, AppResponse<CurrentUserResponse>>
{
    public async Task<AppResponse<CurrentUserResponse>> Handle(GetCurrentUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userName = httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName))
                return AppResponse.Failure<CurrentUserResponse>("User not authenticated");

            var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return AppResponse.Failure<CurrentUserResponse>("User not found.");

            var appUser = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
            if (appUser == null)
                return AppResponse.Failure<CurrentUserResponse>("User not found.");

            var roles = await userManager.GetRolesAsync(appUser).ConfigureAwait(false);
            var rolesList = roles.ToList();

            var twoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(appUser).ConfigureAwait(false);
            var emailConfirmed = await userManager.IsEmailConfirmedAsync(appUser).ConfigureAwait(false);
            await userManager.IsPhoneNumberConfirmedAsync(appUser).ConfigureAwait(false);

            var lastLoginClaim = httpContextAccessor.HttpContext?.User?.FindFirst("LastLogin");

            DateTimeOffset? lastLoginAt = null;
            if (lastLoginClaim?.Value != null && DateTimeOffset.TryParse(lastLoginClaim.Value, out var lastLogin))
            {
                lastLoginAt = lastLogin;
            }

            bool? isAuthenticated = httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

            return AppResponse.Success("CurrentUser", new CurrentUserResponse(
                userId,
                appUser.EmployeeId ?? Guid.Empty,
                appUser.CustomerId ?? Guid.Empty,
                appUser.NationalId ?? string.Empty,
                appUser.UserName ?? string.Empty,
                appUser.Email ?? string.Empty,
                appUser.FirstName ?? string.Empty,
                appUser.LastName ?? string.Empty,
                appUser.PhoneNumber ?? string.Empty,
                emailConfirmed,
                twoFactorEnabled,
                appUser.Gender.MapToString(),
                isAuthenticated,
                lastLoginAt,
                rolesList));
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogGetCurrentUserError(logger, ex);
            throw;
        }
    }
}
