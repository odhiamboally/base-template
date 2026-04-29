using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Banking.DataContext;

namespace BT.Persistence.Banking.Repositories;

internal sealed class BankingCustomerRepository(BankingDbContext context) : Repository<Customer>(context), ICustomerRepository { }
