using BT.Application.Features.Banking.Customers.Mappings;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Features.IAM.Users.Mappings;
using BT.Application.Features.Shared.EmailTemplates.Mappings;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.HR.Employees.Entities;
using BT.Application.Features.HR.Employees.Contracts.Interfaces;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Employees.Dtos;
using BT.SharedKernel.Features.Shared.Phone;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.HR.Employees.CommandHandlers;

public sealed record CreateEmployeeCommand(CreateEmployeeRequest Request, string User) : IRequest<AppResponse<EmployeeResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [];
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("employees")];
}

internal sealed class CreateEmployeeCommandHandler(
    IHrUnitOfWork unitOfWork,
    IEmployeeNumberGenerator numberGenerator,
    ILogger<CreateEmployeeCommandHandler> logger) 
    : IRequestHandler<CreateEmployeeCommand, AppResponse<EmployeeResponse>>
{
    public async Task<AppResponse<EmployeeResponse>> Handle(CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        try
        {
            var duplicateEmail = await unitOfWork.EmployeeRepository
                .AnyAsync(e => e.Email == request.Email, cancellationToken)
                .ConfigureAwait(false);

            if (duplicateEmail)
            {
                LogDefinitions.LogEmployeeDuplicateRegistration(logger, request.Email);
                return AppResponse.Failure<EmployeeResponse>("An employee with this email already exists.");
            }

            var result = await unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var employeeNumber = await numberGenerator.GenerateAsync(request.DepartmentId, cancellationToken).ConfigureAwait(false);
                var phone = PhoneNumberFormatter.Normalize(
                    request.CountryCode,
                    request.PhoneNationalNumber,
                    request.PhoneNumber);

                var entityToCreate = Employee.Create(
                        employeeNumber,
                        request.Email,
                        request.FirstName,
                        request.LastName,
                        request.IdNumber,
                        phone.CountryCode,
                        phone.NationalNumber,
                        phone.E164,
                        request.DepartmentId,
                        request.ManagerId,
                        command.User);

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
