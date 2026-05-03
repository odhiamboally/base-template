using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Shared.Entities;
using BT.Persistence.Common.Repositories;
using BT.Persistence.Features.Shared.DataContext;

namespace BT.Persistence.Features.Shared.FailedMessages.Repositories;

internal sealed class SharedFailedMessageRepository(SharedDBContext context) : Repository<FailedMessage>(context), IFailedMessageRepository { }
