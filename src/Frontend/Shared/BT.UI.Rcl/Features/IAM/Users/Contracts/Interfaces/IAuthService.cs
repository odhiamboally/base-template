using BT.SharedKernel.Features.IAM.Users.Dtos;
using BT.SharedKernel.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;

public interface IAuthService
{
    Task<AppResponse<LoginResponse>> LoginAsync(LoginRequest loginRequest);
    Task<AppResponse<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<AppResponse<CurrentUserResponse>> GetCurrentUserAsync();
    Task<AppResponse<ProfilePictureResponse>> UpdateProfilePictureAsync(byte[] content, string fileName, string contentType);
    Task<AppResponse<TwoFactorSetupInfo>> InitiateTotpSetupAsync();
    Task<AppResponse<VerifyOtpResponse>> VerifyTotpAsync(VerifyOtpRequest request);
    Task<AppResponse<bool>> DisableTotpAsync();
    Task<AppResponse<OtpStatusResponse>> GetTotpStatusAsync(string userId);
    Task<AppResponse<bool>> LogoutAsync();
}
