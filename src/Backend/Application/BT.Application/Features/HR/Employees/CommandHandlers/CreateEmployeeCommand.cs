using BT.Application.Features.Banking.Customers.Mappings;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Features.IAM.Users.Mappings;
using BT.Application.Features.Shared.EmailTemplates.Mappings;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.HR.Employees.Entities;
using BT.Application.Features.HR.Employees.Contracts.Interfaces;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Employees.Dtos;
using BT.SharedKernel.Features.Shared.Phone;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Employees.CommandHandlers;


public sealed record CreateEmployeeCommand(CreateEmployeeRequest Request, string User) : IRequest<AppResponse<EmployeeResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [];
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("employees")];
}

