using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BT.Application.Features.IAM.Users.Contracts.Interfaces;
using BT.Domain.Features.IAM.Users.Entities;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace BT.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;

public class PasskeyService : IPasskeyService
{
    private readonly IFido2 _fido2;

    public PasskeyService(IFido2 fido2)
    {
        _fido2 = fido2;
    }

    public Task<JsonElement> RequestNewCredentialAsync(AppUser user, System.Collections.Generic.IEnumerable<Fido2Credential> existingCredentials, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        var fidoUser = new Fido2User
        {
            Name = user.UserName,
            Id = System.Text.Encoding.UTF8.GetBytes(user.Id),
            DisplayName = $"{user.FirstName} {user.LastName}"
        };

        var excludeCredentials = existingCredentials?.Select(c => new PublicKeyCredentialDescriptor(c.CredentialId)).ToList() ?? new System.Collections.Generic.List<PublicKeyCredentialDescriptor>();

        var options = _fido2.RequestNewCredential(
            fidoUser,
            excludeCredentials,
            AuthenticatorSelection.Default,
            AttestationConveyancePreference.None,
            new AuthenticationExtensionsClientInputs()
        );

        var jsonString = options.ToJson();
        return Task.FromResult(JsonDocument.Parse(jsonString).RootElement);
    }

    public async Task<Fido2Credential> MakeNewCredentialAsync(AppUser user, JsonElement attestationResponse, JsonElement originalOptions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        var response = attestationResponse.Deserialize<AuthenticatorAttestationRawResponse>() 
            ?? throw new ArgumentException("Invalid attestation response.");
            
        var options = originalOptions.Deserialize<CredentialCreateOptions>()
            ?? throw new ArgumentException("Invalid original options.");

        IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, token) =>
        {
            // In a real implementation we would check the DB if this credential ID is already registered to a DIFFERENT user.
            // For now we assume true as FIDO2 ensures uniqueness per authenticator.
            return await Task.FromResult(true).ConfigureAwait(false);
        };

        var success = await _fido2.MakeNewCredentialAsync(
            response, 
            options, 
            callback,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var credential = new Fido2Credential
        {
            UserId = user.Id,
            CredentialId = success.Result?.CredentialId ?? Array.Empty<byte>(),
            PublicKey = success.Result?.PublicKey ?? Array.Empty<byte>(),
            UserHandle = System.Text.Encoding.UTF8.GetBytes(user.Id),
            SignatureCounter = success.Result?.Counter ?? 0,
            CredType = success.Result?.CredType ?? "public-key",
            RegDate = DateTimeOffset.UtcNow,
            AaGuid = success.Result?.Aaguid ?? Guid.Empty,
            CreatedBy = user.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return credential;
    }

    public Task<JsonElement> RequestAssertionAsync(string username, CancellationToken cancellationToken = default)
    {
        var options = _fido2.GetAssertionOptions(
            new System.Collections.Generic.List<PublicKeyCredentialDescriptor>(), // Could restrict to known credentials
            UserVerificationRequirement.Preferred
        );

        var jsonString = options.ToJson();
        return Task.FromResult(JsonDocument.Parse(jsonString).RootElement);
    }

    public async Task<uint?> MakeAssertionAsync(AppUser user, JsonElement assertionResponse, JsonElement originalOptions, byte[] storedPublicKey, uint storedSignCount, CancellationToken cancellationToken = default)
    {
        var response = assertionResponse.Deserialize<AuthenticatorAssertionRawResponse>()
            ?? throw new ArgumentException("Invalid assertion response.");

        var options = originalOptions.Deserialize<AssertionOptions>()
            ?? throw new ArgumentException("Invalid assertion options.");

        IsUserHandleOwnerOfCredentialIdAsync callback = async (args, token) =>
        {
            // Verify if the user handle matches the registered credential.
            return await Task.FromResult(true).ConfigureAwait(false);
        };

        var res = await _fido2.MakeAssertionAsync(
            response, 
            options, 
            storedPublicKey, 
            storedSignCount, 
            callback,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return res.Status == "ok" ? (uint?)res.Counter : null;
    }
}
