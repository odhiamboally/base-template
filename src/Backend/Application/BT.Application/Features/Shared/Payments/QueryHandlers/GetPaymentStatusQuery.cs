using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using MediatR;

namespace BT.Application.Features.Shared.Payments.QueryHandlers;

public sealed record GetPaymentStatusQuery(string Provider, string PaymentReference)
    : IRequest<AppResponse<PaymentStatusResponse>>, ICachableRequest
{
    public string CacheGroup => "payments";

    public string Discriminator => CacheKeys.Entity(Provider, PaymentReference);

    public string? CacheUserId => null;

    public bool IsVersioned => false;

    public bool BypassCache => true;
}
