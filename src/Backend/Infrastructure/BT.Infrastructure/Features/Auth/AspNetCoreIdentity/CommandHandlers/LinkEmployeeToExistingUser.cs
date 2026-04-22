using BT.Application.Extensions;
using BT.Application.Mappings;
using BT.Application.Features.Auth.Commands;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Entities;
using BT.Domain.Enums;
using BT.SharedKernel.Dtos.Client;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Features.Auth.AspNetCoreIdentity.CommandHandlers;

internal sealed class LinkEmployeeToExistingUser(IHrUnitOfWork hrUnitOfWork, UserManager<AppUser> userManager) 
    : IRequestHandler<LinkEmployeeToExistingUserCommand, AppResponse<EmployeeResponse>>
{
    public async Task<AppResponse<EmployeeResponse>> Handle(LinkEmployeeToExistingUserCommand command, CancellationToken ct)
    {
        // Find existing AppUser by NationalId
        var existingUser = await userManager.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.NationalId == command.NationalId, ct)
            .ConfigureAwait(false);

        if (existingUser is null)
        {
            return AppResponse.Failure<EmployeeResponse>("No existing user found with this National ID. Use CreateEmployee instead.");
        }

        if (existingUser.EmployeeId.HasValue)
        {
            return AppResponse.Failure<EmployeeResponse>("This user already has an employee record linked.");
        }

        // Create Employee record
        var employee = Employee.Create(
            command.EmployeeDetails.EmployeeNumber,
            command.EmployeeDetails.Email,
            existingUser.FirstName,  // Use AppUser's verified name
            existingUser.LastName,
            command.NationalId,
            command.EmployeeDetails.PhoneNumber,
            command.EmployeeDetails.DepartmentId,
            command.EmployeeDetails.ManagerId ?? Guid.Empty,
            command.CreatedBy);

        await hrUnitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await hrUnitOfWork.EmployeeRepository.CreateAsync(employee, ct).ConfigureAwait(false);

            // Link AppUser to Employee (behaviour method raises domain event)
            existingUser.LinkToEmployee(employee.Id);

            await userManager.UpdateAsync(existingUser).ConfigureAwait(false);
            await userManager.AddToRoleAsync(existingUser, Roles.Employee.ToDisplayString()).ConfigureAwait(false);

            await hrUnitOfWork.CompleteAsync(ct).ConfigureAwait(false);
            return true;

        }, ct).ConfigureAwait(false);

        return AppResponse.Success("Employee record created and linked to existing user.",
            employee.ToEmployeeResponse());
    }
}