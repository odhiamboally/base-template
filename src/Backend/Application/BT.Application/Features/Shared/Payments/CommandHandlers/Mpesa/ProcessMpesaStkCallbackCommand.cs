using BT.SharedKernel.Dtos.Common;
using MediatR;
using System.Text.Json;

namespace BT.Application.Features.Shared.Payments.CommandHandlers.Mpesa;

public sealed record ProcessMpesaStkCallbackCommand(JsonElement Payload) : IRequest<AppResponse<string>>;
