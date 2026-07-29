using BT.Domain.Features.ControlPlane.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BT.Persistence.Features.ControlPlane.EntityConfigurations;

internal class TenantModuleConfiguration : IEntityTypeConfiguration<TenantModule>
{
    public void Configure(EntityTypeBuilder<TenantModule> builder)
    {
        builder.ToTable("TenantModules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ModuleKey)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(128);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.ModuleKey }).IsUnique();
    }
}
