using BT.Application.Mappings;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Dtos.Employees;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Employees.CommandHandlers;

public sealed record CreateEmployeeCommand(CreateEmployeeRequest Request, string User) : IRequest<AppResponse<EmployeeResponse>>, ICacheInvalidatorRequest
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

        try
        {
            var existing = await unitOfWork.EmployeeRepository
                .FindByCondition(e => e.EmployeeNumber == request.EmployeeNumber)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (existing != null)
            {
                LogDefinitions.LogEmployeeDuplicateRegistration(logger, request.EmployeeNumber);
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
            LogDefinitions.LogEmployeeRegistrationFailed(logger, request.Email, ex);

            throw;
        }
    }
}
