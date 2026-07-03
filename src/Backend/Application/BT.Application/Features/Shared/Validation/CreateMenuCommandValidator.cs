using BT.Application.Features.IAM.Menus.Commands;
using BT.SharedKernel.Validation.Features.IAM.Menus.Validators;
using FluentValidation;

namespace BT.Application.Features.Shared.Validation;

public sealed class CreateMenuCommandValidator : AbstractValidator<CreateMenuCommand>
{
    public CreateMenuCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreateMenuRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
