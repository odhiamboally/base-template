using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using MediatR;

namespace BT.Application.Features.Shared.Payments.CommandHandlers;

public sealed record InitiatePaymentCommand(PaymentInitiationRequest Request) : IRequest<AppResponse<PaymentInitiationResponse>>;
