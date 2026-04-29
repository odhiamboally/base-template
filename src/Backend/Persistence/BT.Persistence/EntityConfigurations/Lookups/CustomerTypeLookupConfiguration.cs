using BT.Domain.Banking.Enums;
using BT.Domain.Banking.Lookups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.EntityConfigurations.Lookups;

internal sealed class CustomerTypeLookupConfiguration : BaseLookupConfiguration<CustomerTypeLookup>
{
    public override void Configure(EntityTypeBuilder<CustomerTypeLookup> builder)
    {
        base.Configure(builder);
        builder.ToTable("Lkp_ClientTypes");

        builder.HasData(
            Row((int)CustomerType.Individual, "Individual", "Individual", 1),
            Row((int)CustomerType.Corporate, "Corporate", "Corporate", 2),
            Row((int)CustomerType.Institutional, "Institutional", "Institutional", 3),
            Row((int)CustomerType.SmallMediumEnterprise, "SmallMediumEnterprise", "Small & Medium Enterprise", 4),
            Row((int)CustomerType.Enterprise, "Enterprise", "Enterprise", 5)
        );
    }
}
