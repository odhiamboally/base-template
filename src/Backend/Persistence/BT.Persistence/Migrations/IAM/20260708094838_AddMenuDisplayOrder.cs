using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BT.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class AddMenuDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20105"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20106"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20108"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20109"));

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "MenuItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10101"),
                column: "DisplayOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"),
                column: "DisplayOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10301"),
                column: "DisplayOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20101"),
                column: "DisplayOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20102"),
                column: "DisplayOrder",
                value: 20);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20103"),
                column: "DisplayOrder",
                value: 30);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20104"),
                column: "DisplayOrder",
                value: 40);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20107"),
                column: "DisplayOrder",
                value: 50);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20110"),
                column: "DisplayOrder",
                value: 70);

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10401"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage subscription and payments.", 4, "CreditCard", true, false, "billing", null, "Sidebar", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Billing", null, null, "/billing" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20111"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage users, roles, permissions, and trusted devices.", 60, "Group", true, false, "admin-iam", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Identity & Access", null, null, "/admin/iam" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10401"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20111"));

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "MenuItems");

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20105"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Permission catalog and access keys.", "LockPerson", true, false, "admin-permissions", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Permissions", null, null, "/admin/permissions" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20106"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Role catalog and assignments.", "AdminPanelSettings", true, false, "admin-roles", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Roles", null, null, "/admin/roles" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20108"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Trusted device review and revocation.", "Devices", true, false, "admin-user-devices", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("0194f700-0000-7000-8000-000000000001"), "User Devices", null, null, "/admin/user-devices" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c20109"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Create accounts and manage lifecycle.", "Group", true, false, "admin-users", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Users", null, null, "/admin/users" }
                });
        }
    }
}
