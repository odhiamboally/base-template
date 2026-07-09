using BT.Domain.Features.Shared.Payments.Contracts.Repositories;
using BT.Domain.Features.Shared.Payments.Entities;
using BT.Persistence.Common.Repositories;
using BT.Persistence.Features.Shared.DataContext;

namespace BT.Persistence.Features.Shared.Payments.Repositories;

internal sealed class SharedPaymentRecordRepository(SharedDBContext context) : Repository<PaymentRecord>(context), IPaymentRecordRepository;
