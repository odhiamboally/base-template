using BT.Domain.Shared.Contracts.Specifications;
using BT.Domain.Features.Banking.Customers.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Features.Banking.Customers.Contracts.Specifications;

public interface ICustomerSearchSpec : ISpecification<Customer, Guid>
{ }

