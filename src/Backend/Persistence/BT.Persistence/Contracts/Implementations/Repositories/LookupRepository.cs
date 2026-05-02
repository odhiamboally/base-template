using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.Banking.Customers.Enums;
using BT.Domain.Features.Shared.FailedMessages.Enums;
using BT.Domain.Features.Shared.Lookups.Enums;
using BT.Domain.Features.Shared.Outbox.Enums;
using BT.Domain.Features.Shared.Lookups.Entities;
using BT.Persistence.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BT.Persistence.Contracts.Implementations.Repositories;

internal sealed class LookupRepository(DBContext context) : Repository<BaseLookup>(context), ILookupRepository
{
    private readonly DBContext _context = context;

    public async Task<Collection<BaseLookup>> GetLookupsByTypeAsync(LookupType lookupType, CancellationToken cancellationToken)
    {
        IQueryable<BaseLookup> query = lookupType switch
        {
            LookupType.ClientStatus => _context.ClientStatuses,
            LookupType.ClientType => _context.ClientTypes,
            LookupType.SegmentType => _context.SegmentTypes,
            LookupType.SubSegmentType => _context.SubSegmentTypes,
            LookupType.LineOfBusiness => _context.LinesOfBusiness,
            LookupType.IdentificationType => _context.IdentificationTypes,
            LookupType.DirectorRelationType => _context.DirectorRelationTypes,
            _ => throw new ArgumentOutOfRangeException(nameof(lookupType), lookupType, "Unsupported lookup type.")
        };

        var list = await query
            .AsNoTracking()
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new Collection<BaseLookup>(list);
    }
}
