using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Contracts.Interfaces.Repositories;

public interface IAppUserProfileRepository : IRepository<AppUserProfile>
{
    Task<AppUserProfile?> GetByUserIdAsync(string userId);
    Task<AppUserProfile> CreateOrUpdateAsync(string userId, AppUserProfile profile, CancellationToken cancellationToken);
}