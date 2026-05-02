using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.Banking.Customers.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Features.Banking.Customers.Contracts.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{

}
