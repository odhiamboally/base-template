using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BT.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentRecordSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "PaymentRecords",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "PaymentRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PaymentRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PaymentRecords");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "PaymentRecords");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PaymentRecords");
        }
    }
}
