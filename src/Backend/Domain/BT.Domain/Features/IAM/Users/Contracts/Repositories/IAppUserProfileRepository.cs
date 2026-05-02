using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Features.IAM.Users.Contracts.Repositories;

public interface IAppUserProfileRepository : IRepository<AppUserProfile>
{
    Task<AppUserProfile?> GetByUserIdAsync(string userId);
    Task<AppUserProfile> CreateOrUpdateAsync(string userId, AppUserProfile profile, CancellationToken cancellationToken);
}