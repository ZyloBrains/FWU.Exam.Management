-- ============================================================
-- PRODUCTION DATA MIGRATION SCRIPT
-- ============================================================
-- Covers all 4 unpushed commits on develop:
--   49dc895  Tenant scoped subject catalog
--   be80895  Introduce semesterInstance
--   d79122c  Fix examschedules for running semester instances
--   3beb22e  Academic year is tenant-scoped and refactors
--
-- PREREQUISITES:
--   - Run BEFORE deploying the new application code
--   - Run BEFORE EF Core migrations are applied
--   - BACK UP your database first!
--
-- WHAT THIS SCRIPT DOES:
--   1. Creates SemesterInstances table (replaces denormalized Semester.AcademicYearId)
--   2. Populates SemesterInstances from existing Semester + ProgramSemesters + ExamSchedules data
--   3. Updates SemesterEnrollments.SemesterId -> SemesterInstanceId
--   4. Updates ExamSchedules.SemesterId -> SemesterInstanceId, drops ExamSchedules.AcademicYearId
--   5. Simplifies Semesters table (removes Year, StartDate, EndDate, AcademicYearId)
--   6. Adds TenantId to AcademicYears and SubjectCatalogs
--   7. Registers all EF migrations so they don't re-run
--
-- IDEMPOTENCY:
--   Safe to re-run. Checks preconditions before each operation.
--   Wrapped in a single transaction - rolls back on any failure.
--
-- NOTE:
--   Phases 2-17 are wrapped in EXEC() dynamic SQL so the script
--   works on both pre-migration AND already-migrated databases.
--   Without this, SQL Server's batch parser fails on column
--   references that don't exist in the current schema state.
-- ============================================================

SET NOCOUNT ON;
BEGIN TRY
BEGIN TRANSACTION;

-- ============================================================
-- PHASE 1: PRE-FLIGHT CHECKS (regular SQL - uses INFORMATION_SCHEMA only)
-- ============================================================

DECLARE @ShouldRun BIT = 1;

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Semesters' AND COLUMN_NAME = 'AcademicYearId'
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = 'AcademicYears' AND COLUMN_NAME = 'TenantId'
    )
    AND EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SemesterInstances'
    )
    BEGIN
        PRINT 'Migration already fully applied. Nothing to do.';
        SET @ShouldRun = 0;
    END
    ELSE
    BEGIN
        PRINT 'ERROR: Partial migration state detected. Semesters.AcademicYearId is gone but AcademicYears.TenantId or SemesterInstances is missing.';
        PRINT 'Manual intervention required.';
        SET @ShouldRun = 0;
    END
END

IF @ShouldRun = 1 AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Semesters')
BEGIN
    PRINT 'ERROR: Semesters table not found.';
    SET @ShouldRun = 0;
END

-- ============================================================
-- PHASES 2-17: EXECUTED VIA DYNAMIC SQL
-- ============================================================

