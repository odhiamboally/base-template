using BT.Application.Features.IAM.Users.Commands;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class UpdateRole(RoleManager<AppRole> roleManager, ILogger<UpdateRole> logger)
    : IRequestHandler<UpdateRoleCommand, AppResponse<AdminRoleListResponse>>
{
    public async Task<AppResponse<AdminRoleListResponse>> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var role = await roleManager.FindByIdAsync(command.RoleId).ConfigureAwait(false);
            if (role is null)
            {
                return AppResponse.Failure<AdminRoleListResponse>("Role not found.");
            }

            var name = command.Request.Name.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return AppResponse.Failure<AdminRoleListResponse>("Role name is required.");
            }

            role.Name = name;
            role.DepartmentId = command.Request.DepartmentId;
            var result = await roleManager.UpdateAsync(role).ConfigureAwait(false);

            return result.Succeeded
                ? AppResponse.Success("Role updated.", new AdminRoleListResponse(role.Id, role.Name ?? name, role.NormalizedName ?? name.ToUpperInvariant(), role.DepartmentId, role.DepartmentId.HasValue ? "Department-scoped" : "Global", 0))
                : AppResponse.Failure<AdminRoleListResponse>(string.Join(", ", result.Errors.Select(static error => error.Description)));
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogRoleUpdateError(logger, command.RoleId, ex);
            throw;
        }
    }
}
