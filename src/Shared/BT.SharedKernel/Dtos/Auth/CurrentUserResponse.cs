namespace BT.SharedKernel.Dtos.Auth;

public record CurrentUserResponse(
    string AppUserId,
    Guid StaffId,
    Guid MemberId,
    string IdNumber,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    string Gender,
    bool? IsAuthenticated,
    DateTimeOffset? LastLoginAt,
    List<string> Roles);