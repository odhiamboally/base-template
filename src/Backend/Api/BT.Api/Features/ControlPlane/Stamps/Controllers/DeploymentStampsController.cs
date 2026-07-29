using Asp.Versioning;
using BT.Api.Common.Authorization;
using BT.Api.Common.Controllers;
using BT.Application.Features.ControlPlane.Stamps.Commands;
using BT.Application.Features.ControlPlane.Stamps.Queries;
using BT.SharedKernel.Features.ControlPlane.Stamps.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BT.Api.Features.ControlPlane.Stamps.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/control-plane/stamps")]
[ApiController]
[Authorize]
public sealed class DeploymentStampsController(ISender sender) : BaseController
{
    [HttpGet]
    [RequirePermission("control_plane.manage")]
    public async Task<IActionResult> GetAllDeploymentStamps()
        => HandleResponse(await sender.Send(new GetAllDeploymentStampsQuery()).ConfigureAwait(false));

    [HttpGet("{id:guid}")]
    [RequirePermission("control_plane.manage")]
    public async Task<IActionResult> GetDeploymentStampById(Guid id)
        => HandleResponse(await sender.Send(new GetDeploymentStampByIdQuery(id)).ConfigureAwait(false));

    [HttpPost]
    [RequirePermission("control_plane.manage")]
    public async Task<IActionResult> CreateDeploymentStamp(CreateDeploymentStampRequest request)
        => HandleResponse(await sender.Send(new CreateDeploymentStampCommand(request)).ConfigureAwait(false));

    [HttpPut("{id:guid}")]
    [RequirePermission("control_plane.manage")]
    public async Task<IActionResult> UpdateDeploymentStamp(Guid id, UpdateDeploymentStampRequest request)
        => HandleResponse(await sender.Send(new UpdateDeploymentStampCommand(id, request)).ConfigureAwait(false));
}
