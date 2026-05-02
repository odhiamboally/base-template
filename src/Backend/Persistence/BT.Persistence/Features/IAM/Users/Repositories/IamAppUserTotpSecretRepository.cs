using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Features.IAM.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BT.Persistence.Features.IAM.Users.Repositories;

internal sealed class IamAppUserTotpSecretRepository(IamDBContext context) : Repository<AppUserTotpSecret>(context), IAppUserTotpSecretRepository
{
    public async Task<AppUserTotpSecret?> GetByUserIdAsync(string userId)
    {
        return await FindByCondition(x => x.AppUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<AppUserTotpSecret?> GetActiveSecretByUserIdAsync(string userId)
    {
        return await FindByCondition(x =>
                x.AppUserId == userId &&
                x.IsActive &&
                (x.ExpiresAt == null || x.ExpiresAt > DateTimeOffset.UtcNow))
            .AsNoTracking()
            .FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<bool> DeactivateUserSecretsAsync(string userId)
    {
        try
        {
            var secrets = await FindByCondition(x => x.AppUserId == userId && x.IsActive)
                .ToListAsync().ConfigureAwait(false);

            foreach (var secret in secrets)
            {
                secret.IsActive = false;
                secret.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await UpdateRangeAsync(new Collection<AppUserTotpSecret>(secrets)).ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
