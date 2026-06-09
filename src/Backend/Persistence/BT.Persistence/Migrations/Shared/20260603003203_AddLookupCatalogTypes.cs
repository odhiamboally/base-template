using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BT.Persistence.Migrations.Shared
{
    /// <inheritdoc />
    public partial class AddLookupCatalogTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LookupCatalogTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupCatalogTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LookupCatalogTypes",
                columns: new[] { "Id", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label" },
                values: new object[,]
                {
                    { 1, null, null, "Lifecycle statuses available to customer records.", true, false, "CustomerStatus", "Customer statuses" },
                    { 2, null, null, "Classification values used when creating and segmenting customers.", true, false, "CustomerType", "Customer types" },
                    { 3, null, null, "Relationship labels used for customer directors and signatories.", true, false, "DirectorRelationType", "Director relation types" },
                    { 4, null, null, "Operational statuses for failed message tracking.", true, false, "FailedMessageStatus", "Failed message statuses" },
                    { 5, null, null, "Identity document types used across onboarding and verification.", true, false, "IdentificationType", "Identification types" },
                    { 6, null, null, "Business line values used by banking and reporting flows.", true, false, "LineOfBusiness", "Lines of business" },
                    { 7, null, null, "Primary customer segmentation values.", true, false, "SegmentType", "Segment types" },
                    { 8, null, null, "Secondary customer segmentation values.", true, false, "SubSegmentType", "Sub-segment types" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LookupCatalogTypes_Key",
                table: "LookupCatalogTypes",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LookupCatalogTypes");
        }
    }
}
