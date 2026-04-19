using BT.Application.Features.Auth.Commands;
using BT.Domain.Events;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BT.Infrastructure.Features.Auth.AspNetCoreIdentity.Handlers;


internal sealed class Logout(
    SignInManager<Domain.Entities.AppUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    IPublisher publisher,
    ILogger<Logout> logger) : IRequestHandler<LogoutCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionId = httpContextAccessor.HttpContext?.Request.Headers["X-Session-Id"].FirstOrDefault();

            logger.LogInformation("API sign out called. UserId: {UserId}, SessionId: {SessionId}", userId, sessionId);

            await signInManager.SignOutAsync().ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await publisher.Publish(new UserLogoutEvent(userId, sessionId), cancellationToken).ConfigureAwait(false);
            }

            return AppResponse.Success("Signed out successfully", true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during API sign out");
            throw;
        }
    }
}
