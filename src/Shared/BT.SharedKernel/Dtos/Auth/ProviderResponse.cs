namespace BT.SharedKernel.Dtos.Auth;

public record ProviderResponse(
    List<TwoFactorProvider>? Providers,
    string PreferredProvider);
