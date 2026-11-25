/*
    Migration: 20261125115721_AddStudentAdmissionAppUserIdIndex
    Purpose:   Replace the single-column AppUserId index with a composite index
               that also supports the student dashboard's active/newest lookup.

    Safe to re-run:
    - drops the old index only if it exists
    - creates the new index only if it does not exist
    - records the migration only if it is not already recorded

    If your SQL Server edition supports online index creation and you are
    applying this during business hours, add WITH (ONLINE = ON) before the
    final semicolon on the CREATE INDEX statement below.
*/

BEGIN TRANSACTION;

BEGIN TRY
    IF EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'[dbo].[StudentAdmissions]')
          AND name = N'IX_StudentAdmissions_AppUserId'
    )
    BEGIN
        DROP INDEX [IX_StudentAdmissions_AppUserId] ON [dbo].[StudentAdmissions];
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'[dbo].[StudentAdmissions]')
          AND name = N'IX_StudentAdmissions_AppUserId_IsActive_Id'
    )
    BEGIN
        CREATE INDEX [IX_StudentAdmissions_AppUserId_IsActive_Id]
            ON [dbo].[StudentAdmissions] ([AppUserId], [IsActive], [Id])
            INCLUDE ([TenantId], [CollegeId], [ProgramsId], [HasFeeExemption]);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[__EFMigrationsHistory]
        WHERE [MigrationId] = N'20261125115721_AddStudentAdmissionAppUserIdIndex'
    )
    BEGIN
        INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20261125115721_AddStudentAdmissionAppUserIdIndex', N'10.0.9');
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
