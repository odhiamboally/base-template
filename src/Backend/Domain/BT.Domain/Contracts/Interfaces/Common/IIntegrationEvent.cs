using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Contracts.Interfaces.Common; 

public interface IIntegrationEvent
{
    DateTimeOffset OccurredAt { get; }
}
