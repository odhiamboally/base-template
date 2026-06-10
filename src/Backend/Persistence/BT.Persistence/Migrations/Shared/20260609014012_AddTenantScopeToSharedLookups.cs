using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BT.Persistence.Migrations.Shared
{
    /// <inheritdoc />
    public partial class AddTenantScopeToSharedLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LookupCatalogTypes_Key",
                table: "LookupCatalogTypes");

            migrationBuilder.DropIndex(
                name: "IX_SubSegmentTypeLookup_Code",
                table: "Lkp_SubSegmentTypes");

            migrationBuilder.DropIndex(
                name: "IX_SegmentTypeLookup_Code",
                table: "Lkp_SegmentTypes");

            migrationBuilder.DropIndex(
                name: "IX_LineOfBusinessLookup_Code",
                table: "Lkp_LineOfBusiness");

            migrationBuilder.DropIndex(
                name: "IX_IdentificationTypeLookup_Code",
                table: "Lkp_IdentificationTypes");

            migrationBuilder.DropIndex(
                name: "IX_FailedMessageStatusLookup_Code",
                table: "Lkp_FailedMessageStatuses");

            migrationBuilder.DropIndex(
                name: "IX_DirectorRelationTypeLookup_Code",
                table: "Lkp_DirectorRelationTypes");

            migrationBuilder.DropIndex(
                name: "IX_CustomerTypeLookup_Code",
                table: "Lkp_CustomerTypes");

            migrationBuilder.DropIndex(
                name: "IX_CustomerStatusLookup_Code",
                table: "Lkp_CustomerStatuses");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "LookupCatalogTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Lkp_SubSegmentTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Lkp_SegmentTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Lkp_LineOfBusiness",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Lkp_IdentificationTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Lkp_FailedMessageStatuses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Lkp_DirectorRelationTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Lkp_CustomerTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Lkp_CustomerStatuses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE LookupCatalogTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                UPDATE Lkp_CustomerStatuses SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                UPDATE Lkp_CustomerTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                UPDATE Lkp_DirectorRelationTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                UPDATE Lkp_FailedMessageStatuses SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                UPDATE Lkp_IdentificationTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                UPDATE Lkp_LineOfBusiness SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                UPDATE Lkp_SegmentTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                UPDATE Lkp_SubSegmentTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.UpdateData(
                table: "Lkp_CustomerStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_CustomerStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_CustomerStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_CustomerStatuses",
                keyColumn: "Id",
                keyValue: 4,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_CustomerStatuses",
                keyColumn: "Id",
                keyValue: 5,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_CustomerTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_CustomerTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_CustomerTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_CustomerTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_CustomerTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_DirectorRelationTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_DirectorRelationTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_DirectorRelationTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_DirectorRelationTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_DirectorRelationTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_FailedMessageStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_FailedMessageStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_FailedMessageStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_IdentificationTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_IdentificationTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_IdentificationTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_IdentificationTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_LineOfBusiness",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_LineOfBusiness",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_LineOfBusiness",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_LineOfBusiness",
                keyColumn: "Id",
                keyValue: 4,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_LineOfBusiness",
                keyColumn: "Id",
                keyValue: 5,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_LineOfBusiness",
                keyColumn: "Id",
                keyValue: 6,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_LineOfBusiness",
                keyColumn: "Id",
                keyValue: 7,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_LineOfBusiness",
                keyColumn: "Id",
                keyValue: 8,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_SegmentTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_SegmentTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_SegmentTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_SegmentTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_SegmentTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_SegmentTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_SegmentTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Lkp_SubSegmentTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Lkp_SubSegmentTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Lkp_SubSegmentTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Lkp_SubSegmentTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "TenantId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Lkp_SubSegmentTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "TenantId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Lkp_SubSegmentTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "TenantId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Lkp_SubSegmentTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "TenantId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Lkp_SubSegmentTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "TenantId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Lkp_SubSegmentTypes",
                keyColumn: "Id",
                keyValue: 9,
                column: "TenantId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "LookupCatalogTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "LookupCatalogTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "LookupCatalogTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "LookupCatalogTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "LookupCatalogTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "LookupCatalogTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "LookupCatalogTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.UpdateData(
                table: "LookupCatalogTypes",
                keyColumn: "Id",
                keyValue: 8,
                column: "TenantId",
                value: new Guid("0194f700-0000-7000-8000-000000000001"));

            migrationBuilder.CreateIndex(
                name: "UX_LookupCatalogTypes_TenantId_Key",
                table: "LookupCatalogTypes",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_SubSegmentTypeLookup_TenantId_Code",
                table: "Lkp_SubSegmentTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_SegmentTypeLookup_TenantId_Code",
                table: "Lkp_SegmentTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_LineOfBusinessLookup_TenantId_Code",
                table: "Lkp_LineOfBusiness",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_IdentificationTypeLookup_TenantId_Code",
                table: "Lkp_IdentificationTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_FailedMessageStatusLookup_TenantId_Code",
                table: "Lkp_FailedMessageStatuses",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_DirectorRelationTypeLookup_TenantId_Code",
                table: "Lkp_DirectorRelationTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CustomerTypeLookup_TenantId_Code",
                table: "Lkp_CustomerTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CustomerStatusLookup_TenantId_Code",
                table: "Lkp_CustomerStatuses",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_LookupCatalogTypes_TenantId_Key",
                table: "LookupCatalogTypes");

            migrationBuilder.DropIndex(
                name: "UX_SubSegmentTypeLookup_TenantId_Code",
                table: "Lkp_SubSegmentTypes");

            migrationBuilder.DropIndex(
                name: "UX_SegmentTypeLookup_TenantId_Code",
                table: "Lkp_SegmentTypes");

            migrationBuilder.DropIndex(
                name: "UX_LineOfBusinessLookup_TenantId_Code",
                table: "Lkp_LineOfBusiness");

            migrationBuilder.DropIndex(
                name: "UX_IdentificationTypeLookup_TenantId_Code",
                table: "Lkp_IdentificationTypes");

            migrationBuilder.DropIndex(
                name: "UX_FailedMessageStatusLookup_TenantId_Code",
                table: "Lkp_FailedMessageStatuses");

            migrationBuilder.DropIndex(
                name: "UX_DirectorRelationTypeLookup_TenantId_Code",
                table: "Lkp_DirectorRelationTypes");

            migrationBuilder.DropIndex(
                name: "UX_CustomerTypeLookup_TenantId_Code",
                table: "Lkp_CustomerTypes");

            migrationBuilder.DropIndex(
                name: "UX_CustomerStatusLookup_TenantId_Code",
                table: "Lkp_CustomerStatuses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "LookupCatalogTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Lkp_SubSegmentTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Lkp_SegmentTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Lkp_LineOfBusiness");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Lkp_IdentificationTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Lkp_FailedMessageStatuses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Lkp_DirectorRelationTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Lkp_CustomerTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Lkp_CustomerStatuses");

            migrationBuilder.CreateIndex(
                name: "IX_LookupCatalogTypes_Key",
                table: "LookupCatalogTypes",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubSegmentTypeLookup_Code",
                table: "Lkp_SubSegmentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SegmentTypeLookup_Code",
                table: "Lkp_SegmentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LineOfBusinessLookup_Code",
                table: "Lkp_LineOfBusiness",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentificationTypeLookup_Code",
                table: "Lkp_IdentificationTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FailedMessageStatusLookup_Code",
                table: "Lkp_FailedMessageStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DirectorRelationTypeLookup_Code",
                table: "Lkp_DirectorRelationTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTypeLookup_Code",
                table: "Lkp_CustomerTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStatusLookup_Code",
                table: "Lkp_CustomerStatuses",
                column: "Code",
                unique: true);
        }
    }
}
