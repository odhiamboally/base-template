using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Contracts.Specifications;

public interface ICustomerSearchSpec : ISpecification<Customer, Guid>
{ }

