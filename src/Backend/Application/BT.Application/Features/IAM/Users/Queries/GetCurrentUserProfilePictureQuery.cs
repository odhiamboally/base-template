using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.SharedKernel.Dtos.Common;
using MediatR;

namespace BT.Application.Features.IAM.Users.Queries;

public sealed record GetCurrentUserProfilePictureQuery(string UserId)
    : IRequest<AppResponse<ProfilePictureFile>>, ICachableRequest
{
    public string CacheGroup => "profile-pictures";

    public string Discriminator => UserId;

    public string? CacheUserId => UserId;

    public bool IsVersioned => false;

    public bool BypassCache => true;
}
