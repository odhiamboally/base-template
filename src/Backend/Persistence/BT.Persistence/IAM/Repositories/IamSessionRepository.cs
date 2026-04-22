using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Domain.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.IAM.DataContext;
using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.IAM.Repositories;

internal sealed class IamSessionRepository : Repository<AppUserSession>, ISessionRepository
{
    private readonly IamDbContext _iamContext;

    public IamSessionRepository(IamDbContext context) : base(context)
    {
        _iamContext = context;
    }

    public async Task<List<AppUserSession>> GetActiveSessionsByUserIdAsync(string userId)
    {
        return await _iamContext.AppUserSessions
            .Where(s => s.AppUserId == userId && s.IsActive && s.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(s => s.LastAccessedAt)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<AppUserSession?> GetOldestSessionByUserIdAsync(string userId)
    {
        return await _iamContext.AppUserSessions
            .Where(s => s.AppUserId == userId && s.IsActive)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<List<AppUserSession>> GetExpiredSessionsAsync()
    {
        return await _iamContext.AppUserSessions
            .Where(s => s.IsActive && s.IsRevoked && s.ExpiresAt <= DateTimeOffset.UtcNow)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<bool> PurgeOldSessionsAsync(DateTimeOffset retentionLimit)
    {
        var oldSessions = await _iamContext.AppUserSessions
            .Where(s => s.CreatedAt < retentionLimit)
            .ToListAsync().ConfigureAwait(false);

        if (oldSessions.Count == 0)
            return false;

        _iamContext.AppUserSessions.RemoveRange(oldSessions);
        return true;
    }
}
