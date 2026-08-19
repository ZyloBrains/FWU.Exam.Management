--==========================================================================
-- PRODUCTION MIGRATION SCRIPT
-- Last 5 migrations: 20260816 - 20260818
-- Database: SQL Server

-- IMPORTANT:
--  - Run inside a TRANSACTION. If anything fails, ROLLBACK.
--  - Take a FULL DATABASE BACKUP before executing.
--  - Stop the application before running.
--  - Run each step in order; do NOT skip steps.
--==========================================================================

BEGIN TRANSACTION;

BEGIN TRY

-- =========================================================================
-- MIGRATION 1: AddTenantScopingToSubjectCatalog (20260816152655)
-- Adds TenantId column to SubjectCatalogs with FK to Tenants
-- =========================================================================
PRINT '--- Migration 1: AddTenantScopingToSubjectCatalog ---';

-- 1a. Add TenantId column (nullable initially)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('SubjectCatalogs') AND name = 'TenantId'
)
BEGIN
    ALTER TABLE [SubjectCatalogs] ADD [TenantId] [int] NULL;
    PRINT '  + Added TenantId column to SubjectCatalogs';
END
ELSE
    PRINT '  ~ TenantId column already exists on SubjectCatalogs, skipping.';

-- 1b. Backfill TenantId for existing rows (set to first tenant)
IF EXISTS (SELECT 1 FROM [Tenants])
BEGIN
    UPDATE sc
    SET sc.[TenantId] = (SELECT MIN([Id]) FROM [Tenants])
    FROM [SubjectCatalogs] sc
    WHERE sc.[TenantId] IS NULL;
    PRINT '  + Backfilled TenantId on SubjectCatalogs';
END
ELSE
    PRINT '  ! WARNING: No rows in Tenants table. TenantId remains NULL on SubjectCatalogs.';

-- 1c. Create index
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_SubjectCatalogs_TenantId' AND object_id = OBJECT_ID('SubjectCatalogs')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SubjectCatalogs_TenantId]
        ON [SubjectCatalogs] ([TenantId]);
    PRINT '  + Created index IX_SubjectCatalogs_TenantId';
END
ELSE
    PRINT '  ~ Index IX_SubjectCatalogs_TenantId already exists, skipping.';

-- 1d. Add FK constraint
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_SubjectCatalogs_Tenants_TenantId'
)
BEGIN
    ALTER TABLE [SubjectCatalogs]
        ADD CONSTRAINT [FK_SubjectCatalogs_Tenants_TenantId]
        FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id])
        ON DELETE RESTRICT;
    PRINT '  + Added FK_SubjectCatalogs_Tenants_TenantId';
END
ELSE
    PRINT '  ~ FK_SubjectCatalogs_Tenants_TenantId already exists, skipping.';

-- 1e. Record migration
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = '20260816152655_AddTenantScopingToSubjectCatalog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260816152655_AddTenantScopingToSubjectCatalog', '10.0.7');
    PRINT '  + Recorded migration in __EFMigrationsHistory';
END

PRINT '--- Migration 1 complete ---';
PRINT '';

-- =========================================================================
-- MIGRATION 2: SemesterInstances (20260817024022)
-- Creates SemesterInstances table, migrates data, restructures FKs
-- *** THIS IS A BREAKING MIGRATION ***
-- =========================================================================
PRINT '--- Migration 2: SemesterInstances ---';

-- 2a. Create SemesterInstances table
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'SemesterInstances'
)
BEGIN
    CREATE TABLE [SemesterInstances] (
        [Id]             INT            IDENTITY(1,1) NOT NULL,
        [TenantId]       INT            NOT NULL,
        [SemesterId]     INT            NOT NULL,
        [AcademicYearId] INT            NOT NULL,
        [ProgramId]      INT            NOT NULL,
        [StartDate]      DATETIME2      NOT NULL,
        [EndDate]        DATETIME2      NOT NULL,
        [Remark]         NVARCHAR(50)   NULL,
        CONSTRAINT [PK_SemesterInstances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SemesterInstances_AcademicYears_AcademicYearId]
            FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE RESTRICT,
        CONSTRAINT [FK_SemesterInstances_Programs_ProgramId]
            FOREIGN KEY ([ProgramId]) REFERENCES [Programs] ([Id]) ON DELETE RESTRICT,
        CONSTRAINT [FK_SemesterInstances_Semesters_SemesterId]
            FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([Id]) ON DELETE RESTRICT,
        CONSTRAINT [FK_SemesterInstances_Tenants_TenantId]
            FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE RESTRICT
    );
    PRINT '  + Created SemesterInstances table';
