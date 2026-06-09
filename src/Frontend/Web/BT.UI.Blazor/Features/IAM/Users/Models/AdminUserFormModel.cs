using BT.SharedKernel.Features.IAM.Users.Dtos;

namespace BT.UI.Blazor.Features.IAM.Users.Models;

internal sealed class AdminUserFormModel
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = "Admin@12345";

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? IdNumber { get; set; }

    public string Gender { get; set; } = "Other";

    public string Role { get; set; } = "User";

    public static AdminUserFormModel From(AdminUserListResponse user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var names = user.FullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return new AdminUserFormModel
        {
            Username = user.UserName,
            Email = user.Email,
            FirstName = names.Length > 0 ? names[0] : user.FullName,
            LastName = names.Length > 1 ? names[1] : string.Empty,
            PhoneNumber = user.PhoneNumber,
            Role = user.Roles.Count > 0 ? user.Roles[0] : "User"
        };
    }

    public CreateAppUserRequest ToRequest() => new(
        Username,
        Email,
        Password,
        FirstName,
        LastName,
        PhoneNumber,
        IdNumber,
        Gender,
        EmployeeId: null,
        CustomerId: null,
        Roles: string.IsNullOrWhiteSpace(Role) ? [] : [Role]);

    public UpdateAdminUserRequest ToUpdateRequest() => new(
        Username,
        Email,
        FirstName,
        LastName,
        PhoneNumber,
        IdNumber,
        Gender,
        string.IsNullOrWhiteSpace(Role) ? [] : [Role]);
}
