using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using BT.SharedKernel.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Services;

public interface IAppUserService
{
    Task<bool> IsInRoleAsync(AppUser appUser, string roleName);
    Task<bool> DeleteAsync(AppUser appUser);
    Task<bool> UpdateAsync(AppUser user);
    Task<AppUser?> FindByIdAsync(string appUserId);
    Task<AppUser?> FindByEmailAsync(string email);
    Task<(bool Success, string Message)> CreateAsync(AppUser user, string password);
}
