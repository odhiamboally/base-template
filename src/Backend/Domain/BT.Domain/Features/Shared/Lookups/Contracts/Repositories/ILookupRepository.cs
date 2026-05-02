using BT.Domain.Features.Shared.Lookups.Enums;
using BT.Domain.Features.Shared.Lookups.Entities;
using BT.Domain.Shared.Contracts.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BT.Domain.Features.Shared.Lookups.Contracts.Repositories;

public interface ILookupRepository : IRepository<BaseLookup>
{
    Task<Collection<BaseLookup>> GetLookupsByTypeAsync(LookupType lookupType, CancellationToken cancellationToken);
}
