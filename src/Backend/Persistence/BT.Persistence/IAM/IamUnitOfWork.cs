using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Persistence.Common;
using BT.Persistence.IAM.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.IAM;

public sealed class IamUnitOfWork(
    IamDbContext context,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    ITokenRepository tokenRepository,
    IAppUserProfileRepository appUserProfileRepository,
    IAppUserTotpSecretRepository appUserTotpSecretRepository,
    ITempTotpSecretRepository tempTotpSecretRepository,
    IPublisher publisher,
    ILogger<IamUnitOfWork> logger
) : BaseUnitOfWork<IamDbContext>(context, publisher, logger), IIamUnitOfWork
{
    public IUserRepository UserRepository { get; } = userRepository;
    public ISessionRepository SessionRepository { get; } = sessionRepository;
    public ITokenRepository TokenRepository { get; } = tokenRepository;
    public IAppUserProfileRepository AppUserProfileRepository { get; } = appUserProfileRepository;
    public IAppUserTotpSecretRepository AppUserTotpSecretRepository { get; } = appUserTotpSecretRepository;
    public ITempTotpSecretRepository TempTotpSecretRepository { get; } = tempTotpSecretRepository;
}
