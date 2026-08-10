using BT.Domain.Features.ControlPlane.Auditing.Entities;
using BT.Domain.Shared.Contracts.Repositories;

namespace BT.Domain.Features.ControlPlane.Auditing.Contracts.Repositories;

public interface IImpersonationRecordRepository : IRepository<ImpersonationRecord>
{
}
