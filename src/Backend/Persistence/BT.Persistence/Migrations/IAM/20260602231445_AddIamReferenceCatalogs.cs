using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BT.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class AddIamReferenceCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuIcons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuIcons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuPlacements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuPlacements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    PlacementKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionContexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionContexts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ContextKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionResources", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MenuIcons",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "IsActive", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10501"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "AccountTree", "Account tree", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10502"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "AdminPanelSettings", "Admin panel", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10503"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "AutoStories", "Story/book", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10504"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "Badge", "Badge", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10505"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "Business", "Business", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10506"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "Dashboard", "Dashboard", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10507"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "Devices", "Devices", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10508"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "Group", "Group", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10509"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "LockPerson", "Security lock", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10510"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "Menu", "Generic menu", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10511"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "MenuOpen", "Menu", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10512"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved MudBlazor icon key.", true, "Settings", "Settings", new Guid("00000000-0000-0000-0000-000000000000"), null, null }
                });

            migrationBuilder.InsertData(
                table: "MenuPlacements",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "IsActive", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10401"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Main application navigation.", true, "Sidebar", "Sidebar", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10402"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Administration landing tiles.", true, "AdminCenter", "Admin Center", new Guid("00000000-0000-0000-0000-000000000000"), null, null }
                });

            migrationBuilder.InsertData(
                table: "MenuRoutes",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "IsActive", "Key", "Label", "PlacementKey", "TenantId", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10601"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "dashboard", "Dashboard", "Sidebar", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/dashboard" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10602"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "admin-center", "Admin Center", "Sidebar", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/admin" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10603"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "solution-overview", "Solution Overview", "Sidebar", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/overview" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10604"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "admin-customers", "Customers", "AdminCenter", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/admin/customers" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10605"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "admin-departments", "Departments", "AdminCenter", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/admin/departments" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10606"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "admin-employees", "Employees", "AdminCenter", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/admin/employees" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10607"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "admin-menus", "Menus", "AdminCenter", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/admin/menus" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10608"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "admin-permissions", "Permissions", "AdminCenter", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/admin/permissions" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10609"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "admin-roles", "Roles", "AdminCenter", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/admin/roles" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10610"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "admin-settings", "Settings", "AdminCenter", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/admin/settings" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10611"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "admin-user-devices", "User Devices", "AdminCenter", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/admin/user-devices" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10612"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "admin-users", "Users", "AdminCenter", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/admin/users" }
                });

            migrationBuilder.InsertData(
                table: "PermissionActions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "IsActive", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10301"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Read and list records.", true, "view", "View", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10302"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Create new records.", true, "create", "Create", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10303"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Update existing records.", true, "edit", "Edit", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10304"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Soft-delete or remove records.", true, "delete", "Delete", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10305"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Disable active records or accounts.", true, "deactivate", "Deactivate", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10306"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Assign or revoke permissions.", true, "manage_permissions", "Manage permissions", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10307"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Assign or revoke roles.", true, "manage_roles", "Manage roles", new Guid("00000000-0000-0000-0000-000000000000"), null, null }
                });

            migrationBuilder.InsertData(
                table: "PermissionContexts",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "IsActive", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10101"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Customer, accounts, loans, and financial operations.", true, "Banking", "Banking", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10102"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Departments, employees, and staff operations.", true, "HR", "Human Resources", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10103"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Users, roles, permissions, sessions, and devices.", true, "IAM", "Identity and Access", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10104"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Cross-cutting platform configuration and navigation.", true, "Platform", "Platform", new Guid("00000000-0000-0000-0000-000000000000"), null, null }
                });

            migrationBuilder.InsertData(
                table: "PermissionResources",
                columns: new[] { "Id", "ContextKey", "CreatedAt", "CreatedBy", "Description", "IsActive", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10201"), "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Customer records and onboarding.", true, "customers", "Customers", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10202"), "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Department catalog and staff grouping.", true, "departments", "Departments", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10203"), "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Employee records and IAM linkage.", true, "employees", "Employees", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10204"), "Platform", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Navigation registry and menu visibility.", true, "menus", "Menus", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10205"), "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Permission catalog and assignment surface.", true, "permissions", "Permissions", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10206"), "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Role catalog and permission bundles.", true, "roles", "Roles", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10207"), "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Application user accounts.", true, "users", "Users", new Guid("00000000-0000-0000-0000-000000000000"), null, null }
                });

            migrationBuilder.CreateIndex(
                name: "UX_MenuIcons_Key",
                table: "MenuIcons",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_MenuPlacements_Key",
                table: "MenuPlacements",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_MenuRoutes_Key",
                table: "MenuRoutes",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_MenuRoutes_Url",
                table: "MenuRoutes",
                column: "Url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_PermissionActions_Key",
                table: "PermissionActions",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_PermissionContexts_Key",
                table: "PermissionContexts",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionResources_Key",
                table: "PermissionResources",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "UX_PermissionResources_Context_Key",
                table: "PermissionResources",
                columns: new[] { "ContextKey", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuIcons");

            migrationBuilder.DropTable(
                name: "MenuPlacements");

            migrationBuilder.DropTable(
                name: "MenuRoutes");

            migrationBuilder.DropTable(
                name: "PermissionActions");

            migrationBuilder.DropTable(
                name: "PermissionContexts");

            migrationBuilder.DropTable(
                name: "PermissionResources");
        }
    }
}
