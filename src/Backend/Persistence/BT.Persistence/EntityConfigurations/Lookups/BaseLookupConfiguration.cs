using BT.Domain.Lookups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.EntityConfigurations.Lookups;

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

        // PK = enum int value — stable, meaningful, no identity column needed.
        builder.HasKey(x => x.Id);

        // Code = enum member name stored by EF's HasConversion<string>().
        // Unique index ensures the lookup table and the enum stay in 1-to-1 sync.
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName($"IX_{typeof(TLookup).Name}_Code");

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
            Code = code,
            Description = description,
            DisplayOrder = displayOrder
        };
    }
}
