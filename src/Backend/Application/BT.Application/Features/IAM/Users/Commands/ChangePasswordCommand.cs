using BT.Application.Contracts.Interfaces.Common;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace BT.Application.Features.IAM.Users.Commands;

public sealed record ChangePasswordCommand(ChangePasswordRequest Request) : IRequest<AppResponse<bool>>;
