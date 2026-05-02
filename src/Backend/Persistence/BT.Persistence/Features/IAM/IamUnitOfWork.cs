using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Persistence.Common;
using BT.Persistence.Features.IAM.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.Features.IAM;

public sealed class IamUnitOfWork(
    IamDBContext context,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    ITokenRepository tokenRepository,
    IAppUserProfileRepository appUserProfileRepository,
    IAppUserTotpSecretRepository appUserTotpSecretRepository,
    ITempTotpSecretRepository tempTotpSecretRepository,
    IPublisher publisher,
    ILogger<IamUnitOfWork> logger
) : BaseUnitOfWork<IamDBContext>(context, publisher, logger), IIamUnitOfWork
{
    public IUserRepository UserRepository { get; } = userRepository;
    public ISessionRepository SessionRepository { get; } = sessionRepository;
    public ITokenRepository TokenRepository { get; } = tokenRepository;
    public IAppUserProfileRepository AppUserProfileRepository { get; } = appUserProfileRepository;
    public IAppUserTotpSecretRepository AppUserTotpSecretRepository { get; } = appUserTotpSecretRepository;
    public ITempTotpSecretRepository TempTotpSecretRepository { get; } = tempTotpSecretRepository;
}
