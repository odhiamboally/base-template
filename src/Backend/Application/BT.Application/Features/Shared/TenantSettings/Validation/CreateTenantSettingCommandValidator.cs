using BT.Application.Features.Shared.TenantSettings.CommandHandlers;
using FluentValidation;

namespace BT.Application.Features.Shared.TenantSettings.Validation;

public class CreateTenantSettingCommandValidator : AbstractValidator<CreateTenantSettingCommand>
{
    public CreateTenantSettingCommandValidator()
    {
        RuleFor(x => x.Request.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(100).WithMessage("Key must not exceed 100 characters.");

        RuleFor(x => x.Request.Value)
            .NotEmpty().WithMessage("Value is required.");
    }
}
