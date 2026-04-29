

namespace BT.SharedKernel.Dtos.Auth;
public record VerifyOtpResponse(
    string Token, 
    string RefreshToken, 
    string UserId, 
    bool IsAuthenticated, 
    DateTimeOffset ExpiresAt, 
    AppUserResponse UserInfo, 
    List<UserClaimsResponse>? UserClaims
    );
