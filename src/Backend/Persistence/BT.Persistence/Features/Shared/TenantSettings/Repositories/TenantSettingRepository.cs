using BT.Domain.Features.Shared.TenantSettings.Contracts.Repositories;
using BT.Domain.Features.Shared.TenantSettings.Entities;
using BT.Persistence.Common.Repositories;
using BT.Persistence.Features.Shared.DataContext;
using Microsoft.EntityFrameworkCore;

namespace BT.Persistence.Features.Shared.TenantSettings.Repositories;

public class TenantSettingRepository(SharedDBContext dbContext) 
    : Repository<TenantSetting>(dbContext), ITenantSettingRepository
{
    private readonly SharedDBContext _dbContext = dbContext;
}
