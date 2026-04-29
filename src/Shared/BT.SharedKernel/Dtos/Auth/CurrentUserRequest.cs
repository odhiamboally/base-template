namespace BT.SharedKernel.Dtos.Auth;

public record CurrentUserRequest(
    string UserId,
    string UserName,
    string Email,
    IList<string> Roles
);

