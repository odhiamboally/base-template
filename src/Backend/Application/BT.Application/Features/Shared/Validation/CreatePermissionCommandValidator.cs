using BT.Application.Features.IAM.Permissions.Commands;
using BT.SharedKernel.Validation.Features.IAM.Permissions.Validators;
using FluentValidation;

namespace BT.Application.Features.Shared.Validation;

public sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreatePermissionRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
