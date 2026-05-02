using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Features.IAM.DataContext;
using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.IAM.Users.Repositories;

internal sealed class IamTokenRepository : Repository<RefreshToken>, ITokenRepository
{
    private readonly IamDBContext _iamContext;

    public IamTokenRepository(IamDBContext context) : base(context)
    {
        _iamContext = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        _iamContext.RefreshTokens.Add(refreshToken);
        await _iamContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        var now = DateTimeOffset.UtcNow;
        return await FindByCondition(token =>
            token.AppUserId == userId &&
            !token.RevokedAt.HasValue &&
            now < token.ExpiresAt &&
            !token.UsedAt.HasValue)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<RefreshToken?> GetByTokenAndUserAsync(string token, string userId)
    {
        return await _iamContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.Token == token && t.AppUserId == userId).ConfigureAwait(false);
    }

    public async Task<RefreshToken?> GetTokenAsync(string token)
    {
        return await FindByCondition(t => t.Token == token).AsNoTracking().FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, string userId)
    {
        return await _iamContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.AppUserId == userId).ConfigureAwait(false);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _iamContext.RefreshTokens
            .Include(rt => rt.AppUser)
            .FirstOrDefaultAsync(rt => rt.Token == token).ConfigureAwait(false);
    }

    public async Task<List<RefreshToken>> GetUserTokensAsync(string userId, int limit = 10)
    {
        return await _iamContext.RefreshTokens
            .Where(rt => rt.AppUserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .Take(limit)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<RefreshToken>> GetExpiredTokensAsync(int daysOld = 30)
    {
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-daysOld);
        return await _iamContext.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTimeOffset.UtcNow && rt.CreatedAt < cutoffDate)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _iamContext.RefreshTokens.Update(refreshToken);
        await _iamContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task RevokeTokensAsync(List<RefreshToken> tokens, string reason, string? revokedByIp = null)
    {
        if (tokens.Count == 0) return;
        revokedByIp ??= "Unknown";

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
        _iamContext.RefreshTokens.Update(refreshToken);
        await _iamContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task RevokeAllUserTokensAsync(string userId, string reason, string? revokedByIp = null)
    {
        var activeTokens = await GetActiveTokensByUserIdAsync(userId).ConfigureAwait(false);
        if (activeTokens.Count == 0) return;

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            token.RevokedReason = reason;
            token.RevokedByIp = revokedByIp;
            token.UpdatedAt = DateTimeOffset.UtcNow;
        }

        _iamContext.UpdateRange(activeTokens);
        await _iamContext.SaveChangesAsync().ConfigureAwait(false);
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
        _iamContext.RefreshTokens.Update(refreshToken);
        await _iamContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task CleanupExpiredTokensAsync(string? userId = null)
    {
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-30);
        var now = DateTimeOffset.UtcNow;

        var expiredTokens = await _iamContext.RefreshTokens
            .Where(rt =>
                (rt.ExpiresAt < now ||
                 rt.CreatedAt < cutoffDate ||
                 (rt.IsRevoked && rt.RevokedAt < cutoffDate)))
            .ToListAsync().ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(userId))
            expiredTokens = expiredTokens.Where(t => t.AppUserId == userId).ToList();

        if (expiredTokens.Count != 0)
        {
            _iamContext.RefreshTokens.RemoveRange(expiredTokens);
            await _iamContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task PerformMaintenanceAsync()
    {
        await CleanupExpiredTokensAsync().ConfigureAwait(false);
        await _iamContext.RefreshTokens.CountAsync().ConfigureAwait(false);
        await _iamContext.RefreshTokens.CountAsync(rt => rt.IsActive).ConfigureAwait(false);
        await _iamContext.RefreshTokens.CountAsync(rt => rt.IsExpired).ConfigureAwait(false);
    }
}
