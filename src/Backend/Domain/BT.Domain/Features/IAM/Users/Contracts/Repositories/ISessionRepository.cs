using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Features.IAM.Users.Contracts.Repositories;

public interface ISessionRepository : IRepository<AppUserSession>
{
    Task<List<AppUserSession>> GetActiveSessionsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<AppUserSession?> GetTrackedByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<AppUserSession?> GetOldestSessionByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<AppUserSession>> GetExpiredSessionsAsync(DateTimeOffset retentionLimit, CancellationToken cancellationToken = default);
    Task<bool> PurgeOldSessionsAsync(DateTimeOffset retentionLimit, CancellationToken cancellationToken = default);

}
