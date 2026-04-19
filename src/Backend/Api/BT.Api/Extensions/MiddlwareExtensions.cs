using BT.Api.Middleware;

namespace BT.Api.Extensions;

internal static class MiddlwareExtensions
{
    public static IApplicationBuilder UsePostAuthMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionValidationMiddleware>();
    }
}
