using Asp.Versioning;
using BT.Application.Features.Customers.CommandHandlers;
using BT.SharedKernel.Dtos.Client;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BT.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
internal sealed class CustomerController(ISender sender) : BaseController
{
    [HttpPost("customer")]
    public async Task<ActionResult<AppResponse<CustomerResponse>>> Create(CreateCustomerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));

        var command = new CreateCustomerCommand(request, userId);

        var response = await sender.Send(command).ConfigureAwait(false);
        return !response.Successful ? BadRequest(response) : Ok(response);
    }
}
