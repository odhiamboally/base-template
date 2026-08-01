using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BT.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class ConsolidateMenuAndReferenceSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuIcons",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10514"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Approved MudBlazor icon key.", true, false, "Dns", "DNS/platform", new Guid("0194f700-0000-7000-8000-000000000001"), null, null });

            migrationBuilder.InsertData(
                table: "MenuPlacements",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10403"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Platform control plane tiles.", true, false, "ControlPanel", "Control Panel", new Guid("0194f700-0000-7000-8000-000000000001"), null, null });

            migrationBuilder.InsertData(
                table: "MenuRoutes",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "PlacementKey", "TenantId", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10616"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Approved application route.", true, false, "control-panel", "Control Panel", "Sidebar", new Guid("0194f700-0000-7000-8000-000000000001"), null, null, "/control-panel" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10617"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Approved application route.", true, false, "control-panel-tenants", "Tenants", "ControlPanel", new Guid("0194f700-0000-7000-8000-000000000001"), null, null, "/control-panel/tenants" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10618"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Approved application route.", true, false, "control-panel-stamps", "Stamps", "ControlPanel", new Guid("0194f700-0000-7000-8000-000000000001"), null, null, "/control-panel/stamps" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10619"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Approved application route.", true, false, "admin-tenant-settings", "Tenant Settings", "AdminCenter", new Guid("0194f700-0000-7000-8000-000000000001"), null, null, "/admin/tenant-settings" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10620"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Approved application route.", true, false, "admin-iam", "Identity & Access", "AdminCenter", new Guid("0194f700-0000-7000-8000-000000000001"), null, null, "/admin/iam" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ParentId",
                table: "MenuItems",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuItems_ParentId",
                table: "MenuItems",
                column: "ParentId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM MenuItems WHERE Id = '018fd81d-2c94-7ad0-a4a3-f1edb9c10501')
BEGIN
    INSERT INTO MenuItems (Id, TenantId, ParentId, DepartmentId, [Key], Title, Description, Url, Icon, Placement, RequiredPermissionKey, RequiredModule, DisplayOrder, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsActive, IsDeleted, DeletedAt, DeletedBy)
    VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9c10501', '0194f700-0000-7000-8000-000000000001', NULL, NULL, 'control-panel', 'Control Panel', 'Platform management.', '/control-panel', 'Dns', 'Sidebar', 'Permissions.ControlPlane.Manage', NULL, 4, '2026-01-01T00:00:00.0000000Z', 'System', NULL, NULL, 1, 0, NULL, NULL);
END

IF NOT EXISTS (SELECT 1 FROM MenuItems WHERE Id = '018fd81d-2c94-7ad0-a4a3-f1edb9c30101')
BEGIN
    INSERT INTO MenuItems (Id, TenantId, ParentId, DepartmentId, [Key], Title, Description, Url, Icon, Placement, RequiredPermissionKey, RequiredModule, DisplayOrder, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsActive, IsDeleted, DeletedAt, DeletedBy)
    VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9c30101', '0194f700-0000-7000-8000-000000000001', '018fd81d-2c94-7ad0-a4a3-f1edb9c10501', NULL, 'control-panel-tenants', 'Tenants', 'Manage SaaS tenants.', '/control-panel/tenants', 'Business', 'ControlPanel', 'Permissions.ControlPlane.Manage', NULL, 10, '2026-01-01T00:00:00.0000000Z', 'System', NULL, NULL, 1, 0, NULL, NULL);
END

IF NOT EXISTS (SELECT 1 FROM MenuItems WHERE Id = '018fd81d-2c94-7ad0-a4a3-f1edb9c30102')
BEGIN
    INSERT INTO MenuItems (Id, TenantId, ParentId, DepartmentId, [Key], Title, Description, Url, Icon, Placement, RequiredPermissionKey, RequiredModule, DisplayOrder, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsActive, IsDeleted, DeletedAt, DeletedBy)
    VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9c30102', '0194f700-0000-7000-8000-000000000001', '018fd81d-2c94-7ad0-a4a3-f1edb9c10501', NULL, 'control-panel-stamps', 'Stamps', 'Deployment stamps.', '/control-panel/stamps', 'Dns', 'ControlPanel', 'Permissions.ControlPlane.Manage', NULL, 20, '2026-01-01T00:00:00.0000000Z', 'System', NULL, NULL, 1, 0, NULL, NULL);
END

IF NOT EXISTS (SELECT 1 FROM MenuItems WHERE Id = '018fd81d-2c94-7ad0-a4a3-f1edb9c10401')
BEGIN
    INSERT INTO MenuItems (Id, TenantId, ParentId, DepartmentId, [Key], Title, Description, Url, Icon, Placement, RequiredPermissionKey, RequiredModule, DisplayOrder, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsActive, IsDeleted, DeletedAt, DeletedBy)
    VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9c10401', '0194f700-0000-7000-8000-000000000001', NULL, NULL, 'features', 'Features', 'Reusable platform capability showcases.', '/features', 'MenuOpen', 'Sidebar', NULL, NULL, 4, '2026-01-01T00:00:00.0000000Z', 'System', NULL, NULL, 1, 0, NULL, NULL);
END

IF NOT EXISTS (SELECT 1 FROM MenuItems WHERE Id = '018fd81d-2c94-7ad0-a4a3-f1edb9c10402')
BEGIN
    INSERT INTO MenuItems (Id, TenantId, ParentId, DepartmentId, [Key], Title, Description, Url, Icon, Placement, RequiredPermissionKey, RequiredModule, DisplayOrder, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsActive, IsDeleted, DeletedAt, DeletedBy)
    VALUES ('018fd81d-2c94-7ad0-a4a3-f1edb9c10402', '0194f700-0000-7000-8000-000000000001', '018fd81d-2c94-7ad0-a4a3-f1edb9c10401', NULL, 'features-payments', 'Payments', 'Test card and mobile-money payment flows.', '/features/payments', 'CreditCard', 'Sidebar', 'payments.view', NULL, 10, '2026-01-01T00:00:00.0000000Z', 'System', NULL, NULL, 1, 0, NULL, NULL);
END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuItems_ParentId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_ParentId",
                table: "MenuItems");

            migrationBuilder.DeleteData(
                table: "MenuIcons",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10514"));

            migrationBuilder.DeleteData(
                table: "MenuPlacements",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10403"));

            migrationBuilder.DeleteData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10616"));

            migrationBuilder.DeleteData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10617"));

            migrationBuilder.DeleteData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10618"));

            migrationBuilder.DeleteData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10619"));

            migrationBuilder.DeleteData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10620"));
        }
    }
}
