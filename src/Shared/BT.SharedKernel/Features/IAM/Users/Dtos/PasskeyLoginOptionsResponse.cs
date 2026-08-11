using System;
using System.Text.Json;

namespace BT.SharedKernel.Features.IAM.Users.Dtos;

public record PasskeyLoginOptionsResponse(
    JsonElement Options,
    Guid CorrelationId);
