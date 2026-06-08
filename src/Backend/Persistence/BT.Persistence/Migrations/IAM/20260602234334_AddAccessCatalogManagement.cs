using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BT.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class AddAccessCatalogManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[] { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20110"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Source-of-truth permission and menu reference data.", "LockPerson", true, false, "admin-access-catalog", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("00000000-0000-0000-0000-000000000000"), "Access Catalog", null, null, "/admin/access-catalog" });

            migrationBuilder.InsertData(
                table: "MenuRoutes",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "IsActive", "Key", "Label", "PlacementKey", "TenantId", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[] { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10613"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Approved application route.", true, "admin-access-catalog", "Access Catalog", "AdminCenter", new Guid("00000000-0000-0000-0000-000000000000"), null, null, "/admin/access-catalog" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20110"));

            migrationBuilder.DeleteData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10613"));
        }
    }
}
