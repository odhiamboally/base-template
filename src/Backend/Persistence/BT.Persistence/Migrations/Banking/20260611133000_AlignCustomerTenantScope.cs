using System;
using BT.Persistence.Features.Banking.DataContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BT.Persistence.Migrations.Banking
{
    /// <inheritdoc />
    [DbContext(typeof(BankingDBContext))]
    [Migration("20260611133000_AlignCustomerTenantScope")]
    public partial class AlignCustomerTenantScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Customers
                SET TenantId = '0194f700-0000-7000-8000-000000000001'
                WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
