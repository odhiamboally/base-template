using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.SharedKernel.Validation.Validators.Common;
using FluentValidation;

namespace BT.SharedKernel.Validation.Features.IAM.Users.Validators;

public sealed class RefreshTokenRequestValidator : Validator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(request => request.AccessToken).NotEmpty();
        RuleFor(request => request.RefreshToken).NotEmpty();
    }
}
