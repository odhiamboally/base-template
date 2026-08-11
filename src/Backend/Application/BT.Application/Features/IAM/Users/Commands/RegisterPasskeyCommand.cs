using System.Text.Json;
using BT.SharedKernel.Dtos.Common;
using MediatR;

namespace BT.Application.Features.IAM.Users.Commands;

public record RegisterPasskeyCommand(JsonElement AttestationResponse) : IRequest<AppResponse<bool>>;
