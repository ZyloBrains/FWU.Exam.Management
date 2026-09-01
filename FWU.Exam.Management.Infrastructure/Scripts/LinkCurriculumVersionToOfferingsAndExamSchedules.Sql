SET XACT_ABORT ON;

BEGIN TRANSACTION;
BEGIN TRY
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807180334_LinkSubjectOfferingsToCurriculumVersion'
)
BEGIN
    DROP INDEX [IX_SubjectOfferings_SubjectCatalogId_ProgramId_SemesterId] ON [SubjectOfferings];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807180334_LinkSubjectOfferingsToCurriculumVersion'
)
BEGIN
    ALTER TABLE [SubjectOfferings] ADD [CurriculumVersionId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807180334_LinkSubjectOfferingsToCurriculumVersion'
)
BEGIN
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SubjectOfferings')
    BEGIN
        INSERT INTO CurriculumVersions (TenantId, ProgramId, EffectiveAcademicYearId, Name, Description, IsActive)
        SELECT DISTINCT
            so.TenantId,
            so.ProgramId,
            s.AcademicYearId,
            LEFT(N'Default - ' + ISNULL(p.ProgramName, N'Program') + N' (' + ISNULL(ay.AcademicYearName, N'') + N')', 100),
            N'Auto-created curriculum version for existing subject offerings.',
            1
        FROM SubjectOfferings so
        INNER JOIN Semesters s ON so.SemesterId = s.Id
        LEFT JOIN Programs p ON so.ProgramId = p.Id
        LEFT JOIN AcademicYears ay ON s.AcademicYearId = ay.Id
        WHERE so.CurriculumVersionId IS NULL
          AND NOT EXISTS (
              SELECT 1 FROM CurriculumVersions cv
              WHERE cv.TenantId = so.TenantId
                AND cv.ProgramId = so.ProgramId
                AND cv.EffectiveAcademicYearId = s.AcademicYearId
          );

        UPDATE so
        SET so.CurriculumVersionId = cv.Id
        FROM SubjectOfferings so
        INNER JOIN Semesters s ON so.SemesterId = s.Id
        INNER JOIN (
            SELECT TenantId, ProgramId, EffectiveAcademicYearId, MAX(Id) AS Id
            FROM CurriculumVersions
            WHERE Name LIKE N'Default - %'
            GROUP BY TenantId, ProgramId, EffectiveAcademicYearId
        ) cv ON cv.TenantId = so.TenantId
            AND cv.ProgramId = so.ProgramId
            AND cv.EffectiveAcademicYearId = s.AcademicYearId
        WHERE so.CurriculumVersionId IS NULL;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807180334_LinkSubjectOfferingsToCurriculumVersion'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId] ON [SubjectOfferings] ([CurriculumVersionId], [SubjectCatalogId], [ProgramId], [SemesterId]) WHERE [CurriculumVersionId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807180334_LinkSubjectOfferingsToCurriculumVersion'
)
BEGIN
    CREATE INDEX [IX_SubjectOfferings_SubjectCatalogId] ON [SubjectOfferings] ([SubjectCatalogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807180334_LinkSubjectOfferingsToCurriculumVersion'
)
BEGIN
    ALTER TABLE [SubjectOfferings] ADD CONSTRAINT [FK_SubjectOfferings_CurriculumVersions_CurriculumVersionId] FOREIGN KEY ([CurriculumVersionId]) REFERENCES [CurriculumVersions] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807180334_LinkSubjectOfferingsToCurriculumVersion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807180334_LinkSubjectOfferingsToCurriculumVersion', N'10.0.7');
END;

COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

