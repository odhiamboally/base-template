using System;
using System.Threading;
using System.Threading.Tasks;
using BT.Domain.Features.ControlPlane.Contracts;
using BT.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;
using BT.Persistence.Features.ControlPlane.DataContext;

namespace BT.Persistence.Features.ControlPlane;

public class ControlPlaneUnitOfWork : IControlPlaneUnitOfWork
{
    private readonly ControlPlaneDBContext _context;
    
    public ITenantRepository Tenants { get; }
    public IDeploymentStampRepository DeploymentStamps { get; }

    public ControlPlaneUnitOfWork(
        ControlPlaneDBContext context,
        ITenantRepository tenants,
        IDeploymentStampRepository deploymentStamps)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        Tenants = tenants ?? throw new ArgumentNullException(nameof(tenants));
        DeploymentStamps = deploymentStamps ?? throw new ArgumentNullException(nameof(deploymentStamps));
    }

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
