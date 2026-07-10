using BT.SharedKernel.Dtos.Common;
using MediatR;

namespace BT.Application.Features.Shared.Payments.CommandHandlers.Mpesa;

public sealed record SimulateMpesaC2BPaymentCommand(decimal Amount, string PhoneNumber, string BillRefNumber) : IRequest<AppResponse<string>>;
