using BT.SharedKernel.Dtos.Common;
using MediatR;

namespace BT.Application.Features.IAM.Users.Commands;

public sealed record DisableTotpCommand(string UserId, string DisabledBy)
    : IRequest<AppResponse<bool>>;
