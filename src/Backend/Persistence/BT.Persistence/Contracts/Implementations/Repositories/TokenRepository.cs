using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Persistence.DataContext;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Contracts.Implementations.Repositories;

internal sealed class TokenRepository(DBContext context) : Repository<RefreshToken>(context), ITokenRepository
{
    private readonly DBContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        var now = DateTimeOffset.UtcNow;

        return await FindByCondition(token =>
            token.AppUserId == userId &&
            !token.RevokedAt.HasValue &&           // Not revoked
            now < token.ExpiresAt &&               // Not expired
            !token.UsedAt.HasValue)                // Not used
            .ToListAsync().ConfigureAwait(false);

    }

    public async Task<RefreshToken?> GetByTokenAndUserAsync(string token, string userId)
    {
        return await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.Token == token && t.AppUserId == userId).ConfigureAwait(false);
    }

    public async Task<RefreshToken?> GetTokenAsync(string token)
    {
        return await FindByCondition(t => t.Token == token).AsNoTracking().FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, string userId)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.AppUserId == userId).ConfigureAwait(false);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.AppUser)
            .FirstOrDefaultAsync(rt => rt.Token == token).ConfigureAwait(false);
    }

    public async Task<List<RefreshToken>> GetUserTokensAsync(string userId, int limit = 10)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.AppUserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .Take(limit)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<RefreshToken>> GetExpiredTokensAsync(int daysOld = 30)
    {
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-daysOld);

        return await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTimeOffset.UtcNow && rt.CreatedAt < cutoffDate)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task RevokeTokensAsync(List<RefreshToken> tokens, string reason, string? revokedByIp = null)
    {
        if (tokens.Count == 0) return;
        if (string.IsNullOrWhiteSpace(revokedByIp))
        {
            revokedByIp = "Unknown";
        }

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            token.RevokedReason = reason;
            token.RevokedByIp = revokedByIp;
            token.UpdatedAt = DateTimeOffset.UtcNow;
            await UpdateAsync(token).ConfigureAwait(false);
        }

    }

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string reason, string? revokedByIp = null)
    {
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        refreshToken.RevokedReason = reason;
        refreshToken.RevokedByIp = revokedByIp;

        await UpdateRefreshTokenAsync(refreshToken).ConfigureAwait(false);
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string reason)
    {
        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        refreshToken.RevokedReason = reason;

        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync().ConfigureAwait(false);

    }

    public async Task RevokeAllUserTokensAsync(string userId, string reason, string? revokedByIp = null)
    {
        var activeTokens = await GetActiveTokensByUserIdAsync(userId).ConfigureAwait(false);

        if (activeTokens.Count == 0)
        {
            return;
        }

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            token.RevokedReason = reason;
            token.RevokedByIp = revokedByIp;
            token.UpdatedAt = DateTimeOffset.UtcNow;

        }

        _context.UpdateRange(activeTokens);
        await _context.SaveChangesAsync().ConfigureAwait(false);

    }

    public async Task<bool> IsTokenActiveAsync(string token)
    {
        var refreshToken = await FindByCondition(t =>
            t.Token == token &&
            t.IsActive &&
            !t.IsExpired &&
            !t.IsRevoked &&
            !t.IsUsed)
            .AsNoTracking()
            .FirstOrDefaultAsync().ConfigureAwait(false);

        return refreshToken != null;
    }

    public async Task MarkTokenAsUsedAsync(RefreshToken refreshToken)
    {
        refreshToken.UsedAt = DateTimeOffset.UtcNow;

        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync().ConfigureAwait(false);

    }

    public async Task CleanupExpiredTokensAsync(string? userId = null)
    {
        // Clean up tokens older than 30 days
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-30);
        var now = DateTimeOffset.UtcNow;

        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.AppUserId == userId &&
                        (rt.ExpiresAt < now ||
                         rt.CreatedAt < cutoffDate ||
                         (rt.IsRevoked && rt.RevokedAt < cutoffDate)))
            .ToListAsync().ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            expiredTokens = expiredTokens.Where(t => t.AppUserId == userId).ToList();
        }

        if (expiredTokens.Count != 0)
        {
            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task PerformMaintenanceAsync()
    {
        await CleanupExpiredTokensAsync().ConfigureAwait(false);

        // ToDo: Log statistics
        await _context.RefreshTokens.CountAsync().ConfigureAwait(false);
        await _context.RefreshTokens.CountAsync(rt => rt.IsActive).ConfigureAwait(false);
        await _context.RefreshTokens.CountAsync(rt => rt.IsExpired).ConfigureAwait(false);
    }



}

