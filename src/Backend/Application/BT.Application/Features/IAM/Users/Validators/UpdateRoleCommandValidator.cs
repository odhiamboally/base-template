using BT.Application.Features.IAM.Users.Commands;
using BT.SharedKernel.Validation.Features.IAM.Users.Validators;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(command => command.RoleId).NotEmpty().MaximumLength(450);
        RuleFor(command => command.UpdatedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request)
            .NotNull()
            .SetValidator(new UpdateRoleRequestValidator());
    }
}