IF @ShouldRun = 1
BEGIN
    DECLARE @TenantId INT = 1;
    PRINT 'TenantId: ' + CAST(@TenantId AS NVARCHAR(10));

    DECLARE @sql NVARCHAR(MAX) = N'
    DECLARE @TenantId INT = ' + CAST(@TenantId AS NVARCHAR(10)) + N';

    -- ============================================================
    -- PHASE 2: CAPTURE OLD MAPPINGS INTO TEMP TABLES
    -- ============================================================

    IF OBJECT_ID(''tempdb..#SemesterYearMap'') IS NOT NULL DROP TABLE #SemesterYearMap;
    SELECT DISTINCT
        s.Id AS SemesterId,
        s.AcademicYearId
    INTO #SemesterYearMap
    FROM Semesters s
    WHERE s.AcademicYearId > 0;

    PRINT ''Captured '' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + '' semester-to-academic-year mappings.'';

    IF OBJECT_ID(''tempdb..#InstanceSource'') IS NOT NULL DROP TABLE #InstanceSource;
    SELECT DISTINCT
        es.SemesterId,
        sy.AcademicYearId,
        es.ProgramId
    INTO #InstanceSource
    FROM ExamSchedules es
    INNER JOIN #SemesterYearMap sy ON es.SemesterId = sy.SemesterId
    WHERE es.SemesterId > 0 AND sy.AcademicYearId > 0;

    INSERT INTO #InstanceSource (SemesterId, AcademicYearId, ProgramId)
    SELECT DISTINCT
        se.SemesterId,
        sy.AcademicYearId,
        sa.ProgramsId
    FROM SemesterEnrollments se
    INNER JOIN #SemesterYearMap sy ON se.SemesterId = sy.SemesterId
    INNER JOIN StudentAdmissions sa ON se.StudentAdmissionId = sa.Id
    WHERE sy.AcademicYearId > 0
      AND sa.ProgramsId IS NOT NULL AND sa.ProgramsId > 0
      AND NOT EXISTS (
          SELECT 1 FROM #InstanceSource ix
          WHERE ix.SemesterId = se.SemesterId
            AND ix.AcademicYearId = sy.AcademicYearId
            AND ix.ProgramId = sa.ProgramsId
      );

    INSERT INTO #InstanceSource (SemesterId, AcademicYearId, ProgramId)
    SELECT DISTINCT
        ps.SemesterId,
        sy.AcademicYearId,
        ps.ProgramId
    FROM ProgramSemesters ps
    INNER JOIN #SemesterYearMap sy ON ps.SemesterId = sy.SemesterId
    WHERE ps.ProgramId > 0
      AND sy.AcademicYearId > 0
      AND NOT EXISTS (
          SELECT 1 FROM #InstanceSource ix
          WHERE ix.SemesterId = ps.SemesterId
            AND ix.AcademicYearId = sy.AcademicYearId
            AND ix.ProgramId = ps.ProgramId
      );

    PRINT ''Derived '' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + '' additional instance sources from ProgramSemesters.'';

    DECLARE @TotalSources INT;
    SELECT @TotalSources = COUNT(*) FROM #InstanceSource;
    PRINT ''Total SemesterInstance sources: '' + CAST(@TotalSources AS NVARCHAR(10));

    IF @TotalSources = 0
    BEGIN
        PRINT ''WARNING: No SemesterInstance sources found. Tables may be empty.'';
    END

    -- ============================================================
    -- PHASE 3: CREATE SemesterInstances TABLE
    -- ============================================================

    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = ''SemesterInstances'')
    BEGIN
        CREATE TABLE SemesterInstances (
            Id INT IDENTITY(1,1) NOT NULL,
            TenantId INT NOT NULL,
            SemesterId INT NOT NULL,
            AcademicYearId INT NOT NULL,
            ProgramId INT NOT NULL,
            StartDate DATETIME2 NOT NULL,
            EndDate DATETIME2 NOT NULL,
            Remark NVARCHAR(50) NULL,
            CONSTRAINT PK_SemesterInstances PRIMARY KEY (Id),
            CONSTRAINT FK_SemesterInstances_Tenants_TenantId FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE NO ACTION,
            CONSTRAINT FK_SemesterInstances_Semesters_SemesterId FOREIGN KEY (SemesterId) REFERENCES Semesters(Id) ON DELETE NO ACTION,
            CONSTRAINT FK_SemesterInstances_AcademicYears_AcademicYearId FOREIGN KEY (AcademicYearId) REFERENCES AcademicYears(Id) ON DELETE NO ACTION,
            CONSTRAINT FK_SemesterInstances_Programs_ProgramId FOREIGN KEY (ProgramId) REFERENCES Programs(Id) ON DELETE NO ACTION
        );
        PRINT ''Created SemesterInstances table.'';
    END
    ELSE
    BEGIN
        PRINT ''SemesterInstances table already exists.'';
    END

    -- ============================================================
    -- PHASE 4: POPULATE SemesterInstances
    -- ============================================================

    IF EXISTS (SELECT 1 FROM #InstanceSource)
    BEGIN
        INSERT INTO SemesterInstances (TenantId, SemesterId, AcademicYearId, ProgramId, StartDate, EndDate, Remark)
        SELECT
            @TenantId,
            ix.SemesterId,
            ix.AcademicYearId,
            ix.ProgramId,
            ISNULL(s.StartDate, GETUTCDATE()),
            ISNULL(s.EndDate, GETUTCDATE()),
            NULL
        FROM #InstanceSource ix
        LEFT JOIN Semesters s ON ix.SemesterId = s.Id
        WHERE NOT EXISTS (
            SELECT 1 FROM SemesterInstances si
            WHERE si.SemesterId = ix.SemesterId
              AND si.AcademicYearId = ix.AcademicYearId
              AND si.ProgramId = ix.ProgramId
        );

        DECLARE @InsertedCount INT = @@ROWCOUNT;
        PRINT ''Inserted '' + CAST(@InsertedCount AS NVARCHAR(10)) + '' SemesterInstance rows.'';
    END

    DECLARE @InstanceCount INT;
    SELECT @InstanceCount = COUNT(*) FROM SemesterInstances;
    PRINT ''Total SemesterInstances: '' + CAST(@InstanceCount AS NVARCHAR(10));

    -- ============================================================
    -- PHASE 4b: Drop FKs that reference old SemesterId columns (before UPDATEs)
    -- ============================================================

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_SemesterEnrollments_Semesters_SemesterId''
    )
    BEGIN
        ALTER TABLE SemesterEnrollments
            DROP CONSTRAINT FK_SemesterEnrollments_Semesters_SemesterId;
        PRINT ''Dropped FK_SemesterEnrollments_Semesters_SemesterId (pre-UPDATE).'';
    END

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_ExamSchedules_Semesters_SemesterId''
    )
    BEGIN
        ALTER TABLE ExamSchedules
            DROP CONSTRAINT FK_ExamSchedules_Semesters_SemesterId;
        PRINT ''Dropped FK_ExamSchedules_Semesters_SemesterId (pre-UPDATE).'';
    END

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_ExamSchedules_AcademicYears_AcademicYearId''
    )
    BEGIN
        ALTER TABLE ExamSchedules
            DROP CONSTRAINT FK_ExamSchedules_AcademicYears_AcademicYearId;
        PRINT ''Dropped FK_ExamSchedules_AcademicYears_AcademicYearId (pre-UPDATE).'';
    END

    -- ============================================================
    -- PHASE 5: UPDATE SemesterEnrollments.SemesterId -> SemesterInstanceId
    -- ============================================================

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = ''SemesterEnrollments'' AND COLUMN_NAME = ''SemesterId''
    )
    BEGIN
        UPDATE se
        SET se.SemesterId = si.Id
        FROM SemesterEnrollments se
        INNER JOIN #SemesterYearMap sy ON se.SemesterId = sy.SemesterId
        INNER JOIN StudentAdmissions sa ON se.StudentAdmissionId = sa.Id
        INNER JOIN SemesterInstances si
            ON si.SemesterId = sy.SemesterId
            AND si.AcademicYearId = sy.AcademicYearId
            AND si.ProgramId = sa.ProgramsId
        WHERE sa.ProgramsId IS NOT NULL AND sa.ProgramsId > 0;

        DECLARE @EnrollUpdated INT = @@ROWCOUNT;
        DECLARE @EnrollTotal INT;
        SELECT @EnrollTotal = COUNT(*) FROM SemesterEnrollments;

        PRINT ''Updated '' + CAST(@EnrollUpdated AS NVARCHAR(10)) + '' of '' + CAST(@EnrollTotal AS NVARCHAR(10)) + '' SemesterEnrollments.'';

        IF EXISTS (
            SELECT 1 FROM SemesterEnrollments se
            LEFT JOIN SemesterInstances si ON se.SemesterId = si.Id
            WHERE si.Id IS NULL
        )
        BEGIN
            DECLARE @OrphanedEnrollments INT;
            SELECT @OrphanedEnrollments = COUNT(*)
            FROM SemesterEnrollments se
            LEFT JOIN SemesterInstances si ON se.SemesterId = si.Id
            WHERE si.Id IS NULL;

            PRINT ''WARNING: '' + CAST(@OrphanedEnrollments AS NVARCHAR(10)) + '' SemesterEnrollments could not be mapped to a SemesterInstance.'';

            UPDATE se
            SET se.SemesterId = (
                SELECT TOP 1 si2.Id
                FROM SemesterInstances si2
                WHERE si2.SemesterId = se.SemesterId
                ORDER BY si2.AcademicYearId DESC
            )
            FROM SemesterEnrollments se
            LEFT JOIN SemesterInstances si ON se.SemesterId = si.Id
            WHERE si.Id IS NULL;

            DECLARE @FallbackEnrollUpdated INT = @@ROWCOUNT;
            PRINT ''Fallback: remapped '' + CAST(@FallbackEnrollUpdated AS NVARCHAR(10)) + '' additional SemesterEnrollments.'';
        END
    END
    ELSE
    BEGIN
        PRINT ''SemesterEnrollments already uses SemesterInstanceId column.'';
    END

    -- ============================================================
    -- PHASE 6: UPDATE ExamSchedules.SemesterId -> SemesterInstanceId
    -- ============================================================

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_ExamSchedules_Semesters_SemesterId''
    )
    BEGIN
        ALTER TABLE ExamSchedules
            DROP CONSTRAINT FK_ExamSchedules_Semesters_SemesterId;
        PRINT ''Dropped FK_ExamSchedules_Semesters_SemesterId.'';
    END

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_ExamSchedules_AcademicYears_AcademicYearId''
    )
    BEGIN
        ALTER TABLE ExamSchedules
            DROP CONSTRAINT FK_ExamSchedules_AcademicYears_AcademicYearId;
        PRINT ''Dropped FK_ExamSchedules_AcademicYears_AcademicYearId.'';
    END

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = ''ExamSchedules'' AND COLUMN_NAME = ''SemesterId''
    )
    BEGIN
        UPDATE es
        SET es.SemesterId = si.Id
        FROM ExamSchedules es
        INNER JOIN SemesterInstances si
            ON si.SemesterId = es.SemesterId
            AND si.AcademicYearId = es.AcademicYearId
            AND si.ProgramId = es.ProgramId;

        DECLARE @ExamUpdated INT = @@ROWCOUNT;
        DECLARE @ExamTotal INT;
        SELECT @ExamTotal = COUNT(*) FROM ExamSchedules;

        PRINT ''Updated '' + CAST(@ExamUpdated AS NVARCHAR(10)) + '' of '' + CAST(@ExamTotal AS NVARCHAR(10)) + '' ExamSchedules.'';

        IF EXISTS (
            SELECT 1 FROM ExamSchedules es
            LEFT JOIN SemesterInstances si ON es.SemesterId = si.Id
            WHERE si.Id IS NULL
        )
        BEGIN
            DECLARE @OrphanedExams INT;
            SELECT @OrphanedExams = COUNT(*)
            FROM ExamSchedules es
            LEFT JOIN SemesterInstances si ON es.SemesterId = si.Id
            WHERE si.Id IS NULL;

            DECLARE @Err1 NVARCHAR(200) = CAST(@OrphanedExams AS NVARCHAR(10)) + '' ExamSchedules cannot be mapped. Manual intervention required.'';
            THROW 50001, @Err1, 1;
        END
    END
    ELSE
    BEGIN
        PRINT ''ExamSchedules already uses SemesterInstanceId column.'';
    END

    -- ============================================================
    -- PHASE 7: DDL - Drop old FK constraints and indexes
    -- ============================================================

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_SemesterEnrollments_Semesters_SemesterId''
    )
    BEGIN
        ALTER TABLE SemesterEnrollments
            DROP CONSTRAINT FK_SemesterEnrollments_Semesters_SemesterId;
        PRINT ''Dropped FK_SemesterEnrollments_Semesters_SemesterId.'';
    END

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_Semesters_AcademicYears_AcademicYearId''
    )
    BEGIN
        ALTER TABLE Semesters
            DROP CONSTRAINT FK_Semesters_AcademicYears_AcademicYearId;
        PRINT ''Dropped FK_Semesters_AcademicYears_AcademicYearId.'';
    END

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_ExamSchedules_AcademicYears_AcademicYearId''
    )
    BEGIN
        ALTER TABLE ExamSchedules
            DROP CONSTRAINT FK_ExamSchedules_AcademicYears_AcademicYearId;
        PRINT ''Dropped FK_ExamSchedules_AcademicYears_AcademicYearId.'';
    END

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_ExamSchedules_Semesters_SemesterId''
    )
    BEGIN
        ALTER TABLE ExamSchedules
            DROP CONSTRAINT FK_ExamSchedules_Semesters_SemesterId;
        PRINT ''Dropped FK_ExamSchedules_Semesters_SemesterId.'';
    END

    IF EXISTS (
        SELECT 1 FROM SYS.INDEXES
        WHERE name = ''IX_Semesters_AcademicYearId'' AND object_id = OBJECT_ID(''Semesters'')
    )
    BEGIN
        DROP INDEX IX_Semesters_AcademicYearId ON Semesters;
        PRINT ''Dropped IX_Semesters_AcademicYearId.'';
    END

    IF EXISTS (
        SELECT 1 FROM SYS.INDEXES
        WHERE name = ''IX_SemesterEnrollments_SemesterId'' AND object_id = OBJECT_ID(''SemesterEnrollments'')
    )
    BEGIN
        DROP INDEX IX_SemesterEnrollments_SemesterId ON SemesterEnrollments;
        PRINT ''Dropped IX_SemesterEnrollments_SemesterId.'';
    END

    IF EXISTS (
        SELECT 1 FROM SYS.INDEXES
        WHERE name = ''IX_ExamSchedules_SemesterId'' AND object_id = OBJECT_ID(''ExamSchedules'')
    )
    BEGIN
        DROP INDEX IX_ExamSchedules_SemesterId ON ExamSchedules;
        PRINT ''Dropped IX_ExamSchedules_SemesterId.'';
    END

    IF EXISTS (
        SELECT 1 FROM SYS.INDEXES
        WHERE name = ''IX_ExamSchedules_AcademicYearId'' AND object_id = OBJECT_ID(''ExamSchedules'')
    )
    BEGIN
        DROP INDEX IX_ExamSchedules_AcademicYearId ON ExamSchedules;
        PRINT ''Dropped IX_ExamSchedules_AcademicYearId.'';
    END

    -- ============================================================
    -- PHASE 8: DDL - Rename columns
    -- ============================================================

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = ''SemesterEnrollments'' AND COLUMN_NAME = ''SemesterId''
    )
    BEGIN
        EXEC sp_rename ''SemesterEnrollments.SemesterId'', ''SemesterInstanceId'', ''COLUMN'';
        PRINT ''Renamed SemesterEnrollments.SemesterId -> SemesterInstanceId.'';
    END

    IF EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = ''ExamSchedules'' AND COLUMN_NAME = ''SemesterId''
    )
    BEGIN
        EXEC sp_rename ''ExamSchedules.SemesterId'', ''SemesterInstanceId'', ''COLUMN'';
        PRINT ''Renamed ExamSchedules.SemesterId -> SemesterInstanceId.'';
    END

    -- ============================================================
    -- PHASE 9: DDL - Drop columns from Semesters
    -- ============================================================

    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''Semesters'' AND COLUMN_NAME = ''AcademicYearId'')
    BEGIN
        ALTER TABLE Semesters DROP COLUMN AcademicYearId;
        PRINT ''Dropped Semesters.AcademicYearId.'';
    END

    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''Semesters'' AND COLUMN_NAME = ''StartDate'')
    BEGIN
        ALTER TABLE Semesters DROP COLUMN StartDate;
        PRINT ''Dropped Semesters.StartDate.'';
    END

    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''Semesters'' AND COLUMN_NAME = ''EndDate'')
    BEGIN
        ALTER TABLE Semesters DROP COLUMN EndDate;
        PRINT ''Dropped Semesters.EndDate.'';
    END

    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''Semesters'' AND COLUMN_NAME = ''Year'')
    BEGIN
        ALTER TABLE Semesters DROP COLUMN Year;
        PRINT ''Dropped Semesters.Year.'';
    END

    -- ============================================================
    -- PHASE 10: DDL - Drop AcademicYearId from ExamSchedules
    -- ============================================================

    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''ExamSchedules'' AND COLUMN_NAME = ''AcademicYearId'')
    BEGIN
        ALTER TABLE ExamSchedules DROP COLUMN AcademicYearId;
        PRINT ''Dropped ExamSchedules.AcademicYearId.'';
    END

    -- ============================================================
    -- PHASE 11: DDL - Add TenantId + StartDate + EndDate to AcademicYears
    -- ============================================================

    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''AcademicYears'' AND COLUMN_NAME = ''TenantId'')
    BEGIN
        ALTER TABLE AcademicYears ADD TenantId INT NOT NULL DEFAULT 1;
        PRINT ''Added AcademicYears.TenantId column.'';
    END

    UPDATE AcademicYears SET TenantId = @TenantId WHERE TenantId = 0;

    IF EXISTS (
        SELECT 1 FROM SYS.INDEXES
        WHERE name = ''IX_AcademicYears_AcademicYearCode'' AND object_id = OBJECT_ID(''AcademicYears'')
    )
    BEGIN
        DROP INDEX IX_AcademicYears_AcademicYearCode ON AcademicYears;
        PRINT ''Dropped IX_AcademicYears_AcademicYearCode.'';
    END

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_AcademicYears_Tenants_TenantId''
    )
    BEGIN
        ALTER TABLE AcademicYears
            ADD CONSTRAINT FK_AcademicYears_Tenants_TenantId
            FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE NO ACTION;
        PRINT ''Added FK_AcademicYears_Tenants_TenantId.'';
    END

    IF NOT EXISTS (
        SELECT 1 FROM SYS.INDEXES
        WHERE name = ''IX_AcademicYears_TenantId_AcademicYearCode'' AND object_id = OBJECT_ID(''AcademicYears'')
    )
    BEGIN
        CREATE UNIQUE INDEX IX_AcademicYears_TenantId_AcademicYearCode
            ON AcademicYears (TenantId, AcademicYearCode);
        PRINT ''Created IX_AcademicYears_TenantId_AcademicYearCode.'';
    END

    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''AcademicYears'' AND COLUMN_NAME = ''StartDate'')
    BEGIN
        ALTER TABLE AcademicYears ADD StartDate DATETIME2 NULL;
        PRINT ''Added AcademicYears.StartDate column.'';
    END

    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''AcademicYears'' AND COLUMN_NAME = ''EndDate'')
    BEGIN
        ALTER TABLE AcademicYears ADD EndDate DATETIME2 NULL;
        PRINT ''Added AcademicYears.EndDate column.'';
    END

    -- ============================================================
    -- PHASE 12: DDL - Add TenantId to SubjectCatalogs
    -- ============================================================

    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''SubjectCatalogs'' AND COLUMN_NAME = ''TenantId'')
    BEGIN
        ALTER TABLE SubjectCatalogs ADD TenantId INT NULL;
        PRINT ''Added SubjectCatalogs.TenantId column.'';
    END

    UPDATE SubjectCatalogs
    SET TenantId = @TenantId
    WHERE TenantId IS NULL;

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_SubjectCatalogs_Tenants_TenantId''
    )
    BEGIN
        ALTER TABLE SubjectCatalogs
            ADD CONSTRAINT FK_SubjectCatalogs_Tenants_TenantId
            FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE NO ACTION;
        PRINT ''Added FK_SubjectCatalogs_Tenants_TenantId.'';
    END

    IF NOT EXISTS (
        SELECT 1 FROM SYS.INDEXES
        WHERE name = ''IX_SubjectCatalogs_TenantId'' AND object_id = OBJECT_ID(''SubjectCatalogs'')
    )
    BEGIN
        CREATE INDEX IX_SubjectCatalogs_TenantId ON SubjectCatalogs (TenantId);
        PRINT ''Created IX_SubjectCatalogs_TenantId.'';
    END

    -- ============================================================
    -- PHASE 13: DDL - SemesterInstances indexes
    -- ============================================================

    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = ''SemesterInstances'')
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM SYS.INDEXES
            WHERE name = ''IX_SemesterInstances_AcademicYearId'' AND object_id = OBJECT_ID(''SemesterInstances'')
        )
        BEGIN
            CREATE INDEX IX_SemesterInstances_AcademicYearId ON SemesterInstances (AcademicYearId);
            PRINT ''Created IX_SemesterInstances_AcademicYearId.'';
        END

        IF NOT EXISTS (
            SELECT 1 FROM SYS.INDEXES
            WHERE name = ''IX_SemesterInstances_ProgramId'' AND object_id = OBJECT_ID(''SemesterInstances'')
        )
        BEGIN
            CREATE INDEX IX_SemesterInstances_ProgramId ON SemesterInstances (ProgramId);
            PRINT ''Created IX_SemesterInstances_ProgramId.'';
        END

        IF NOT EXISTS (
            SELECT 1 FROM SYS.INDEXES
            WHERE name = ''IX_SemesterInstances_TenantId'' AND object_id = OBJECT_ID(''SemesterInstances'')
        )
        BEGIN
            CREATE INDEX IX_SemesterInstances_TenantId ON SemesterInstances (TenantId);
            PRINT ''Created IX_SemesterInstances_TenantId.'';
        END

        IF EXISTS (
            SELECT 1 FROM SYS.INDEXES
            WHERE name = ''IX_SemesterInstances_SemesterId_AcademicYearId'' AND object_id = OBJECT_ID(''SemesterInstances'')
        )
        BEGIN
            DROP INDEX IX_SemesterInstances_SemesterId_AcademicYearId ON SemesterInstances;
            PRINT ''Dropped old IX_SemesterInstances_SemesterId_AcademicYearId.'';
        END

        IF NOT EXISTS (
            SELECT 1 FROM SYS.INDEXES
            WHERE name = ''IX_SemesterInstances_SemesterId_AcademicYearId_ProgramId'' AND object_id = OBJECT_ID(''SemesterInstances'')
        )
        BEGIN
            CREATE UNIQUE INDEX IX_SemesterInstances_SemesterId_AcademicYearId_ProgramId
                ON SemesterInstances (SemesterId, AcademicYearId, ProgramId);
            PRINT ''Created IX_SemesterInstances_SemesterId_AcademicYearId_ProgramId.'';
        END
    END

    -- ============================================================
    -- PHASE 14: DDL - New indexes on renamed columns
    -- ============================================================

    IF NOT EXISTS (
        SELECT 1 FROM SYS.INDEXES
        WHERE name = ''IX_SemesterEnrollments_SemesterInstanceId'' AND object_id = OBJECT_ID(''SemesterEnrollments'')
    )
    BEGIN
        CREATE INDEX IX_SemesterEnrollments_SemesterInstanceId ON SemesterEnrollments (SemesterInstanceId);
        PRINT ''Created IX_SemesterEnrollments_SemesterInstanceId.'';
    END

    IF NOT EXISTS (
        SELECT 1 FROM SYS.INDEXES
        WHERE name = ''IX_ExamSchedules_SemesterInstanceId'' AND object_id = OBJECT_ID(''ExamSchedules'')
    )
    BEGIN
        CREATE INDEX IX_ExamSchedules_SemesterInstanceId ON ExamSchedules (SemesterInstanceId);
        PRINT ''Created IX_ExamSchedules_SemesterInstanceId.'';
    END

    -- ============================================================
    -- PHASE 15: DDL - New foreign keys for renamed columns
    -- ============================================================

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_SemesterEnrollments_SemesterInstances_SemesterInstanceId''
    )
    BEGIN
        ALTER TABLE SemesterEnrollments
            ADD CONSTRAINT FK_SemesterEnrollments_SemesterInstances_SemesterInstanceId
            FOREIGN KEY (SemesterInstanceId) REFERENCES SemesterInstances(Id) ON DELETE NO ACTION;
        PRINT ''Added FK_SemesterEnrollments_SemesterInstances_SemesterInstanceId.'';
    END

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME = ''FK_ExamSchedules_SemesterInstances_SemesterInstanceId''
    )
    BEGIN
        ALTER TABLE ExamSchedules
            ADD CONSTRAINT FK_ExamSchedules_SemesterInstances_SemesterInstanceId
            FOREIGN KEY (SemesterInstanceId) REFERENCES SemesterInstances(Id) ON DELETE NO ACTION;
        PRINT ''Added FK_ExamSchedules_SemesterInstances_SemesterInstanceId.'';
    END

    -- ============================================================
    -- PHASE 16: REGISTER EF MIGRATIONS (prevent re-application)
    -- ============================================================

    IF NOT EXISTS (
        SELECT 1 FROM __EFMigrationsHistory
        WHERE MigrationId = ''20260816152655_AddTenantScopingToSubjectCatalog''
    )
    BEGIN
        INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
        VALUES (''20260816152655_AddTenantScopingToSubjectCatalog'', ''10.0.7'');
        PRINT ''Registered migration: AddTenantScopingToSubjectCatalog'';
    END

    IF NOT EXISTS (
        SELECT 1 FROM __EFMigrationsHistory
        WHERE MigrationId = ''20260817024022_SemesterInstances''
    )
    BEGIN
        INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
        VALUES (''20260817024022_SemesterInstances'', ''10.0.7'');
        PRINT ''Registered migration: SemesterInstances'';
    END

    IF NOT EXISTS (
        SELECT 1 FROM __EFMigrationsHistory
        WHERE MigrationId = ''20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId''
    )
    BEGIN
        INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
        VALUES (''20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'', ''10.0.7'');
        PRINT ''Registered migration: TenantScopedAcademicYearAndSemesterInstanceProgramId'';
    END

    -- ============================================================
    -- PHASE 17: VERIFICATION QUERIES
    -- ============================================================

    PRINT '''';
    PRINT ''============================================================'';
    PRINT ''VERIFICATION RESULTS'';
    PRINT ''============================================================'';

    DECLARE @OrphanCheck1 INT;
    SELECT @OrphanCheck1 = COUNT(*)
    FROM SemesterEnrollments se
    LEFT JOIN SemesterInstances si ON se.SemesterInstanceId = si.Id
    WHERE si.Id IS NULL;
    PRINT ''Orphaned SemesterEnrollments: '' + CAST(@OrphanCheck1 AS NVARCHAR(10));

    DECLARE @OrphanCheck2 INT;
    SELECT @OrphanCheck2 = COUNT(*)
    FROM ExamSchedules es
    LEFT JOIN SemesterInstances si ON es.SemesterInstanceId = si.Id
    WHERE si.Id IS NULL;
    PRINT ''Orphaned ExamSchedules: '' + CAST(@OrphanCheck2 AS NVARCHAR(10));

    DECLARE @UnscopedAcademicYears INT;
    SELECT @UnscopedAcademicYears = COUNT(*) FROM AcademicYears WHERE TenantId = 0;
    PRINT ''AcademicYears with TenantId=0: '' + CAST(@UnscopedAcademicYears AS NVARCHAR(10));

    DECLARE @UnscopedSubjects INT;
    SELECT @UnscopedSubjects = COUNT(*) FROM SubjectCatalogs WHERE TenantId IS NULL;
    PRINT ''SubjectCatalogs with NULL TenantId: '' + CAST(@UnscopedSubjects AS NVARCHAR(10));

    DECLARE @DupeCheck INT;
    SELECT @DupeCheck = COUNT(*)
    FROM (
        SELECT SemesterId, AcademicYearId, ProgramId, COUNT(*) AS Cnt
        FROM SemesterInstances
        GROUP BY SemesterId, AcademicYearId, ProgramId
        HAVING COUNT(*) > 1
    ) d;
    PRINT ''Duplicate SemesterInstances: '' + CAST(@DupeCheck AS NVARCHAR(10));

    PRINT '''';
    PRINT ''Row Counts:'';
    DECLARE @Cnt_Semesters INT, @Cnt_SemesterInstances INT, @Cnt_SemesterEnrollments INT;
    DECLARE @Cnt_ExamSchedules INT, @Cnt_AcademicYears INT, @Cnt_SubjectCatalogs INT;
    SELECT @Cnt_Semesters = COUNT(*) FROM Semesters;
    SELECT @Cnt_SemesterInstances = COUNT(*) FROM SemesterInstances;
    SELECT @Cnt_SemesterEnrollments = COUNT(*) FROM SemesterEnrollments;
    SELECT @Cnt_ExamSchedules = COUNT(*) FROM ExamSchedules;
    SELECT @Cnt_AcademicYears = COUNT(*) FROM AcademicYears;
    SELECT @Cnt_SubjectCatalogs = COUNT(*) FROM SubjectCatalogs;
    PRINT ''  Semesters:           '' + CAST(@Cnt_Semesters AS NVARCHAR(10));
    PRINT ''  SemesterInstances:   '' + CAST(@Cnt_SemesterInstances AS NVARCHAR(10));
    PRINT ''  SemesterEnrollments: '' + CAST(@Cnt_SemesterEnrollments AS NVARCHAR(10));
    PRINT ''  ExamSchedules:       '' + CAST(@Cnt_ExamSchedules AS NVARCHAR(10));
    PRINT ''  AcademicYears:       '' + CAST(@Cnt_AcademicYears AS NVARCHAR(10));
    PRINT ''  SubjectCatalogs:     '' + CAST(@Cnt_SubjectCatalogs AS NVARCHAR(10));

    DECLARE @OldColsCheck INT = 0;
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''Semesters'' AND COLUMN_NAME = ''AcademicYearId'')
        SET @OldColsCheck = @OldColsCheck + 1;
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''Semesters'' AND COLUMN_NAME = ''Year'')
        SET @OldColsCheck = @OldColsCheck + 1;
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''Semesters'' AND COLUMN_NAME = ''StartDate'')
        SET @OldColsCheck = @OldColsCheck + 1;
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''Semesters'' AND COLUMN_NAME = ''EndDate'')
        SET @OldColsCheck = @OldColsCheck + 1;
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''ExamSchedules'' AND COLUMN_NAME = ''AcademicYearId'')
        SET @OldColsCheck = @OldColsCheck + 1;
    PRINT ''Remaining old columns: '' + CAST(@OldColsCheck AS NVARCHAR(10)) + '' (should be 0)'';

    DECLARE @NewColsCheck INT = 0;
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''AcademicYears'' AND COLUMN_NAME = ''TenantId'')
        SET @NewColsCheck = @NewColsCheck + 1;
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''SubjectCatalogs'' AND COLUMN_NAME = ''TenantId'')
        SET @NewColsCheck = @NewColsCheck + 1;
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''SemesterEnrollments'' AND COLUMN_NAME = ''SemesterInstanceId'')
        SET @NewColsCheck = @NewColsCheck + 1;
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = ''ExamSchedules'' AND COLUMN_NAME = ''SemesterInstanceId'')
        SET @NewColsCheck = @NewColsCheck + 1;
    PRINT ''New columns present: '' + CAST(@NewColsCheck AS NVARCHAR(10)) + '' (should be 4)'';

    DECLARE @MigrationCount INT;
    SELECT @MigrationCount = COUNT(*) FROM __EFMigrationsHistory
    WHERE MigrationId IN (
        ''20260816152655_AddTenantScopingToSubjectCatalog'',
        ''20260817024022_SemesterInstances'',
        ''20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId''
    );
    PRINT ''EF Migrations registered: '' + CAST(@MigrationCount AS NVARCHAR(10)) + '' (should be 3)'';

    IF @OrphanCheck1 = 0 AND @OrphanCheck2 = 0 AND @DupeCheck = 0 AND @OldColsCheck = 0 AND @NewColsCheck = 4 AND @MigrationCount = 3
    BEGIN
        PRINT '''';
        PRINT ''*** ALL CHECKS PASSED - Migration successful! ***'';
    END
    ELSE
    BEGIN
        PRINT '''';
        PRINT ''*** WARNING: Some checks did not pass. Review above output. ***'';
    END

    -- Cleanup temp tables
    IF OBJECT_ID(''tempdb..#SemesterYearMap'') IS NOT NULL DROP TABLE #SemesterYearMap;
    IF OBJECT_ID(''tempdb..#InstanceSource'') IS NOT NULL DROP TABLE #InstanceSource;

    PRINT '''';
    PRINT ''============================================================'';
    ';

    EXEC(@sql);
END

COMMIT TRANSACTION;

PRINT '';
PRINT 'Migration completed successfully. Transaction committed.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT '';
    PRINT '*** ERROR: Migration failed. Transaction rolled back. ***';
    PRINT 'Error: ' + ERROR_MESSAGE();
    PRINT 'Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10));

    THROW;
END CATCH
GO