END
ELSE
    PRINT '  ~ SemesterInstances table already exists, skipping.';

-- 2b. Create indexes on SemesterInstances
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_SemesterInstances_AcademicYearId' AND object_id = OBJECT_ID('SemesterInstances')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SemesterInstances_AcademicYearId]
        ON [SemesterInstances] ([AcademicYearId]);
    PRINT '  + Created index IX_SemesterInstances_AcademicYearId';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_SemesterInstances_ProgramId' AND object_id = OBJECT_ID('SemesterInstances')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SemesterInstances_ProgramId]
        ON [SemesterInstances] ([ProgramId]);
    PRINT '  + Created index IX_SemesterInstances_ProgramId';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_SemesterInstances_SemesterId_AcademicYearId_ProgramId' AND object_id = OBJECT_ID('SemesterInstances')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_SemesterInstances_SemesterId_AcademicYearId_ProgramId]
        ON [SemesterInstances] ([SemesterId], [AcademicYearId], [ProgramId]);
    PRINT '  + Created unique index IX_SemesterInstances_SemesterId_AcademicYearId_ProgramId';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_SemesterInstances_TenantId' AND object_id = OBJECT_ID('SemesterInstances')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SemesterInstances_TenantId]
        ON [SemesterInstances] ([TenantId]);
    PRINT '  + Created index IX_SemesterInstances_TenantId';
END

-- 2c. Drop FKs that reference Semesters from child tables
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SemesterEnrollments_Semesters_SemesterId')
BEGIN
    ALTER TABLE [SemesterEnrollments] DROP CONSTRAINT [FK_SemesterEnrollments_Semesters_SemesterId];
    PRINT '  + Dropped FK_SemesterEnrollments_Semesters_SemesterId';
END

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ExamSchedules_Semesters_SemesterId')
BEGIN
    ALTER TABLE [ExamSchedules] DROP CONSTRAINT [FK_ExamSchedules_Semesters_SemesterId];
    PRINT '  + Dropped FK_ExamSchedules_Semesters_SemesterId';
END

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ExamSchedules_AcademicYears_AcademicYearId')
BEGIN
    ALTER TABLE [ExamSchedules] DROP CONSTRAINT [FK_ExamSchedules_AcademicYears_AcademicYearId];
    PRINT '  + Dropped FK_ExamSchedules_AcademicYears_AcademicYearId';
END

-- 2d. DATA MIGRATION: Populate SemesterInstances from existing data
DECLARE @TenantId INT = 1;

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SemesterInstances')
AND NOT EXISTS (SELECT 1 FROM [SemesterInstances])
BEGIN
    INSERT INTO [SemesterInstances] ([TenantId], [SemesterId], [AcademicYearId], [ProgramId], [StartDate], [EndDate])
    SELECT @TenantId, s.[Id], s.[AcademicYearId], ps.[ProgramId], s.[StartDate], s.[EndDate]
    FROM [Semesters] s
    INNER JOIN [ProgramSemesters] ps ON ps.[SemesterId] = s.[Id]
    WHERE s.[AcademicYearId] > 0
      AND ps.[ProgramId] > 0
      AND ps.[IsActive] = 1;

    PRINT '  + Inserted SemesterInstances from Semesters + ProgramSemesters: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows';

    INSERT INTO [SemesterInstances] ([TenantId], [SemesterId], [AcademicYearId], [ProgramId], [StartDate], [EndDate])
    SELECT @TenantId, es.[SemesterId], es.[AcademicYearId], es.[ProgramId], s.[StartDate], s.[EndDate]
    FROM [ExamSchedules] es
    INNER JOIN [Semesters] s ON es.[SemesterId] = s.[Id]
    WHERE s.[AcademicYearId] > 0
      AND NOT EXISTS (
          SELECT 1 FROM [SemesterInstances] si
          WHERE si.[SemesterId] = es.[SemesterId]
            AND si.[AcademicYearId] = es.[AcademicYearId]
            AND si.[ProgramId] = es.[ProgramId]);

    PRINT '  + Inserted additional SemesterInstances from ExamSchedules: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows';
END
ELSE
    PRINT '  ~ SemesterInstances already has data, skipping data migration.';

