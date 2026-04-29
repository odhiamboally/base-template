
using System.Security.Claims;

namespace BT.SharedKernel.Dtos.Auth;
public record RefreshTokenResponse(
    string Token,
    string RefreshToken,
    string UserId,
    DateTimeOffset TokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    AppUserResponse UserInfo,
    List<Claim> UserClaims
);

