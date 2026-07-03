using BT.Application.Features.IAM.Menus.Commands;
using FluentValidation;

namespace BT.Application.Features.Shared.Validation;

public sealed class DeleteMenuCommandValidator : AbstractValidator<DeleteMenuCommand>
{
    public DeleteMenuCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Menu ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
