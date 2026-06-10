using BT.Domain.Features.Shared.Lookups.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Features.Shared.Lookups.EntityConfigurations;

/// <summary>
/// Generic base configuration applied to every lookup table.
/// </summary>
/// <remarks>
/// Concrete subclasses call <c>base.Configure(builder)</c> then provide their
/// <c>ToTable</c> name and <c>HasData</c> seed rows. Nothing else should differ
/// between lookup configurations — the schema is identical for all of them.
/// </remarks>
internal abstract class BaseLookupConfiguration<TLookup> : IEntityTypeConfiguration<TLookup> where TLookup : BaseLookup, new()
{
    public virtual void Configure(EntityTypeBuilder<TLookup> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Lookup IDs are database-generated. Codes carry the stable business meaning.
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TenantId)
            .IsRequired();

        // Code is the stable value used by application logic and dropdown binding.
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique()
            .HasDatabaseName($"UX_{typeof(TLookup).Name}_TenantId_Code");

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);
    }

    protected static TLookup Row(int id, string code, string description, int displayOrder)
    {
        return new TLookup
        {
            Id = id,
            TenantId = new Guid("0194f700-0000-7000-8000-000000000001"),
            Code = code,
            Description = description,
            DisplayOrder = displayOrder
        };
    }
}
