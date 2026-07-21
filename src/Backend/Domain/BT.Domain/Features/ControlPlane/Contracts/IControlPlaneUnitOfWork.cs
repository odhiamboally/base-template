using System;
using System.Threading;
using System.Threading.Tasks;
using BT.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;

namespace BT.Domain.Features.ControlPlane.Contracts;

public interface IControlPlaneUnitOfWork : IDisposable, IAsyncDisposable
{
    ITenantRepository Tenants { get; }
    IDeploymentStampRepository DeploymentStamps { get; }
    
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}
