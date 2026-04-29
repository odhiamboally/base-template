using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Contracts.Interfaces.Repositories;

public interface IAppUserTotpSecretRepository : IRepository<AppUserTotpSecret>
{
    Task<AppUserTotpSecret?> GetByUserIdAsync(string userId);
    Task<AppUserTotpSecret?> GetActiveSecretByUserIdAsync(string userId);
    Task<bool> DeactivateUserSecretsAsync(string userId);
}
