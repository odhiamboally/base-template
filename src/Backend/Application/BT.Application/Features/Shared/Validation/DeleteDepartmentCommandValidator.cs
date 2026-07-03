using BT.Application.Features.HR.Departments.CommandHandlers;
using FluentValidation;

namespace BT.Application.Features.Shared.Validation;

public sealed class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Department ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
