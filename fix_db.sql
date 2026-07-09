-- Fix missing DisplayOrder column on MenuItems
IF COL_LENGTH('MenuItems', 'DisplayOrder') IS NULL
BEGIN
    ALTER TABLE [MenuItems] ADD [DisplayOrder] int NOT NULL DEFAULT 0;
END
GO

-- Create PaymentRecords table if it does not exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentRecords]') AND type in (N'U'))
BEGIN
    CREATE TABLE [PaymentRecords] (
        [Id] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [CustomerReference] nvarchar(100) NOT NULL,
        [Provider] nvarchar(50) NOT NULL,
        [ProviderReference] nvarchar(200) NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeletedAt] datetimeoffset NULL,
        [DeletedBy] nvarchar(100) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PaymentRecords] PRIMARY KEY ([Id])
    );
END
GO
