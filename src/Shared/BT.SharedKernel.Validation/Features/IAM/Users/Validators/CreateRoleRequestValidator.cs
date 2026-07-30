using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.SharedKernel.Validation.Validators.Common;
using FluentValidation;

namespace BT.SharedKernel.Validation.Features.IAM.Users.Validators;

public sealed class CreateRoleRequestValidator : Validator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(80);
    }
}
