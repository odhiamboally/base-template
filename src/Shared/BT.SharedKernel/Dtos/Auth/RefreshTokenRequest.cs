namespace BT.SharedKernel.Dtos.Auth;
public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
