using System.Threading;
using System.Threading.Tasks;

namespace BT.Application.Features.ControlPlane.Tenants.Contracts;

public interface IStampProvisioner
{
    Task ProvisionIsolatedStampAsync(string tenantId, string stampId, string resourceGroup, string databaseProvider, CancellationToken cancellationToken = default);
}
