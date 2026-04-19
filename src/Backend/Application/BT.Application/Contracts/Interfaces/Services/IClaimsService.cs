using BT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Services;

public interface IClaimsService
{
    Task<List<Claim>> GetUserClaimsAsync(AppUser appUser);
    Task<List<Claim>> GetUserClaimsCombinedAsync(AppUser user);
    Task<bool> AddUserClaimAsync(AppUser user, Claim claim);
    Task<bool> RemoveUserClaimAsync(AppUser user, Claim claim);
    Task<bool> UpdateUserClaimAsync(AppUser user, Claim existingClaim, Claim newClaim);
}
