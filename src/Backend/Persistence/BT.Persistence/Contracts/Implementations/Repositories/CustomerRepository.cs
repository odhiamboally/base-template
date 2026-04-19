using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Domain.Entities;
using BT.Domain.Contracts.Specifications;
using BT.Persistence.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Contracts.Implementations.Repositories;

internal sealed class CustomerRepository(DBContext context) : Repository<Customer>(context), ICustomerRepository
{
    
}
