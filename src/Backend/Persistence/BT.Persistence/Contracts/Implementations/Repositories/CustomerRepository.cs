using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.Banking.Customers.Entities;
using BT.Domain.Features.Banking.Customers.Contracts.Specifications;
using BT.Domain.Shared.Contracts.Specifications;
using BT.Persistence.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Contracts.Implementations.Repositories;

internal sealed class CustomerRepository(DBContext context) : Repository<Customer>(context), ICustomerRepository
{
    
}
