namespace BT.SharedKernel.Features.IAM.Permissions.Dtos;

public sealed record RolePermissionsResponse(
    string RoleId,
    string RoleName,
    IReadOnlyList<string> PermissionKeys);
