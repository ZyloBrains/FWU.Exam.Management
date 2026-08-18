SET XACT_ABORT ON;

BEGIN TRANSACTION;
BEGIN TRY

-- Add IsActive column (default true for existing records)
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260818183135_AddIsActiveToSubjectOffering'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SubjectOfferings' AND COLUMN_NAME = 'IsActive')
    BEGIN
        ALTER TABLE [SubjectOfferings] ADD [IsActive] bit NOT NULL CONSTRAINT [DF_SubjectOfferings_IsActive] DEFAULT (1);
    END;
END;

-- Recreate unique index with IsActive filter
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260818183135_AddIsActiveToSubjectOffering'
)
BEGIN
    DROP INDEX IF EXISTS [IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId] ON [SubjectOfferings];

    CREATE UNIQUE INDEX [IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId]
        ON [SubjectOfferings] ([CurriculumVersionId], [SubjectCatalogId], [ProgramId], [SemesterId])
        WHERE [IsActive] = 1;
END;

-- Record migration
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260818183135_AddIsActiveToSubjectOffering'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260818183135_AddIsActiveToSubjectOffering', N'10.0.0');
END;

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH;
