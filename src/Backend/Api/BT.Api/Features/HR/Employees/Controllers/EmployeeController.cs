using Asp.Versioning;
using BT.Application.Features.HR.Employees.CommandHandlers;
using BT.Api.Common.Controllers;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Employees.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BT.Api.Features.HR.Employees.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hr/employees")]
[ApiController]
internal sealed class EmployeeController(ISender sender) : BaseController
{
    [HttpPost]
    public async Task<ActionResult<AppResponse<EmployeeResponse>>> Create(CreateEmployeeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ArgumentNullException.ThrowIfNull(userId, nameof(userId));

        var command = new CreateEmployeeCommand(request, userId);

        var response = await sender.Send(command).ConfigureAwait(false);
        return !response.Successful ? BadRequest(response) : Ok(response);
    }
}
