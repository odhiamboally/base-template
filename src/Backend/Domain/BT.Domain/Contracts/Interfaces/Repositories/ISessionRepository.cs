using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
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
