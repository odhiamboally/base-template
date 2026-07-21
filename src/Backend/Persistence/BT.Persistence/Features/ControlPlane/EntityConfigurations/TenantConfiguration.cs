using BT.Domain.Features.ControlPlane.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BT.Persistence.Features.ControlPlane.EntityConfigurations;

internal class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.HostName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne(x => x.DeploymentStamp)
            .WithMany()
            .HasForeignKey(x => x.DeploymentStampId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(128);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.HostName).IsUnique();
    }
}
