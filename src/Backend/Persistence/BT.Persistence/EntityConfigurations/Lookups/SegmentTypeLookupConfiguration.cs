using BT.Domain.Features.Banking.Customers.Enums;
using BT.Domain.Features.Banking.Customers.Lookups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.EntityConfigurations.Lookups;

internal sealed class SegmentTypeLookupConfiguration : BaseLookupConfiguration<SegmentTypeLookup>
{
    public override void Configure(EntityTypeBuilder<SegmentTypeLookup> builder)
    {
        base.Configure(builder);
        builder.ToTable("Lkp_SegmentTypes");

        builder.HasData(
            Row((int)SegmentType.PublicLimitedCompany, "PublicLimitedCompany", "Public Limited Company", 1),
            Row((int)SegmentType.PrivateLimitedCompany, "PrivateLimitedCompany", "Private Limited Company", 2),
            Row((int)SegmentType.SoleProprietorship, "SoleProprietorship", "Sole Proprietorship", 3),
            Row((int)SegmentType.Corporate, "Corporate", "Corporate", 4),
            Row((int)SegmentType.Retail, "Retail", "Retail", 5),
            Row((int)SegmentType.SME, "SME", "Small & Medium Enterprise", 6),
            Row((int)SegmentType.Individual, "Individual", "Individual", 7)
        );
    }
}
