using System.Collections.ObjectModel;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Departments.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

internal sealed class SearchDepartmentsQueryHandler(IHrUnitOfWork unitOfWork, ILogger<SearchDepartmentsQueryHandler> logger)
    : IRequestHandler<SearchDepartmentsQuery, AppResponse<PagedResponse<DepartmentResponse, Guid>>>
{
    public async Task<AppResponse<PagedResponse<DepartmentResponse, Guid>>> Handle(SearchDepartmentsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var request = query.SearchRequest;
            var pageSize = Math.Clamp(request.PageSize, 1, 50);
            var departments = unitOfWork.DepartmentRepository.FindAll();

            if (!string.IsNullOrWhiteSpace(request.GlobalSearch))
            {
                var search = request.GlobalSearch.Trim();
                departments = departments.Where(department =>
                    department.Code.Contains(search)
                    || department.Name.Contains(search)
                    || department.Description.Contains(search));
            }

            if (request.IsActive.HasValue)
            {
                departments = departments.Where(department => department.IsActive == request.IsActive.Value);
            }

            var totalCount = await departments.CountAsync(cancellationToken).ConfigureAwait(false);

            if (request.Cursor.HasValue && request.Cursor != Guid.Empty)
            {
                departments = departments.Where(department => department.Id.CompareTo(request.Cursor.Value) > 0);
            }

            var page = await departments
                .OrderBy(static department => department.Id)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var hasNextPage = page.Count > pageSize;
            if (hasNextPage)
            {
                page.RemoveAt(page.Count - 1);
            }

            var items = page
                .Select(static department => department.ToDepartmentResponse())
                .OrderBy(static department => department.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var nextCursor = hasNextPage ? page[^1].Id : (Guid?)null;
            var result = new PagedResponse<DepartmentResponse, Guid>(
                new Collection<DepartmentResponse>(items),
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
