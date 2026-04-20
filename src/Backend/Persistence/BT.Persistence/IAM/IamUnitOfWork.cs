using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Interfaces.Repositories;
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
