using System.Collections.ObjectModel;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Departments.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.HR.Departments.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Departments.QueryHandlers;


public sealed record SearchDepartmentsQuery(DepartmentSearchRequest SearchRequest, string UserId)
    : IRequest<AppResponse<PagedResponse<DepartmentResponse, Guid>>>, ICachableRequest
{
    public string CacheGroup => "departments";

    public string Discriminator => CacheKeys.Discriminator(SearchRequest);

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

