using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Features.IAM.Users.Contracts.Repositories;

public interface IAppUserTotpSecretRepository : IRepository<AppUserTotpSecret>
{
    Task<AppUserTotpSecret?> GetByUserIdAsync(string userId);
    Task<AppUserTotpSecret?> GetActiveSecretByUserIdAsync(string userId);
    Task<bool> DeactivateUserSecretsAsync(string userId);
}
