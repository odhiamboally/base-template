using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Configurations;

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Endpoints ApiEndpoints { get; set; } = new();

    public class Endpoints
    {
        public AuthEndpoints Auth { get; set; } = new();
        public TwoFactorEndpoints TwoFactor { get; set; } = new();
        public ProfileEndpoints Profile { get; set; } = new();
        public DashboardEndpoints Dashboard { get; set; } = new();
        public EmployeeEndpoints Employee { get; set; } = new();
       
    }

    public class AuthEndpoints
    {
        public string RegisterEmployee { get; set; } = string.Empty;
        public string SendEmailConfirmation { get; set; } = string.Empty;
        public string ResendEmailConfirmation { get; set; } = string.Empty;
        public string ConfirmUserEmail { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string GetCurrentUser { get; set; } = string.Empty;
        public string Get2FAProviders { get; set; } = string.Empty;
        public string Send2FACode { get; set; } = string.Empty;
        public string Verify2FACode { get; set; } = string.Empty;
        public string ProcessExternalLogin { get; set; } = string.Empty;
        public string HandleExternalLoginCallback { get; set; } = string.Empty;
        public string LinkExternalLogin { get; set; } = string.Empty;
        public string RequestPasswordReset { get; set; } = string.Empty;
        public string ValidatePasswordResetToken { get; set; } = string.Empty;
        public string ResetPassword { get; set; } = string.Empty;
        public string VerifyPassword { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string SignOut { get; set; } = string.Empty;
        public string SessionKeepAlive { get; set; } = string.Empty;
        public string SessionUnlock { get; set; } = string.Empty;
    }

    public class TwoFactorEndpoints
    {
        public string GetSetupInfo { get; set; } = string.Empty;
        public string Enable { get; set; } = string.Empty;
        public string Disable { get; set; } = string.Empty;
        public string GetStatus { get; set; } = string.Empty;
        public string GenerateBackupCodes { get; set; } = string.Empty;
        public string VerifyTotpCode { get; set; } = string.Empty;
    }

    public class ProfileEndpoints
    {
        public string GetUserProfile { get; set; } = string.Empty;
        public string UpdatePersonalDetails { get; set; } = string.Empty;
        public string UpdateContactInfo { get; set; } = string.Empty;
        public string UpdateBankingInfo { get; set; } = string.Empty;
        public string UpdateProfilePicture { get; set; } = string.Empty;
        public string ValidateProfileData { get; set; } = string.Empty;
        public string CalculateProfileCompletion { get; set; } = string.Empty;
    }

    public class DashboardEndpoints
    {
        public string GetDashboardData { get; set; } = string.Empty;
    }

    public class EmployeeEndpoints
    {
        public string Employees { get; set; } = string.Empty;
        public string EmployeeById { get; set; } = string.Empty;

    }

}

