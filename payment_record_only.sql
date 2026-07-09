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
