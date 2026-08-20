-- ============================================================
-- PRODUCTION FIX: Invalid column name 'GradeLetterPractical'
--                  Invalid column name 'GradeLetterTheory'
--
-- Root cause: Migration AddPerPartGradesAndResultLevel was
-- placed in Data\Migrations instead of Migrations, so
-- dotnet ef database update never applied it.
--
-- Date: 2026-08-20
-- ============================================================

-- ============================================================
-- FIX 1 (CRITICAL): Add missing columns to ExamSubjectResults
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ExamSubjectResults') AND name = 'GradeLetterPractical')
BEGIN
    ALTER TABLE [ExamSubjectResults] ADD [GradeLetterPractical] nvarchar(5) NULL;
    PRINT 'Added GradeLetterPractical column to ExamSubjectResults';
END
ELSE
    PRINT 'GradeLetterPractical column already exists, skipping.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ExamSubjectResults') AND name = 'GradeLetterTheory')
BEGIN
    ALTER TABLE [ExamSubjectResults] ADD [GradeLetterTheory] nvarchar(5) NULL;
    PRINT 'Added GradeLetterTheory column to ExamSubjectResults';
END
ELSE
    PRINT 'GradeLetterTheory column already exists, skipping.';
GO

-- ============================================================
-- FIX 2: Add LevelId to ResultRecords (same migration)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ResultRecords') AND name = 'LevelId')
BEGIN
    ALTER TABLE [ResultRecords] ADD [LevelId] int NULL;
    PRINT 'Added LevelId column to ResultRecords';
END
ELSE
    PRINT 'LevelId column already exists, skipping.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ResultRecords_LevelId' AND object_id = OBJECT_ID('ResultRecords'))
BEGIN
    CREATE INDEX [IX_ResultRecords_LevelId] ON [ResultRecords] ([LevelId]);
    PRINT 'Created IX_ResultRecords_LevelId index';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ResultRecords_Levels_LevelId')
BEGIN
    ALTER TABLE [ResultRecords] ADD CONSTRAINT [FK_ResultRecords_Levels_LevelId]
        FOREIGN KEY ([LevelId]) REFERENCES [Levels] ([Id]) ON DELETE RESTRICT;
    PRINT 'Added FK_ResultRecords_Levels_LevelId constraint';
END
GO

-- ============================================================
-- Record this migration in __EFMigrationsHistory
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260816180547_AddPerPartGradesAndResultLevel')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260816180547_AddPerPartGradesAndResultLevel', '9.0.0');
    PRINT 'Recorded migration in __EFMigrationsHistory';
END
GO

PRINT '=== Fix applied successfully ===';
PRINT 'The GradeLetterPractical and GradeLetterTheory columns now exist.';
PRINT 'This should resolve the SqlException 207 errors in production.';
GO
