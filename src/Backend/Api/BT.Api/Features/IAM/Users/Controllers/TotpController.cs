using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BT.Api.Features.IAM.Users.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/iam/users/totp")]
[ApiController]
internal sealed class TotpController : ControllerBase
{
}
