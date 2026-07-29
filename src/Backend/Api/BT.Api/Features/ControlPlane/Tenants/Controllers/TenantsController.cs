using Asp.Versioning;
using BT.Api.Common.Authorization;
using BT.Api.Common.Controllers;
using BT.Application.Features.ControlPlane.Tenants.Commands;
using BT.Application.Features.ControlPlane.Tenants.Queries;
using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BT.Api.Features.ControlPlane.Tenants.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/control-plane/tenants")]
[ApiController]
[Authorize]
public sealed class TenantsController(ISender sender) : BaseController
{
    [HttpGet]
    [RequirePermission("control_plane.manage")]
    public async Task<IActionResult> GetAllTenants()
        => HandleResponse(await sender.Send(new GetAllTenantsQuery()).ConfigureAwait(false));

    [HttpGet("{id:guid}")]
    [RequirePermission("control_plane.manage")]
    public async Task<IActionResult> GetTenantById(Guid id)
        => HandleResponse(await sender.Send(new GetTenantByIdQuery(id)).ConfigureAwait(false));

    [HttpPost]
    [RequirePermission("control_plane.manage")]
    public async Task<IActionResult> CreateTenant(CreateTenantRequest request)
        => HandleResponse(await sender.Send(new CreateTenantCommand(request)).ConfigureAwait(false));

    [HttpPut("{id:guid}")]
    [RequirePermission("control_plane.manage")]
    public async Task<IActionResult> UpdateTenant(Guid id, UpdateTenantRequest request)
        => HandleResponse(await sender.Send(new UpdateTenantCommand(id, request)).ConfigureAwait(false));

    [HttpPost("{id:guid}/activate")]
    [RequirePermission("control_plane.manage")]
    public async Task<IActionResult> ActivateTenant(Guid id)
        => HandleResponse(await sender.Send(new ActivateTenantCommand(id)).ConfigureAwait(false));

    [HttpPost("{id:guid}/suspend")]
    [RequirePermission("control_plane.manage")]
    public async Task<IActionResult> SuspendTenant(Guid id)
        => HandleResponse(await sender.Send(new SuspendTenantCommand(id)).ConfigureAwait(false));
}
