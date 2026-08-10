using BT.Domain.Features.ControlPlane.Auditing.Contracts.Repositories;
using BT.Domain.Features.ControlPlane.Auditing.Entities;
using BT.Persistence.Common;
using BT.Persistence.Common.Repositories;
using BT.Persistence.Features.ControlPlane.DataContext;

namespace BT.Persistence.Features.ControlPlane.Auditing.Repositories;

internal sealed class ImpersonationRecordRepository : Repository<ImpersonationRecord>, IImpersonationRecordRepository
{
    public ImpersonationRecordRepository(ControlPlaneDBContext context) : base(context)
    {
    }
}
