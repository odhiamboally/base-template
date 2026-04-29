using BT.Domain.Banking.Enums;
using BT.Domain.HR.Enums;
using BT.Domain.IAM.Enums;
using BT.Domain.Shared.Enums;
using BT.Domain.Banking.Lookups;
using BT.Domain.IAM.Lookups;
using BT.Domain.Shared.Lookups;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BT.Domain.Contracts.Interfaces.Repositories;

public interface ILookupRepository : IRepository<BaseLookup>
{
    Task<Collection<BaseLookup>> GetLookupsByTypeAsync(LookupType lookupType, CancellationToken cancellationToken);
}
