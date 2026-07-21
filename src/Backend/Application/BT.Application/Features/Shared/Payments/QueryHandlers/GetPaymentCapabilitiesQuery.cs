using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;

using MediatR;

namespace BT.Application.Features.Shared.Payments.QueryHandlers;

public sealed record GetPaymentCapabilitiesQuery
    : IRequest<AppResponse<IReadOnlyCollection<PaymentProviderCapabilityResponse>>>, ICachableRequest
{
    public string CacheGroup => "payments";
    public string Discriminator => "capabilities";
    public string? CacheUserId => null;
    public bool IsVersioned => false;
    public bool BypassCache => true;
}
