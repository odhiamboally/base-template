using FluentValidation;
using BT.Application.Features.ControlPlane.Tenants.Commands;

namespace BT.Application.Features.ControlPlane.Tenants.Validators;

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(v => v.Request.DisplayName)
            .NotEmpty().WithMessage("Display Name is required.")
            .MaximumLength(256).WithMessage("Display Name must not exceed 256 characters.");

        RuleFor(v => v.Request.HostName)
            .NotEmpty().WithMessage("Host Name is required.")
            .MaximumLength(256).WithMessage("Host Name must not exceed 256 characters.");

        RuleFor(v => v.Request.ContactEmail)
            .MaximumLength(256).WithMessage("Contact Email must not exceed 256 characters.")
            .EmailAddress().WithMessage("Contact Email must be a valid email address.")
            .When(v => !string.IsNullOrEmpty(v.Request.ContactEmail));

        RuleFor(v => v.Request.MaxUsers)
            .GreaterThanOrEqualTo(0).WithMessage("Max Users must be a non-negative number.");
    }
}
