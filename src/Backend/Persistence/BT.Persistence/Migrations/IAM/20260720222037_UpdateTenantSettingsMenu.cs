using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BT.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class UpdateTenantSettingsMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20107"),
                columns: new[] { "Description", "Key", "Title", "Url" },
                values: new object[] { "Tenant-specific configuration surface.", "admin-tenant-settings", "Tenant Settings", "/admin/tenant-settings" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20107"),
                columns: new[] { "Description", "Key", "Title", "Url" },
                values: new object[] { "Platform configuration surface.", "admin-settings", "Settings", "/admin/settings" });
        }
    }
}
