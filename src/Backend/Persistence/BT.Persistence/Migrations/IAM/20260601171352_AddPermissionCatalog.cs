using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BT.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class AddPermissionCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Context = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
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
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Context", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Resource", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10101"), "view", "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "View application users.", true, false, "users.view", "users", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10102"), "create", "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Create application users.", true, false, "users.create", "users", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10103"), "edit", "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Update application users.", true, false, "users.edit", "users", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10104"), "deactivate", "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Deactivate application users.", true, false, "users.deactivate", "users", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10105"), "manage_roles", "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Manage user role assignments.", true, false, "users.manage_roles", "users", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10106"), "manage_permissions", "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Manage direct user permissions.", true, false, "users.manage_permissions", "users", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10201"), "view", "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "View platform roles.", true, false, "roles.view", "roles", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10202"), "create", "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Create platform roles.", true, false, "roles.create", "roles", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10203"), "edit", "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Update platform roles.", true, false, "roles.edit", "roles", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10204"), "delete", "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Delete platform roles.", true, false, "roles.delete", "roles", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10205"), "manage_permissions", "IAM", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Manage role permission assignments.", true, false, "roles.manage_permissions", "roles", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10301"), "view", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "View departments.", true, false, "departments.view", "departments", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10302"), "create", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Create departments.", true, false, "departments.create", "departments", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10303"), "edit", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Update departments.", true, false, "departments.edit", "departments", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10304"), "delete", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Delete departments.", true, false, "departments.delete", "departments", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10401"), "view", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "View employees.", true, false, "employees.view", "employees", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10402"), "create", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Create employees.", true, false, "employees.create", "employees", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10403"), "edit", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Update employees.", true, false, "employees.edit", "employees", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10404"), "delete", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Delete employees.", true, false, "employees.delete", "employees", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10501"), "view", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "View customers.", true, false, "customers.view", "customers", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10502"), "create", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Create customers.", true, false, "customers.create", "customers", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10503"), "edit", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Update customers.", true, false, "customers.edit", "customers", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10504"), "delete", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Delete customers.", true, false, "customers.delete", "customers", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10601"), "view", "Platform", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "View menu catalog.", true, false, "menus.view", "menus", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10602"), "create", "Platform", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Create menu items.", true, false, "menus.create", "menus", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10603"), "edit", "Platform", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Update menu items.", true, false, "menus.edit", "menus", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10604"), "delete", "Platform", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Delete menu items.", true, false, "menus.delete", "menus", new Guid("00000000-0000-0000-0000-000000000000"), null, null }
                });

            migrationBuilder.CreateIndex(
                name: "UX_Permissions_Key",
                table: "Permissions",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Permissions");
        }
    }
}
