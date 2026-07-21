using BT.Application.Features.Shared.TenantSettings.CommandHandlers;
using FluentValidation;

namespace BT.Application.Features.Shared.TenantSettings.Validation;

public class UpdateTenantSettingCommandValidator : AbstractValidator<UpdateTenantSettingCommand>
{
    public UpdateTenantSettingCommandValidator()
    {
        RuleFor(x => x.Request.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Request.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(100).WithMessage("Key must not exceed 100 characters.");

        RuleFor(x => x.Request.Value)
            .NotEmpty().WithMessage("Value is required.");
    }
}
