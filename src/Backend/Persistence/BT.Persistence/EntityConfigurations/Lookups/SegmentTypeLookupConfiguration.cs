using BT.Domain.Banking.Enums;
using BT.Domain.HR.Enums;
using BT.Domain.IAM.Enums;
using BT.Domain.Shared.Enums;
using BT.Domain.Banking.Lookups;
using BT.Domain.IAM.Lookups;
using BT.Domain.Shared.Lookups;
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
