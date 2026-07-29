using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BT.Persistence.Migrations.IamPostgreSqlDB
{
    /// <inheritdoc />
    public partial class AddControlPanelMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10501"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Platform management.", 4, "Dns", true, false, "control-panel", null, "Sidebar", "permissions.controlplane.manage", new Guid("0194f700-0000-7000-8000-000000000001"), "Control Panel", null, null, "/system/control-panel/tenants" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c30101"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage SaaS tenants.", 10, "Business", true, false, "control-panel-tenants", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10501"), "Sidebar", "permissions.controlplane.manage", new Guid("0194f700-0000-7000-8000-000000000001"), "Tenants", null, null, "/system/control-panel/tenants" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c30102"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Deployment stamps.", 20, "Dns", true, false, "control-panel-stamps", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10501"), "Sidebar", "permissions.controlplane.manage", new Guid("0194f700-0000-7000-8000-000000000001"), "Stamps", null, null, "/system/control-panel/stamps" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Context", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "IsActive", "IsDeleted", "Key", "Resource", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10801"), "manage", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage Control Plane Tenants and Stamps.", true, false, "manage.manage", "manage", new Guid("0194f700-0000-7000-8000-000000000001"), null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10501"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c30101"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c30102"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10801"));
        }
    }
}
