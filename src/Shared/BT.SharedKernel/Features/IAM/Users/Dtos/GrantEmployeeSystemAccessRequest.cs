namespace BT.SharedKernel.Features.IAM.Users.Dtos;

public sealed record GrantEmployeeSystemAccessRequest(IReadOnlyList<string> Roles);
