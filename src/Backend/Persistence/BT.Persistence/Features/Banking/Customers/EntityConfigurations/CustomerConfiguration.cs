using BT.Domain.Features.Banking.Customers.Entities;
using BT.Domain.Features.HR.Employees.Entities;
using BT.Persistence.Features.Banking.Customers.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Features.Banking.Customers.EntityConfigurations;

/// <summary>
/// EF Core entity type configuration for <see cref="Customer"/>.
/// </summary>
/// <remarks>
/// Owned entity seed data (<see cref="CorporateDetail"/>, <see cref="Address"/>,
/// <see cref="CommunicationPreference"/>) MUST be provided inside the <c>OwnsOne</c>
/// builder — calling <c>HasData</c> on the <c>Client</c> builder itself for owned
/// navigation properties does not work and will silently be ignored by EF Core.
/// </remarks>
internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClientNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(c => c.ClientNumber)
            .IsUnique()
            .HasDatabaseName("IX_Clients_ClientNumber");

        builder.Property(c => c.ClientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ClientType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.SegmentType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.SubSegmentType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        // Soft delete
        builder.Property(c => c.IsDeleted).HasDefaultValue(false);
        builder.Property(c => c.DeletedAt);
        builder.Property(c => c.DeletedBy).HasMaxLength(100);

        // Concurrency token
        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // FK to Employee
        builder.HasOne(c => c.RelationshipManager)
            .WithMany()
            .HasForeignKey(c => c.RelationshipManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------------------------------------------------------
        // Owned entities — HasData MUST live inside OwnsOne builder
        // -------------------------------------------------------------------------

        builder.OwnsOne(c => c.CorporateDetail, cd =>
        {
            cd.ToTable("Clients"); // same table (table-splitting) — or use a separate table:
                                   // cd.ToTable("ClientCorporateDetails");

            cd.Property(d => d.CompanyName).IsRequired().HasMaxLength(200);
            cd.Property(d => d.RegistrationNumber).IsRequired().HasMaxLength(100);
            cd.Property(d => d.LineOfBusiness).HasConversion<string>().HasMaxLength(50);
            cd.Property(d => d.IdentificationType).HasConversion<string>().HasMaxLength(80);
            cd.Property(d => d.NatureOfBusiness).HasMaxLength(200);
            cd.Property(d => d.Website).HasMaxLength(200);
            cd.Property(d => d.TINNumber).HasMaxLength(50);
            cd.Property(d => d.ClientClassification).HasMaxLength(50);
            cd.Property(d => d.Comments).HasMaxLength(1000);

            // Seed data for owned entity — ClientId is the shadow FK
            cd.HasData(CustomerSeed.GetCorporateDetailSeedData());
        });

        builder.OwnsOne(c => c.Address, a =>
        {
            a.ToTable("Clients"); // table-splitting — adjust if you prefer a separate table

            a.Property(x => x.ResidentialAddress).IsRequired().HasMaxLength(300);
            a.Property(x => x.Country).IsRequired().HasMaxLength(100);
            a.Property(x => x.Region).IsRequired().HasMaxLength(100);
            a.Property(x => x.Ward).IsRequired().HasMaxLength(100);
            a.Property(x => x.District).IsRequired().HasMaxLength(100);
            a.Property(x => x.Email).HasMaxLength(150);
            a.Property(x => x.Mobile).HasMaxLength(20);
            a.Property(x => x.PhoneWork).HasMaxLength(20);
            a.Property(x => x.ZipCode).HasMaxLength(20);
            a.Property(x => x.Street).HasMaxLength(200);
            a.Property(x => x.LandMark).HasMaxLength(200);

            a.HasData(CustomerSeed.GetAddressSeedData());
        });

        builder.OwnsOne(c => c.CommunicationPreference, cp =>
        {
            cp.ToTable("Clients"); // table-splitting

            cp.HasData(CustomerSeed.GetCommunicationPreferenceSeedData());
        });

        // Root entity seed data
        builder.HasData(CustomerSeed.GetClientSeedData());
    }
}
