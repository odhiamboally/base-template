using BT.Domain.Contracts.Interfaces.Repositories;

namespace BT.Domain.Contracts.Interfaces.Common;

public interface IIamUnitOfWork
{
    IUserRepository UserRepository { get; }
    ISessionRepository SessionRepository { get; }
    ITokenRepository TokenRepository { get; }
    IAppUserProfileRepository AppUserProfileRepository { get; }
    IAppUserTotpSecretRepository AppUserTotpSecretRepository { get; }
    ITempTotpSecretRepository TempTotpSecretRepository { get; }

    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken);
    Task<TResult> ExecuteInTransactionWithRetryAsync<TResult>(Func<Task<TResult>> operation, int maxRetries = 3, int baseDelayMs = 50);
    Task<int> CompleteAsync(CancellationToken ct = default);
}
