using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.Banking.Customers.Enums;
using BT.Domain.Features.Shared.FailedMessages.Enums;
using BT.Domain.Features.Shared.Lookups.Enums;
using BT.Domain.Features.Shared.Outbox.Enums;
using BT.Domain.Features.Shared.Lookups.Entities;
using BT.Persistence.Common.Repositories;
using BT.Persistence.Features.Shared.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BT.Persistence.Features.Shared.Lookups.Repositories;

internal sealed class SharedLookupRepository : Repository<BaseLookup>, ILookupRepository
{
    private readonly SharedDBContext _sharedContext;

    public SharedLookupRepository(SharedDBContext context) : base(context)
    {
        _sharedContext = context;
    }

    public async Task<Collection<BaseLookup>> GetLookupsByTypeAsync(LookupType lookupType, CancellationToken cancellationToken)
    {
        IQueryable<BaseLookup> query = lookupType switch
        {
            LookupType.CustomerStatus => _sharedContext.CustomerStatuses,
            LookupType.CustomerType => _sharedContext.CustomerTypes,
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
