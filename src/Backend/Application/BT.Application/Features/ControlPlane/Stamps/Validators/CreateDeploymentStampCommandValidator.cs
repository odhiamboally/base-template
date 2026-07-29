using FluentValidation;
using BT.Application.Features.ControlPlane.Stamps.Commands;

namespace BT.Application.Features.ControlPlane.Stamps.Validators;

public class CreateDeploymentStampCommandValidator : AbstractValidator<CreateDeploymentStampCommand>
{
    public CreateDeploymentStampCommandValidator()
    {
        RuleFor(v => v.Request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(v => v.Request.TargetResourceGroup)
            .NotEmpty().WithMessage("Target Resource Group is required.")
            .MaximumLength(256).WithMessage("Target Resource Group must not exceed 256 characters.");

        RuleFor(v => v.Request.KeyVaultUri)
            .MaximumLength(1024).WithMessage("Key Vault URI must not exceed 1024 characters.");
    }
}
