using BT.Application.IntegrationEvents;
using BT.Domain.Banking.Events;
using BT.Domain.HR.Events;
using BT.Domain.IAM.Events;
using BT.Domain.Shared.Events;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Services;

public interface IEmailComposer
{
    bool CanHandle(Type eventType, EmailTemplateType template);

    // Auth
    //Task<AppResponse<ComposeEmailResponse>> ComposeAppUserCreatedAsync(AppUserCreatedEvent evt, EmailTemplate emailTemplate);
    //Task<AppResponse<ComposeEmailResponse>> ComposeRequestPasswordResetAsync(RequestPasswordResetEvent evt, EmailTemplate emailTemplate);
    //Task<AppResponse<ComposeEmailResponse>> ComposePasswordResetSuccessAsync(PasswordResetSuccessEvent evt, EmailTemplate passwordResetSuccess);
    //Task<AppResponse<ComposeEmailResponse>> ComposeSecuritySettingsChangedAsync(SecuritySettingsChangedEvent evt, EmailTemplate emailTemplate);

    // Client
    Task<AppResponse<ComposeEmailResponse>> ComposeClientCreatedAsync(SendWelcomeEmailRequest req, EmailTemplateType emailTemplate);
    Task<AppResponse<ComposeEmailResponse>> ComposeClientCreatedAsync(CustomerCreatedIntegrationEvent evt, EmailTemplateType emailTemplate);

    // Employee
    Task<AppResponse<ComposeEmailResponse>> ComposeEmployeeCreatedAsync(EmployeeCreatedEvent evt, EmailTemplateType emailTemplate);

   


    

}
