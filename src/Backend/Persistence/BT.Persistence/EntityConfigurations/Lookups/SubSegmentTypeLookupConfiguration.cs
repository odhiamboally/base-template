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

internal sealed class SubSegmentTypeLookupConfiguration : BaseLookupConfiguration<SubSegmentTypeLookup>
{
    public override void Configure(EntityTypeBuilder<SubSegmentTypeLookup> builder)
    {
        base.Configure(builder);
        builder.ToTable("Lkp_SubSegmentTypes");

        builder.HasData(
            new SubSegmentTypeLookup 
            { 
                Id = (int)SubSegmentType.Local, 
                Code = "Local", 
                Description = "Local", 
                DisplayOrder = 1 
            },
            new SubSegmentTypeLookup 
            { 
                Id = (int)SubSegmentType.Multinational, 
                Code = "Multinational", 
                Description = "Multinational", 
                DisplayOrder = 2 
            },
            new SubSegmentTypeLookup 
            { 
                Id = (int)SubSegmentType.GovernmentOwned, 
                Code = "GovernmentOwned", 
                Description = "Government Owned", 
                DisplayOrder = 3 
            },
            new SubSegmentTypeLookup 
            { 
                Id = (int)SubSegmentType.Partnership, 
                Code = "Partnership", 
                Description = "Partnership", 
                DisplayOrder = 4 
            },
            new SubSegmentTypeLookup 
            { 
                Id = (int)SubSegmentType.PrivateLimitedCompany, 
                Code = "PrivateLimitedCompany", 
                Description = "Private Limited Company", 
                DisplayOrder = 5 
            },
            new SubSegmentTypeLookup 
            { 
                Id = (int)SubSegmentType.PublicLimitedCompany, 
                Code = "PublicLimitedCompany", 
                Description = "Public Limited Company", 
                DisplayOrder = 6 
            },
            new SubSegmentTypeLookup 
            { 
                Id = (int)SubSegmentType.SoleProprietorship, 
                Code = "SoleProprietorship", 
                Description = "Sole Proprietorship", 
                DisplayOrder = 7 
            },
            new SubSegmentTypeLookup 
            { 
                Id = (int)SubSegmentType.NGO, 
                Code = "NGO", 
                Description = "Non-Governmental Organisation", 
                DisplayOrder = 8 
            },
            new SubSegmentTypeLookup 
            { 
                Id = (int)SubSegmentType.Individual, 
                Code = "Individual", 
                Description = "Individual", 
                DisplayOrder = 9 
            }
        );

    }
}
