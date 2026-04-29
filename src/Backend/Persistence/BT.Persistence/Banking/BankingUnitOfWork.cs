using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Persistence.Common;
using BT.Persistence.Banking.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.Banking;

public sealed class BankingUnitOfWork(
    BankingDbContext context,
    ICustomerRepository customerRepository,
    IPublisher publisher,
    ILogger<BankingUnitOfWork> logger
) : BaseUnitOfWork<BankingDbContext>(context, publisher, logger), IBankingUnitOfWork
{
    public ICustomerRepository CustomerRepository { get; } = customerRepository;
}
