using BT.Application.Contracts.Dtos;
using BT.Application.Contracts.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace BT.Infrastructure.Contracts.Implementation.Services;


internal sealed class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
{
    public UserIdentityContext GetCurrentContext()
    {
        var user = httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("No active HTTP context.");

        return !user.Identity?.IsAuthenticated ?? true
            ? throw new UnauthorizedAccessException("User is not authenticated.")
            : UserIdentityContext.FromClaims(user);
    }

    public Task SwitchContextAsync(string context, CancellationToken ct = default)
    {
        // re-issue token with updated active_context claim
        throw new NotImplementedException();
    }
}