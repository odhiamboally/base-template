using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Features.IAM.DataContext;
using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.IAM.Users.Repositories;

internal sealed class IamAppUserProfileRepository(IamDBContext context) : Repository<AppUserProfile>(context), IAppUserProfileRepository
{
    public async Task<AppUserProfile> CreateOrUpdateAsync(string userId, AppUserProfile profile, CancellationToken cancellationToken)
    {
        var existing = await GetByUserIdAsync(userId).ConfigureAwait(false);

        if (existing == null)
        {
            profile.CreatedAt = DateTimeOffset.UtcNow;
            profile.UpdatedAt = DateTimeOffset.UtcNow;
            await CreateAsync(profile, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            existing.TelephoneNo = profile.TelephoneNo;
            existing.MobileNo = profile.MobileNo;
            existing.Email = profile.Email;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.UpdatedBy = profile.UpdatedBy;
            profile = existing;
            await UpdateAsync(existing).ConfigureAwait(false);
        }

        return profile;
    }

    public async Task<AppUserProfile?> GetByUserIdAsync(string userId)
    {
        return await context.AppUserProfiles
            .FirstOrDefaultAsync(x => x.AppUserId == userId && !x.IsDeleted)
            .ConfigureAwait(false);
    }
}
