using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Interfaces.Repositories;
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
