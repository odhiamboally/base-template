using BT.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;
using BT.Domain.Features.ControlPlane.Tenants.Entities;
using BT.Persistence.Common;
using BT.Persistence.Common.Repositories;
using BT.Persistence.Features.ControlPlane.DataContext;
using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.ControlPlane.Tenants.Repositories;

public class DeploymentStampRepository : Repository<DeploymentStamp>, IDeploymentStampRepository
{
    public DeploymentStampRepository(ControlPlaneDBContext context) : base(context)
    {
    }
}
