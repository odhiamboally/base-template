using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Contracts.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Contracts.Implementations.Common;

internal sealed class ServiceManager : IServiceManager
{
    public ICacheService CacheService => _cacheService.Value;
    public ISessionService SessionService => _sessionService.Value;
    public IEncryptionService EncryptionService => _encryptionService.Value;
    public IBackgroundJobService BackgroundJobService => _backgroundJobService.Value;
    public IEmailComposer MailComposer => _mailComposer.Value;
    public IEmailService EmailService => _emailService.Value;
    public ISmsComposer SmsComposer => _smsComposer.Value;
    public IAppUserService AppUserService => _appUserService.Value;

    private readonly IServiceProvider _serviceProvider;

    private readonly Lazy<ICacheService> _cacheService;
    private readonly Lazy<ISessionService> _sessionService;
    private readonly Lazy<IEncryptionService> _encryptionService;
    private readonly Lazy<IBackgroundJobService> _backgroundJobService;
    private readonly Lazy<IEmailComposer> _mailComposer;
    private readonly Lazy<IEmailService> _emailService;
    private readonly Lazy<ISmsComposer> _smsComposer;
    private readonly Lazy<IAppUserService> _appUserService;


    public ServiceManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _cacheService = new Lazy<ICacheService>(() => _serviceProvider.GetRequiredService<ICacheService>());
        _sessionService = new Lazy<ISessionService>(() => _serviceProvider.GetRequiredService<ISessionService>());
        _encryptionService = new Lazy<IEncryptionService>(() => _serviceProvider.GetRequiredService<IEncryptionService>());
        _backgroundJobService = new Lazy<IBackgroundJobService>(() => _serviceProvider.GetRequiredService<IBackgroundJobService>());
        _mailComposer = new Lazy<IEmailComposer>(() => _serviceProvider.GetRequiredService<IEmailComposer>());
        _emailService = new Lazy<IEmailService>(() => _serviceProvider.GetRequiredService<IEmailService>());
        _smsComposer = new Lazy<ISmsComposer>(() => _serviceProvider.GetRequiredService<ISmsComposer>());
        _appUserService = new Lazy<IAppUserService>(() => _serviceProvider.GetRequiredService<IAppUserService>());

    }
}

