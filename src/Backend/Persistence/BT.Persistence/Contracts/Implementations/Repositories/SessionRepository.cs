using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using BT.Persistence.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Contracts.Implementations.Repositories;

internal sealed class SessionRepository : Repository<AppUserSession>, ISessionRepository
{
    private readonly DBContext _context;

    public SessionRepository(DBContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<AppUserSession>> GetActiveSessionsByUserIdAsync(string userId)
    {
        return await _context.AppUserSessions
            .Where(s => s.AppUserId == userId &&
                       s.IsActive &&
                       s.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(s => s.LastAccessedAt)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<AppUserSession?> GetOldestSessionByUserIdAsync(string userId)
    {
        return await _context.AppUserSessions
            .Where(s => s.AppUserId == userId && s.IsActive)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<List<AppUserSession>> GetExpiredSessionsAsync()
    {
        return await _context.AppUserSessions
            .Where(s =>
            s.IsActive &&
            s.IsRevoked &&
            s.ExpiresAt <= DateTimeOffset.UtcNow).ToListAsync().ConfigureAwait(false);

    }

    public async Task<bool> PurgeOldSessionsAsync(DateTimeOffset retentionLimit)
    {
        var oldSessions = await _context.AppUserSessions
            .Where(s => s.CreatedAt < retentionLimit)
            .ToListAsync().ConfigureAwait(false);

        if (oldSessions.Count == 0)
            return false;

        _context.AppUserSessions.RemoveRange(oldSessions);

        //await _context.SaveChangesAsync();

        return true;
    }
}
