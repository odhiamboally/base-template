using BT.Domain.Features.Shared.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;

using MediatR;

namespace BT.Application.Features.Shared.Payments.QueryHandlers;

internal sealed class GetPaymentHistoryHandler(ISharedUnitOfWork sharedUnitOfWork)
    : IRequestHandler<GetPaymentHistoryQuery, AppResponse<PaymentHistoryResponse>>
{
    public async Task<AppResponse<PaymentHistoryResponse>> Handle(
        GetPaymentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;
        var repository = sharedUnitOfWork.PaymentRecordRepository;

        var totalCount = await repository.CountAsync(cancellationToken).ConfigureAwait(false);
        var records = await repository.ListAsync(
            query => query
                .OrderByDescending(record => record.CreatedAt)
                .Skip(skip)
                .Take(request.PageSize),
            cancellationToken).ConfigureAwait(false);

        var items = records
            .Select(record => new PaymentHistoryItemResponse(
                record.Id,
                record.CustomerReference,
                record.Provider,
                record.Amount.Amount,
                record.Amount.Currency,
                record.Description,
                record.Status.ToString(),
                record.CheckoutUrl ?? string.Empty,
                record.CreatedAt,
                record.UpdatedAt))
            .ToArray();

        return AppResponses.Success(new PaymentHistoryResponse(
            items,
            request.Page,
            request.PageSize,
            totalCount));
    }
}
