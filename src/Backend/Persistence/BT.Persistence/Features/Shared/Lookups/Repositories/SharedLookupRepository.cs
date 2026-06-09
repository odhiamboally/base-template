using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.Banking.Customers.Enums;
using BT.Domain.Features.Shared.FailedMessages.Enums;
using BT.Domain.Features.Shared.Lookups.Enums;
using BT.Domain.Features.Shared.Lookups.Entities;
using BT.Domain.Features.Banking.Customers.Lookups;
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

    public async Task<IReadOnlyList<LookupCatalogType>> GetCatalogTypesAsync(CancellationToken cancellationToken)
        => await _sharedContext.LookupCatalogTypes
            .AsNoTracking()
            .Where(type => type.IsActive)
            .OrderBy(type => type.Label)
            .ThenBy(type => type.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<Collection<BaseLookup>> GetLookupsByTypeAsync(LookupType lookupType, CancellationToken cancellationToken)
    {
        IQueryable<BaseLookup> query = GetSet(lookupType);

        var list = await query
            .AsNoTracking()
            .OrderBy(l => l.Description)
            .ThenBy(l => l.Code)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new Collection<BaseLookup>(list);
    }

    public async Task<BaseLookup?> GetLookupByIdAsync(LookupType lookupType, int id, CancellationToken cancellationToken)
        => await GetSet(lookupType)
            .SingleOrDefaultAsync(lookup => lookup.Id == id, cancellationToken)
            .ConfigureAwait(false);

    public async Task<BaseLookup> CreateLookupAsync(
        LookupType lookupType,
        string code,
        string description,
        int displayOrder,
        CancellationToken cancellationToken)
    {
        var lookup = CreateLookupInstance(lookupType);
        var nextDisplayOrder = displayOrder > 0 ? displayOrder : await GetNextDisplayOrderAsync(lookupType, cancellationToken).ConfigureAwait(false);
        lookup.Update(code, description, nextDisplayOrder);

        await _sharedContext.AddAsync(lookup, cancellationToken).ConfigureAwait(false);
        return lookup;
    }

    public async Task<BaseLookup> UpdateLookupAsync(
        LookupType lookupType,
        int id,
        string code,
        string description,
        int displayOrder,
        CancellationToken cancellationToken)
    {
        var lookup = await GetLookupByIdAsync(lookupType, id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Lookup {lookupType}/{id} was not found.");

        lookup.Update(code, description, displayOrder > 0 ? displayOrder : lookup.DisplayOrder);
        _sharedContext.Update(lookup);
        return lookup;
    }

    public async Task<BaseLookup> SoftDeleteLookupAsync(
        LookupType lookupType,
        int id,
        string deletedBy,
        CancellationToken cancellationToken)
    {
        var lookup = await GetLookupByIdAsync(lookupType, id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Lookup {lookupType}/{id} was not found.");

        lookup.MarkAsDeleted(deletedBy);
        _sharedContext.Update(lookup);
        return lookup;
    }

    private IQueryable<BaseLookup> GetSet(LookupType lookupType) => lookupType switch
    {
        LookupType.CustomerStatus => _sharedContext.CustomerStatuses,
        LookupType.CustomerType => _sharedContext.CustomerTypes,
        LookupType.SegmentType => _sharedContext.SegmentTypes,
        LookupType.SubSegmentType => _sharedContext.SubSegmentTypes,
        LookupType.LineOfBusiness => _sharedContext.LinesOfBusiness,
        LookupType.IdentificationType => _sharedContext.IdentificationTypes,
        LookupType.DirectorRelationType => _sharedContext.DirectorRelationTypes,
        LookupType.FailedMessageStatus => _sharedContext.FailedMessageStatuses,
        _ => throw new ArgumentOutOfRangeException(nameof(lookupType), lookupType, "Unsupported lookup type.")
    };

    private static BaseLookup CreateLookupInstance(LookupType lookupType) => lookupType switch
    {
        LookupType.CustomerStatus => new CustomerStatusLookup(),
        LookupType.CustomerType => new CustomerTypeLookup(),
        LookupType.SegmentType => new SegmentTypeLookup(),
        LookupType.SubSegmentType => new SubSegmentTypeLookup(),
        LookupType.LineOfBusiness => new LineOfBusinessLookup(),
        LookupType.IdentificationType => new IdentificationTypeLookup(),
        LookupType.DirectorRelationType => new DirectorRelationTypeLookup(),
        LookupType.FailedMessageStatus => new FailedMessageStatusLookup(),
        _ => throw new ArgumentOutOfRangeException(nameof(lookupType), lookupType, "Unsupported lookup type.")
    };

    private async Task<int> GetNextDisplayOrderAsync(LookupType lookupType, CancellationToken cancellationToken)
    {
        var currentMax = await GetSet(lookupType)
            .IgnoreQueryFilters()
            .MaxAsync(lookup => (int?)lookup.DisplayOrder, cancellationToken)
            .ConfigureAwait(false);

        return (currentMax ?? 0) + 1;
    }
}
