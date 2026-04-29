using BT.Application.Features.IAM.Commands;
using BT.Application.Mappings;
using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.HR.Enums;
using BT.Domain.IAM.Enums;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using FluentEmail.Core;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class CreateAppUser(
    UserManager<AppUser> userManager,
    IHrUnitOfWork hrUnitOfWork,
    IIamUnitOfWork iamUnitOfWork,
    ILogger<CreateAppUser> logger) : IRequestHandler<CreateAppUserCommand, AppResponse<AppUserResponse>>
{
    public async Task<AppResponse<AppUserResponse>> Handle(CreateAppUserCommand command, CancellationToken ct)
    {
        var req = command.Request;
        AppUser? createdUser = null;

        try
        {
            Employee? employee = null;

            if (req.EmployeeId.HasValue)
            {
                employee = await hrUnitOfWork.EmployeeRepository
                    .FindByCondition(e => e.Id == req.EmployeeId.Value)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(ct)
                    .ConfigureAwait(false);

                if (employee is null)
                    return AppResponse.Failure<AppUserResponse>("The specified employee does not exist or has been deactivated.");

                var alreadyLinked = await userManager.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.EmployeeId == req.EmployeeId.Value, ct)
                    .ConfigureAwait(false);

                if (alreadyLinked)
                    return AppResponse.Failure<AppUserResponse>("A user account already exists for this employee.");
            }

            var emailOrUsernameExists = await userManager.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserName == req.Username || u.Email == req.Email, ct)
                .ConfigureAwait(false);

            if (emailOrUsernameExists)
                return AppResponse.Failure<AppUserResponse>("An account with this username or email already exists.");

            var appUser = employee is not null
                ? AppUser.Create(
                    Guid.Empty,
                    employee.Id,
                    req.Username,
                    employee.FirstName,
                    employee.LastName,
                    employee.Email,
                    employee.PhoneNumber,
                    createdBy: "System")
                : AppUser.Create(
                    Guid.Empty,
                    employeeId: null,
                    req.Username,
                    req.FirstName,
                    req.LastName,
                    req.Email,
                    req.PhoneNumber ?? string.Empty,
                    createdBy: "System");

            appUser.NationalId = req.IdNumber ?? string.Empty;
            appUser.CustomerId = req.MemberId;
            appUser.Gender = Enum.TryParse<Gender>(req.Gender, true, out var g) ? g : Gender.Other;

            var identityResult = await userManager
                .CreateAsync(appUser, req.Password)
                .ConfigureAwait(false);

            if (!identityResult.Succeeded)
            {
                var error = identityResult.Errors.First().Description;
                ServiceLogDefinitions.LogAppUserCreationWarning(logger, req.Email, error);
                return AppResponse.Failure<AppUserResponse>(error);
            }

            createdUser = appUser;

            await iamUnitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                if (req.Roles?.Count > 0)
                {
                    var roleResult = await userManager
                        .AddToRolesAsync(appUser, req.Roles)
                        .ConfigureAwait(false);

                    if (!roleResult.Succeeded)
                        throw new InvalidOperationException(
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }

                var profile = new AppUserProfile
                {
                    AppUserId = appUser.Id,
                    TelephoneNo = appUser.PhoneNumber,
                    Email = appUser.Email,
                    CreatedBy = "System"
                };

                await iamUnitOfWork.AppUserProfileRepository
                    .CreateOrUpdateAsync(appUser.Id, profile, ct)
                    .ConfigureAwait(false);

                return true;
            }).ConfigureAwait(false);

            appUser.RaiseAppUserCreatedEvent();

            _ = await userManager.GenerateEmailConfirmationTokenAsync(appUser).ConfigureAwait(false);

            ServiceLogDefinitions.LogAppUserCreated(logger, appUser.Id, appUser.Email ?? string.Empty);

            return AppResponse.Success("User created successfully.", appUser.ToAppUserResponse());
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogAppUserCreationFailed(logger, req.Email, ex);

            if (createdUser is not null)
            {
                try
                {
                    await userManager.DeleteAsync(createdUser).ConfigureAwait(false);
                    ServiceLogDefinitions.LogIdentityRollbackSucceeded(logger, req.Email);
                }
                catch (Exception)
                {
                    ServiceLogDefinitions.LogIdentityRollbackCritical(logger, createdUser.Id, req.Email);
                }
            }

            throw;
        }
    }
}
