using BT.Application.Features.IAM.ReferenceData.Commands;
using BT.SharedKernel.Validation.Features.IAM.ReferenceData.Validators;
using FluentValidation;

namespace BT.Application.Features.Shared.Validation;

public sealed class UpdateReferenceCatalogItemCommandValidator : AbstractValidator<UpdateReferenceCatalogItemCommand>
{
    public UpdateReferenceCatalogItemCommandValidator()
    {
        RuleFor(x => x.CatalogType).NotEmpty().WithMessage("Catalog type is required.");
        RuleFor(x => x.Id).NotEmpty().WithMessage("Catalog item ID is required.");
        RuleFor(x => x.Request).SetValidator(new ReferenceCatalogItemRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
