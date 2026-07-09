BEGIN TRANSACTION;
ALTER TABLE [FailedMessages] ADD [DeletedAt] datetimeoffset NULL;

ALTER TABLE [FailedMessages] ADD [DeletedBy] nvarchar(max) NULL;

ALTER TABLE [FailedMessages] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory_Shared] ([MigrationId], [ProductVersion])
VALUES (N'20260601080046_AddFailedMessageSoftDelete', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [LookupCatalogTypes] (
    [Id] int NOT NULL,
    [Key] nvarchar(100) NOT NULL,
    [Label] nvarchar(120) NOT NULL,
    [Description] nvarchar(250) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetimeoffset NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_LookupCatalogTypes] PRIMARY KEY ([Id])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'DeletedAt', N'DeletedBy', N'Description', N'IsActive', N'IsDeleted', N'Key', N'Label') AND [object_id] = OBJECT_ID(N'[LookupCatalogTypes]'))
    SET IDENTITY_INSERT [LookupCatalogTypes] ON;
INSERT INTO [LookupCatalogTypes] ([Id], [DeletedAt], [DeletedBy], [Description], [IsActive], [IsDeleted], [Key], [Label])
VALUES (1, NULL, NULL, N'Lifecycle statuses available to customer records.', CAST(1 AS bit), CAST(0 AS bit), N'CustomerStatus', N'Customer statuses'),
(2, NULL, NULL, N'Classification values used when creating and segmenting customers.', CAST(1 AS bit), CAST(0 AS bit), N'CustomerType', N'Customer types'),
(3, NULL, NULL, N'Relationship labels used for customer directors and signatories.', CAST(1 AS bit), CAST(0 AS bit), N'DirectorRelationType', N'Director relation types'),
(4, NULL, NULL, N'Operational statuses for failed message tracking.', CAST(1 AS bit), CAST(0 AS bit), N'FailedMessageStatus', N'Failed message statuses'),
(5, NULL, NULL, N'Identity document types used across onboarding and verification.', CAST(1 AS bit), CAST(0 AS bit), N'IdentificationType', N'Identification types'),
(6, NULL, NULL, N'Business line values used by banking and reporting flows.', CAST(1 AS bit), CAST(0 AS bit), N'LineOfBusiness', N'Lines of business'),
(7, NULL, NULL, N'Primary customer segmentation values.', CAST(1 AS bit), CAST(0 AS bit), N'SegmentType', N'Segment types'),
(8, NULL, NULL, N'Secondary customer segmentation values.', CAST(1 AS bit), CAST(0 AS bit), N'SubSegmentType', N'Sub-segment types');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'DeletedAt', N'DeletedBy', N'Description', N'IsActive', N'IsDeleted', N'Key', N'Label') AND [object_id] = OBJECT_ID(N'[LookupCatalogTypes]'))
    SET IDENTITY_INSERT [LookupCatalogTypes] OFF;

CREATE UNIQUE INDEX [IX_LookupCatalogTypes_Key] ON [LookupCatalogTypes] ([Key]);

