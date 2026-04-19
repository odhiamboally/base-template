using BT.Application.Features.Auth.Commands;
using BT.Application.Mappings;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Entities;
using BT.Domain.Enums;
using BT.SharedKernel.Dtos.Auth;
using BT.SharedKernel.Dtos.Common;
using FluentEmail.Core;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Features.Auth.AspNetCoreIdentity.CommandHandlers;


internal sealed class CreateAppUser(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, ILogger<CreateAppUser> logger)
 : IRequestHandler<CreateAppUserCommand, AppResponse<AppUserResponse>>
{
    public async Task<AppResponse<AppUserResponse>> Handle(CreateAppUserCommand command, CancellationToken ct)
    {
        var req = command.Request;
        AppUser? createdUser = null;

        try
        {
            // ── 1. Employee-linked path ────────────────────────────────────────
            Employee? employee = null;

            if (req.EmployeeId.HasValue)
            {
                employee = await unitOfWork.EmployeeRepository
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
                    Guid.Empty, // TODO: resolve from current tenant context 
                    employee.Id,
                    req.Username,
                    employee.FirstName,
                    employee.LastName,
                    employee.Email,
                    employee.PhoneNumber,
                    createdBy: "System") // TODO: resolve from current user claims

                : AppUser.Create(
                    Guid.Empty,
                    employeeId: null,
                    req.Username,
                    req.FirstName,
                    req.LastName,
                    req.Email,
                    req.PhoneNumber ?? string.Empty,
                    createdBy: "System");

            appUser.IdNumber = req.IdNumber ?? string.Empty;
            appUser.MemberId = req.MemberId;
            appUser.Gender = Enum.TryParse<Gender>(req.Gender, true, out var g) ? g : Gender.Other;

            var identityResult = await userManager
                .CreateAsync(appUser, req.Password)
                .ConfigureAwait(false);

            if (!identityResult.Succeeded)
            {
                var error = identityResult.Errors.First().Description;
                logger.LogWarning("AppUser creation failed for {Email}: {Error}", req.Email, error);
                return AppResponse.Failure<AppUserResponse>(error);
            }

            createdUser = appUser;

            await unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
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

                await unitOfWork.AppUserProfileRepository
                    .CreateOrUpdateAsync(appUser.Id, profile, ct)
                    .ConfigureAwait(false);

                await unitOfWork.CompleteAsync(ct).ConfigureAwait(false);

                return true;

            }).ConfigureAwait(false);

            appUser.RaiseAppUserCreatedEvent();

            var confirmationToken = await userManager
                .GenerateEmailConfirmationTokenAsync(appUser)
                .ConfigureAwait(false);

            // TODO: Publish AppUserEmailConfirmationRequestedEvent
            // or call IEmailService.SendEmailConfirmationAsync(appUser.Email, token)

            logger.LogInformation("AppUser {UserId} created successfully for {Email}", appUser.Id, appUser.Email);

            return AppResponse.Success("User created successfully.", appUser.ToAppUserResponse());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AppUser creation failed for {Email}. Attempting Identity rollback.", req.Email);

            if (createdUser is not null)
            {
                try
                {
                    await userManager.DeleteAsync(createdUser).ConfigureAwait(false);
                    logger.LogInformation("Identity rollback succeeded for {Email}", req.Email);
                }
                catch (Exception cleanupEx)
                {
                    logger.LogCritical(cleanupEx,
                        "MANUAL CLEANUP REQUIRED — orphaned Identity user {UserId} for {Email}",
                        createdUser.Id, req.Email);
                }
            }

            throw;
        }
    }
}