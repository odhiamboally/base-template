using BT.Domain.Features.HR.Employees.Entities;
using BT.Persistence.Features.HR.Employees.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Features.HR.Employees.EntityConfigurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number)
            .IsRequired()
            .HasColumnName("EmployeeNumber")
            .HasMaxLength(20);

        builder.HasIndex(x => x.Number)
            .IsUnique()
            .HasDatabaseName("IX_Employees_EmployeeNumber");

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Audit columns
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("datetimeoffset");

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetimeoffset");

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(100);

        // Optimistic concurrency
        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsRequired();

        // Seed — wired here so the migration knows where the data belongs.
        builder.HasData(EmployeeSeed.GetSeedData());
    }
}
