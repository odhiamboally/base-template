using BT.Domain.Features.Banking.Customers.Enums;
using BT.Domain.Features.Banking.Customers.Lookups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BT.Persistence.EntityConfigurations.Lookups;

internal sealed class DirectorRelationTypeLookupConfiguration : BaseLookupConfiguration<DirectorRelationTypeLookup>
{
    public override void Configure(EntityTypeBuilder<DirectorRelationTypeLookup> builder)
    {
        base.Configure(builder);
        builder.ToTable("Lkp_DirectorRelationTypes");

        builder.HasData(
            Row((int)DirectorRelationType.Director,        "Director",        "Director",         1),
            Row((int)DirectorRelationType.Shareholder,     "Shareholder",     "Shareholder",      2),
            Row((int)DirectorRelationType.Signatory,       "Signatory",       "Signatory",        3),
            Row((int)DirectorRelationType.BeneficialOwner, "BeneficialOwner", "Beneficial Owner", 4),
            Row((int)DirectorRelationType.Guarantor,       "Guarantor",       "Guarantor",        5)
        );
    }
}
