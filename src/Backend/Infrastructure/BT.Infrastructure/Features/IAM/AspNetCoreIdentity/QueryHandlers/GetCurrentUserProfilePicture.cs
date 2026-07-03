using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Application.Features.IAM.Users.Queries;
using BT.Domain.Features.IAM.Users.Entities;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.QueryHandlers;

internal sealed class GetCurrentUserProfilePicture(
    UserManager<AppUser> userManager,
    IProfilePictureStorage storage)
    : IRequestHandler<GetCurrentUserProfilePictureQuery, AppResponse<ProfilePictureFile>>
{
    public async Task<AppResponse<ProfilePictureFile>> Handle(
        GetCurrentUserProfilePictureQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.UserId))
        {
            return AppResponses.Failure<ProfilePictureFile>("Authenticated user was not resolved.");
        }

        var user = await userManager.FindByIdAsync(query.UserId).ConfigureAwait(false);
        if (user is null || user.ProfilePictureUrl is null)
        {
            return AppResponses.Failure<ProfilePictureFile>("Profile picture was not found.");
        }

        var profilePicture = await storage
            .OpenReadAsync(user.ProfilePictureUrl, cancellationToken)
            .ConfigureAwait(false);

        return profilePicture is null
            ? AppResponses.Failure<ProfilePictureFile>("Profile picture was not found.")
            : AppResponses.Success("Profile picture loaded.", profilePicture);
    }
}
