using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Banking.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Banking.DataContext;

namespace BT.Persistence.Banking.Repositories;

internal sealed class BankingCustomerRepository(BankingDbContext context) : Repository<Customer>(context), ICustomerRepository { }
