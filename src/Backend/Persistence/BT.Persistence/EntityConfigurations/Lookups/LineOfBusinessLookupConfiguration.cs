using BT.Domain.Banking.Enums;
using BT.Domain.Banking.Lookups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.EntityConfigurations.Lookups;

internal sealed class LineOfBusinessLookupConfiguration : BaseLookupConfiguration<LineOfBusinessLookup>
{
    public override void Configure(EntityTypeBuilder<LineOfBusinessLookup> builder)
    {
        base.Configure(builder);
        builder.ToTable("Lkp_LineOfBusiness");

        builder.HasData(
            Row((int)LineOfBusiness.Agriculture,        "Agriculture",        "Agriculture",        1),
            Row((int)LineOfBusiness.Manufacturing,      "Manufacturing",      "Manufacturing",      2),
            Row((int)LineOfBusiness.Technology,         "Technology",         "Technology",         3),
            Row((int)LineOfBusiness.FinancialServices,  "FinancialServices",  "Financial Services", 4),
            Row((int)LineOfBusiness.Retail,             "Retail",             "Retail",             5),
            Row((int)LineOfBusiness.Services,           "Services",           "Services",           6),
            Row((int)LineOfBusiness.Proprietary,        "Proprietary",        "Proprietary",        7),
            Row((int)LineOfBusiness.Trading,            "Trading",            "Trading",            8)
        );
    }
}
