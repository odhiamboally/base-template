using BT.Domain.Shared.Contracts.Specifications;
using BT.Domain.Banking.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Banking.Contracts.Specifications;

public interface ICustomerSearchSpec : ISpecification<Customer, Guid>
{ }

