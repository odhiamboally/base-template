
using System.Security.Claims;

namespace BT.SharedKernel.Features.IAM.Users.Dtos;
public record RefreshTokenResponse(
    string Token,
    string RefreshToken,
    string UserId,
    DateTimeOffset TokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    AppUserResponse UserInfo,
    List<Claim> UserClaims
);

