using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Common;

public interface IBackgroundJobService
{
    void Enqueue(IRequest request);
    Task EnqueueAsync(IRequest? request, CancellationToken ct = default);
}
