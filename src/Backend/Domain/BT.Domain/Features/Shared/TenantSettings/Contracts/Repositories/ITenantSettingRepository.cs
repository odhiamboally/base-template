using BT.Domain.Features.Shared.TenantSettings.Entities;
using BT.Domain.Shared.Contracts.Repositories;

namespace BT.Domain.Features.Shared.TenantSettings.Contracts.Repositories;

public interface ITenantSettingRepository : IRepository<TenantSetting>
{
}
