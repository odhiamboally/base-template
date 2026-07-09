ALTER TABLE [PaymentRecords] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
ALTER TABLE [PaymentRecords] ADD [DeletedAt] datetimeoffset NULL;
ALTER TABLE [PaymentRecords] ADD [DeletedBy] nvarchar(max) NULL;
GO
