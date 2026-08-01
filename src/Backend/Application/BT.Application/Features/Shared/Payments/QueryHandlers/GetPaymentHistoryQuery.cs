using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Utilities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;
using BT.Domain.Features.Shared.Payments.Enums;
using MediatR;

namespace BT.Application.Features.Shared.Payments.QueryHandlers;

public sealed record GetPaymentHistoryQuery(
    Guid? Cursor = null,
    int PageSize = 20,
    string? SearchTerm = null,
    string? Provider = null,
    decimal? ExactAmount = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null,
    PaymentStatus? Status = null)
    : IRequest<AppResponse<PagedResponse<PaymentHistoryItemResponse, Guid>>>, ICachableRequest
{
    public string CacheGroup => "payments";
    public string Discriminator => CacheKeys.Entity("history", $"{Cursor}_{PageSize}");
    public string? CacheUserId => null;
    public bool IsVersioned => false;
    public bool BypassCache => true;
}
