using BT.Domain.Features.Shared.FailedMessages.Entities;
using BT.Domain.Shared.Contracts.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Features.Shared.FailedMessages.Contracts.Repositories;

public interface IFailedMessageRepository : IRepository<FailedMessage>
{
}