-- 2e. Update SemesterEnrollments: set SemesterId = matching SemesterInstance Id
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('SemesterEnrollments') AND name = 'SemesterId'
)
BEGIN
    UPDATE se
    SET se.[SemesterId] = si.[Id]
    FROM [SemesterEnrollments] se
    INNER JOIN [Semesters] s ON se.[SemesterId] = s.[Id]
    INNER JOIN [StudentAdmissions] sa ON se.[StudentAdmissionId] = sa.[Id]
    INNER JOIN [SemesterInstances] si
        ON si.[SemesterId] = s.[Id]
        AND si.[AcademicYearId] = s.[AcademicYearId]
        AND si.[ProgramId] = sa.[ProgramsId]
    WHERE sa.[ProgramsId] IS NOT NULL AND sa.[ProgramsId] > 0;
    PRINT '  + Updated SemesterEnrollments.SemesterId to SemesterInstance Ids: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows';
END

-- 2f. Update ExamSchedules: set SemesterId = matching SemesterInstance Id
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('ExamSchedules') AND name = 'SemesterId'
)
BEGIN
    UPDATE es
    SET es.[SemesterId] = si.[Id]
    FROM [ExamSchedules] es
    INNER JOIN [SemesterInstances] si
        ON si.[SemesterId] = es.[SemesterId]
        AND si.[AcademicYearId] = es.[AcademicYearId]
        AND si.[ProgramId] = es.[ProgramId];
    PRINT '  + Updated ExamSchedules.SemesterId to SemesterInstance Ids: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows';
END

-- 2g. Drop FK and index on Semesters.AcademicYearId
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Semesters_AcademicYears_AcademicYearId')
BEGIN
    ALTER TABLE [Semesters] DROP CONSTRAINT [FK_Semesters_AcademicYears_AcademicYearId];
    PRINT '  + Dropped FK_Semesters_AcademicYears_AcademicYearId';
END

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Semesters_AcademicYearId' AND object_id = OBJECT_ID('Semesters'))
BEGIN
    DROP INDEX [IX_Semesters_AcademicYearId] ON [Semesters];
    PRINT '  + Dropped index IX_Semesters_AcademicYearId';
END

-- 2h. Drop columns from Semesters table
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Semesters') AND name = 'AcademicYearId')
BEGIN
    ALTER TABLE [Semesters] DROP COLUMN [AcademicYearId];
    PRINT '  + Dropped column AcademicYearId from Semesters';
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Semesters') AND name = 'EndDate')
BEGIN
    ALTER TABLE [Semesters] DROP COLUMN [EndDate];
    PRINT '  + Dropped column EndDate from Semesters';
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Semesters') AND name = 'StartDate')
BEGIN
    ALTER TABLE [Semesters] DROP COLUMN [StartDate];
    PRINT '  + Dropped column StartDate from Semesters';
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Semesters') AND name = 'Year')
BEGIN
    ALTER TABLE [Semesters] DROP COLUMN [Year];
    PRINT '  + Dropped column Year from Semesters';
END

-- 2i. Rename SemesterEnrollments.SemesterId -> SemesterInstanceId
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('SemesterEnrollments') AND name = 'SemesterId'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SemesterEnrollments_SemesterId' AND object_id = OBJECT_ID('SemesterEnrollments'))
        DROP INDEX [IX_SemesterEnrollments_SemesterId] ON [SemesterEnrollments];

    EXEC sp_rename 'SemesterEnrollments.SemesterId', 'SemesterInstanceId', 'COLUMN';
    PRINT '  + Renamed SemesterEnrollments.SemesterId -> SemesterInstanceId';
END

-- Rename index
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SemesterEnrollments_SemesterId' AND object_id = OBJECT_ID('SemesterEnrollments'))
BEGIN
    EXEC sp_rename 'IX_SemesterEnrollments_SemesterId', 'IX_SemesterEnrollments_SemesterInstanceId', 'INDEX';
    PRINT '  + Renamed index IX_SemesterEnrollments_SemesterId -> IX_SemesterEnrollments_SemesterInstanceId';
END

-- 2j. Add FK from SemesterEnrollments to SemesterInstances
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_SemesterEnrollments_SemesterInstances_SemesterInstanceId'
)
BEGIN
    ALTER TABLE [SemesterEnrollments]
        ADD CONSTRAINT [FK_SemesterEnrollments_SemesterInstances_SemesterInstanceId]
        FOREIGN KEY ([SemesterInstanceId]) REFERENCES [SemesterInstances] ([Id])
        ON DELETE RESTRICT;
    PRINT '  + Added FK_SemesterEnrollments_SemesterInstances_SemesterInstanceId';
