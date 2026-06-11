using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Persistence.Common.Repositories;
using BT.Persistence.Features.IAM.DataContext;
using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.IAM.Users.Repositories;

internal sealed class IamSessionRepository : Repository<AppUserSession>, ISessionRepository
{
    private readonly IamDBContext _iamContext;

    public IamSessionRepository(IamDBContext context) : base(context)
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

    public async Task<AppUserSession?> GetTrackedByIdAsync(Guid sessionId)
    {
        return await _iamContext.AppUserSessions
            .FirstOrDefaultAsync(session => session.Id == sessionId)
            .ConfigureAwait(false);
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

        if (!oldSessions.Any())
            return false;

        _iamContext.AppUserSessions.RemoveRange(oldSessions);
        return true;
    }
}
