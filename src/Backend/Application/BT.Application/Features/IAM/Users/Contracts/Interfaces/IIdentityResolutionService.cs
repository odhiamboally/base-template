using BT.Application.Features.IAM.Users.Contracts.Dtos;
using BT.Domain.Features.IAM.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.IAM.Users.Contracts.Interfaces;

public interface IIdentityResolutionService
{
    Task<AppUser?> FindByNationalIdAsync(string nationalId, CancellationToken ct = default);
    Task<bool> IsNationalIdRegisteredAsync(string nationalId, CancellationToken ct = default);
    Task<UserIdentityContext> ResolveContextAsync(string appUserId, CancellationToken ct = default);
}
