using BT.Application.Features.IAM.Permissions.Commands;
using BT.SharedKernel.Validation.Features.IAM.Permissions.Validators;
using FluentValidation;

namespace BT.Application.Features.Shared.Validation;

public sealed class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Permission ID is required.");
        RuleFor(x => x.Request).SetValidator(new UpdatePermissionRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
