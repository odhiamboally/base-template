using BT.Domain.Enums;
using BT.Domain.Lookups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BT.Persistence.EntityConfigurations.Lookups;

internal sealed class IdentificationTypeLookupConfiguration : BaseLookupConfiguration<IdentificationTypeLookup>
{
    public override void Configure(EntityTypeBuilder<IdentificationTypeLookup> builder)
    {
        base.Configure(builder);
        builder.ToTable("Lkp_IdentificationTypes");

        builder.HasData(
            Row((int)IdentificationType.CertificateOfIncorporation,
                "CertificateOfIncorporation",
                "Certificate of Incorporation",
                1),

            Row((int)IdentificationType.TIN,
                "TIN",
                "Tax Identification Number",
                2),

            Row((int)IdentificationType.BusinessLicense,
                "BusinessLicense",
                "Business License",
                3),

            Row((int)IdentificationType.CompanyRegistrationCertificate,
                "CompanyRegistrationCertificate",
                "Company Registration Certificate",
                4)
        );
    }
}

