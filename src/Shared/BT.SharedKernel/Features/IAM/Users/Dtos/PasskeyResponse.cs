using System;

namespace BT.SharedKernel.Features.IAM.Users.Dtos;

public record PasskeyResponse(
    Guid Id,
    string DisplayName,
    DateTimeOffset CreatedAt);
