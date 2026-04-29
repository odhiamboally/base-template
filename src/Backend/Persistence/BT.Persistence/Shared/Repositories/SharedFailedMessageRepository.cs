using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Shared.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Shared.DataContext;

namespace BT.Persistence.Shared.Repositories;

internal sealed class SharedFailedMessageRepository(SharedDbContext context) : Repository<FailedMessage>(context), IFailedMessageRepository { }
