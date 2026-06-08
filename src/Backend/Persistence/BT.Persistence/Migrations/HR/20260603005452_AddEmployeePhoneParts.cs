using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BT.Persistence.Migrations.HR
{
    /// <inheritdoc />
    public partial class AddEmployeePhoneParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "Employees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Employees",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "+254");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNationalNumber",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE Employees
                SET
                    CountryCode = CASE
                        WHEN PhoneNumber LIKE '+254%' THEN '+254'
                        WHEN PhoneNumber LIKE '0%' THEN '+254'
                        ELSE CountryCode
                    END,
                    PhoneNationalNumber = CASE
                        WHEN PhoneNumber LIKE '+254%' THEN SUBSTRING(PhoneNumber, 5, LEN(PhoneNumber) - 4)
                        WHEN PhoneNumber LIKE '0%' THEN SUBSTRING(PhoneNumber, 2, LEN(PhoneNumber) - 1)
                        ELSE PhoneNationalNumber
                    END
                WHERE PhoneNationalNumber = '';
                """);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000001"),
                columns: new[] { "CountryCode", "PhoneNationalNumber" },
                values: new object[] { "+254", "788029249" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000002"),
                columns: new[] { "CountryCode", "PhoneNationalNumber" },
                values: new object[] { "+254", "761722609" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000003"),
                columns: new[] { "CountryCode", "PhoneNationalNumber" },
                values: new object[] { "+254", "774633490" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000004"),
                columns: new[] { "CountryCode", "PhoneNationalNumber" },
                values: new object[] { "+254", "756631912" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000005"),
                columns: new[] { "CountryCode", "PhoneNationalNumber" },
                values: new object[] { "+254", "782832156" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PhoneNationalNumber",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);
        }
    }
}
