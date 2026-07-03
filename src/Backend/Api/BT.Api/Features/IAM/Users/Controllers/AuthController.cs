using Asp.Versioning;
using BT.Api.Common.Controllers;
using BT.Application.Features.IAM.Users.Commands;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BT.Api.Features.IAM.Users.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/iam/auth")]
[ApiController]
public sealed class AuthController(ISender sender) : BaseController
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Login(LoginApiRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await sender
            .Send(new LoginCommand(request))
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting("RefreshTokenPolicy")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await sender
            .Send(new RefreshTokenCommand(request))
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var response = await sender
            .Send(new LogoutCommand())
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var response = await sender
            .Send(new GetCurrentUserCommand())
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpPost("email-otp/send")]
    [AllowAnonymous]
    [EnableRateLimiting("TwoFactorPolicy")]
    public async Task<IActionResult> SendEmailOtp(SendEmailOtpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await sender
            .Send(new SendEmailOtpCommand(request))
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpPost("email-otp/verify")]
    [AllowAnonymous]
    [EnableRateLimiting("TwoFactorPolicy")]
    public async Task<IActionResult> VerifyEmailOtp(VerifyEmailOtpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await sender
            .Send(new VerifyEmailOtpCommand(request))
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpGet("otp-status/{userId}")]
    [AllowAnonymous]
    [EnableRateLimiting("TwoFactorPolicy")]
    public async Task<IActionResult> GetOtpStatus(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var response = await sender
            .Send(new GetOtpStatusCommand(userId))
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpPost("password/verify")]
    [Authorize]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> VerifyPassword(VerifyPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await sender
            .Send(new VerifyPasswordCommand(request))
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpPost("password/reset")]
    [AllowAnonymous]
    [EnableRateLimiting("PasswordResetPolicy")]
    public async Task<IActionResult> ResetPassword(ResetPasswordApiRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await sender
            .Send(new ResetPasswordCommand(request))
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

}
