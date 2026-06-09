using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BT.Persistence.Migrations.HR
{
    /// <inheritdoc />
    public partial class Init_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DepartmentId", "Email", "FirstName", "IdNumber", "LastName", "ManagerId", "Number", "PhoneNumber", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0194f800-0000-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", new Guid("0194f800-0000-7000-8000-000000000100"), "beau.koelpin.1@basetemplate.local", "Beau", "14042262", "Koelpin", new Guid("00000000-0000-0000-0000-000000000000"), "EMP-001", "+254788029249", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("0194f800-0000-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", new Guid("0194f800-0000-7000-8000-000000000200"), "murphy.greenfelder.2@basetemplate.local", "Murphy", "67532424", "Greenfelder", new Guid("0194f800-0000-7000-8000-000000000001"), "EMP-002", "+254761722609", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("0194f800-0000-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", new Guid("0194f800-0000-7000-8000-000000000300"), "bud.gorczany.3@basetemplate.local", "Bud", "76945774", "Gorczany", new Guid("0194f800-0000-7000-8000-000000000001"), "EMP-003", "+254774633490", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("0194f800-0000-7000-8000-000000000004"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", new Guid("0194f800-0000-7000-8000-000000000400"), "aurelio.herman.4@basetemplate.local", "Aurelio", "98151865", "Herman", new Guid("0194f800-0000-7000-8000-000000000001"), "EMP-004", "+254756631912", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("0194f800-0000-7000-8000-000000000005"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", new Guid("0194f800-0000-7000-8000-000000000500"), "claudie.bogisich.5@basetemplate.local", "Claudie", "31186878", "Bogisich", new Guid("0194f800-0000-7000-8000-000000000001"), "EMP-005", "+254782832156", new Guid("00000000-0000-0000-0000-000000000000"), null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Number",
                table: "Employees",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
