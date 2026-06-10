using BT.Application.Features.IAM.Permissions.Commands;
using BT.Application.Features.IAM.Users.Commands;
using BT.Domain.Features.IAM.Users.Enums;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.LoginRequest).NotNull();
        RuleFor(command => command.LoginRequest.UserName).NotEmpty().MaximumLength(256);
        RuleFor(command => command.LoginRequest.Password).NotEmpty().MaximumLength(256);
        RuleFor(command => command.LoginRequest.DeviceFingerprint).NotEmpty().MaximumLength(256);
        RuleFor(command => command.LoginRequest.ReturnUrl).MaximumLength(512);
    }
}

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.AccessToken).NotEmpty();
        RuleFor(command => command.Request.RefreshToken).NotEmpty();
    }
}

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.Request.NewPassword ?? command.Request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.");
        RuleFor(command => command.Request.ConfirmPassword)
            .Equal(command => command.Request.NewPassword ?? command.Request.Password)
            .WithMessage("Password confirmation must match.");
    }
}

public sealed class SendEmailOtpCommandValidator : AbstractValidator<SendEmailOtpCommand>
{
    public SendEmailOtpCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request.Purpose)
            .NotEmpty()
            .Must(BeKnownOtpPurpose)
            .WithMessage("OTP purpose is not supported.");
    }

    private static bool BeKnownOtpPurpose(string purpose)
        => Enum.TryParse<OtpPurpose>(purpose, ignoreCase: true, out _);
}

public sealed class VerifyEmailOtpCommandValidator : AbstractValidator<VerifyEmailOtpCommand>
{
    public VerifyEmailOtpCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request.Code).NotEmpty().Matches("^\\d{6}$");
        RuleFor(command => command.Request.Purpose)
            .NotEmpty()
            .Must(static purpose => Enum.TryParse<OtpPurpose>(purpose, ignoreCase: true, out _))
            .WithMessage("OTP purpose is not supported.");
        RuleFor(command => command.Request.DeviceFingerprint).MaximumLength(256);
    }
}

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request.Code).NotEmpty().Matches("^\\d{6}$");
        RuleFor(command => command.Request.DeviceFingerprint).MaximumLength(256);
    }
}

public sealed class VerifyPasswordCommandValidator : AbstractValidator<VerifyPasswordCommand>
{
    public VerifyPasswordCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.Request.EmployeeNumber).MaximumLength(50);
        RuleFor(command => command.Request.Password).NotEmpty().MaximumLength(256);
    }
}

public sealed class GrantEmployeeSystemAccessCommandValidator : AbstractValidator<GrantEmployeeSystemAccessCommand>
{
    public GrantEmployeeSystemAccessCommandValidator()
    {
        RuleFor(command => command.EmployeeId).NotEmpty();
        RuleFor(command => command.GrantedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Roles).NotNull().Must(static roles => roles.Count > 0)
            .WithMessage("At least one role is required.");
        RuleForEach(command => command.Roles).NotEmpty().MaximumLength(80);
    }
}

public sealed class RevokeEmployeeSystemAccessCommandValidator : AbstractValidator<RevokeEmployeeSystemAccessCommand>
{
    public RevokeEmployeeSystemAccessCommandValidator()
    {
        RuleFor(command => command.EmployeeId).NotEmpty();
        RuleFor(command => command.RevokedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(command => command.CreatedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(80);
    }
}

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(command => command.RoleId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.UpdatedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(80);
    }
}

public sealed class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.UpdatedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Roles).NotNull();
        RuleForEach(command => command.Request.Roles).NotEmpty().MaximumLength(80);
    }
}

public sealed class UpdateUserPermissionsCommandValidator : AbstractValidator<UpdateUserPermissionsCommand>
{
    public UpdateUserPermissionsCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.UpdatedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.PermissionKeys).NotNull();
        RuleForEach(command => command.Request.PermissionKeys).NotEmpty().MaximumLength(160);
    }
}

public sealed class UpdateRolePermissionsCommandValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsCommandValidator()
    {
        RuleFor(command => command.RoleId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.UserId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.PermissionKeys).NotNull();
        RuleForEach(command => command.Request.PermissionKeys).NotEmpty().MaximumLength(160);
    }
}
