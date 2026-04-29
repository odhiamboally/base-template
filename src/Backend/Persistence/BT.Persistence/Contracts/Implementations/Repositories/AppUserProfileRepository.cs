using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using BT.Persistence.DataContext;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Contracts.Implementations.Repositories;

internal sealed class AppUserProfileRepository(DBContext context) : Repository<AppUserProfile>(context), IAppUserProfileRepository
{
    public async Task<AppUserProfile> CreateOrUpdateAsync(string userId, AppUserProfile profile, CancellationToken cancellationToken)
    {
        var existing = await GetByUserIdAsync(userId).ConfigureAwait(false);

        if (existing == null)
        {
            //profile.UserId = userId;
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
        return await context.AppUserProfiles.FirstOrDefaultAsync(x => x.AppUserId == userId && !x.IsDeleted).ConfigureAwait(false);
    }
}

