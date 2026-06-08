using BT.Api.Middleware;
using BT.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BT.Tests.Unit.Api.Middleware;

public sealed class MfaEnrollmentMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Should_Block_Protected_Api_When_Required_Role_Is_Not_Enrolled()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateContext("/api/v1/iam/admin/users", isMfaEnrolled: false);

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Contains("MFA_ENROLLMENT_REQUIRED", await ReadResponseAsync(context));
    }

    [Fact]
    public async Task InvokeAsync_Should_Allow_Totp_Setup_When_Required_Role_Is_Not_Enrolled()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateContext("/api/v1/iam/users/totp/setup", isMfaEnrolled: false);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_Should_Allow_Protected_Api_When_Required_Role_Is_Enrolled()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateContext("/api/v1/iam/admin/users", isMfaEnrolled: true);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    private static MfaEnrollmentMiddleware CreateMiddleware(RequestDelegate next)
    {
        var settings = Options.Create(new MfaSettings
        {
            Enabled = true,
            EnforceEnrollment = true,
            RequiredRoles = ["System Administrator"]
        });

        return new MfaEnrollmentMiddleware(next, settings, NullLogger<MfaEnrollmentMiddleware>.Instance);
    }

    private static DefaultHttpContext CreateContext(string path, bool isMfaEnrolled)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "user-1"),
                new Claim(ClaimTypes.Role, "System Administrator"),
                new Claim("mfa_enrolled", isMfaEnrolled ? "true" : "false")
            ],
            authenticationType: "Test"));

        return context;
    }

    private static async Task<string> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
