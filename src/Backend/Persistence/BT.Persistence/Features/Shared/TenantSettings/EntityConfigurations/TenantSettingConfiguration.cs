using BT.Domain.Features.Shared.TenantSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BT.Persistence.Features.Shared.TenantSettings.EntityConfigurations;

internal class TenantSettingConfiguration : IEntityTypeConfiguration<TenantSetting>
{
    public void Configure(EntityTypeBuilder<TenantSetting> builder)
    {
        builder.ToTable("TenantSettings");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(x => x.Value)
            .IsRequired(); // No max length, could be large JSON or encrypted string
            
        builder.Property(x => x.Description)
            .HasMaxLength(1000);
            
        // Ensure a tenant can only have one setting per key
        builder.HasIndex(x => new { x.TenantId, x.Key })
            .IsUnique();
    }
}
