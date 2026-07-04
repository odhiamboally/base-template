using BT.Application.Features.HR.Departments.CommandHandlers;
using BT.SharedKernel.Validation.Features.HR.Departments.Validators;
using FluentValidation;

namespace BT.Application.Features.Shared.Validation;

public sealed class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreateDepartmentRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
