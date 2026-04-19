using BT.Application.Extensions;
using BT.Application.Features.Auth.Commands;
using BT.Domain.Entities;
using BT.Domain.Enums;
using BT.SharedKernel.Dtos.Client;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Infrastructure.Features.Auth.AspNetCoreIdentity.CommandHandlers;


internal sealed class LinkCustomerToExistingUser(UserManager<AppUser> userManager)
    : IRequestHandler<LinkCustomerToExistingUserCommand, AppResponse<CustomerResponse>>
{
    public async Task<AppResponse<CustomerResponse>> Handle(LinkCustomerToExistingUserCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.AppUserId).ConfigureAwait(false);

        if (user is null)
            return AppResponse.Failure<CustomerResponse>("User not found.");

        if (user.CustomerId.HasValue)
            return AppResponse.Failure<CustomerResponse>("User is already linked to a customer record.");

        // Domain behaviour — raises CustomerLinkedToUserEvent
        user.LinkToCustomer(command.CustomerId);

        await userManager.UpdateAsync(user).ConfigureAwait(false);
        await userManager.AddToRoleAsync(user, Roles.Customer.ToDisplayString()).ConfigureAwait(false);

        return AppResponse.Success("Customer account linked to user.", );
    }
}