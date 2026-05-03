using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BT.SharedKernel.Features.Shared.EmailTemplates.Enums;

public enum EmailTemplateType
{
    [Description("Standard Welcome")]
    StandardWelcome,

    [Description("App User Created Email")]
    AppUserCreated,

    [Description("Tenant Welcome Email")]
    TenantWelcome,

    [Description("Tenant Password Reset Email")]
    TenantPasswordReset,

    [Description("Tenant Notification Email")]
    TenantNotification,

    [Description("Client Created Email")]
    ClientCreated,

    [Description("Client Created Email")]
    ClientApproved,

    [Description("Client Welcome Email")]
    ClientWelcome,

    [Description("Employee Created Email")]
    EmployeeCreated,

    [Description("Client Created Email")]
    EmployeApproved,

    [Description("Employee Welcome Email")]
    EmployeeWelcome,

    [Description("Member Welcome Email")]
    PasswordResetRequest,

    [Description("Password Reset Code Email")]
    PasswordResetCode,

    [Description("Password Reset Success Email")]
    PasswordResetSuccess,

    [Description("Security Settings Changed Email")]
    SecuritySettingsChanged,

    [Description("Institutional")]
    Institutional,

    [Description("Corporate")]
    Corporate,

    [Description("Small Medium Enterprise")]
    SmallMediumEnterprise,

    [Description("Individual Welcome")]
    IndividualWelcome,

    [Description("Enterprise Welcome")]
    EnterpriseWelcome
}