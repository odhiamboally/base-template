using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BT.Persistence.Migrations.HR
{
    /// <inheritdoc />
    public partial class AddEmployeeNumberSequencesAndCleanEmployeeSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [Customers]
                SET [RelationshipManagerId] = CASE [RelationshipManagerId]
                    WHEN '0194f800-0000-7000-8000-000000000004' THEN '0194f800-0000-7000-8000-000000000001'
                    WHEN '0194f800-0000-7000-8000-000000000005' THEN '0194f800-0000-7000-8000-000000000002'
                    ELSE [RelationshipManagerId]
                END
                WHERE [RelationshipManagerId] IN (
                    '0194f800-0000-7000-8000-000000000004',
                    '0194f800-0000-7000-8000-000000000005'
                );
                """);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000005"));

            migrationBuilder.CreateTable(
                name: "EmployeeNumberSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeNumberSequences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeNumberSequences_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000001"),
                columns: new[] { "Email", "FirstName", "LastName", "PhoneNationalNumber", "PhoneNumber" },
                values: new object[] { "aamodhiambo@gmail.com", "Alex", "Odhiambo", "798980115", "+254798980115" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000002"),
                columns: new[] { "Email", "FirstName", "LastName", "PhoneNationalNumber", "PhoneNumber" },
                values: new object[] { "allan.alex0803@gmail.com", "Allan", "Alex", "700057578", "+254700057578" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000003"),
                columns: new[] { "Email", "FirstName", "LastName", "PhoneNationalNumber", "PhoneNumber" },
                values: new object[] { "omitolaura469@gmail.com", "Laura", "Omito", "719423686", "+254719423686" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeNumberSequences_DepartmentId_Year",
                table: "EmployeeNumberSequences",
                columns: new[] { "DepartmentId", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeNumberSequences");

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000001"),
                columns: new[] { "Email", "FirstName", "LastName", "PhoneNationalNumber", "PhoneNumber" },
                values: new object[] { "beau.koelpin.1@basetemplate.local", "Beau", "Koelpin", "788029249", "+254788029249" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000002"),
                columns: new[] { "Email", "FirstName", "LastName", "PhoneNationalNumber", "PhoneNumber" },
                values: new object[] { "murphy.greenfelder.2@basetemplate.local", "Murphy", "Greenfelder", "761722609", "+254761722609" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("0194f800-0000-7000-8000-000000000003"),
                columns: new[] { "Email", "FirstName", "LastName", "PhoneNationalNumber", "PhoneNumber" },
                values: new object[] { "bud.gorczany.3@basetemplate.local", "Bud", "Gorczany", "774633490", "+254774633490" });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "CountryCode", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Email", "FirstName", "IdNumber", "IsDeleted", "LastName", "ManagerId", "Number", "PhoneNationalNumber", "PhoneNumber", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0194f800-0000-7000-8000-000000000004"), "+254", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, new Guid("0194f800-0000-7000-8000-000000000400"), "aurelio.herman.4@basetemplate.local", "Aurelio", "98151865", false, "Herman", new Guid("0194f800-0000-7000-8000-000000000001"), "EMP-004", "756631912", "+254756631912", new Guid("00000000-0000-0000-0000-000000000000"), null, null },
                    { new Guid("0194f800-0000-7000-8000-000000000005"), "+254", new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, new Guid("0194f800-0000-7000-8000-000000000500"), "claudie.bogisich.5@basetemplate.local", "Claudie", "31186878", false, "Bogisich", new Guid("0194f800-0000-7000-8000-000000000001"), "EMP-005", "782832156", "+254782832156", new Guid("00000000-0000-0000-0000-000000000000"), null, null }
                });
        }
    }
}
