using System.Text.Json;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.SharedKernel.Dtos.Common;
using MediatR;

namespace BT.Application.Features.IAM.Users.Commands;

public record RequestPasskeyLoginOptionsCommand(string? Username) : IRequest<AppResponse<PasskeyLoginOptionsResponse>>;
