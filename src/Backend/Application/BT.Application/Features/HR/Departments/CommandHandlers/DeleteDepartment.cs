using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Departments.CommandHandlers;

public sealed record DeleteDepartmentCommand(Guid Id, string UserId)
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("departments", Id.ToString())];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("departments")];
}

internal sealed class DeleteDepartmentCommandHandler(IHrUnitOfWork unitOfWork, ILogger<DeleteDepartmentCommandHandler> logger)
    : IRequestHandler<DeleteDepartmentCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var department = await unitOfWork.DepartmentRepository.FindByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
            if (department is null)
            {
                return AppResponse.Failure<bool>($"Department {command.Id} not found.");
            }

            var hasEmployees = await unitOfWork.EmployeeRepository
                .FindByCondition(employee => employee.DepartmentId == command.Id)
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);

            if (hasEmployees)
            {
                return AppResponse.Failure<bool>("This department has assigned employees. Reassign them before deleting the department.");
            }

            department.MarkAsDeleted(command.UserId);
            await unitOfWork.DepartmentRepository.UpdateAsync(department).ConfigureAwait(false);
            var saved = await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false) > 0;

            return saved
                ? AppResponse.Success("Department deleted.", true)
                : AppResponse.Failure<bool>("Department delete failed.");
        }
        catch (Exception ex)
        {
            LogDefinitions.LogStaffMembersFetchFailed(logger, ex);
            throw;
        }
    }
}
