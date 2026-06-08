using BT.Domain.Features.Shared.Lookups.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BT.Persistence.Features.Shared.Lookups.EntityConfigurations;

internal sealed class LookupCatalogTypeConfiguration : IEntityTypeConfiguration<LookupCatalogType>
{
    public void Configure(EntityTypeBuilder<LookupCatalogType> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("LookupCatalogTypes");

        builder.HasKey(type => type.Id);

        builder.Property(type => type.Id)
            .ValueGeneratedNever();

        builder.Property(type => type.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(type => type.Key)
            .IsUnique()
            .HasDatabaseName("IX_LookupCatalogTypes_Key");

        builder.Property(type => type.Label)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(type => type.Description)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(type => type.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasData(
            Row(1, "CustomerStatus", "Customer statuses", "Lifecycle statuses available to customer records."),
            Row(2, "CustomerType", "Customer types", "Classification values used when creating and segmenting customers."),
            Row(3, "DirectorRelationType", "Director relation types", "Relationship labels used for customer directors and signatories."),
            Row(4, "FailedMessageStatus", "Failed message statuses", "Operational statuses for failed message tracking."),
            Row(5, "IdentificationType", "Identification types", "Identity document types used across onboarding and verification."),
            Row(6, "LineOfBusiness", "Lines of business", "Business line values used by banking and reporting flows."),
            Row(7, "SegmentType", "Segment types", "Primary customer segmentation values."),
            Row(8, "SubSegmentType", "Sub-segment types", "Secondary customer segmentation values."));
    }

    private static LookupCatalogType Row(int id, string key, string label, string description)
        => new()
        {
            Id = id,
            Key = key,
            Label = label,
            Description = description,
            IsActive = true
        };
}
