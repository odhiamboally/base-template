using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Departments.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.HR.Departments.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Departments.CommandHandlers;

public sealed record CreateDepartmentCommand(CreateDepartmentRequest Request, string UserId)
    : IRequest<AppResponse<DepartmentResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("departments")];
}

internal sealed class CreateDepartmentCommandHandler(IHrUnitOfWork unitOfWork, ILogger<CreateDepartmentCommandHandler> logger)
    : IRequestHandler<CreateDepartmentCommand, AppResponse<DepartmentResponse>>
{
    public async Task<AppResponse<DepartmentResponse>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var request = command.Request;
            var code = request.Code.Trim().ToUpperInvariant();
            var duplicate = await unitOfWork.DepartmentRepository
                .FindByCondition(department => department.Code == code)
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);

            if (duplicate)
            {
                return AppResponse.Failure<DepartmentResponse>($"Department code {code} is already in use.");
            }

            var department = Department.Create(code, request.Name, request.Description, command.UserId);
            await unitOfWork.DepartmentRepository.CreateAsync(department, cancellationToken).ConfigureAwait(false);
            var saved = await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false) > 0;

            return saved
                ? AppResponse.Success("Department created.", department.ToDepartmentResponse())
                : AppResponse.Failure<DepartmentResponse>("Department create failed.");
        }
        catch (Exception ex)
        {
            LogDefinitions.LogStaffMembersFetchFailed(logger, ex);
            throw;
        }
    }
}
