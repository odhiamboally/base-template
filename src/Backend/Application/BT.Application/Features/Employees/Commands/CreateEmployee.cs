using BT.Application.Mappings;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Entities;
using BT.SharedKernel.Dtos.Client;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Dtos.Employees;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.Employees.Commands;

internal sealed record CreateEmployeeCommand(CreateEmployeeRequest Request) : IRequest<AppResponse<EmployeeResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("employees", "all")];
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [];
}

internal sealed class CreateEmployee(IHrUnitOfWork unitOfWork, ILogger<CreateEmployee> logger) 
    : IRequestHandler<CreateEmployeeCommand, AppResponse<EmployeeResponse>>
{
    public async Task<AppResponse<EmployeeResponse>> Handle(CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        AppUser? createdUser = null;

        try
        {
            var existing = await unitOfWork.EmployeeRepository
                .FindByCondition(e => e.EmployeeNumber == request.EmployeeNumber)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existing != null)
            {
                logger.LogWarning("Duplicate registration attempt for employee: {EmployeeNumber}", request.EmployeeNumber);
                return AppResponse.Failure<EmployeeResponse>("You are already registered. Please log in.");
            }

            var entityToCreate = Employee.Create(
                    request.EmployeeNumber,
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.IdNumber,
                    request.PhoneNumber,
                    request.DepartmentId,
                    request.ManagerId ?? Guid.Empty,
                    "System");

            var result = await unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var createdEmployee = await unitOfWork.EmployeeRepository.CreateAsync(entityToCreate).ConfigureAwait(false);

                return AppResponse.Success(
                    "Account created successfully! Please check your email to confirm your account.",
                    createdEmployee.ToEmployeeResponse());

            }).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Registration failed for employee {Email}. Rolling back changes.", request.Email);

            throw;
        }
    }
}
