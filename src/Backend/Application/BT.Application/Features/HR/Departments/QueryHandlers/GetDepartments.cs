using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Departments.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Departments.QueryHandlers;

public sealed record GetDepartmentsQuery(string UserId) : IRequest<AppResponse<IReadOnlyList<DepartmentResponse>>>, ICachableRequest
{
    public string CacheGroup => "departments";

    public string Discriminator => CacheKeys.Discriminator("departments:list");

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

internal sealed class GetDepartmentsQueryHandler(IHrUnitOfWork unitOfWork, ILogger<GetDepartmentsQueryHandler> logger)
    : IRequestHandler<GetDepartmentsQuery, AppResponse<IReadOnlyList<DepartmentResponse>>>
{
    public async Task<AppResponse<IReadOnlyList<DepartmentResponse>>> Handle(GetDepartmentsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var departments = await unitOfWork.DepartmentRepository
                .ListAsync(
                    departments => departments
                        .Where(static department => department.IsActive)
                        .OrderBy(static department => department.Name),
                    cancellationToken)
                .ConfigureAwait(false);

            return AppResponse.Success<IReadOnlyList<DepartmentResponse>>(
                departments.Select(static department => department.ToDepartmentResponse()).ToList());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogStaffMembersFetchFailed(logger, ex);
            throw;
        }
    }
}
