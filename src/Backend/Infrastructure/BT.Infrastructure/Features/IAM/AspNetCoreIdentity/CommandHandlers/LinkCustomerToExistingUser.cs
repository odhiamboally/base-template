using BT.Application.Extensions;
using BT.Application.Features.IAM.Commands;
using BT.Domain.Banking.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.IAM.Enums;
using BT.SharedKernel.Dtos.Banking.Customers;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

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

        user.LinkToCustomer(command.CustomerId);

        await userManager.UpdateAsync(user).ConfigureAwait(false);
        await userManager.AddToRoleAsync(user, Roles.Customer.ToDisplayString()).ConfigureAwait(false);

        return AppResponse.Success<CustomerResponse>("Customer account linked to user.", default!);
    }
}