END

-- 2k. Record migration
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = '20260817024022_SemesterInstances'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260817024022_SemesterInstances', '10.0.7');
    PRINT '  + Recorded migration in __EFMigrationsHistory';
END

PRINT '--- Migration 2 complete ---';
PRINT '';

-- =========================================================================
-- MIGRATION 3: TenantScopedAcademicYearAndSemesterInstanceProgramId
--                (20260817054948)
-- Drops AcademicYearId from ExamSchedules, renames SemesterId ->
-- SemesterInstanceId, adds TenantId/StartDate/EndDate to AcademicYears
-- *** THIS IS A BREAKING MIGRATION ***
-- =========================================================================
PRINT '--- Migration 3: TenantScopedAcademicYearAndSemesterInstanceProgramId ---';

-- 3a. Drop index on ExamSchedules.AcademicYearId (before dropping the column)
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExamSchedules_AcademicYearId' AND object_id = OBJECT_ID('ExamSchedules'))
BEGIN
    DROP INDEX [IX_ExamSchedules_AcademicYearId] ON [ExamSchedules];
    PRINT '  + Dropped index IX_ExamSchedules_AcademicYearId';
END

-- 3b. Drop index on AcademicYears.AcademicYearCode (before recreating with TenantId)
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AcademicYears_AcademicYearCode' AND object_id = OBJECT_ID('AcademicYears'))
BEGIN
    DROP INDEX [IX_AcademicYears_AcademicYearCode] ON [AcademicYears];
    PRINT '  + Dropped index IX_AcademicYears_AcademicYearCode';
END

-- 3c. Drop AcademicYearId column from ExamSchedules
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ExamSchedules') AND name = 'AcademicYearId')
BEGIN
    ALTER TABLE [ExamSchedules] DROP COLUMN [AcademicYearId];
    PRINT '  + Dropped column AcademicYearId from ExamSchedules';
END

-- 3d. Rename ExamSchedules.SemesterId -> SemesterInstanceId
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('ExamSchedules') AND name = 'SemesterId'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExamSchedules_SemesterId' AND object_id = OBJECT_ID('ExamSchedules'))
        DROP INDEX [IX_ExamSchedules_SemesterId] ON [ExamSchedules];

    EXEC sp_rename 'ExamSchedules.SemesterId', 'SemesterInstanceId', 'COLUMN';
    PRINT '  + Renamed ExamSchedules.SemesterId -> SemesterInstanceId';
END

-- Rename index
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExamSchedules_SemesterId' AND object_id = OBJECT_ID('ExamSchedules'))
BEGIN
    EXEC sp_rename 'IX_ExamSchedules_SemesterId', 'IX_ExamSchedules_SemesterInstanceId', 'INDEX';
    PRINT '  + Renamed index IX_ExamSchedules_SemesterId -> IX_ExamSchedules_SemesterInstanceId';
END

-- 3e. Add StartDate, EndDate, TenantId columns to AcademicYears
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AcademicYears') AND name = 'StartDate')
BEGIN
    ALTER TABLE [AcademicYears] ADD [StartDate] [datetime2] NULL;
    PRINT '  + Added StartDate to AcademicYears';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AcademicYears') AND name = 'EndDate')
BEGIN
    ALTER TABLE [AcademicYears] ADD [EndDate] [datetime2] NULL;
    PRINT '  + Added EndDate to AcademicYears';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AcademicYears') AND name = 'TenantId')
BEGIN
    ALTER TABLE [AcademicYears] ADD [TenantId] [int] NOT NULL CONSTRAINT [DF_AcademicYears_TenantId] DEFAULT 1;
    PRINT '  + Added TenantId to AcademicYears (default 1)';
END

-- 3f. Create composite unique index on AcademicYears (TenantId, AcademicYearCode)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_AcademicYears_TenantId_AcademicYearCode' AND object_id = OBJECT_ID('AcademicYears')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_AcademicYears_TenantId_AcademicYearCode]
        ON [AcademicYears] ([TenantId], [AcademicYearCode]);
    PRINT '  + Created unique index IX_AcademicYears_TenantId_AcademicYearCode';
END

