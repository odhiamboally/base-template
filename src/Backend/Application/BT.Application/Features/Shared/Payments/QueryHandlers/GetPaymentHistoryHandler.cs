using BT.Domain.Features.Shared.Contracts;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Payments.Dtos;

using MediatR;

namespace BT.Application.Features.Shared.Payments.QueryHandlers;

internal sealed class GetPaymentHistoryHandler(ISharedUnitOfWork sharedUnitOfWork)
    : IRequestHandler<GetPaymentHistoryQuery, AppResponse<PagedResponse<PaymentHistoryItemResponse, Guid>>>
{
    public async Task<AppResponse<PagedResponse<PaymentHistoryItemResponse, Guid>>> Handle(
        GetPaymentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var repository = sharedUnitOfWork.PaymentRecordRepository;

        var totalCount = await repository.CountAsync(cancellationToken).ConfigureAwait(false);

        var cursorRecord = request.Cursor.HasValue
            ? await repository.FindByIdAsync(request.Cursor.Value, cancellationToken).ConfigureAwait(false)
            : null;

        var records = await repository.ListAsync(
            query =>
            {
                var q = query.OrderByDescending(record => record.CreatedAt).ThenByDescending(record => record.Id);

                if (cursorRecord != null)
                {
                    q = (System.Linq.IOrderedQueryable<BT.Domain.Features.Shared.Payments.Entities.PaymentRecord>)q.Where(record => 
                        record.CreatedAt < cursorRecord.CreatedAt || 
                        (record.CreatedAt == cursorRecord.CreatedAt && record.Id.CompareTo(cursorRecord.Id) < 0));
                }

                return q.Take(pageSize + 1);
            },
            cancellationToken).ConfigureAwait(false);

        var hasNextPage = records.Count > pageSize;
        if (hasNextPage)
        {
            records.RemoveAt(records.Count - 1);
        }

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

        var isFirstPage = !request.Cursor.HasValue;
        var nextCursor = hasNextPage ? records[^1].Id : (Guid?)null;

        var response = new PagedResponse<PaymentHistoryItemResponse, Guid>(
            new System.Collections.ObjectModel.Collection<PaymentHistoryItemResponse>(items),
            totalCount,
            1,
            pageSize,
            isFirstPage,
            nextCursor ?? Guid.Empty);

        return AppResponses.Success(response);
    }
}
