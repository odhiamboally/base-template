using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace BT.Application.Features.IAM.Users.Commands;

public sealed record ExchangeSsoCodeCommand(string Code) : IRequest<AppResponse<LoginResponse>>;
