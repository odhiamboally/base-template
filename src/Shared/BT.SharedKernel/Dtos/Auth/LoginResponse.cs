
namespace BT.SharedKernel.Dtos.Auth;
public record LoginResponse(
    string UserId, 
    string FirstName, 
    string LastName, 
    string Email, 
    bool Requires2FA,
    bool RequiresEmailConfirmation,
    bool IsAuthenticated,
    string Token,
    string RefreshToken,
    DateTimeOffset TokenExpiresAt,
    AppUserResponse? UserInfo,
    List<ClaimResponse>? UserClaims

    );


