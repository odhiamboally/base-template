using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BT.Persistence.Migrations.HR
{
    /// <inheritdoc />
    public partial class AddTenantScopeToEmployeeSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeNumberSequences_DepartmentId_Year",
                table: "EmployeeNumberSequences");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "EmployeeNumberSequences",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE EmployeeNumberSequences
                SET TenantId = '0194f700-0000-7000-8000-000000000001'
                WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeNumberSequences_DepartmentId",
                table: "EmployeeNumberSequences",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "UX_EmployeeNumberSequences_TenantId_DepartmentId_Year",
                table: "EmployeeNumberSequences",
                columns: new[] { "TenantId", "DepartmentId", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeNumberSequences_DepartmentId",
                table: "EmployeeNumberSequences");

            migrationBuilder.DropIndex(
                name: "UX_EmployeeNumberSequences_TenantId_DepartmentId_Year",
                table: "EmployeeNumberSequences");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EmployeeNumberSequences");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeNumberSequences_DepartmentId_Year",
                table: "EmployeeNumberSequences",
                columns: new[] { "DepartmentId", "Year" },
                unique: true);
        }
    }
}
