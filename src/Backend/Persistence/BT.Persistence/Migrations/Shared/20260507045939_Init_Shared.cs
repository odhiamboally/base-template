using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BT.Persistence.Migrations.Shared
{
    /// <inheritdoc />
    public partial class Init_Shared : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FailedMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ErrorStackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    FailedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxState",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Received = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiveCount = table.Column<int>(type: "int", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Consumed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Delivered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxState", x => x.Id);
                    table.UniqueConstraint("AK_InboxState_MessageId_ConsumerId", x => new { x.MessageId, x.ConsumerId });
                });

            migrationBuilder.CreateTable(
                name: "Lkp_CustomerStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lkp_CustomerStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lkp_CustomerTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lkp_CustomerTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lkp_DirectorRelationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lkp_DirectorRelationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lkp_FailedMessageStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lkp_FailedMessageStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lkp_IdentificationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lkp_IdentificationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lkp_LineOfBusiness",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lkp_LineOfBusiness", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lkp_SegmentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lkp_SegmentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lkp_SubSegmentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lkp_SubSegmentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxState",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Delivered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true),
                    BusName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxState", x => x.OutboxId);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnqueueTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Headers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InboxMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OutboxId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ResponseAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FaultAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.SequenceNumber);
                    table.ForeignKey(
                        name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                        columns: x => new { x.InboxMessageId, x.InboxConsumerId },
                        principalTable: "InboxState",
                        principalColumns: new[] { "MessageId", "ConsumerId" });
                    table.ForeignKey(
                        name: "FK_OutboxMessage_OutboxState_OutboxId",
                        column: x => x.OutboxId,
                        principalTable: "OutboxState",
                        principalColumn: "OutboxId");
                });

            migrationBuilder.InsertData(
                table: "Lkp_CustomerStatuses",
                columns: new[] { "Id", "Code", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "Draft", null, null, "Draft", 1, false },
                    { 2, "Active", null, null, "Active", 3, false },
                    { 3, "Suspended", null, null, "Suspended", 4, false },
                    { 4, "Closed", null, null, "Closed", 5, false },
                    { 5, "PendingApproval", null, null, "Pending Approval", 2, false }
                });

            migrationBuilder.InsertData(
                table: "Lkp_CustomerTypes",
                columns: new[] { "Id", "Code", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "Individual", null, null, "Individual", 1, false },
                    { 2, "Corporate", null, null, "Corporate", 2, false },
                    { 3, "Institutional", null, null, "Institutional", 3, false },
                    { 4, "SmallMediumEnterprise", null, null, "Small & Medium Enterprise", 4, false },
                    { 5, "Enterprise", null, null, "Enterprise", 5, false }
                });

            migrationBuilder.InsertData(
                table: "Lkp_DirectorRelationTypes",
                columns: new[] { "Id", "Code", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "Director", null, null, "Director", 1, false },
                    { 2, "Shareholder", null, null, "Shareholder", 2, false },
                    { 3, "Signatory", null, null, "Signatory", 3, false },
                    { 4, "BeneficialOwner", null, null, "Beneficial Owner", 4, false },
                    { 5, "Guarantor", null, null, "Guarantor", 5, false }
                });

            migrationBuilder.InsertData(
                table: "Lkp_FailedMessageStatuses",
                columns: new[] { "Id", "Code", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "Transient", null, null, "Transient", 1, false },
                    { 2, "Permanent", null, null, "Permanent", 2, false },
                    { 3, "ManualRetry", null, null, "Manual Retry", 3, false }
                });

            migrationBuilder.InsertData(
                table: "Lkp_IdentificationTypes",
                columns: new[] { "Id", "Code", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "CertificateOfIncorporation", null, null, "Certificate of Incorporation", 1, false },
                    { 2, "TIN", null, null, "Tax Identification Number", 2, false },
                    { 3, "BusinessLicense", null, null, "Business License", 3, false },
                    { 4, "CompanyRegistrationCertificate", null, null, "Company Registration Certificate", 4, false }
                });

            migrationBuilder.InsertData(
                table: "Lkp_LineOfBusiness",
                columns: new[] { "Id", "Code", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "Agriculture", null, null, "Agriculture", 1, false },
                    { 2, "Manufacturing", null, null, "Manufacturing", 2, false },
                    { 3, "Technology", null, null, "Technology", 3, false },
                    { 4, "FinancialServices", null, null, "Financial Services", 4, false },
                    { 5, "Retail", null, null, "Retail", 5, false },
                    { 6, "Services", null, null, "Services", 6, false },
                    { 7, "Proprietary", null, null, "Proprietary", 7, false },
                    { 8, "Trading", null, null, "Trading", 8, false }
                });

            migrationBuilder.InsertData(
                table: "Lkp_SegmentTypes",
                columns: new[] { "Id", "Code", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "PublicLimitedCompany", null, null, "Public Limited Company", 1, false },
                    { 2, "PrivateLimitedCompany", null, null, "Private Limited Company", 2, false },
                    { 3, "SoleProprietorship", null, null, "Sole Proprietorship", 3, false },
                    { 4, "Corporate", null, null, "Corporate", 4, false },
                    { 5, "Retail", null, null, "Retail", 5, false },
                    { 6, "SME", null, null, "Small & Medium Enterprise", 6, false },
                    { 7, "Individual", null, null, "Individual", 7, false }
                });

            migrationBuilder.InsertData(
                table: "Lkp_SubSegmentTypes",
                columns: new[] { "Id", "Code", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "Local", null, null, "Local", 1, false },
                    { 2, "Multinational", null, null, "Multinational", 2, false },
                    { 3, "GovernmentOwned", null, null, "Government Owned", 3, false },
                    { 4, "Partnership", null, null, "Partnership", 4, false },
                    { 5, "PrivateLimitedCompany", null, null, "Private Limited Company", 5, false },
                    { 6, "PublicLimitedCompany", null, null, "Public Limited Company", 6, false },
                    { 7, "SoleProprietorship", null, null, "Sole Proprietorship", 7, false },
                    { 8, "NGO", null, null, "Non-Governmental Organisation", 8, false },
                    { 9, "Individual", null, null, "Individual", 9, false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Name",
                table: "EmailTemplates",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FailedMessages_MessageId",
                table: "FailedMessages",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_FailedMessages_Status_IsResolved_FailedAt",
                table: "FailedMessages",
                columns: new[] { "Status", "IsResolved", "FailedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxState_Delivered",
                table: "InboxState",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerStatusLookup_Code",
                table: "Lkp_CustomerStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTypeLookup_Code",
                table: "Lkp_CustomerTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DirectorRelationTypeLookup_Code",
                table: "Lkp_DirectorRelationTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FailedMessageStatusLookup_Code",
                table: "Lkp_FailedMessageStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentificationTypeLookup_Code",
                table: "Lkp_IdentificationTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LineOfBusinessLookup_Code",
                table: "Lkp_LineOfBusiness",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SegmentTypeLookup_Code",
                table: "Lkp_SegmentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubSegmentTypeLookup_Code",
                table: "Lkp_SubSegmentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                table: "OutboxMessage",
                column: "EnqueueTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                table: "OutboxMessage",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId", "SequenceNumber" },
                unique: true,
                filter: "[InboxMessageId] IS NOT NULL AND [InboxConsumerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "OutboxId", "SequenceNumber" },
                unique: true,
                filter: "[OutboxId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxState_BusName_Created",
                table: "OutboxState",
                columns: new[] { "BusName", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxState_Created",
                table: "OutboxState",
                column: "Created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "FailedMessages");

            migrationBuilder.DropTable(
                name: "Lkp_CustomerStatuses");

            migrationBuilder.DropTable(
                name: "Lkp_CustomerTypes");

            migrationBuilder.DropTable(
                name: "Lkp_DirectorRelationTypes");

            migrationBuilder.DropTable(
                name: "Lkp_FailedMessageStatuses");

            migrationBuilder.DropTable(
                name: "Lkp_IdentificationTypes");

            migrationBuilder.DropTable(
                name: "Lkp_LineOfBusiness");

            migrationBuilder.DropTable(
                name: "Lkp_SegmentTypes");

            migrationBuilder.DropTable(
                name: "Lkp_SubSegmentTypes");

            migrationBuilder.DropTable(
                name: "OutboxMessage");

            migrationBuilder.DropTable(
                name: "InboxState");

            migrationBuilder.DropTable(
                name: "OutboxState");
        }
    }
}
