using Asp.Versioning;
using BT.Api.Common.Authorization;
using BT.Api.Common.Controllers;
using BT.Application.Features.Shared.Payments.CommandHandlers;
using BT.Application.Features.Shared.Payments.QueryHandlers;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text;

namespace BT.Api.Features.Shared.Payments.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/shared/payments")]
[ApiController]
[Authorize]
public sealed class PaymentsController(ISender sender) : BaseController
{
    [HttpPost("checkout")]
    [RequirePermission("payments.create")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> Checkout(PaymentInitiationRequest request, CancellationToken ct)
    {
        var response = await sender
            .Send(new InitiatePaymentCommand(request), ct)
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpGet("{provider}/{paymentReference}")]
    [RequirePermission("payments.view")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> Status(string provider, string paymentReference, CancellationToken ct)
    {
        var response = await sender
            .Send(new GetPaymentStatusQuery(provider, paymentReference), ct)
            .ConfigureAwait(false);

        return HandleResponse(response);
    }

    [HttpPost("stripe/webhook")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> StripeWebhook(CancellationToken ct)
    {
        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(ct).ConfigureAwait(false);

        var response = await sender
            .Send(new VerifyPaymentWebhookCommand("Stripe", payload, signatureHeader), ct)
            .ConfigureAwait(false);

        return HandleResponse(response);
    }
}
