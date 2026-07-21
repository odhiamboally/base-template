using Asp.Versioning;
using BT.Api.Common.Authorization;
using BT.Api.Common.Controllers;
using BT.Application.Features.Shared.TenantSettings.CommandHandlers;
using BT.Application.Features.Shared.TenantSettings.QueryHandlers;
using BT.SharedKernel.Features.Shared.TenantSettings.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BT.Api.Features.Shared.TenantSettings.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/shared/tenant-settings")]
[ApiController]
[Authorize]
public sealed class TenantSettingsController(ISender sender) : BaseController
{
    [HttpGet]
    [RequirePermission("tenant-settings.view")]
    public async Task<IActionResult> GetTenantSettings(CancellationToken ct)
    {
        var response = await sender.Send(new GetTenantSettingsQuery(), ct);
        return HandleResponse(response);
    }

    [HttpGet("{key}")]
    [RequirePermission("tenant-settings.view")]
    public async Task<IActionResult> GetTenantSettingByKey(string key, CancellationToken ct)
    {
        var response = await sender.Send(new GetTenantSettingByKeyQuery(key), ct);
        return HandleResponse(response);
    }

    [HttpPost]
    [RequirePermission("tenant-settings.create")]
    public async Task<IActionResult> CreateTenantSetting([FromBody] CreateTenantSettingRequest request, CancellationToken ct)
    {
        var response = await sender.Send(new CreateTenantSettingCommand(request, GetUserId()), ct);
        return HandleResponse(response);
    }

    [HttpPut]
    [RequirePermission("tenant-settings.update")]
    public async Task<IActionResult> UpdateTenantSetting([FromBody] UpdateTenantSettingRequest request, CancellationToken ct)
    {
        var response = await sender.Send(new UpdateTenantSettingCommand(request, GetUserId()), ct);
        return HandleResponse(response);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("tenant-settings.delete")]
    public async Task<IActionResult> DeleteTenantSetting(Guid id, CancellationToken ct)
    {
        var response = await sender.Send(new DeleteTenantSettingCommand(id, GetUserId()), ct);
        return HandleResponse(response);
    }

    private string GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("User context is not available.");
        }
        return userId;
    }
}
