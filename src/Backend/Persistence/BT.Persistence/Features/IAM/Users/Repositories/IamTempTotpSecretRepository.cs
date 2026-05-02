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

internal sealed class IamTempTotpSecretRepository(IamDBContext context) : Repository<TempTotpSecret>(context), ITempTotpSecretRepository
{
    public async Task<TempTotpSecret?> GetValidTempSecretByUserIdAsync(string userId)
    {
        return await FindByCondition(x => x.UserId == userId &&
                                         !x.IsDeleted &&
                                         x.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> DeleteExpiredSecretsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var expiredSecrets = await FindByCondition(x => x.ExpiresAt <= DateTimeOffset.UtcNow && !x.IsDeleted)
                .ToListAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            foreach (var secret in expiredSecrets)
            {
                secret.IsDeleted = true;
                secret.DeletedAt = DateTimeOffset.UtcNow;
                secret.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await UpdateRangeAsync(new Collection<TempTotpSecret>(expiredSecrets), cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserTempSecretsAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var tempSecret = await FindByCondition(x => x.UserId == userId && !x.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (tempSecret == null) return false;

            tempSecret.IsDeleted = true;
            tempSecret.DeletedAt = DateTimeOffset.UtcNow;
            tempSecret.UpdatedAt = DateTimeOffset.UtcNow;

            await UpdateAsync(tempSecret).ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
