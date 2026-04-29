using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Shared.Contracts.Common;

public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredAt { get; }
}
