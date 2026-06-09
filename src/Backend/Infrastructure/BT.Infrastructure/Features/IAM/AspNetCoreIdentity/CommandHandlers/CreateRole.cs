using BT.Application.Features.IAM.Users.Commands;
using BT.Infrastructure.Logging;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BT.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class CreateRole(RoleManager<AppRole> roleManager, ILogger<CreateRole> logger)
    : IRequestHandler<CreateRoleCommand, AppResponse<AdminRoleListResponse>>
{
    public async Task<AppResponse<AdminRoleListResponse>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var roleName = command.Request.Name.Trim();
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return AppResponse.Failure<AdminRoleListResponse>("Role name is required.");
            }

            if (await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false))
            {
                return AppResponse.Failure<AdminRoleListResponse>("A role with this name already exists.");
            }

            var role = new AppRole { Name = roleName, DepartmentId = command.Request.DepartmentId };
            var result = await roleManager.CreateAsync(role).ConfigureAwait(false);

            return result.Succeeded
                ? AppResponse.Success("Role created.", new AdminRoleListResponse(role.Id, role.Name ?? roleName, role.NormalizedName ?? roleName.ToUpperInvariant(), role.DepartmentId, role.DepartmentId.HasValue ? "Department-scoped" : "Global", 0))
                : AppResponse.Failure<AdminRoleListResponse>(string.Join(", ", result.Errors.Select(static error => error.Description)));
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogRoleCreateError(logger, command.Request.Name, ex);
            throw;
        }
    }
}
