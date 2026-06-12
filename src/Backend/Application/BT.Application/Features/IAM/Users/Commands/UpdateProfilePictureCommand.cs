using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace BT.Application.Features.IAM.Users.Commands;

public sealed record UpdateProfilePictureCommand(
    string UserId,
    Stream Content,
    string FileName,
    string ContentType,
    long Length,
    string UpdatedBy)
    : IRequest<AppResponse<ProfilePictureResponse>>;
