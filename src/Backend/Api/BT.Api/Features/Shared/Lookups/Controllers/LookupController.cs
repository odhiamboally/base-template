using Asp.Versioning;
using BT.Api.Common.Authorization;
using BT.Api.Common.Controllers;
using BT.Application.Features.Shared.Lookups.CommandHandlers;
using BT.Application.Features.Shared.Lookups.QueryHandlers;
using BT.SharedKernel.Features.Shared.Lookups.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Security.Claims;

namespace BT.Api.Features.Shared.Lookups.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/shared/lookups")]
[ApiController]
[Authorize]
public sealed class LookupController(ISender sender, IOutputCacheStore cacheStore) : BaseController
{
    [HttpGet("catalog-types")]
    [RequirePermission("menus.view")]
    [OutputCache(PolicyName = "LookupCachePolicy")]
    public async Task<IActionResult> CatalogTypes()
    {
        var response = await sender
            .Send(new GetLookupCatalogTypesQuery())
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpGet("{lookupType}")]
    [RequirePermission("menus.view")]
    [OutputCache(PolicyName = "LookupCachePolicy")]
    public async Task<IActionResult> Get(string lookupType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lookupType);

        var response = await sender
            .Send(new GetLookupQuery(new GetLookupRequest(lookupType), GetUserId()))
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpPost("{lookupType}")]
    [RequirePermission("menus.create")]
    public async Task<IActionResult> Create(string lookupType, CreateLookupRequest request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lookupType);

        var response = await sender
            .Send(new CreateLookupCommand(lookupType, request, GetUserId()), ct)
            .ConfigureAwait(false);

        if (response.Successful)
        {
            await cacheStore.EvictByTagAsync("lookups", ct).ConfigureAwait(false);
        }

        return HandleResponse(response);
    }

    [HttpPut("{lookupType}/{id:int}")]
    [RequirePermission("menus.edit")]
    public async Task<IActionResult> Update(string lookupType, int id, UpdateLookupRequest request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lookupType);

        var response = await sender
            .Send(new UpdateLookupCommand(lookupType, id, request, GetUserId()), ct)
            .ConfigureAwait(false);

        if (response.Successful)
        {
            await cacheStore.EvictByTagAsync("lookups", ct).ConfigureAwait(false);
        }

        return HandleResponse(response);
    }

    [HttpDelete("{lookupType}/{id:int}")]
    [RequirePermission("menus.delete")]
    public async Task<IActionResult> Delete(string lookupType, int id, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lookupType);

        var response = await sender
            .Send(new DeleteLookupCommand(lookupType, id, GetUserId()), ct)
            .ConfigureAwait(false);

        if (response.Successful)
        {
            await cacheStore.EvictByTagAsync("lookups", ct).ConfigureAwait(false);
        }

        return HandleResponse(response);
    }

    private string GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Authenticated user id was not found.");
        }

        return userId;
    }
}
