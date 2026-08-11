using System.Text.Json;

namespace BT.SharedKernel.Features.IAM.Users.Dtos;

public sealed record RegisterPasskeyRequest(JsonElement AttestationResponse);
