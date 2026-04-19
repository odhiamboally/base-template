using BT.Application.Contracts.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Common;

public interface IServiceManager
{
    ICacheService CacheService { get; }
    ISessionService SessionService { get; }
    IBackgroundJobService BackgroundJobService { get; }
    IEmailComposer MailComposer { get; }
    IEmailService EmailService { get; }
    ISmsComposer SmsComposer { get; }
    IEncryptionService EncryptionService { get; }
    IAppUserService AppUserService { get; }
  
}
