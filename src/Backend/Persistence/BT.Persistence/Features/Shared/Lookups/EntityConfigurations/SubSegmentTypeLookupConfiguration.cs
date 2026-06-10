using BT.Domain.Features.Banking.Customers.Enums;
using BT.Domain.Features.Banking.Customers.Lookups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Features.Shared.Lookups.EntityConfigurations;

internal sealed class SubSegmentTypeLookupConfiguration : BaseLookupConfiguration<SubSegmentTypeLookup>
{
    public override void Configure(EntityTypeBuilder<SubSegmentTypeLookup> builder)
    {
        base.Configure(builder);
        builder.ToTable("Lkp_SubSegmentTypes");

        builder.HasData(
            Row((int)SubSegmentType.Local, "Local", "Local", 1),
            Row((int)SubSegmentType.Multinational, "Multinational", "Multinational", 2),
            Row((int)SubSegmentType.GovernmentOwned, "GovernmentOwned", "Government Owned", 3),
            Row((int)SubSegmentType.Partnership, "Partnership", "Partnership", 4),
            Row((int)SubSegmentType.PrivateLimitedCompany, "PrivateLimitedCompany", "Private Limited Company", 5),
            Row((int)SubSegmentType.PublicLimitedCompany, "PublicLimitedCompany", "Public Limited Company", 6),
            Row((int)SubSegmentType.SoleProprietorship, "SoleProprietorship", "Sole Proprietorship", 7),
            Row((int)SubSegmentType.NGO, "NGO", "Non-Governmental Organisation", 8),
            Row((int)SubSegmentType.Individual, "Individual", "Individual", 9)
        );

    }
}
