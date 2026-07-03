using Asp.Versioning;
using BT.Api.Common.Authorization;
using BT.Application.Features.Banking.Customers.CommandHandlers;
using BT.Application.Features.Banking.Customers.QueryHandlers;
using BT.Api.Common.Controllers;
using BT.SharedKernel.Features.Banking.Customers.Dtos;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BT.Api.Features.Banking.Customers.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/banking/customers")]
[ApiController]
[Authorize]
public sealed class CustomerController(ISender sender) : BaseController
{
    [HttpGet]
    [RequirePermission("customers.view")]
    public async Task<ActionResult<AppResponse<PagedResponse<CustomerResponse, Guid>>>> List([FromQuery] CustomerSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));

        var response = await sender.Send(new SearchCustomerListQuery(request, userId)).ConfigureAwait(false);
        return !response.IsSuccess ? BadRequest(response) : Ok(response);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("customers.view")]
    public async Task<ActionResult<AppResponse<CustomerResponse>>> GetById(Guid id)
    {
        var response = await sender.Send(new GetCustomerByIdQuery(id)).ConfigureAwait(false);
        return !response.IsSuccess ? NotFound(response) : Ok(response);
    }

    [HttpPost]
    [RequirePermission("customers.create")]
    public async Task<ActionResult<AppResponse<CustomerResponse>>> Create(CreateCustomerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));

        var command = new CreateCustomerCommand(request, userId);

        var response = await sender.Send(command).ConfigureAwait(false);
        return !response.IsSuccess ? BadRequest(response) : Ok(response);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("customers.edit")]
    public async Task<ActionResult<AppResponse<CustomerResponse>>> Update(Guid id, UpdateCustomerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));

        var command = new UpdateCustomerCommand(id, request with { Id = id }, userId);
        var response = await sender.Send(command).ConfigureAwait(false);
        return !response.IsSuccess ? BadRequest(response) : Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("customers.delete")]
    public async Task<ActionResult<AppResponse<bool>>> Delete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));

        var response = await sender.Send(new DeleteCustomerCommand(id, userId)).ConfigureAwait(false);
        return !response.IsSuccess ? BadRequest(response) : Ok(response);
    }
}
