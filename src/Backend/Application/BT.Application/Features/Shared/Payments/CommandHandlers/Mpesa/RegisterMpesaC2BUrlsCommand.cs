using BT.SharedKernel.Dtos.Common;
using MediatR;

namespace BT.Application.Features.Shared.Payments.CommandHandlers.Mpesa;

public sealed record RegisterMpesaC2BUrlsCommand() : IRequest<AppResponse<string>>;