-- 3g. Add FK from AcademicYears to Tenants
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AcademicYears_Tenants_TenantId'
)
BEGIN
    ALTER TABLE [AcademicYears]
        ADD CONSTRAINT [FK_AcademicYears_Tenants_TenantId]
        FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id])
        ON DELETE RESTRICT;
    PRINT '  + Added FK_AcademicYears_Tenants_TenantId';
END

-- 3h. Add FK from ExamSchedules to SemesterInstances
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ExamSchedules_SemesterInstances_SemesterInstanceId'
)
BEGIN
    ALTER TABLE [ExamSchedules]
        ADD CONSTRAINT [FK_ExamSchedules_SemesterInstances_SemesterInstanceId]
        FOREIGN KEY ([SemesterInstanceId]) REFERENCES [SemesterInstances] ([Id])
        ON DELETE RESTRICT;
    PRINT '  + Added FK_ExamSchedules_SemesterInstances_SemesterInstanceId';
END

-- 3i. Record migration
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = '20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId', '10.0.7');
    PRINT '  + Recorded migration in __EFMigrationsHistory';
END

PRINT '--- Migration 3 complete ---';
PRINT '';

-- =========================================================================
-- MIGRATION 4: PendingChanges (20260817175434)
-- Makes ResultRecordMasterId nullable in ResultRecords
-- =========================================================================
PRINT '--- Migration 4: PendingChanges ---';

-- 4a. Make ResultRecordMasterId nullable
IF EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('ResultRecords')
      AND c.name = 'ResultRecordMasterId'
      AND c.is_nullable = 0
)
BEGIN
    ALTER TABLE [ResultRecords] ALTER COLUMN [ResultRecordMasterId] [int] NULL;
    PRINT '  + Made ResultRecordMasterId nullable in ResultRecords';
END
ELSE
    PRINT '  ~ ResultRecordMasterId is already nullable or does not exist, skipping.';

-- 4b. Record migration
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = '20260817175434_PendingChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260817175434_PendingChanges', '10.0.7');
    PRINT '  + Recorded migration in __EFMigrationsHistory';
END

PRINT '--- Migration 4 complete ---';
PRINT '';

-- =========================================================================
-- MIGRATION 5: AddIsActiveToSubjectOffering (20260818183135)
-- Adds IsActive (bit) to SubjectOfferings, recreates filtered unique index
-- =========================================================================
PRINT '--- Migration 5: AddIsActiveToSubjectOffering ---';

-- 5a. Drop existing unique index (will recreate with filter)
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId'
      AND object_id = OBJECT_ID('SubjectOfferings')
)
BEGIN
    DROP INDEX [IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId]
        ON [SubjectOfferings];
    PRINT '  + Dropped old unique index IX_SubjectOfferings_...';
END

-- 5b. Add IsActive column (bit, not null, default true)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('SubjectOfferings') AND name = 'IsActive'
)
BEGIN
    ALTER TABLE [SubjectOfferings] ADD [IsActive] [bit] NOT NULL CONSTRAINT [DF_SubjectOfferings_IsActive] DEFAULT 1;
    PRINT '  + Added IsActive column to SubjectOfferings';
END
ELSE
    PRINT '  ~ IsActive column already exists on SubjectOfferings, skipping.';

-- 5c. Recreate unique index with IsActive filter
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId'
      AND object_id = OBJECT_ID('SubjectOfferings')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId]
        ON [SubjectOfferings] ([CurriculumVersionId], [SubjectCatalogId], [ProgramId], [SemesterId])
        WHERE [IsActive] = 1;
    PRINT '  + Created filtered unique index IX_SubjectOfferings_... WHERE [IsActive] = 1';
END

-- 5d. Record migration
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = '20260818183135_AddIsActiveToSubjectOffering'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260818183135_AddIsActiveToSubjectOffering', '10.0.7');
    PRINT '  + Recorded migration in __EFMigrationsHistory';
END

PRINT '--- Migration 5 complete ---';
PRINT '';

COMMIT TRANSACTION;
PRINT '==========================================================================';
PRINT ' ALL 5 MIGRATIONS APPLIED SUCCESSFULLY ';
PRINT '==========================================================================';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '!!! TRANSACTION ROLLED BACK DUE TO ERROR !!!';
    PRINT 'Error: ' + ERROR_MESSAGE();
    PRINT 'Line: ' + CAST(ERROR_LINE() AS VARCHAR);
    THROW;
END CATCH
