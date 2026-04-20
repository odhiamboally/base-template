using BT.Domain.Contracts.Interfaces.Repositories;

namespace BT.Domain.Contracts.Interfaces.Common;

public interface IIamUnitOfWork : ITransactionalUnitOfWork
{
    IUserRepository UserRepository { get; }
    ISessionRepository SessionRepository { get; }
    ITokenRepository TokenRepository { get; }
    IAppUserProfileRepository AppUserProfileRepository { get; }
    IAppUserTotpSecretRepository AppUserTotpSecretRepository { get; }
    ITempTotpSecretRepository TempTotpSecretRepository { get; }
}
