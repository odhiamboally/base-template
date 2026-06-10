using BT.Application.Behaviours;
using BT.Application.Features.IAM.Users.Commands;
using BT.Application.Features.IAM.Users.Validators;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace BT.Tests.Unit.Application.Validation;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_Should_Return_AppResponse_ValidationFailure_When_Command_Is_Invalid()
    {
        var validator = new LoginCommandValidator();
        var behavior = new ValidationBehavior<LoginCommand, AppResponse<LoginResponse>>([validator]);
        var command = new LoginCommand(new TestLoginRequest("", "", false, null, ""));
        var nextCalled = false;

        var response = await behavior.Handle(
            command,
            _ =>
            {
                nextCalled = true;
                return Task.FromResult(AppResponse.Success("ok", CreateLoginResponse("token")));
            },
            CancellationToken.None);

        Assert.False(nextCalled);
        Assert.False(response.Successful);
        Assert.Equal("VALIDATION_ERROR", response.ErrorCode);
        Assert.NotEmpty(response.ValidationErrors);
    }

    [Fact]
    public async Task Handle_Should_Call_Next_When_Command_Is_Valid()
    {
        var validator = new LoginCommandValidator();
        var behavior = new ValidationBehavior<LoginCommand, AppResponse<LoginResponse>>([validator]);
        var command = new LoginCommand(new TestLoginRequest("admin@basetemplate.local", "Password1!", false, null, "device-1"));

        var response = await behavior.Handle(
            command,
            _ => Task.FromResult(AppResponse.Success("ok", CreateLoginResponse("token"))),
            CancellationToken.None);

        Assert.True(response.Successful);
        Assert.Equal("token", response.Data?.Token);
    }

    private static LoginResponse CreateLoginResponse(string token)
        => new(
            "user-1",
            "Template",
            "Admin",
            "admin@basetemplate.local",
            Requires2FA: false,
            RequiresEmailConfirmation: false,
            IsAuthenticated: true,
            token,
            "refresh",
            "session",
            DateTimeOffset.UtcNow,
            UserInfo: null,
            UserClaims: []);

    private sealed record TestLoginRequest(
        string UserName,
        string Password,
        bool RememberMe,
        string? ReturnUrl,
        string DeviceFingerprint)
        : LoginRequest(UserName, Password, RememberMe, ReturnUrl, DeviceFingerprint);
}
