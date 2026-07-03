using System.Collections.ObjectModel;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.HR.Employees.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Employees.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Employees.QueryHandlers;


public sealed record SearchEmployeesQuery(EmployeeSearchRequest SearchRequest, string UserId)
    : IRequest<AppResponse<PagedResponse<EmployeeResponse, Guid>>>, ICachableRequest
{
    public string CacheGroup => "employees";

    public string Discriminator => CacheKeys.Discriminator(SearchRequest);

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

