namespace BT.SharedKernel.Dtos.Auth;
public abstract record LoginRequest(
    string UserName,
    string Password,
    bool RememberMe,
    string? ReturnUrl,
    string DeviceFingerprint
);

