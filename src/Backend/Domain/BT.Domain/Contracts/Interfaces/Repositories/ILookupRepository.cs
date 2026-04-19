using BT.Domain.Enums;
using BT.Domain.Lookups;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BT.Domain.Contracts.Interfaces.Repositories;

public interface ILookupRepository : IRepository<BaseLookup>
{
    Task<Collection<BaseLookup>> GetLookupsByTypeAsync(LookupType lookupType, CancellationToken cancellationToken);
}
