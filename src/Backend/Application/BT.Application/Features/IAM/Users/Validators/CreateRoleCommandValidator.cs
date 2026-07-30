using BT.Application.Features.IAM.Users.Commands;
using BT.SharedKernel.Validation.Features.IAM.Users.Validators;
using FluentValidation;

namespace BT.Application.Features.IAM.Users.Validators;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(command => command.CreatedBy).NotEmpty().MaximumLength(450);
        RuleFor(command => command.Request)
            .NotNull()
            .SetValidator(new CreateRoleRequestValidator());
    }
}
