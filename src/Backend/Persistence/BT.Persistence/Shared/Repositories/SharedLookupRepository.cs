using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Domain.Enums;
using BT.Domain.Lookups;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Shared.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BT.Persistence.Shared.Repositories;

internal sealed class SharedLookupRepository : Repository<BaseLookup>, ILookupRepository
{
    private readonly SharedDbContext _sharedContext;

    public SharedLookupRepository(SharedDbContext context) : base(context)
    {
        _sharedContext = context;
    }

    public async Task<Collection<BaseLookup>> GetLookupsByTypeAsync(LookupType lookupType, CancellationToken cancellationToken)
    {
        IQueryable<BaseLookup> query = lookupType switch
        {
            LookupType.ClientStatus => _sharedContext.ClientStatuses,
            LookupType.ClientType => _sharedContext.ClientTypes,
            LookupType.SegmentType => _sharedContext.SegmentTypes,
            LookupType.SubSegmentType => _sharedContext.SubSegmentTypes,
            LookupType.LineOfBusiness => _sharedContext.LinesOfBusiness,
            LookupType.IdentificationType => _sharedContext.IdentificationTypes,
            LookupType.DirectorRelationType => _sharedContext.DirectorRelationTypes,
            _ => throw new ArgumentOutOfRangeException(nameof(lookupType), lookupType, "Unsupported lookup type.")
        };

        var list = await query
            .AsNoTracking()
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new Collection<BaseLookup>(list);
    }
}
