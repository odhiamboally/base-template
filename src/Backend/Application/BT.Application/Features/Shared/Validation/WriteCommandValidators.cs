using BT.Application.Features.Banking.Customers.CommandHandlers;
using BT.Application.Features.HR.Departments.CommandHandlers;
using BT.Application.Features.HR.Employees.CommandHandlers;
using BT.Application.Features.IAM.Menus.Commands;
using BT.Application.Features.IAM.Permissions.Commands;
using BT.Application.Features.IAM.ReferenceData.Commands;
using BT.SharedKernel.Validation.Features.Banking.Customers.Validators;
using BT.SharedKernel.Validation.Features.HR.Departments.Validators;
using BT.SharedKernel.Validation.Features.HR.Employees.Validators;
using BT.SharedKernel.Validation.Features.IAM.Menus.Validators;
using BT.SharedKernel.Validation.Features.IAM.Permissions.Validators;
using BT.SharedKernel.Validation.Features.IAM.ReferenceData.Validators;
using FluentValidation;

namespace BT.Application.Features.Shared.Validation;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.CreateCustomerRequest).SetValidator(new CreateCustomerRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Customer ID is required.");
        RuleFor(x => x.UpdateCustomerRequest).SetValidator(new UpdateCustomerRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Customer ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreateDepartmentRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Department ID is required.");
        RuleFor(x => x.Request).SetValidator(new UpdateDepartmentRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Department ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreateEmployeeRequestValidator());
        RuleFor(x => x.User).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Employee ID is required.");
        RuleFor(x => x.Request).SetValidator(new UpdateEmployeeRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Employee ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class CreateMenuCommandValidator : AbstractValidator<CreateMenuCommand>
{
    public CreateMenuCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreateMenuRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class UpdateMenuCommandValidator : AbstractValidator<UpdateMenuCommand>
{
    public UpdateMenuCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Menu ID is required.");
        RuleFor(x => x.Request).SetValidator(new UpdateMenuRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class DeleteMenuCommandValidator : AbstractValidator<DeleteMenuCommand>
{
    public DeleteMenuCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Menu ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreatePermissionRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Permission ID is required.");
        RuleFor(x => x.Request).SetValidator(new UpdatePermissionRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class DeletePermissionCommandValidator : AbstractValidator<DeletePermissionCommand>
{
    public DeletePermissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Permission ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

public sealed class CreateReferenceCatalogItemCommandValidator : AbstractValidator<CreateReferenceCatalogItemCommand>
{
    public CreateReferenceCatalogItemCommandValidator()
    {
        RuleFor(x => x.CatalogType).NotEmpty().WithMessage("Catalog type is required.");
        RuleFor(x => x.Request).SetValidator(new ReferenceCatalogItemRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

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

public sealed class DeleteReferenceCatalogItemCommandValidator : AbstractValidator<DeleteReferenceCatalogItemCommand>
{
    public DeleteReferenceCatalogItemCommandValidator()
    {
        RuleFor(x => x.CatalogType).NotEmpty().WithMessage("Catalog type is required.");
        RuleFor(x => x.Id).NotEmpty().WithMessage("Catalog item ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
