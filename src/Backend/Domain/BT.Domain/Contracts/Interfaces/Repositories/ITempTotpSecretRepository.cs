using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Contracts.Interfaces.Repositories;

public interface ITempTotpSecretRepository : IRepository<TempTotpSecret>
{
    Task<TempTotpSecret?> GetValidTempSecretByUserIdAsync(string userId);
    Task<bool> DeleteExpiredSecretsAsync(CancellationToken cancellationToken);
    Task<bool> DeleteUserTempSecretsAsync(string userId, CancellationToken cancellationToken);
}
