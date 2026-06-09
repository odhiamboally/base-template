using BT.Domain.Features.HR.Departments.Entities;
using BT.Persistence.Features.HR.Departments.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BT.Persistence.Features.HR.Departments.EntityConfigurations;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(static department => department.Id);

        builder.Property(static department => department.Code)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(static department => department.Code)
            .IsUnique()
            .HasDatabaseName("IX_Departments_Code");

        builder.Property(static department => department.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(static department => department.Description)
            .HasMaxLength(300);

        builder.Property(static department => department.IsActive)
            .IsRequired();

        builder.Property(static department => department.CreatedAt)
            .IsRequired()
            .HasColumnType("datetimeoffset");

        builder.Property(static department => department.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(static department => department.UpdatedAt)
            .HasColumnType("datetimeoffset");

        builder.Property(static department => department.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(static department => department.RowVersion)
            .IsRowVersion()
            .IsRequired();

        builder.HasData(DepartmentSeed.GetSeedData());
    }
}
