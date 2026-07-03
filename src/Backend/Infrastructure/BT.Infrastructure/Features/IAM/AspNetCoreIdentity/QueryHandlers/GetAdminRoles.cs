using BT.Application.Features.IAM.Users.Queries;
using BT.Domain.Features.IAM.Users.Entities;
using BT.Persistence.Features.IAM.DataContext;
using BT.Persistence.Features.HR.DataContext;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.QueryHandlers;

internal sealed class GetAdminRoles(RoleManager<AppRole> roleManager, IamDBContext context, HrDBContext hrContext)
    : IRequestHandler<GetAdminRolesQuery, AppResponse<IReadOnlyList<AdminRoleListResponse>>>
{
    public async Task<AppResponse<IReadOnlyList<AdminRoleListResponse>>> Handle(GetAdminRolesQuery request, CancellationToken cancellationToken)
    {
        var userCounts = await context.UserRoles
            .AsNoTracking()
            .GroupBy(static userRole => userRole.RoleId)
            .Select(static group => new { RoleId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(static item => item.RoleId, static item => item.Count, cancellationToken)
            .ConfigureAwait(false);

        var roles = await roleManager.Roles
            .AsNoTracking()
            .OrderBy(static role => role.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var departmentIds = roles
            .Where(static role => role.DepartmentId.HasValue)
            .Select(static role => role.DepartmentId!.Value)
            .Distinct()
            .ToList();

        var departments = await hrContext.Departments
            .AsNoTracking()
            .Where(department => departmentIds.Contains(department.Id))
            .ToDictionaryAsync(static department => department.Id, static department => department.Name, cancellationToken)
            .ConfigureAwait(false);

        var result = roles
            .Select(role => new AdminRoleListResponse(
                role.Id,
                role.Name ?? string.Empty,
                role.NormalizedName ?? string.Empty,
                role.DepartmentId,
                role.DepartmentId.HasValue ? departments.GetValueOrDefault(role.DepartmentId.Value, "Unknown") : "Global",
                userCounts.GetValueOrDefault(role.Id)))
            .ToList();

        return AppResponses.Success("Roles loaded.", (IReadOnlyList<AdminRoleListResponse>)result);
    }
}
