using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.IAM.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.IAM.Contracts.Repositories;

public interface IAppUserProfileRepository : IRepository<AppUserProfile>
{
    Task<AppUserProfile?> GetByUserIdAsync(string userId);
    Task<AppUserProfile> CreateOrUpdateAsync(string userId, AppUserProfile profile, CancellationToken cancellationToken);
}