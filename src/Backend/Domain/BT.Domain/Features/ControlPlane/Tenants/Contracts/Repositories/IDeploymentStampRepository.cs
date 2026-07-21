using BT.Domain.Features.ControlPlane.Tenants.Entities;
using BT.Domain.Shared.Contracts.Repositories;

namespace BT.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;

public interface IDeploymentStampRepository : IRepository<DeploymentStamp>
{
}
