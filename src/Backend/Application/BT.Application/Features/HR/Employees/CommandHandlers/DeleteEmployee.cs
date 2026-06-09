using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.HR.Contracts;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Employees.CommandHandlers;

public sealed record DeleteEmployeeCommand(Guid Id, string UserId)
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("employees", Id.ToString())];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("employees")];
}

internal sealed class DeleteEmployeeCommandHandler(IHrUnitOfWork unitOfWork, ILogger<DeleteEmployeeCommandHandler> logger)
    : IRequestHandler<DeleteEmployeeCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(DeleteEmployeeCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await unitOfWork.EmployeeRepository.FindByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
            if (employee is null)
            {
                return AppResponse.Failure<bool>($"Employee {command.Id} not found.");
            }

            await unitOfWork.EmployeeRepository.SoftDeleteAsync(command.Id, cancellationToken).ConfigureAwait(false);
            var saved = await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false) > 0;

            return saved
                ? AppResponse.Success("Employee deleted.", true)
                : AppResponse.Failure<bool>("Failed to delete employee.");
        }
        catch (Exception ex)
        {
            LogDefinitions.LogStaffMembersFetchFailed(logger, ex);
            throw;
        }
    }
}
