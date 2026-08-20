-- ============================================================
-- PENDING MIGRATIONS FIX - Run against production database
-- Generated: 2026-08-20
-- ============================================================
-- CRITICAL FIX: Adds GradeLetterPractical and GradeLetterTheory
-- columns to ExamSubjectResults table (fixes SqlException 207)
-- ============================================================

BEGIN TRANSACTION;

-- ============================================================
-- 1. AddPerPartGradesAndResultLevel (20260816180547)
--    CRITICAL: This fixes the production error
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ExamSubjectResults') AND name = 'GradeLetterPractical')
BEGIN
    ALTER TABLE [ExamSubjectResults] ADD [GradeLetterPractical] nvarchar(5) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ExamSubjectResults') AND name = 'GradeLetterTheory')
BEGIN
    ALTER TABLE [ExamSubjectResults] ADD [GradeLetterTheory] nvarchar(5) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ResultRecords') AND name = 'LevelId')
BEGIN
    ALTER TABLE [ResultRecords] ADD [LevelId] int NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ResultRecords_LevelId' AND object_id = OBJECT_ID('ResultRecords'))
BEGIN
    CREATE INDEX [IX_ResultRecords_LevelId] ON [ResultRecords] ([LevelId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ResultRecords_Levels_LevelId')
BEGIN
    ALTER TABLE [ResultRecords] ADD CONSTRAINT [FK_ResultRecords_Levels_LevelId]
        FOREIGN KEY ([LevelId]) REFERENCES [Levels] ([Id]) ON DELETE RESTRICT;
END
GO

-- ============================================================
-- 2. SemesterInstances (20260817024022)
--    NOTE: This is a MAJOR migration with data migration SQL.
--    Review carefully before applying. Commented out for safety.
-- ============================================================
-- If SemesterInstances table doesn't exist, run the full
-- SemesterInstances migration manually. It contains complex
-- data migration logic (INSERT INTO SemesterInstances, UPDATE
-- SemesterEnrollments, etc.)
-- ============================================================

-- ============================================================
-- 3. TenantScopedAcademicYearAndSemesterInstanceProgramId (20260817054948)
--    Drops AcademicYearId from ExamSchedules, renames SemesterId
--    to SemesterInstanceId, adds TenantId to AcademicYears.
--    DESTRUCTIVE - review carefully.
-- ============================================================

-- ============================================================
-- 4. PendingChanges (20260817175434)
-- ============================================================
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ResultRecords') AND name = 'ResultRecordMasterId' AND is_nullable = 0)
BEGIN
    ALTER TABLE [ResultRecords] ALTER COLUMN [ResultRecordMasterId] int NULL;
END
GO

-- ============================================================
-- 5. AddIsActiveToSubjectOffering (20260818183135)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SubjectOfferings') AND name = 'IsActive')
BEGIN
    ALTER TABLE [SubjectOfferings] ADD [IsActive] bit NOT NULL CONSTRAINT [DF_SubjectOfferings_IsActive] DEFAULT 1;
END
GO

-- Recreate unique index with IsActive filter if needed
-- NOTE: Only run if the old index exists without IsActive filter
-- IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId')
-- BEGIN
--     DROP INDEX [IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId] ON [SubjectOfferings];
--     CREATE UNIQUE INDEX [IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId]
--         ON [SubjectOfferings] ([CurriculumVersionId], [SubjectCatalogId], [ProgramId], [SemesterId])
--         WHERE [IsActive] = 1;
-- END
-- GO

-- ============================================================
-- Mark migrations as applied in __EFMigrationsHistory
-- (Only if not already recorded)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260816180547_AddPerPartGradesAndResultLevel')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260816180547_AddPerPartGradesAndResultLevel', '9.0.0');
END
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260817000000_MakeResultRecordMasterIdNullable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260817000000_MakeResultRecordMasterIdNullable', '9.0.0');
END
GO

COMMIT TRANSACTION;
