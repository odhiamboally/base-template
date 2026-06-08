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
    Task<IReadOnlyList<LookupCatalogType>> GetCatalogTypesAsync(CancellationToken cancellationToken);

    Task<Collection<BaseLookup>> GetLookupsByTypeAsync(LookupType lookupType, CancellationToken cancellationToken);

    Task<BaseLookup?> GetLookupByIdAsync(LookupType lookupType, int id, CancellationToken cancellationToken);

    Task<BaseLookup> CreateLookupAsync(LookupType lookupType, string code, string description, int displayOrder, CancellationToken cancellationToken);

    Task<BaseLookup> UpdateLookupAsync(LookupType lookupType, int id, string code, string description, int displayOrder, CancellationToken cancellationToken);

    Task<BaseLookup> SoftDeleteLookupAsync(LookupType lookupType, int id, string deletedBy, CancellationToken cancellationToken);
}
