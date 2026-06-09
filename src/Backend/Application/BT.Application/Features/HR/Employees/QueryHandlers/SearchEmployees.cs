using System.Collections.ObjectModel;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Employees.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

internal sealed class SearchEmployeesQueryHandler(IHrUnitOfWork unitOfWork, ILogger<SearchEmployeesQueryHandler> logger)
    : IRequestHandler<SearchEmployeesQuery, AppResponse<PagedResponse<EmployeeResponse, Guid>>>
{
    public async Task<AppResponse<PagedResponse<EmployeeResponse, Guid>>> Handle(SearchEmployeesQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var request = query.SearchRequest;
            var pageSize = Math.Clamp(request.PageSize, 1, 50);

            var employees = unitOfWork.EmployeeRepository.FindAll();

            if (!string.IsNullOrWhiteSpace(request.GlobalSearch))
            {
                var search = request.GlobalSearch.Trim();
                employees = employees.Where(employee =>
                    employee.Number.Contains(search)
                    || employee.FirstName.Contains(search)
                    || employee.LastName.Contains(search)
                    || employee.Email.Contains(search)
                    || employee.PhoneNumber.Contains(search)
                    || employee.IdNumber.Contains(search));
            }

            if (request.DepartmentId.HasValue)
            {
                employees = employees.Where(employee => employee.DepartmentId == request.DepartmentId.Value);
            }

            if (request.ManagerId.HasValue)
            {
                employees = employees.Where(employee => employee.ManagerId == request.ManagerId.Value);
            }

            var totalCount = await employees.CountAsync(cancellationToken).ConfigureAwait(false);

            if (request.Cursor.HasValue && request.Cursor != Guid.Empty)
            {
                employees = employees.Where(employee => employee.Id.CompareTo(request.Cursor.Value) > 0);
            }

            var page = await employees
                .OrderBy(static employee => employee.Id)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var hasNextPage = page.Count > pageSize;
            if (hasNextPage)
            {
                page.RemoveAt(page.Count - 1);
            }

            var departmentIds = page.Select(static employee => employee.DepartmentId).Distinct().ToArray();
            var managerIds = page
                .Select(static employee => employee.ManagerId)
                .Where(static managerId => managerId.HasValue)
                .Select(static managerId => managerId!.Value)
                .Distinct()
                .ToArray();
            var departments = await unitOfWork.DepartmentRepository
                .FindByCondition(department => departmentIds.Contains(department.Id))
                .ToDictionaryAsync(static department => department.Id, static department => department.Name, cancellationToken)
                .ConfigureAwait(false);
            var managers = managerIds.Length == 0
                ? new Dictionary<Guid, string>()
                : await unitOfWork.EmployeeRepository
                    .FindByCondition(manager => managerIds.Contains(manager.Id))
                    .ToDictionaryAsync(static manager => manager.Id, static manager => $"{manager.FirstName} {manager.LastName}", cancellationToken)
                    .ConfigureAwait(false);

            var items = page
                .Select(employee => employee.ToEmployeeResponse(
                    departments.GetValueOrDefault(employee.DepartmentId, string.Empty),
                    employee.ManagerId.HasValue ? managers.GetValueOrDefault(employee.ManagerId.Value, string.Empty) : string.Empty))
                .OrderBy(static employee => employee.Number, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var nextCursor = hasNextPage ? items[^1].Id : (Guid?)null;
            var result = new PagedResponse<EmployeeResponse, Guid>(
                new Collection<EmployeeResponse>(items),
                totalCount,
                1,
                pageSize,
                request.Cursor is null || request.Cursor == Guid.Empty,
                nextCursor ?? Guid.Empty);

            return AppResponse.Success(result);
        }
        catch (Exception ex)
        {
            LogDefinitions.LogStaffMembersFetchFailed(logger, ex);
            throw;
        }
    }
}
