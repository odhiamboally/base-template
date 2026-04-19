using BT.Application.Contracts.Dtos;
using BT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Services;

public interface IIdentityResolutionService
{
    Task<AppUser?> FindByNationalIdAsync(string nationalId, CancellationToken ct = default);
    Task<bool> IsNationalIdRegisteredAsync(string nationalId, CancellationToken ct = default);
    Task<UserIdentityContext> ResolveContextAsync(string appUserId, CancellationToken ct = default);
}
