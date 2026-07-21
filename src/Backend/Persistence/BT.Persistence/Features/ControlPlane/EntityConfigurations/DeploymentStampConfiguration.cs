using BT.Domain.Features.ControlPlane.Tenants.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BT.Persistence.Features.ControlPlane.EntityConfigurations;

internal class DeploymentStampConfiguration : IEntityTypeConfiguration<DeploymentStamp>
{
    public void Configure(EntityTypeBuilder<DeploymentStamp> builder)
    {
        builder.ToTable("DeploymentStamps");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.TargetResourceGroup)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.IsolationTier)
            .IsRequired();

        builder.Property(x => x.DatabaseConnectionString)
            .HasMaxLength(1024);

        builder.Property(x => x.KeyVaultUri)
            .HasMaxLength(1024);

        builder.Property(x => x.CacheConnectionString)
            .HasMaxLength(1024);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(128);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}
