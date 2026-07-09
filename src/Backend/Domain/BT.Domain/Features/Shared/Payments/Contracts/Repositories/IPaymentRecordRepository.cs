using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.Shared.Payments.Entities;

namespace BT.Domain.Features.Shared.Payments.Contracts.Repositories;

public interface IPaymentRecordRepository : IRepository<PaymentRecord>;