INSERT INTO [__EFMigrationsHistory_Shared] ([MigrationId], [ProductVersion])
VALUES (N'20260603003203_AddLookupCatalogTypes', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
DROP INDEX [IX_LookupCatalogTypes_Key] ON [LookupCatalogTypes];

DROP INDEX [IX_SubSegmentTypeLookup_Code] ON [Lkp_SubSegmentTypes];

DROP INDEX [IX_SegmentTypeLookup_Code] ON [Lkp_SegmentTypes];

DROP INDEX [IX_LineOfBusinessLookup_Code] ON [Lkp_LineOfBusiness];

DROP INDEX [IX_IdentificationTypeLookup_Code] ON [Lkp_IdentificationTypes];

DROP INDEX [IX_FailedMessageStatusLookup_Code] ON [Lkp_FailedMessageStatuses];

DROP INDEX [IX_DirectorRelationTypeLookup_Code] ON [Lkp_DirectorRelationTypes];

DROP INDEX [IX_CustomerTypeLookup_Code] ON [Lkp_CustomerTypes];

DROP INDEX [IX_CustomerStatusLookup_Code] ON [Lkp_CustomerStatuses];

ALTER TABLE [LookupCatalogTypes] ADD [TenantId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [Lkp_SubSegmentTypes] ADD [TenantId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [Lkp_SegmentTypes] ADD [TenantId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [Lkp_LineOfBusiness] ADD [TenantId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [Lkp_IdentificationTypes] ADD [TenantId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [Lkp_FailedMessageStatuses] ADD [TenantId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [Lkp_DirectorRelationTypes] ADD [TenantId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [Lkp_CustomerTypes] ADD [TenantId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE [Lkp_CustomerStatuses] ADD [TenantId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

UPDATE LookupCatalogTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE Lkp_CustomerStatuses SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE Lkp_CustomerTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE Lkp_DirectorRelationTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE Lkp_FailedMessageStatuses SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE Lkp_IdentificationTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE Lkp_LineOfBusiness SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE Lkp_SegmentTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';
UPDATE Lkp_SubSegmentTypes SET TenantId = '0194f700-0000-7000-8000-000000000001' WHERE TenantId = '00000000-0000-0000-0000-000000000000';

UPDATE [Lkp_CustomerStatuses] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Lkp_CustomerStatuses] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [Lkp_CustomerStatuses] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [Lkp_CustomerStatuses] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


UPDATE [Lkp_CustomerStatuses] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


UPDATE [Lkp_CustomerTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Lkp_CustomerTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [Lkp_CustomerTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [Lkp_CustomerTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


UPDATE [Lkp_CustomerTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


UPDATE [Lkp_DirectorRelationTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Lkp_DirectorRelationTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [Lkp_DirectorRelationTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [Lkp_DirectorRelationTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


UPDATE [Lkp_DirectorRelationTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


UPDATE [Lkp_FailedMessageStatuses] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Lkp_FailedMessageStatuses] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [Lkp_FailedMessageStatuses] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [Lkp_IdentificationTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Lkp_IdentificationTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [Lkp_IdentificationTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [Lkp_IdentificationTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


UPDATE [Lkp_LineOfBusiness] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Lkp_LineOfBusiness] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [Lkp_LineOfBusiness] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [Lkp_LineOfBusiness] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


UPDATE [Lkp_LineOfBusiness] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


UPDATE [Lkp_LineOfBusiness] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 6;
SELECT @@ROWCOUNT;


UPDATE [Lkp_LineOfBusiness] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 7;
SELECT @@ROWCOUNT;


UPDATE [Lkp_LineOfBusiness] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 8;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 6;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 7;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SubSegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SubSegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SubSegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SubSegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SubSegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SubSegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 6;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SubSegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 7;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SubSegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 8;
SELECT @@ROWCOUNT;


UPDATE [Lkp_SubSegmentTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 9;
SELECT @@ROWCOUNT;


UPDATE [LookupCatalogTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [LookupCatalogTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [LookupCatalogTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [LookupCatalogTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


UPDATE [LookupCatalogTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


UPDATE [LookupCatalogTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 6;
SELECT @@ROWCOUNT;


UPDATE [LookupCatalogTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 7;
SELECT @@ROWCOUNT;


UPDATE [LookupCatalogTypes] SET [TenantId] = '0194f700-0000-7000-8000-000000000001'
WHERE [Id] = 8;
SELECT @@ROWCOUNT;


CREATE UNIQUE INDEX [UX_LookupCatalogTypes_TenantId_Key] ON [LookupCatalogTypes] ([TenantId], [Key]);

CREATE UNIQUE INDEX [UX_SubSegmentTypeLookup_TenantId_Code] ON [Lkp_SubSegmentTypes] ([TenantId], [Code]);

CREATE UNIQUE INDEX [UX_SegmentTypeLookup_TenantId_Code] ON [Lkp_SegmentTypes] ([TenantId], [Code]);

CREATE UNIQUE INDEX [UX_LineOfBusinessLookup_TenantId_Code] ON [Lkp_LineOfBusiness] ([TenantId], [Code]);

CREATE UNIQUE INDEX [UX_IdentificationTypeLookup_TenantId_Code] ON [Lkp_IdentificationTypes] ([TenantId], [Code]);

CREATE UNIQUE INDEX [UX_FailedMessageStatusLookup_TenantId_Code] ON [Lkp_FailedMessageStatuses] ([TenantId], [Code]);

CREATE UNIQUE INDEX [UX_DirectorRelationTypeLookup_TenantId_Code] ON [Lkp_DirectorRelationTypes] ([TenantId], [Code]);

CREATE UNIQUE INDEX [UX_CustomerTypeLookup_TenantId_Code] ON [Lkp_CustomerTypes] ([TenantId], [Code]);

CREATE UNIQUE INDEX [UX_CustomerStatusLookup_TenantId_Code] ON [Lkp_CustomerStatuses] ([TenantId], [Code]);

INSERT INTO [__EFMigrationsHistory_Shared] ([MigrationId], [ProductVersion])
VALUES (N'20260609014012_AddTenantScopeToSharedLookups', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
DROP INDEX [IX_OutboxState_BusName_Created] ON [OutboxState];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxState]') AND [c].[name] = N'BusName');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [OutboxState] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [OutboxState] DROP COLUMN [BusName];

CREATE TABLE [PaymentRecords] (
    [Id] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [CustomerReference] nvarchar(100) NOT NULL,
    [Provider] nvarchar(50) NOT NULL,
    [Status] int NOT NULL,
    [ProviderReference] nvarchar(200) NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedAt] datetimeoffset NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PaymentRecords] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory_Shared] ([MigrationId], [ProductVersion])
VALUES (N'20260708162759_AddPaymentRecord', N'10.0.8');

COMMIT;
GO

