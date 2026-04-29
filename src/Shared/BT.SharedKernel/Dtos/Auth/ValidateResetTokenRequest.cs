namespace BT.SharedKernel.Dtos.Auth;
public record ValidateResetTokenRequest(
    string Email,
    string Token);

