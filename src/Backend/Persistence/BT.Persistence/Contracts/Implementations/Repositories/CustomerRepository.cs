using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Banking.Entities;
using BT.Domain.Banking.Contracts.Specifications;
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
