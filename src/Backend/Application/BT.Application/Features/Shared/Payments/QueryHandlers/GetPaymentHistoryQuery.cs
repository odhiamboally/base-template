using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;

using MediatR;

namespace BT.Application.Features.Shared.Payments.QueryHandlers;

public sealed record GetPaymentHistoryQuery(Guid? Cursor = null, int PageSize = 20)
    : IRequest<AppResponse<PagedResponse<PaymentHistoryItemResponse, Guid>>>, ICachableRequest
{
    public string CacheGroup => "payments";
    public string Discriminator => CacheKeys.Entity("history", $"{Cursor}_{PageSize}");
    public string? CacheUserId => null;
    public bool IsVersioned => false;
    public bool BypassCache => true;
}
