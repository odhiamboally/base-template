using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.Banking.Customers.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Features.Banking.DataContext;

namespace BT.Persistence.Features.Banking.Customers.Repositories;

internal sealed class BankingCustomerRepository(BankingDBContext context) : Repository<Customer>(context), ICustomerRepository { }
