using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Departments.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Departments.QueryHandlers;

public sealed record GetDepartmentByIdQuery(Guid Id, string UserId) : IRequest<AppResponse<DepartmentResponse>>, ICachableRequest
{
    public string CacheGroup => "departments";

    public string Discriminator => CacheKeys.Entity("departments", Id.ToString());

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

internal sealed class GetDepartmentByIdQueryHandler(IHrUnitOfWork unitOfWork, ILogger<GetDepartmentByIdQueryHandler> logger)
    : IRequestHandler<GetDepartmentByIdQuery, AppResponse<DepartmentResponse>>
{
    public async Task<AppResponse<DepartmentResponse>> Handle(GetDepartmentByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var department = await unitOfWork.DepartmentRepository.FindByIdAsync(query.Id, cancellationToken).ConfigureAwait(false);
            return department is null
                ? AppResponse.Failure<DepartmentResponse>($"Department {query.Id} not found.")
                : AppResponse.Success(department.ToDepartmentResponse());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogStaffMembersFetchFailed(logger, ex);
            throw;
        }
    }
}
