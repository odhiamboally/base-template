using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Shared.DataContext;

namespace BT.Persistence.Shared.Repositories;

internal sealed class SharedFailedMessageRepository(SharedDbContext context) : Repository<FailedMessage>(context), IFailedMessageRepository { }
