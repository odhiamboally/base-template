using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Shared.Entities;
using BT.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Contracts.Implementations.Repositories;

internal sealed class FailedMessageRepository(DBContext context) : Repository<FailedMessage>(context), IFailedMessageRepository
{
}
