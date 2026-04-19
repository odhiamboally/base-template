using BT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Contracts.Interfaces.Repositories;

public interface ISessionRepository : IRepository<AppUserSession>
{
    Task<List<AppUserSession>> GetActiveSessionsByUserIdAsync(string userId);
    Task<AppUserSession?> GetOldestSessionByUserIdAsync(string userId);
    Task<List<AppUserSession>> GetExpiredSessionsAsync();
    Task<bool> PurgeOldSessionsAsync(DateTimeOffset retentionLimit);

}
