using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.HR.Departments.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Departments.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Departments.CommandHandlers;

public sealed record UpdateDepartmentCommand(Guid Id, UpdateDepartmentRequest Request, string UserId)
    : IRequest<AppResponse<DepartmentResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("departments", Id.ToString())];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("departments"), CacheKeys.GroupVersion("employees")];
}

internal sealed class UpdateDepartmentCommandHandler(IHrUnitOfWork unitOfWork, ILogger<UpdateDepartmentCommandHandler> logger)
    : IRequestHandler<UpdateDepartmentCommand, AppResponse<DepartmentResponse>>
{
    public async Task<AppResponse<DepartmentResponse>> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var request = command.Request;
            var department = await unitOfWork.DepartmentRepository.FindByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
            if (department is null)
            {
                return AppResponse.Failure<DepartmentResponse>($"Department {command.Id} not found.");
            }

            var code = request.Code.Trim().ToUpperInvariant();
            var duplicate = await unitOfWork.DepartmentRepository
                .FindByCondition(existing => existing.Id != command.Id && existing.Code == code)
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);

            if (duplicate)
            {
                return AppResponse.Failure<DepartmentResponse>($"Department code {code} is already in use.");
            }

            department.Update(code, request.Name, request.Description, request.IsActive, command.UserId);
            await unitOfWork.DepartmentRepository.UpdateAsync(department).ConfigureAwait(false);
            var saved = await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false) > 0;

            return saved
                ? AppResponse.Success("Department updated.", department.ToDepartmentResponse())
                : AppResponse.Failure<DepartmentResponse>("Department update failed.");
        }
        catch (Exception ex)
        {
            LogDefinitions.LogStaffMembersFetchFailed(logger, ex);
            throw;
        }
    }
}
