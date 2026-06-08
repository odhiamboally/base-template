using BT.Application.Features.IAM.Users.Queries;
using BT.Domain.Features.IAM.Users.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.QueryHandlers;

internal sealed class GetAdminUsers(UserManager<AppUser> userManager)
    : IRequestHandler<GetAdminUsersQuery, AppResponse<PagedResponse<AdminUserListResponse, string>>>
{
    public async Task<AppResponse<PagedResponse<AdminUserListResponse, string>>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var req = request.SearchRequest;
        var pageSize = Math.Clamp(req.PageSize, 1, 50);

        var query = userManager.Users
            .AsNoTracking()
            .OrderBy(static user => user.Email)
            .ThenBy(static user => user.Id)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.GlobalSearch))
        {
            var search = req.GlobalSearch.Trim();
            query = query.Where(user =>
                user.UserName!.Contains(search)
                || user.Email!.Contains(search)
                || user.FirstName.Contains(search)
                || user.LastName.Contains(search)
                || user.PhoneNumber!.Contains(search));
        }

        if (req.Status?.Equals("Active", StringComparison.OrdinalIgnoreCase) == true)
        {
            query = query.Where(static user => user.IsActive);
        }
        else if (req.Status?.Equals("Inactive", StringComparison.OrdinalIgnoreCase) == true)
        {
            query = query.Where(static user => !user.IsActive);
        }

        if (req.TwoFactorStatus?.Equals("Enabled", StringComparison.OrdinalIgnoreCase) == true)
        {
            query = query.Where(static user => user.TwoFactorEnabled);
        }
        else if (req.TwoFactorStatus?.Equals("Recommended", StringComparison.OrdinalIgnoreCase) == true)
        {
            query = query.Where(static user => !user.TwoFactorEnabled);
        }

        if (req.LinkedRecordType?.Equals("Employee", StringComparison.OrdinalIgnoreCase) == true)
        {
            query = query.Where(static user => user.EmployeeId != null);
        }
        else if (req.LinkedRecordType?.Equals("Customer", StringComparison.OrdinalIgnoreCase) == true)
        {
            query = query.Where(static user => user.CustomerId != null);
        }
        else if (req.LinkedRecordType?.Equals("System", StringComparison.OrdinalIgnoreCase) == true)
        {
            query = query.Where(static user => user.EmployeeId == null && user.CustomerId == null);
        }

        if (req.EmployeeId.HasValue)
        {
            query = query.Where(user => user.EmployeeId == req.EmployeeId.Value);
        }

        if (req.CustomerId.HasValue)
        {
            query = query.Where(user => user.CustomerId == req.CustomerId.Value);
        }

        var filteredUsers = await query
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(req.Role))
        {
            var roleFilteredUsers = new List<AppUser>();
            foreach (var user in filteredUsers)
            {
                if (await userManager.IsInRoleAsync(user, req.Role).ConfigureAwait(false))
                {
                    roleFilteredUsers.Add(user);
                }
            }

            filteredUsers = roleFilteredUsers;
        }

        var totalRecords = filteredUsers.Count;
        var startIndex = string.IsNullOrWhiteSpace(req.Cursor)
            ? 0
            : filteredUsers.FindIndex(user => string.Equals(user.Id, req.Cursor, StringComparison.Ordinal)) + 1;

        if (startIndex < 0)
        {
            startIndex = 0;
        }

        var pageUsers = filteredUsers
            .Skip(startIndex)
            .Take(pageSize + 1)
            .ToList();

        var hasNextPage = pageUsers.Count > pageSize;
        if (hasNextPage)
        {
            pageUsers.RemoveAt(pageUsers.Count - 1);
        }

        var rows = new List<AdminUserListResponse>(pageUsers.Count);
        foreach (var user in pageUsers)
        {
            var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            rows.Add(new AdminUserListResponse(
                user.Id,
                user.UserName ?? string.Empty,
                $"{user.FirstName} {user.LastName}".Trim(),
                user.Email ?? string.Empty,
                user.PhoneNumber,
                user.IsActive,
                user.EmailConfirmed,
                user.TwoFactorEnabled,
                user.RequirePasswordChange,
                user.CreatedAt,
                user.LastLoginAt,
                user.EmployeeId,
                user.CustomerId,
                [.. roles.Order(StringComparer.OrdinalIgnoreCase)]));
        }

        var nextCursor = hasNextPage ? rows[^1].Id : null;
        var paged = new PagedResponse<AdminUserListResponse, string>(
            new Collection<AdminUserListResponse>(rows),
            totalRecords,
            1,
            pageSize,
            string.IsNullOrWhiteSpace(req.Cursor),
            nextCursor);

        return AppResponse.Success("Users loaded.", paged);
    }
}
