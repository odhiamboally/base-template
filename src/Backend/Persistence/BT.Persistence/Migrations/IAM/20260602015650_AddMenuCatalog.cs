using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BT.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class AddMenuCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Placement = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    RequiredPermissionKey = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10101"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Operations dashboard.", 10, "Dashboard", true, false, "dashboard", null, "Sidebar", null, new Guid("00000000-0000-0000-0000-000000000000"), "Dashboard", null, null, "/dashboard" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Administrative workspace.", 20, "AdminPanelSettings", true, false, "admin-center", null, "Sidebar", null, new Guid("00000000-0000-0000-0000-000000000000"), "Admin Center", null, null, "/admin" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10301"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Architecture and solution overview.", 90, "AutoStories", true, false, "solution-overview", null, "Sidebar", null, new Guid("00000000-0000-0000-0000-000000000000"), "Solution Overview", null, null, "/overview" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20101"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Customer records and onboarding.", 10, "Business", true, false, "admin-customers", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("00000000-0000-0000-0000-000000000000"), "Customers", null, null, "/admin/customers" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20102"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Department catalog and staff grouping.", 20, "AccountTree", true, false, "admin-departments", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("00000000-0000-0000-0000-000000000000"), "Departments", null, null, "/admin/departments" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20103"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Staff records and system access.", 30, "Badge", true, false, "admin-employees", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("00000000-0000-0000-0000-000000000000"), "Employees", null, null, "/admin/employees" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20104"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Navigation catalog and menu visibility.", 40, "MenuOpen", true, false, "admin-menus", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("00000000-0000-0000-0000-000000000000"), "Menus", null, null, "/admin/menus" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20105"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Permission catalog and access keys.", 50, "LockPerson", true, false, "admin-permissions", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("00000000-0000-0000-0000-000000000000"), "Permissions", null, null, "/admin/permissions" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20106"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Role catalog and assignments.", 60, "AdminPanelSettings", true, false, "admin-roles", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("00000000-0000-0000-0000-000000000000"), "Roles", null, null, "/admin/roles" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20107"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Platform configuration surface.", 70, "Settings", true, false, "admin-settings", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("00000000-0000-0000-0000-000000000000"), "Settings", null, null, "/admin/settings" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20108"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Trusted device review and revocation.", 80, "Devices", true, false, "admin-user-devices", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("00000000-0000-0000-0000-000000000000"), "User Devices", null, null, "/admin/user-devices" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20109"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Create accounts and manage lifecycle.", 90, "Group", true, false, "admin-users", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("00000000-0000-0000-0000-000000000000"), "Users", null, null, "/admin/users" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_Placement_DisplayOrder",
                table: "MenuItems",
                columns: new[] { "Placement", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "UX_MenuItems_Key",
                table: "MenuItems",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItems");
        }
    }
}
