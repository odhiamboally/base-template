using Asp.Versioning;
using BT.Api.Common.Controllers;
using BT.Application.Features.IAM.Users.Commands;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Banking.Customers.Dtos;
using BT.SharedKernel.Features.HR.Employees.Dtos;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

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

    [HttpPost("users")]
    [Authorize]
    public async Task<IActionResult> CreateAppUser(CreateAppUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await sender
            .Send(new CreateAppUserCommand(request, GetUserId()))
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

    [HttpPost("employees/{employeeId:guid}/grant-access")]
    [Authorize]
    public async Task<IActionResult> GrantEmployeeSystemAccess(Guid employeeId, GrantEmployeeSystemAccessRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await sender
            .Send(new GrantEmployeeSystemAccessCommand(employeeId, request.Roles, GetUserId()))
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpPost("users/link-customer")]
    [Authorize]
    public async Task<IActionResult> LinkCustomerToExistingUser(LinkCustomerToExistingUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await sender
            .Send(new LinkCustomerToExistingUserCommand(request.AppUserId, request.CustomerId, GetUserId()))
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpPost("users/link-employee")]
    [Authorize]
    public async Task<IActionResult> LinkEmployeeToExistingUser(LinkEmployeeToExistingUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await sender
            .Send(new LinkEmployeeToExistingUserCommand(request.NationalId, request.EmployeeDetails, GetUserId()))
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    private string GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Authenticated user id was not found.");
        }

        return userId;
    }
}

public sealed record LoginApiRequest(
    string UserName,
    string Password,
    bool RememberMe,
    string? ReturnUrl,
    string DeviceFingerprint)
    : LoginRequest(UserName, Password, RememberMe, ReturnUrl, DeviceFingerprint);

public sealed record ResetPasswordApiRequest(
    string Email,
    string? NewPassword,
    string? Password,
    string? ConfirmPassword)
    : ResetPasswordRequest(Email, NewPassword, Password, ConfirmPassword);

public sealed record GrantEmployeeSystemAccessRequest(IReadOnlyList<string> Roles);

public sealed record LinkCustomerToExistingUserRequest(string AppUserId, Guid CustomerId);

public sealed record LinkEmployeeToExistingUserRequest(string NationalId, CreateEmployeeRequest EmployeeDetails);
