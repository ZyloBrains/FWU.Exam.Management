-- ============================================================
-- Migration: MakeTheoryMarksNullable
-- Date: 2026-08-22
-- Description: Makes SubjectOfferings.TheoryFullMarks and
--              TheoryPassMarks nullable (real NOT NULL -> NULL)
--              so theory marks are only required when
--              "Has Theory" is enabled.
-- Safe to re-run (idempotent).
-- ============================================================

BEGIN TRANSACTION;

--------------------------------------------------------------
-- 1. TheoryPassMarks -> NULL
--------------------------------------------------------------
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c]
    ON [d].[parent_column_id] = [c].[column_id]
   AND [d].[parent_object_id] = [c].[object_id]
WHERE [d].[parent_object_id] = OBJECT_ID(N'[SubjectOfferings]')
  AND [c].[name] = N'TheoryPassMarks';
IF @var IS NOT NULL
    EXEC(N'ALTER TABLE [SubjectOfferings] DROP CONSTRAINT ' + @var + ';');

ALTER TABLE [SubjectOfferings] ALTER COLUMN [TheoryPassMarks] real NULL;

--------------------------------------------------------------
-- 2. TheoryFullMarks -> NULL
--------------------------------------------------------------
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c]
    ON [d].[parent_column_id] = [c].[column_id]
   AND [d].[parent_object_id] = [c].[object_id]
WHERE [d].[parent_object_id] = OBJECT_ID(N'[SubjectOfferings]')
  AND [c].[name] = N'TheoryFullMarks';
IF @var1 IS NOT NULL
    EXEC(N'ALTER TABLE [SubjectOfferings] DROP CONSTRAINT ' + @var1 + ';');

ALTER TABLE [SubjectOfferings] ALTER COLUMN [TheoryFullMarks] real NULL;

--------------------------------------------------------------
-- 3. Record migration so EF Core does not re-apply it
--------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822055133_MakeTheoryMarksNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822055133_MakeTheoryMarksNullable', N'10.0.7');
END

COMMIT;
