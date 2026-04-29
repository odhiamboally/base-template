using BT.Domain.Shared.Enums;
using BT.Domain.Shared.Lookups;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BT.Domain.Shared.Contracts.Repositories;

public interface ILookupRepository : IRepository<BaseLookup>
{
    Task<Collection<BaseLookup>> GetLookupsByTypeAsync(LookupType lookupType, CancellationToken cancellationToken);
}
