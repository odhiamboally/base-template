using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Employees.Dtos;
using BT.SharedKernel.Features.Shared.Phone;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Employees.CommandHandlers;


public sealed record UpdateEmployeeCommand(Guid Id, UpdateEmployeeRequest Request, string UserId)
    : IRequest<AppResponse<EmployeeResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("employees", Id.ToString())];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("employees")];
}

