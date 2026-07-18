-- ============================================================================
-- Legacy Data Migration Script (COMPLETE - all data from FWUExams.Legacy)
-- Source: [FWUExams.Legacy].dbo.CivilEngineering, ComputerEngineering, CPM
-- Target: FUExamsDb - All normalized tables
-- TenantId: 2 (Engineering)
-- ============================================================================
-- NOTE: Run AFTER C# seeders (Colleges, Faculties, Levels, Genders, StudentCategories).
--       This script is standalone and creates all reference data it needs.
-- ============================================================================

SET NOCOUNT ON;
SET XACT_ABORT OFF;
SET QUOTED_IDENTIFIER ON;

-- ============================================================================
-- STEP 0: Ensure Tenant Id=2 (Engineering) exists
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Id = 2)
BEGIN
    SET IDENTITY_INSERT Tenants ON;
    INSERT INTO Tenants (Id, Name, OfficeCode, ContactNumber, Address, Email, TenantType, IsActive)
    VALUES (2, 'Engineering', 'ENG', 'N/A', 'N/A', 'N/A', 1, 1);
    SET IDENTITY_INSERT Tenants OFF;
    PRINT 'Created Tenant: Engineering (Id=2)';
END
ELSE
    PRINT 'Tenant Id=2 already exists.';

DECLARE @TenantId INT = 2;

-- ============================================================================
-- STEP 1: Create temp mapping tables
-- ============================================================================

CREATE TABLE #AcademicYearMap (
    SourceYear INT,
    NewId INT
);

CREATE TABLE #ProgramMap (
    SourceCode NVARCHAR(50),
    NewId INT
);

CREATE TABLE #BatchMap (
    SourceBatchId INT,
    SourceBatchName INT,
    SourceProgramCode NVARCHAR(50),
    NewId INT
);

CREATE TABLE #SemesterMap (
    Number INT,
    AcademicYearId INT,
    NewId INT
);

CREATE TABLE #StudentRegMap (
    SourceRegId INT,
    RegistrationNo NVARCHAR(255),
    NewId INT
);

CREATE TABLE #StudentAdmissionMap (
    SourceAdmissionId INT,
    RegistrationNo NVARCHAR(255),
    NewId INT
);

CREATE TABLE #SemesterEnrollmentMap (
    RegistrationNo NVARCHAR(255),
    AcademicYearName INT,
    NewId INT
);

-- ============================================================================
-- STEP 2: Create AcademicYears (2014-2025)
-- ============================================================================

DECLARE @AYYear INT = 2014;
DECLARE @AYId INT;
WHILE @AYYear <= 2025
BEGIN
    IF NOT EXISTS (SELECT 1 FROM AcademicYears WHERE AcademicYearCode = CAST(@AYYear AS VARCHAR))
    BEGIN
        SET IDENTITY_INSERT AcademicYears ON;
        INSERT INTO AcademicYears (Id, AcademicYearCode, AcademicYearCodeNepali, AcademicYearName, AcademicYearNameNepali, IsRunning, IsActive)
        VALUES (@AYYear, CAST(@AYYear AS VARCHAR), CAST(@AYYear AS VARCHAR), CAST(@AYYear AS VARCHAR), CAST(@AYYear AS VARCHAR), 0, 1);
        SET IDENTITY_INSERT AcademicYears OFF;
        PRINT 'Created AcademicYear: ' + CAST(@AYYear AS VARCHAR);
    END
    SET @AYId = (SELECT Id FROM AcademicYears WHERE AcademicYearCode = CAST(@AYYear AS VARCHAR));
    INSERT INTO #AcademicYearMap (SourceYear, NewId) VALUES (@AYYear, @AYId);
    SET @AYYear = @AYYear + 1;
END
PRINT 'Step 2 complete: AcademicYears created.';

-- ============================================================================
-- STEP 3: Create Batches from source data
-- ============================================================================

-- Collect distinct batches from all source tables
SELECT DISTINCT
    CAST(BatchID AS INT) AS BatchId,
    CAST(BatchName AS INT) AS BatchName,
    LTRIM(RTRIM(ProgramCode)) AS ProgramCode
INTO #SourceBatches
FROM (
    SELECT BatchID, BatchName, ProgramCode FROM [FWUExams.Legacy].dbo.CivilEngineering WHERE BatchID IS NOT NULL AND BatchName IS NOT NULL
    UNION
    SELECT BatchID, BatchName, ProgramCode FROM [FWUExams.Legacy].dbo.ComputerEngineering WHERE BatchID IS NOT NULL AND BatchName IS NOT NULL
    UNION
    SELECT BatchID, BatchName, ProgramCode FROM [FWUExams.Legacy].dbo.CPM WHERE BatchID IS NOT NULL AND BatchName IS NOT NULL
) AS AllBatches
WHERE BatchID IS NOT NULL AND BatchName IS NOT NULL;

-- Create batches (deduplicated by BatchName year since target Batch has no ProgramId FK)
DECLARE @BName INT, @BProgCode NVARCHAR(50), @BAyId INT, @ExistingBatchId INT;
DECLARE batch_cursor CURSOR FOR SELECT DISTINCT BatchName, ProgramCode FROM #SourceBatches ORDER BY BatchName, ProgramCode;
OPEN batch_cursor;
FETCH NEXT FROM batch_cursor INTO @BName, @BProgCode;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @BAyId = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = @BName);

    -- Check if batch already exists by name (BatchName is the year)
    SET @ExistingBatchId = NULL;
    IF @BAyId IS NOT NULL
        SET @ExistingBatchId = (SELECT Id FROM Batches WHERE BatchName = CAST(@BName AS NVARCHAR(50)) AND AcademicYearId = @BAyId);

    IF @ExistingBatchId IS NULL AND @BAyId IS NOT NULL
    BEGIN
        INSERT INTO Batches (AcademicYearId, BatchName, Remarks, IsActive)
        VALUES (@BAyId, CAST(@BName AS NVARCHAR(50)), 'Migrated from legacy data', 1);
        SET @ExistingBatchId = SCOPE_IDENTITY();
        PRINT 'Created Batch: ' + CAST(@BName AS NVARCHAR(50)) + ' (Id=' + CAST(@ExistingBatchId AS VARCHAR) + ')';
    END

    -- Insert mapping for this source batch+program combo
    IF @ExistingBatchId IS NOT NULL
    BEGIN
        INSERT INTO #BatchMap (SourceBatchId, SourceBatchName, SourceProgramCode, NewId)
        SELECT sb.BatchId, sb.BatchName, sb.ProgramCode, @ExistingBatchId
        FROM #SourceBatches sb
        WHERE sb.BatchName = @BName AND sb.ProgramCode = @BProgCode
          AND NOT EXISTS (SELECT 1 FROM #BatchMap m WHERE m.SourceBatchId = sb.BatchId AND m.SourceProgramCode = sb.ProgramCode);
    END

    FETCH NEXT FROM batch_cursor INTO @BName, @BProgCode;
END
CLOSE batch_cursor;
DEALLOCATE batch_cursor;

PRINT 'Step 3 complete: Batches created.';

-- ============================================================================
-- STEP 4: Create Programs (ensure they exist)
-- ============================================================================

DECLARE @ProgCivilId INT, @ProgCompId INT, @ProgCPMId INT;

IF NOT EXISTS (SELECT 1 FROM Programs WHERE ProgramCode = 'L092')
BEGIN
    INSERT INTO Programs (LevelId, FacultyId, ProgramCode, ProgramName, ShortName, Duration, GrandTotalMarks, HasMultipleIntakes, IsActive)
    VALUES (1, 7, 'L092', 'Bachelor''s Degree in Civil Engineering', 'BCE', 8, 4000, 0, 1);
    SET @ProgCivilId = SCOPE_IDENTITY();
    PRINT 'Created Program: L092 (Id=' + CAST(@ProgCivilId AS VARCHAR) + ')';
END
ELSE
    SET @ProgCivilId = (SELECT Id FROM Programs WHERE ProgramCode = 'L092');
INSERT INTO #ProgramMap (SourceCode, NewId) VALUES ('L092', @ProgCivilId);

IF NOT EXISTS (SELECT 1 FROM Programs WHERE ProgramCode = 'L117')
BEGIN
    INSERT INTO Programs (LevelId, FacultyId, ProgramCode, ProgramName, ShortName, Duration, GrandTotalMarks, HasMultipleIntakes, IsActive)
    VALUES (1, 7, 'L117', 'Bachelor''s Degree in Computer Engineering', 'BCT', 8, 4000, 0, 1);
    SET @ProgCompId = SCOPE_IDENTITY();
    PRINT 'Created Program: L117 (Id=' + CAST(@ProgCompId AS VARCHAR) + ')';
END
ELSE
    SET @ProgCompId = (SELECT Id FROM Programs WHERE ProgramCode = 'L117');
INSERT INTO #ProgramMap (SourceCode, NewId) VALUES ('L117', @ProgCompId);

IF NOT EXISTS (SELECT 1 FROM Programs WHERE ProgramCode = 'L131')
BEGIN
    INSERT INTO Programs (LevelId, FacultyId, ProgramCode, ProgramName, ShortName, Duration, GrandTotalMarks, HasMultipleIntakes, IsActive)
    VALUES (2, 7, 'L131', 'Master of Science (M.Sc.) in Construction Project Management', 'M.Sc. CPM', 2, 4000, 0, 1);
    SET @ProgCPMId = SCOPE_IDENTITY();
    PRINT 'Created Program: L131 (Id=' + CAST(@ProgCPMId AS VARCHAR) + ')';
END
ELSE
    SET @ProgCPMId = (SELECT Id FROM Programs WHERE ProgramCode = 'L131');
INSERT INTO #ProgramMap (SourceCode, NewId) VALUES ('L131', @ProgCPMId);

PRINT 'Step 4 complete: Programs ensured.';

-- ============================================================================
-- STEP 5: Create Semesters (1-8 per AcademicYear)
-- ============================================================================

-- Create semesters Number 1-8 for each AcademicYear that has students
DECLARE @SemAYId INT, @SemNum INT, @SemYear INT, @SemCode NVARCHAR(30), @SemAYYear INT, @SemNewId INT, @ExistingSemId INT;
DECLARE sem_cursor CURSOR FOR
    SELECT DISTINCT ay.NewId
    FROM #AcademicYearMap ay
    INNER JOIN (
        SELECT DISTINCT CAST(AcademicYearName AS INT) AS AYName FROM [FWUExams.Legacy].dbo.CivilEngineering
        UNION SELECT DISTINCT CAST(AcademicYearName AS INT) FROM [FWUExams.Legacy].dbo.ComputerEngineering
        UNION SELECT DISTINCT CAST(AcademicYearName AS INT) FROM [FWUExams.Legacy].dbo.CPM
    ) src ON ay.SourceYear = src.AYName;

OPEN sem_cursor;
FETCH NEXT FROM sem_cursor INTO @SemAYId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SemNum = 1;
    WHILE @SemNum <= 8
    BEGIN
        SET @SemYear = CEILING(CAST(@SemNum AS FLOAT) / 2);
        SET @SemCode = 'SEM' + CAST(@SemNum AS NVARCHAR(10)) + '-AY' + CAST(@SemAYId AS NVARCHAR(10));

        -- Derive AY year from AcademicYearMap
        SET @SemAYYear = (SELECT SourceYear FROM #AcademicYearMap WHERE NewId = @SemAYId);

        IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = @SemCode)
        BEGIN
            INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId)
            VALUES (@SemNum, @SemYear,
                'Semester ' + CAST(@SemNum AS NVARCHAR(10)),
                @SemCode,
                DATEFROMPARTS(@SemAYYear + ((@SemNum - 1) / 2), CASE WHEN @SemNum % 2 = 1 THEN 1 ELSE 7 END, 1),
                DATEFROMPARTS(@SemAYYear + ((@SemNum - 1) / 2), CASE WHEN @SemNum % 2 = 1 THEN 6 ELSE 12 END, 28),
                @SemAYId);

            SET @SemNewId = SCOPE_IDENTITY();
            INSERT INTO #SemesterMap (Number, AcademicYearId, NewId) VALUES (@SemNum, @SemAYId, @SemNewId);
        END
        ELSE
        BEGIN
            SET @ExistingSemId = (SELECT Id FROM Semesters WHERE Code = @SemCode);
            INSERT INTO #SemesterMap (Number, AcademicYearId, NewId) VALUES (@SemNum, @SemAYId, @ExistingSemId);
        END

        SET @SemNum = @SemNum + 1;
    END

    FETCH NEXT FROM sem_cursor INTO @SemAYId;
END
CLOSE sem_cursor;
DEALLOCATE sem_cursor;

PRINT 'Step 5 complete: Semesters created.';

-- ============================================================================
-- STEP 6: Ensure College (SCH001) exists
-- ============================================================================

DECLARE @CollegeId INT;
IF NOT EXISTS (SELECT 1 FROM Colleges WHERE Code = 'SCH001')
BEGIN
    INSERT INTO Colleges (Code, Name, CollegeNameNepali, ShortName, EstablishedDate, Email, PrincipalName, PrincipalContactNumber, IsExamCenterOnly, IsActive, TenantId)
    VALUES ('SCH001', 'UNIVERSITY CENTRAL CAMPUS', NULL, 'UCC', '1900-01-01', 'info@fwu.edu.np', 'N/A', 'N/A', 0, 1, 1);
    SET @CollegeId = SCOPE_IDENTITY();
    PRINT 'Created College: SCH001 (Id=' + CAST(@CollegeId AS VARCHAR) + ')';
END
ELSE
    SET @CollegeId = (SELECT Id FROM Colleges WHERE Code = 'SCH001');

PRINT 'Step 6 complete: College ensured.';

-- ============================================================================
-- STEP 7: Create StudentRegistrations (deduplicated by RegistrationNo)
-- ============================================================================

-- Get distinct students from all source tables (deduplicate by RegistrationNo)
SELECT
    CAST(MAX(StudentRegistrationID) AS INT) AS SourceRegId,
    RegistrationNo,
    LTRIM(RTRIM(ISNULL(NULLIF(MAX(FirstName), 'NULL'), ''))) AS FirstName,
    LTRIM(RTRIM(ISNULL(NULLIF(MAX(MiddleName), 'NULL'), ''))) AS MiddleName,
    LTRIM(RTRIM(ISNULL(NULLIF(MAX(LastName), 'NULL'), 'N/A'))) AS LastName,
    LTRIM(RTRIM(ISNULL(NULLIF(MAX(Email), 'NULL'), ''))) AS Email,
    MAX(BirthDateAD) AS BirthDateAD,
    MAX(BirthDateBS) AS BirthDateBS,
    LTRIM(RTRIM(ISNULL(NULLIF(MAX(GenderName), 'NULL'), 'Male'))) AS GenderName,
    CAST(MAX(CollegeID) AS INT) AS CollegeId,
    CAST(MAX(LevelID) AS INT) AS LevelId,
    CAST(MAX(FacultyID) AS INT) AS FacultyId,
    CAST(MAX(AcademicYearName) AS INT) AS AcademicYearName,
    LTRIM(RTRIM(ISNULL(NULLIF(MAX(FullNameNepali), 'NULL'), ''))) AS FullNameNepali,
    CASE WHEN ISNULL(CAST(MAX(IsCompleted) AS INT), 0) = 0 THEN 1 ELSE 0 END AS IsActive,
    LTRIM(RTRIM(ISNULL(NULLIF(MAX(ProgramCode), 'NULL'), ''))) AS ProgramCode,
    CAST(MAX(StudentRegistrationIndex) AS INT) AS StudentRegistrationIndex,
    MAX(ContactNo) AS ContactNo
INTO #DistinctStudents
FROM (
    SELECT StudentRegistrationID, RegistrationNo, FirstName, MiddleName, LastName, Email, BirthDateAD, BirthDateBS, GenderName, CollegeID, LevelId, FacultyId, AcademicYearName, FullNameNepali, IsCompleted, ProgramCode, StudentRegistrationIndex, ContactNo FROM [FWUExams.Legacy].dbo.CivilEngineering
    UNION ALL
    SELECT StudentRegistrationID, RegistrationNo, FirstName, MiddleName, LastName, Email, BirthDateAD, BirthDateBS, GenderName, CollegeID, LevelId, FacultyId, AcademicYearName, FullNameNepali, IsCompleted, ProgramCode, StudentRegistrationIndex, ContactNo FROM [FWUExams.Legacy].dbo.ComputerEngineering
    UNION ALL
    SELECT StudentRegistrationID, RegistrationNo, FirstName, MiddleName, LastName, Email, BirthDateAD, BirthDateBS, GenderName, CollegeID, LevelId, FacultyId, AcademicYearName, FullNameNepali, IsCompleted, ProgramCode, StudentRegistrationIndex, ContactNo FROM [FWUExams.Legacy].dbo.CPM
) AS AllStudents
WHERE RegistrationNo IS NOT NULL AND RegistrationNo <> 'NULL' AND LTRIM(RTRIM(RegistrationNo)) <> ''
GROUP BY RegistrationNo;

-- Insert students
DECLARE @SrRegNo NVARCHAR(255), @SrFirstName NVARCHAR(255), @SrMiddleName NVARCHAR(255), @SrLastName NVARCHAR(255);
DECLARE @SrEmail NVARCHAR(255), @SrDobAD DATETIME, @SrDobBS DATETIME;
DECLARE @SrGender NVARCHAR(255), @SrAyName INT, @SrNepaliName NVARCHAR(255), @SrIsActive BIT;
DECLARE @SrProgramCode NVARCHAR(255), @SrStudentRegIndex INT, @SrContactNo FLOAT;
DECLARE @SrGenderId INT, @SrAyId INT, @SrLevelId INT, @SrProgramId INT;
DECLARE @DobAdStr NVARCHAR(10), @DobBsStr NVARCHAR(10), @ContactStr NVARCHAR(15);
DECLARE @SrNewId INT, @SrExistingId INT;

DECLARE sr_cursor CURSOR FOR
    SELECT RegistrationNo, FirstName, MiddleName, LastName, Email, BirthDateAD, BirthDateBS, GenderName, AcademicYearName, FullNameNepali, IsActive, ProgramCode, StudentRegistrationIndex, ContactNo
    FROM #DistinctStudents;

OPEN sr_cursor;
FETCH NEXT FROM sr_cursor INTO @SrRegNo, @SrFirstName, @SrMiddleName, @SrLastName, @SrEmail, @SrDobAD, @SrDobBS, @SrGender, @SrAyName, @SrNepaliName, @SrIsActive, @SrProgramCode, @SrStudentRegIndex, @SrContactNo;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SrGenderId = ISNULL((SELECT Id FROM Genders WHERE GenderName = @SrGender), 1);
    SET @SrAyId = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = @SrAyName);
    SET @SrLevelId = CASE WHEN @SrAyName >= 2023 AND @SrProgramCode = 'L131' THEN 2 ELSE 1 END;
    SET @SrProgramId = (SELECT NewId FROM #ProgramMap WHERE SourceCode = @SrProgramCode);

    IF NOT EXISTS (SELECT 1 FROM StudentRegistrations WHERE RegistrationNumber = @SrRegNo AND TenantId = @TenantId)
    BEGIN
        SET @DobAdStr = NULL;
        IF @SrDobAD IS NOT NULL
            SET @DobAdStr = CONVERT(NVARCHAR(10), @SrDobAD, 120);

        SET @DobBsStr = NULL;
        IF @SrDobBS IS NOT NULL
            SET @DobBsStr = CONVERT(NVARCHAR(10), @SrDobBS, 120);

        SET @ContactStr = NULL;
        IF @SrContactNo IS NOT NULL AND @SrContactNo > 0
            SET @ContactStr = CAST(CAST(@SrContactNo AS BIGINT) AS NVARCHAR(15));

        INSERT INTO StudentRegistrations (LevelId, CollegeId, FacultyId, ProgramId, RegistrationNumber,
            FirstName, MiddleName, LastName, ContactNumber, Email,
            DateOfBirthBS, DateOfBirthAD, GenderId, StudentCategoryId, AcademicYearId,
            IsActive, NepaliName, TenantId, StudentRegistrationIndex)
        VALUES (@SrLevelId, @CollegeId,
            CASE WHEN @SrProgramId IS NOT NULL THEN 7 ELSE NULL END,
            @SrProgramId,
            @SrRegNo, @SrFirstName,
            NULLIF(@SrMiddleName, ''), @SrLastName,
            @ContactStr, NULLIF(@SrEmail, ''),
            ISNULL(@DobBsStr, ''), @DobAdStr,
            @SrGenderId, 1, @SrAyId,
            @SrIsActive, NULLIF(@SrNepaliName, ''), @TenantId,
            @SrStudentRegIndex);

        SET @SrNewId = SCOPE_IDENTITY();
        INSERT INTO #StudentRegMap (SourceRegId, RegistrationNo, NewId) VALUES (0, @SrRegNo, @SrNewId);
    END
    ELSE
    BEGIN
        SET @SrExistingId = (SELECT Id FROM StudentRegistrations WHERE RegistrationNumber = @SrRegNo AND TenantId = @TenantId);
        INSERT INTO #StudentRegMap (SourceRegId, RegistrationNo, NewId) VALUES (0, @SrRegNo, @SrExistingId);
    END

    FETCH NEXT FROM sr_cursor INTO @SrRegNo, @SrFirstName, @SrMiddleName, @SrLastName, @SrEmail, @SrDobAD, @SrDobBS, @SrGender, @SrAyName, @SrNepaliName, @SrIsActive, @SrProgramCode, @SrStudentRegIndex, @SrContactNo;
END
CLOSE sr_cursor;
DEALLOCATE sr_cursor;

DECLARE @CntSR INT = (SELECT COUNT(DISTINCT NewId) FROM #StudentRegMap);
PRINT 'Step 7 complete: StudentRegistrations created. Count=' + CAST(@CntSR AS VARCHAR);

-- ============================================================================
-- STEP 8: Create StudentAdmissions (one per student)
-- ============================================================================

-- Get distinct student admissions from source (one per RegistrationNo + ProgramCode + BatchId)
SELECT DISTINCT
    LTRIM(RTRIM(RegistrationNo)) AS RegistrationNo,
    CAST(StudentAdmissionID AS INT) AS SourceAdmissionId,
    CAST(BatchID AS INT) AS SourceBatchId,
    LTRIM(RTRIM(ProgramCode)) AS ProgramCode,
    CAST(IsCompleted AS INT) AS IsCompleted
INTO #SourceAdmissions
FROM (
    SELECT RegistrationNo, StudentAdmissionID, BatchID, ProgramCode, IsCompleted FROM [FWUExams.Legacy].dbo.CivilEngineering WHERE RegistrationNo IS NOT NULL
    UNION
    SELECT RegistrationNo, StudentAdmissionID, BatchID, ProgramCode, IsCompleted FROM [FWUExams.Legacy].dbo.ComputerEngineering WHERE RegistrationNo IS NOT NULL
    UNION
    SELECT RegistrationNo, StudentAdmissionID, BatchID, ProgramCode, IsCompleted FROM [FWUExams.Legacy].dbo.CPM WHERE RegistrationNo IS NOT NULL
) AS AllAdmissions
WHERE RegistrationNo IS NOT NULL AND RegistrationNo <> 'NULL';

DECLARE @SaRegNo NVARCHAR(255), @SaSourceAdmId INT, @SaSourceBatchId INT, @SaProgCode NVARCHAR(50), @SaIsCompleted INT;
DECLARE @SaStudentRegId INT, @SaProgramId INT, @SaBatchId INT, @SaBatchYear INT;
DECLARE @AdmDate DATE, @SaNewId INT, @SaExistingId INT;
DECLARE sa_cursor CURSOR FOR
    SELECT RegistrationNo, SourceAdmissionId, SourceBatchId, ProgramCode, IsCompleted
    FROM #SourceAdmissions;

OPEN sa_cursor;
FETCH NEXT FROM sa_cursor INTO @SaRegNo, @SaSourceAdmId, @SaSourceBatchId, @SaProgCode, @SaIsCompleted;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SaStudentRegId = (SELECT TOP 1 NewId FROM #StudentRegMap WHERE RegistrationNo = @SaRegNo);
    SET @SaProgramId = (SELECT NewId FROM #ProgramMap WHERE SourceCode = @SaProgCode);
    SET @SaBatchId = (SELECT TOP 1 NewId FROM #BatchMap WHERE SourceBatchId = @SaSourceBatchId AND SourceProgramCode = @SaProgCode);

    -- Get batch year for AdmissionDate
    SET @SaBatchYear = (SELECT TOP 1 SourceBatchName FROM #BatchMap WHERE SourceBatchId = @SaSourceBatchId AND SourceProgramCode = @SaProgCode);

    IF @SaStudentRegId IS NOT NULL AND @SaProgramId IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM StudentAdmissions WHERE StudentRegistrationId = @SaStudentRegId AND ProgramsId = @SaProgramId AND TenantId = @TenantId)
        BEGIN
            SET @AdmDate = DATEFROMPARTS(ISNULL(@SaBatchYear, 2014), 1, 1);

            INSERT INTO StudentAdmissions (StudentRegistrationId, ProgramsId, CollegeId, BatchId,
                AdmissionDate, IsCompleted, IsActive, HasFeeExemption, TenantId)
            VALUES (@SaStudentRegId, @SaProgramId, @CollegeId, @SaBatchId,
                @AdmDate, CASE WHEN @SaIsCompleted = 1 THEN 1 ELSE 0 END, 1, 0, @TenantId);

            SET @SaNewId = SCOPE_IDENTITY();
            INSERT INTO #StudentAdmissionMap (SourceAdmissionId, RegistrationNo, NewId) VALUES (@SaSourceAdmId, @SaRegNo, @SaNewId);
        END
        ELSE
        BEGIN
            SET @SaExistingId = (SELECT Id FROM StudentAdmissions WHERE StudentRegistrationId = @SaStudentRegId AND ProgramsId = @SaProgramId AND TenantId = @TenantId);
            INSERT INTO #StudentAdmissionMap (SourceAdmissionId, RegistrationNo, NewId) VALUES (@SaSourceAdmId, @SaRegNo, @SaExistingId);
        END
    END

    FETCH NEXT FROM sa_cursor INTO @SaRegNo, @SaSourceAdmId, @SaSourceBatchId, @SaProgCode, @SaIsCompleted;
END
CLOSE sa_cursor;
DEALLOCATE sa_cursor;

DECLARE @CntSA INT = (SELECT COUNT(DISTINCT NewId) FROM #StudentAdmissionMap);
PRINT 'Step 8 complete: StudentAdmissions created. Count=' + CAST(@CntSA AS VARCHAR);

-- ============================================================================
-- STEP 9: Create SemesterEnrollments (one per student per AcademicYear)
-- ============================================================================

-- Get distinct student-year combinations
SELECT DISTINCT
    LTRIM(RTRIM(RegistrationNo)) AS RegistrationNo,
    CAST(AcademicYearName AS INT) AS AcademicYearName,
    CAST(BatchID AS INT) AS SourceBatchId,
    LTRIM(RTRIM(ProgramCode)) AS ProgramCode
INTO #SourceEnrollments
FROM (
    SELECT RegistrationNo, AcademicYearName, BatchID, ProgramCode FROM [FWUExams.Legacy].dbo.CivilEngineering WHERE RegistrationNo IS NOT NULL AND AcademicYearName IS NOT NULL
    UNION
    SELECT RegistrationNo, AcademicYearName, BatchID, ProgramCode FROM [FWUExams.Legacy].dbo.ComputerEngineering WHERE RegistrationNo IS NOT NULL AND AcademicYearName IS NOT NULL
    UNION
    SELECT RegistrationNo, AcademicYearName, BatchID, ProgramCode FROM [FWUExams.Legacy].dbo.CPM WHERE RegistrationNo IS NOT NULL AND AcademicYearName IS NOT NULL
) AS AllEnrollments
WHERE RegistrationNo IS NOT NULL AND RegistrationNo <> 'NULL';

DECLARE @SeRegNo NVARCHAR(255), @SeAyName INT, @SeSourceBatchId INT, @SeProgCode NVARCHAR(50);
DECLARE @SeAdmissionId INT, @SeBatchYear INT, @SeYearOffset INT, @SeSemNumber INT;
DECLARE @SeAyId INT, @SeSemesterId INT, @EnrollDate DATE, @SeNewId INT, @SeExistingId INT;
DECLARE se_cursor CURSOR FOR
    SELECT RegistrationNo, AcademicYearName, SourceBatchId, ProgramCode
    FROM #SourceEnrollments;

OPEN se_cursor;
FETCH NEXT FROM se_cursor INTO @SeRegNo, @SeAyName, @SeSourceBatchId, @SeProgCode;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SeAdmissionId = (SELECT TOP 1 NewId FROM #StudentAdmissionMap WHERE RegistrationNo = @SeRegNo);

    -- Calculate semester number based on batch year offset
    SET @SeBatchYear = (SELECT TOP 1 SourceBatchName FROM #BatchMap WHERE SourceBatchId = @SeSourceBatchId AND SourceProgramCode = @SeProgCode);
    SET @SeYearOffset = ISNULL(@SeAyName - @SeBatchYear, 0);
    SET @SeSemNumber = (@SeYearOffset * 2) + 1;

    -- Clamp to valid range (1-8)
    IF @SeSemNumber < 1 SET @SeSemNumber = 1;
    IF @SeSemNumber > 8 SET @SeSemNumber = 8;

    -- Look up the semester for this academic year and number
    SET @SeAyId = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = @SeAyName);
    SET @SeSemesterId = (SELECT TOP 1 NewId FROM #SemesterMap WHERE Number = @SeSemNumber AND AcademicYearId = @SeAyId);

    IF @SeAdmissionId IS NOT NULL AND @SeSemesterId IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM SemesterEnrollments WHERE StudentAdmissionId = @SeAdmissionId AND SemesterId = @SeSemesterId AND TenantId = @TenantId)
        BEGIN
            SET @EnrollDate = DATEFROMPARTS(ISNULL(@SeAyName, 2014), 1, 1);

            INSERT INTO SemesterEnrollments (StudentAdmissionId, SemesterId, EnrollmentStatus, EnrollmentType, PaymentStatus,
                EnrolledDate, TotalCredits, GradePoints, Deficiency, ResultStatus, TotalFee, PaidAmount, TenantId)
            VALUES (@SeAdmissionId, @SeSemesterId,
                1, -- Enrolled
                0, -- Regular
                1, -- Paid
                @EnrollDate, 0, 0, 0, 0, 0, 0, @TenantId);

            SET @SeNewId = SCOPE_IDENTITY();
            INSERT INTO #SemesterEnrollmentMap (RegistrationNo, AcademicYearName, NewId) VALUES (@SeRegNo, @SeAyName, @SeNewId);
        END
        ELSE
        BEGIN
            SET @SeExistingId = (SELECT Id FROM SemesterEnrollments WHERE StudentAdmissionId = @SeAdmissionId AND SemesterId = @SeSemesterId AND TenantId = @TenantId);
            INSERT INTO #SemesterEnrollmentMap (RegistrationNo, AcademicYearName, NewId) VALUES (@SeRegNo, @SeAyName, @SeExistingId);
        END
    END

    FETCH NEXT FROM se_cursor INTO @SeRegNo, @SeAyName, @SeSourceBatchId, @SeProgCode;
END
CLOSE se_cursor;
DEALLOCATE se_cursor;

DECLARE @CntSE INT = (SELECT COUNT(*) FROM #SemesterEnrollmentMap);
PRINT 'Step 9 complete: SemesterEnrollments created. Count=' + CAST(@CntSE AS VARCHAR);

-- ============================================================================
-- STEP 10: ExamTypes and SubjectTypes
-- ============================================================================

DECLARE @ExamTypeRegularId INT, @ExamTypePartialId INT;
IF NOT EXISTS (SELECT 1 FROM ExamTypes WHERE Code = '1')
BEGIN
    INSERT INTO ExamTypes (Code, Name, Remarks, IsActive)
    VALUES ('1', 'Regular', 'Regular examination', 1);
END
SET @ExamTypeRegularId = (SELECT Id FROM ExamTypes WHERE Code = '1');

IF NOT EXISTS (SELECT 1 FROM ExamTypes WHERE Code = '2')
BEGIN
    INSERT INTO ExamTypes (Code, Name, Remarks, IsActive)
    VALUES ('2', 'Partial', 'Partial examination', 1);
END
SET @ExamTypePartialId = (SELECT Id FROM ExamTypes WHERE Code = '2');

DECLARE @SubjectTypeCompId INT;
IF NOT EXISTS (SELECT 1 FROM SubjectTypes WHERE Code = 'COMP')
BEGIN
    INSERT INTO SubjectTypes (Code, Name, IsDefault, IsActive)
    VALUES ('COMP', 'Compulsory', 1, 1);
END
SET @SubjectTypeCompId = (SELECT Id FROM SubjectTypes WHERE Code = 'COMP');

PRINT 'Step 10 complete: ExamTypes and SubjectTypes ensured.';

-- ============================================================================
-- STEP 11: SubjectCatalogs (distinct subjects from all 3 source tables)
-- ============================================================================

CREATE TABLE #SubjectCatalogMap (
    SourceCode NVARCHAR(50),
    SourceName NVARCHAR(200),
    NewId INT
);

SELECT DISTINCT
    LTRIM(RTRIM(SubjectCode)) AS SubjectCode,
    LTRIM(RTRIM(SubjectName)) AS SubjectName,
    CreditHour
INTO #DistinctSubjects
FROM (
    SELECT SubjectCode, SubjectName, CreditHour FROM [FWUExams.Legacy].dbo.CivilEngineering
    UNION
    SELECT SubjectCode, SubjectName, CreditHour FROM [FWUExams.Legacy].dbo.ComputerEngineering
    UNION
    SELECT SubjectCode, SubjectName, CreditHour FROM [FWUExams.Legacy].dbo.CPM
) AS AllSubjects
WHERE SubjectCode IS NOT NULL AND SubjectCode <> 'NULL' AND LTRIM(RTRIM(SubjectCode)) <> '';

DECLARE @SubCode NVARCHAR(50), @SubName NVARCHAR(200), @CreditH INT;
DECLARE sub_cursor CURSOR FOR SELECT SubjectCode, SubjectName, CreditHour FROM #DistinctSubjects;
OPEN sub_cursor;
FETCH NEXT FROM sub_cursor INTO @SubCode, @SubName, @CreditH;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM SubjectCatalogs WHERE SubjectCode = @SubCode)
    BEGIN
        INSERT INTO SubjectCatalogs (SubjectCode, SubjectName, CreditHours, SubjectTypeId, IsActive)
        VALUES (@SubCode, @SubName, CASE WHEN @CreditH > 0 THEN @CreditH ELSE NULL END, @SubjectTypeCompId, 1);
        INSERT INTO #SubjectCatalogMap (SourceCode, SourceName, NewId) VALUES (@SubCode, @SubName, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        INSERT INTO #SubjectCatalogMap (SourceCode, SourceName, NewId)
        SELECT @SubCode, @SubName, Id FROM SubjectCatalogs WHERE SubjectCode = @SubCode;
    END
    FETCH NEXT FROM sub_cursor INTO @SubCode, @SubName, @CreditH;
END
CLOSE sub_cursor;
DEALLOCATE sub_cursor;

DECLARE @CntSC INT = (SELECT COUNT(*) FROM #SubjectCatalogMap);
PRINT 'Step 11 complete: SubjectCatalogs created. Count=' + CAST(@CntSC AS VARCHAR);

-- ============================================================================
-- STEP 12: SubjectOfferings (per Subject + Program + Semester)
-- ============================================================================

CREATE TABLE #SubjectOfferingMap (
    SubjectCatalogId INT,
    ProgramId INT,
    SemesterId INT,
    NewId INT
);

DECLARE @SoSubjectCatalogId INT, @SoProgramId INT, @SoSemesterId INT;
DECLARE @TheoryFM FLOAT, @TheoryPM FLOAT, @PracFM FLOAT, @PracPM FLOAT, @IntFM FLOAT, @IntPM FLOAT;
DECLARE @DisplayOrd INT;

-- Civil Engineering (L092) - Year+Part -> Semester per AY
DECLARE so_civ_cursor CURSOR FOR
    SELECT DISTINCT
        scm.NewId AS SubjectCatalogId,
        pm.NewId AS ProgramId,
        sm.NewId AS SemesterId,
        MAX(CAST(TotalFM AS FLOAT)),
        MAX(CAST(TotalPM AS FLOAT)),
        MAX(CAST(TheoryFullMark AS FLOAT)),
        MAX(CAST(TheoryPassMark AS FLOAT)),
        MAX(CAST(InternalFullMark AS FLOAT)),
        MAX(CAST(InternalPassMark AS FLOAT)),
        MAX(CAST(DisplayOrder AS INT))
    FROM [FWUExams.Legacy].dbo.CivilEngineering ce
    INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(ce.SubjectCode)) = scm.SourceCode
    INNER JOIN #ProgramMap pm ON pm.SourceCode = 'L092'
    INNER JOIN #AcademicYearMap ay ON ay.SourceYear = CAST(ce.AcademicYearName AS INT)
    INNER JOIN #SemesterMap sm ON sm.AcademicYearId = ay.NewId
        AND sm.Number = CASE
            WHEN ce.Year = 'I' AND ce.Part = 'I' THEN 1
            WHEN ce.Year = 'I' AND ce.Part = 'II' THEN 2
            WHEN ce.Year = 'II' AND ce.Part = 'I' THEN 3
            WHEN ce.Year = 'II' AND ce.Part = 'II' THEN 4
            WHEN ce.Year = 'III' AND ce.Part = 'I' THEN 5
            WHEN ce.Year = 'III' AND ce.Part = 'II' THEN 6
            ELSE 1
        END
    GROUP BY scm.NewId, pm.NewId, sm.NewId;

OPEN so_civ_cursor;
FETCH NEXT FROM so_civ_cursor INTO @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @TheoryFM, @TheoryPM, @PracFM, @PracPM, @IntFM, @IntPM, @DisplayOrd;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM SubjectOfferings WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId)
    BEGIN
        INSERT INTO SubjectOfferings (SubjectCatalogId, ProgramId, SemesterId, IsCompulsory, DisplayOrder, HasTheory, HasPractical, HasInternal,
            TheoryFullMarks, TheoryPassMarks, PracticalFullMarks, PracticalPassMarks,
            InternalTheoryFullMarks, InternalTheoryPassMarks, InternalPracticalFullMarks, InternalPracticalPassMarks, TenantId)
        VALUES (@SoSubjectCatalogId, @SoProgramId, @SoSemesterId, 1, ISNULL(@DisplayOrd, 0),
            CASE WHEN @PracFM > 0 THEN 1 ELSE 0 END,
            CASE WHEN @PracFM > 0 THEN 1 ELSE 0 END,
            CASE WHEN @IntFM > 0 THEN 1 ELSE 0 END,
            ISNULL(@PracFM, 0), ISNULL(@PracPM, 0),
            CASE WHEN @PracFM > 0 THEN @TheoryFM - @PracFM - ISNULL(@IntFM, 0) ELSE 0 END,
            CASE WHEN @PracFM > 0 THEN @TheoryPM - @PracPM - ISNULL(@IntPM, 0) ELSE 0 END,
            ISNULL(@IntFM, 0), ISNULL(@IntPM, 0), 0, 0, @TenantId);
        INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId) VALUES (@SoSubjectCatalogId, @SoProgramId, @SoSemesterId, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId)
        SELECT @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, Id FROM SubjectOfferings WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId;
    END
    FETCH NEXT FROM so_civ_cursor INTO @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @TheoryFM, @TheoryPM, @PracFM, @PracPM, @IntFM, @IntPM, @DisplayOrd;
END
CLOSE so_civ_cursor;
DEALLOCATE so_civ_cursor;

-- Computer Engineering (L117) - all subjects to Sem1 per AY
DECLARE so_comp_cursor CURSOR FOR
    SELECT DISTINCT
        scm.NewId AS SubjectCatalogId,
        pm.NewId AS ProgramId,
        sm.NewId AS SemesterId,
        MAX(CAST(TotalFM AS FLOAT)),
        MAX(CAST(TotalPM AS FLOAT)),
        MAX(CAST(TheoryFullMark AS FLOAT)),
        MAX(CAST(TheoryPassMark AS FLOAT)),
        MAX(CAST(InternalFullMark AS FLOAT)),
        MAX(CAST(InternalPassMark AS FLOAT)),
        MAX(CAST(DisplayOrder AS INT))
    FROM [FWUExams.Legacy].dbo.ComputerEngineering ce
    INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(ce.SubjectCode)) = scm.SourceCode
    INNER JOIN #ProgramMap pm ON pm.SourceCode = 'L117'
    INNER JOIN #AcademicYearMap ay ON ay.SourceYear = CAST(ce.AcademicYearName AS INT)
    INNER JOIN #SemesterMap sm ON sm.AcademicYearId = ay.NewId AND sm.Number = 1
    GROUP BY scm.NewId, pm.NewId, sm.NewId;

OPEN so_comp_cursor;
FETCH NEXT FROM so_comp_cursor INTO @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @TheoryFM, @TheoryPM, @PracFM, @PracPM, @IntFM, @IntPM, @DisplayOrd;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM SubjectOfferings WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId)
    BEGIN
        INSERT INTO SubjectOfferings (SubjectCatalogId, ProgramId, SemesterId, IsCompulsory, DisplayOrder, HasTheory, HasPractical, HasInternal,
            TheoryFullMarks, TheoryPassMarks, PracticalFullMarks, PracticalPassMarks,
            InternalTheoryFullMarks, InternalTheoryPassMarks, InternalPracticalFullMarks, InternalPracticalPassMarks, TenantId)
        VALUES (@SoSubjectCatalogId, @SoProgramId, @SoSemesterId, 1, ISNULL(@DisplayOrd, 0),
            CASE WHEN @PracFM > 0 THEN 1 ELSE 0 END,
            CASE WHEN @PracFM > 0 THEN 1 ELSE 0 END,
            CASE WHEN @IntFM > 0 THEN 1 ELSE 0 END,
            ISNULL(@PracFM, 0), ISNULL(@PracPM, 0),
            CASE WHEN @PracFM > 0 THEN @TheoryFM - @PracFM - ISNULL(@IntFM, 0) ELSE 0 END,
            CASE WHEN @PracFM > 0 THEN @TheoryPM - @PracPM - ISNULL(@IntPM, 0) ELSE 0 END,
            ISNULL(@IntFM, 0), ISNULL(@IntPM, 0), 0, 0, @TenantId);
        INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId) VALUES (@SoSubjectCatalogId, @SoProgramId, @SoSemesterId, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId)
        SELECT @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, Id FROM SubjectOfferings WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId;
    END
    FETCH NEXT FROM so_comp_cursor INTO @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @TheoryFM, @TheoryPM, @PracFM, @PracPM, @IntFM, @IntPM, @DisplayOrd;
END
CLOSE so_comp_cursor;
DEALLOCATE so_comp_cursor;

-- CPM (L131) - all subjects to Sem1 per AY
DECLARE so_cpm_cursor CURSOR FOR
    SELECT DISTINCT
        scm.NewId AS SubjectCatalogId,
        pm.NewId AS ProgramId,
        sm.NewId AS SemesterId,
        MAX(CAST(TotalFM AS FLOAT)),
        MAX(CAST(TotalPM AS FLOAT)),
        MAX(CAST(TheoryFullMark AS FLOAT)),
        MAX(CAST(TheoryPassMark AS FLOAT)),
        MAX(CAST(InternalFullMark AS FLOAT)),
        MAX(CAST(InternalPassMark AS FLOAT)),
        MAX(CAST(DisplayOrder AS INT))
    FROM [FWUExams.Legacy].dbo.CPM cpm
    INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(cpm.SubjectCode)) = scm.SourceCode
    INNER JOIN #ProgramMap pm ON pm.SourceCode = 'L131'
    INNER JOIN #AcademicYearMap ay ON ay.SourceYear = CAST(cpm.AcademicYearName AS INT)
    INNER JOIN #SemesterMap sm ON sm.AcademicYearId = ay.NewId AND sm.Number = 1
    GROUP BY scm.NewId, pm.NewId, sm.NewId;

OPEN so_cpm_cursor;
FETCH NEXT FROM so_cpm_cursor INTO @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @TheoryFM, @TheoryPM, @PracFM, @PracPM, @IntFM, @IntPM, @DisplayOrd;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM SubjectOfferings WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId)
    BEGIN
        INSERT INTO SubjectOfferings (SubjectCatalogId, ProgramId, SemesterId, IsCompulsory, DisplayOrder, HasTheory, HasPractical, HasInternal,
            TheoryFullMarks, TheoryPassMarks, PracticalFullMarks, PracticalPassMarks,
            InternalTheoryFullMarks, InternalTheoryPassMarks, InternalPracticalFullMarks, InternalPracticalPassMarks, TenantId)
        VALUES (@SoSubjectCatalogId, @SoProgramId, @SoSemesterId, 1, ISNULL(@DisplayOrd, 0),
            CASE WHEN @PracFM > 0 THEN 1 ELSE 0 END,
            CASE WHEN @PracFM > 0 THEN 1 ELSE 0 END,
            CASE WHEN @IntFM > 0 THEN 1 ELSE 0 END,
            ISNULL(@PracFM, 0), ISNULL(@PracPM, 0),
            CASE WHEN @PracFM > 0 THEN @TheoryFM - @PracFM - ISNULL(@IntFM, 0) ELSE 0 END,
            CASE WHEN @PracFM > 0 THEN @TheoryPM - @PracPM - ISNULL(@IntPM, 0) ELSE 0 END,
            ISNULL(@IntFM, 0), ISNULL(@IntPM, 0), 0, 0, @TenantId);
        INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId) VALUES (@SoSubjectCatalogId, @SoProgramId, @SoSemesterId, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId)
        SELECT @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, Id FROM SubjectOfferings WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId;
    END
    FETCH NEXT FROM so_cpm_cursor INTO @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @TheoryFM, @TheoryPM, @PracFM, @PracPM, @IntFM, @IntPM, @DisplayOrd;
END
CLOSE so_cpm_cursor;
DEALLOCATE so_cpm_cursor;

DECLARE @CntSO INT = (SELECT COUNT(*) FROM #SubjectOfferingMap);
PRINT 'Step 12 complete: SubjectOfferings created. Count=' + CAST(@CntSO AS VARCHAR);

-- ============================================================================
-- STEP 13: ExamSchedules (per Program + AcademicYear + Semester)
-- ============================================================================

CREATE TABLE #ExamScheduleMap (
    ProgramId INT,
    AcademicYearId INT,
    SemesterId INT,
    NewId INT
);

DECLARE es_cursor CURSOR FOR
    SELECT DISTINCT
        sm.AcademicYearId,
        sm.NewId AS SemesterId,
        pm.SourceCode + ' AY' + CAST(ay.SourceYear AS VARCHAR) + ' Sem' + CAST(sm.Number AS VARCHAR) AS EsName
    FROM #SemesterMap sm
    INNER JOIN #AcademicYearMap ay ON ay.NewId = sm.AcademicYearId
    INNER JOIN #ProgramMap pm ON 1=1
    WHERE (pm.SourceCode = 'L092' AND sm.Number BETWEEN 1 AND 6)
       OR (pm.SourceCode = 'L117' AND sm.Number = 1)
       OR (pm.SourceCode = 'L131' AND sm.Number = 1);

DECLARE @EsAcademicYearId INT, @EsSemesterId INT, @EsName NVARCHAR(50);
DECLARE @EsProgramId INT, @EsExamTypeId INT, @EsLevelId INT;

OPEN es_cursor;
FETCH NEXT FROM es_cursor INTO @EsAcademicYearId, @EsSemesterId, @EsName;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE prog_es_cursor CURSOR FOR
        SELECT pm.NewId, @ExamTypeRegularId, p.LevelId
        FROM #ProgramMap pm
        INNER JOIN Programs p ON p.Id = pm.NewId;

    OPEN prog_es_cursor;
    FETCH NEXT FROM prog_es_cursor INTO @EsProgramId, @EsExamTypeId, @EsLevelId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM ExamSchedules WHERE ProgramId = @EsProgramId AND AcademicYearId = @EsAcademicYearId AND SemesterId = @EsSemesterId)
        BEGIN
            INSERT INTO ExamSchedules (ExamScheduleName, AcademicYearId, ProgramId, SemesterId, ExamTypeId, LevelId, IsActive, StartTime, EndTime, TenantId)
            VALUES (@EsName, @EsAcademicYearId, @EsProgramId, @EsSemesterId, @EsExamTypeId, @EsLevelId, 1, '08:00', '11:00', @TenantId);
            INSERT INTO #ExamScheduleMap (ProgramId, AcademicYearId, SemesterId, NewId) VALUES (@EsProgramId, @EsAcademicYearId, @EsSemesterId, SCOPE_IDENTITY());
        END
        ELSE
        BEGIN
            INSERT INTO #ExamScheduleMap (ProgramId, AcademicYearId, SemesterId, NewId)
            SELECT @EsProgramId, @EsAcademicYearId, @EsSemesterId, Id FROM ExamSchedules WHERE ProgramId = @EsProgramId AND AcademicYearId = @EsAcademicYearId AND SemesterId = @EsSemesterId;
        END
        FETCH NEXT FROM prog_es_cursor INTO @EsProgramId, @EsExamTypeId, @EsLevelId;
    END
    CLOSE prog_es_cursor;
    DEALLOCATE prog_es_cursor;

    FETCH NEXT FROM es_cursor INTO @EsAcademicYearId, @EsSemesterId, @EsName;
END
CLOSE es_cursor;
DEALLOCATE es_cursor;

DECLARE @CntES INT = (SELECT COUNT(*) FROM #ExamScheduleMap);
PRINT 'Step 13 complete: ExamSchedules created. Count=' + CAST(@CntES AS VARCHAR);

-- ============================================================================
-- STEP 14: ExamCenters (per ExamSchedule)
-- ============================================================================

CREATE TABLE #ExamCenterMap (
    ExamScheduleId INT,
    CenterName NVARCHAR(200),
    NewId INT
);

INSERT INTO #ExamCenterMap (ExamScheduleId, CenterName, NewId)
SELECT DISTINCT esm.NewId, 'Kanchanpur', 0
FROM #ExamScheduleMap esm;

DECLARE @EcExamScheduleId INT, @EcCenterName NVARCHAR(200);
DECLARE ec_cursor CURSOR FOR SELECT DISTINCT ExamScheduleId, CenterName FROM #ExamCenterMap;
OPEN ec_cursor;
FETCH NEXT FROM ec_cursor INTO @EcExamScheduleId, @EcCenterName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ExamCenters WHERE ExamScheduleId = @EcExamScheduleId AND Code = 1)
    BEGIN
        INSERT INTO ExamCenters (ExamScheduleId, CollegeId, Code, Remark, IsActive, TenantId)
        VALUES (@EcExamScheduleId, @CollegeId, 1, @EcCenterName, 1, @TenantId);
        UPDATE #ExamCenterMap SET NewId = SCOPE_IDENTITY() WHERE ExamScheduleId = @EcExamScheduleId AND CenterName = @EcCenterName;
    END
    ELSE
    BEGIN
        UPDATE #ExamCenterMap SET NewId = (SELECT Id FROM ExamCenters WHERE ExamScheduleId = @EcExamScheduleId AND Code = 1) WHERE ExamScheduleId = @EcExamScheduleId AND CenterName = @EcCenterName;
    END
    FETCH NEXT FROM ec_cursor INTO @EcExamScheduleId, @EcCenterName;
END
CLOSE ec_cursor;
DEALLOCATE ec_cursor;

PRINT 'Step 14 complete: ExamCenters created.';

-- ============================================================================
-- STEP 15: ExamRegistrations (deduplicated by ExamRegistrationID)
-- ============================================================================

CREATE TABLE #ExamRegMap (
    SourceExamRegId INT,
    NewId INT
);

SELECT DISTINCT
    TRY_CAST(ExamRegistrationID AS INT) AS SourceExamRegId,
    LTRIM(RTRIM(RegistrationNo)) AS RegistrationNo,
    NULLIF(LTRIM(RTRIM(ExamRollNo)), '') AS ExamRollNo,
    NULLIF(LTRIM(RTRIM(ExamRollNoCoding)), '') AS ExamRollNoCoding,
    TRY_CAST(AcademicYearName AS INT) AS AcademicYearName,
    LTRIM(RTRIM(ExamTypeName)) AS ExamTypeName,
    NULLIF(LTRIM(RTRIM(CAST(SGPA AS NVARCHAR(50)))), '') AS SGPA,
    LTRIM(RTRIM(GradeLetter)) AS GradeLetter,
    ProgramCode,
    TRY_CAST(BatchID AS INT) AS BatchID
INTO #DistinctExamRegs
FROM (
    SELECT CAST(ExamRegistrationID AS NVARCHAR(50)) AS ExamRegistrationID, RegistrationNo, CAST(ExamRollNo AS NVARCHAR(50)) AS ExamRollNo, CAST(ExamRollNoCoding AS NVARCHAR(50)) AS ExamRollNoCoding, CAST(AcademicYearName AS NVARCHAR(50)) AS AcademicYearName, ExamTypeName, CAST(SGPA AS NVARCHAR(50)) AS SGPA, GradeLetter, ProgramCode, CAST(BatchID AS NVARCHAR(50)) AS BatchID FROM [FWUExams.Legacy].dbo.CivilEngineering WHERE ExamRegistrationID IS NOT NULL
    UNION
    SELECT CAST(ExamRegistrationID AS NVARCHAR(50)), RegistrationNo, CAST(ExamRollNo AS NVARCHAR(50)), CAST(ExamRollNoCoding AS NVARCHAR(50)), CAST(AcademicYearName AS NVARCHAR(50)), ExamTypeName, CAST(SGPA AS NVARCHAR(50)), GradeLetter, ProgramCode, CAST(BatchID AS NVARCHAR(50)) FROM [FWUExams.Legacy].dbo.ComputerEngineering WHERE ExamRegistrationID IS NOT NULL
    UNION
    SELECT CAST(ExamRegistrationID AS NVARCHAR(50)), RegistrationNo, CAST(ExamRollNo AS NVARCHAR(50)), CAST(ExamRollNoCoding AS NVARCHAR(50)), CAST(AcademicYearName AS NVARCHAR(50)), ExamTypeName, CAST(SGPA AS NVARCHAR(50)), GradeLetter, ProgramCode, CAST(BatchID AS NVARCHAR(50)) FROM [FWUExams.Legacy].dbo.CPM WHERE ExamRegistrationID IS NOT NULL
) AS AllExamRegs;

DECLARE @ErSourceId INT, @ErRegNo NVARCHAR(100), @ErRollNo NVARCHAR(50), @ErRollNoCoding NVARCHAR(50);
DECLARE @ErAyName INT, @ErExamType NVARCHAR(50), @ErSgpa NVARCHAR(50), @ErGrade NVARCHAR(50), @ErProgCode NVARCHAR(50), @ErBatchId INT;
DECLARE @ErStudentRegId INT, @ErAyId INT, @ErExamTypeId INT, @ErProgId INT, @ErEsId INT, @ErEcId INT;
DECLARE @ErBatchYear INT, @ErSemNum INT;

DECLARE er_cursor CURSOR FOR
    SELECT SourceExamRegId, RegistrationNo, ExamRollNo, ExamRollNoCoding, AcademicYearName, ExamTypeName, SGPA, GradeLetter, ProgramCode, BatchID
    FROM #DistinctExamRegs;

OPEN er_cursor;
FETCH NEXT FROM er_cursor INTO @ErSourceId, @ErRegNo, @ErRollNo, @ErRollNoCoding, @ErAyName, @ErExamType, @ErSgpa, @ErGrade, @ErProgCode, @ErBatchId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @ErStudentRegId = (SELECT TOP 1 NewId FROM #StudentRegMap WHERE RegistrationNo = @ErRegNo);
    SET @ErAyId = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = @ErAyName);
    SET @ErExamTypeId = CASE WHEN @ErExamType = 'Partial' THEN @ExamTypePartialId ELSE @ExamTypeRegularId END;
    SET @ErProgId = (SELECT NewId FROM #ProgramMap WHERE SourceCode = @ErProgCode);

    SET @ErBatchYear = (SELECT TOP 1 SourceBatchName FROM #BatchMap WHERE SourceBatchId = @ErBatchId AND SourceProgramCode = @ErProgCode);
    SET @ErSemNum = ISNULL((@ErAyName - @ErBatchYear) * 2 + 1, 1);
    IF @ErSemNum < 1 SET @ErSemNum = 1;
    IF @ErSemNum > 8 SET @ErSemNum = 8;

    SET @ErEsId = (SELECT TOP 1 NewId FROM #ExamScheduleMap WHERE ProgramId = @ErProgId AND AcademicYearId = @ErAyId AND SemesterId IN (SELECT NewId FROM #SemesterMap WHERE Number = @ErSemNum AND AcademicYearId = @ErAyId));
    SET @ErEcId = (SELECT TOP 1 NewId FROM #ExamCenterMap WHERE ExamScheduleId = @ErEsId);

    IF @ErStudentRegId IS NOT NULL AND @ErAyId IS NOT NULL AND @ErProgId IS NOT NULL AND @ErEsId IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM ExamRegistrations WHERE Id = @ErSourceId)
        BEGIN
            SET IDENTITY_INSERT ExamRegistrations ON;
            INSERT INTO ExamRegistrations (Id, AcademicYearId, ExamCenterId, CollegeId, ExamRollNumber, ExamRollNumberCoding,
                Sgpa, IsActive, ExamScheduleId, ProgramsId, TenantId, Status)
            VALUES (@ErSourceId, @ErAyId, @ErEcId, @CollegeId,
                NULLIF(@ErRollNo, 'NULL'), CASE WHEN @ErRollNoCoding IS NOT NULL AND ISNUMERIC(@ErRollNoCoding) = 1 THEN CAST(@ErRollNoCoding AS BIGINT) ELSE NULL END,
                NULLIF(@ErSgpa, 'NULL'), 1, @ErEsId, @ErProgId, @TenantId, 1);
            SET IDENTITY_INSERT ExamRegistrations OFF;
            INSERT INTO #ExamRegMap (SourceExamRegId, NewId) VALUES (@ErSourceId, @ErSourceId);
        END
        ELSE
        BEGIN
            INSERT INTO #ExamRegMap (SourceExamRegId, NewId) VALUES (@ErSourceId, @ErSourceId);
        END
    END

    FETCH NEXT FROM er_cursor INTO @ErSourceId, @ErRegNo, @ErRollNo, @ErRollNoCoding, @ErAyName, @ErExamType, @ErSgpa, @ErGrade, @ErProgCode, @ErBatchId;
END
CLOSE er_cursor;
DEALLOCATE er_cursor;

DECLARE @CntER INT = (SELECT COUNT(*) FROM #ExamRegMap);
PRINT 'Step 15 complete: ExamRegistrations created. Count=' + CAST(@CntER AS VARCHAR);

-- ============================================================================
-- STEP 16: ExamSubjectResults (one per source row)
-- ============================================================================

-- Process CivilEngineering
INSERT INTO ExamSubjectResults (ExamRegistrationId, ExamTypeId, SubjectOfferingId, ExamScheduleId,
    ObtainedMarksTheory, ObtainedMarksPractical, ObtainedMarksTheoryInternal, ObtainedMarksPracticalInternal,
    ObtainedMarks, GradeLetter, Remarks, IsActive, IsSubmitted, TenantId, CreatedDate)
SELECT
    er.Id AS ExamRegistrationId,
    CASE WHEN ce.ExamTypeName = 'Partial' THEN @ExamTypePartialId ELSE @ExamTypeRegularId END AS ExamTypeId,
    som.NewId AS SubjectOfferingId,
    esm.NewId AS ExamScheduleId,
    CASE WHEN ce.ObtainedMarks IS NOT NULL AND ISNUMERIC(ce.ObtainedMarks) = 1 THEN CAST(ce.ObtainedMarks AS FLOAT) ELSE NULL END,
    CASE WHEN ce.PracticalMarks IS NOT NULL AND ISNUMERIC(ce.PracticalMarks) = 1 THEN CAST(ce.PracticalMarks AS FLOAT) ELSE NULL END,
    CASE WHEN ce.InternalMarks IS NOT NULL AND ISNUMERIC(ce.InternalMarks) = 1 THEN CAST(ce.InternalMarks AS FLOAT) ELSE NULL END,
    CASE WHEN ce.InternalMarksFinal IS NOT NULL AND ISNUMERIC(ce.InternalMarksFinal) = 1 THEN CAST(ce.InternalMarksFinal AS FLOAT) ELSE NULL END,
    CASE WHEN ce.TotalOM IS NOT NULL AND ISNUMERIC(ce.TotalOM) = 1 THEN CAST(ce.TotalOM AS FLOAT) ELSE NULL END,
    CASE WHEN ce.GradeLetter IS NOT NULL AND ce.GradeLetter <> 'NULL' THEN LTRIM(RTRIM(ce.GradeLetter)) ELSE NULL END,
    CASE WHEN ce.Rem IS NOT NULL AND ce.Rem <> 'NULL' THEN LTRIM(RTRIM(ce.Rem)) ELSE NULL END,
    1, CASE WHEN TRY_CAST(LTRIM(RTRIM(ce.IsResultConfirm)) AS INT) = 1 THEN 1 ELSE 0 END,
    @TenantId, GETDATE()
FROM [FWUExams.Legacy].dbo.CivilEngineering ce
INNER JOIN #ExamRegMap erm ON erm.SourceExamRegId = TRY_CAST(ce.ExamRegistrationID AS INT)
INNER JOIN ExamRegistrations er ON er.Id = erm.NewId
INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(ce.SubjectCode)) = scm.SourceCode
INNER JOIN #ProgramMap pm ON pm.SourceCode = 'L092'
INNER JOIN #AcademicYearMap ay ON ay.SourceYear = CAST(ce.AcademicYearName AS INT)
INNER JOIN #SemesterMap sm ON sm.AcademicYearId = ay.NewId
    AND sm.Number = CASE
        WHEN ce.Year = 'I' AND ce.Part = 'I' THEN 1
        WHEN ce.Year = 'I' AND ce.Part = 'II' THEN 2
        WHEN ce.Year = 'II' AND ce.Part = 'I' THEN 3
        WHEN ce.Year = 'II' AND ce.Part = 'II' THEN 4
        WHEN ce.Year = 'III' AND ce.Part = 'I' THEN 5
        WHEN ce.Year = 'III' AND ce.Part = 'II' THEN 6
        ELSE 1
    END
INNER JOIN #SubjectOfferingMap som ON som.SubjectCatalogId = scm.NewId AND som.ProgramId = pm.NewId AND som.SemesterId = sm.NewId
LEFT JOIN #ExamScheduleMap esm ON esm.ProgramId = pm.NewId AND esm.AcademicYearId = ay.NewId AND esm.SemesterId = sm.NewId;

PRINT 'Step 16a: ExamSubjectResults for CivilEngineering. Rows=' + CAST(@@ROWCOUNT AS VARCHAR);

-- Process ComputerEngineering
INSERT INTO ExamSubjectResults (ExamRegistrationId, ExamTypeId, SubjectOfferingId, ExamScheduleId,
    ObtainedMarksTheory, ObtainedMarksPractical, ObtainedMarksTheoryInternal, ObtainedMarksPracticalInternal,
    ObtainedMarks, GradeLetter, Remarks, IsActive, IsSubmitted, TenantId, CreatedDate)
SELECT
    er.Id,
    CASE WHEN ce.ExamTypeName = 'Partial' THEN @ExamTypePartialId ELSE @ExamTypeRegularId END,
    som.NewId,
    esm.NewId,
    CASE WHEN ce.ObtainedMarks IS NOT NULL AND ISNUMERIC(ce.ObtainedMarks) = 1 THEN CAST(ce.ObtainedMarks AS FLOAT) ELSE NULL END,
    CASE WHEN ce.PracticalMarks IS NOT NULL AND ISNUMERIC(ce.PracticalMarks) = 1 THEN CAST(ce.PracticalMarks AS FLOAT) ELSE NULL END,
    CASE WHEN ce.InternalMarks IS NOT NULL AND ISNUMERIC(ce.InternalMarks) = 1 THEN CAST(ce.InternalMarks AS FLOAT) ELSE NULL END,
    CASE WHEN ce.InternalMarksFinal IS NOT NULL AND ISNUMERIC(ce.InternalMarksFinal) = 1 THEN CAST(ce.InternalMarksFinal AS FLOAT) ELSE NULL END,
    CASE WHEN ce.TotalOM IS NOT NULL AND ISNUMERIC(ce.TotalOM) = 1 THEN CAST(ce.TotalOM AS FLOAT) ELSE NULL END,
    CASE WHEN ce.GradeLetter IS NOT NULL AND ce.GradeLetter <> 'NULL' THEN LTRIM(RTRIM(ce.GradeLetter)) ELSE NULL END,
    CASE WHEN ce.Rem IS NOT NULL AND ce.Rem <> 'NULL' THEN LTRIM(RTRIM(ce.Rem)) ELSE NULL END,
    1, CASE WHEN TRY_CAST(LTRIM(RTRIM(ce.IsResultConfirm)) AS INT) = 1 THEN 1 ELSE 0 END,
    @TenantId, GETDATE()
FROM [FWUExams.Legacy].dbo.ComputerEngineering ce
INNER JOIN #ExamRegMap erm ON erm.SourceExamRegId = TRY_CAST(ce.ExamRegistrationID AS INT)
INNER JOIN ExamRegistrations er ON er.Id = erm.NewId
INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(ce.SubjectCode)) = scm.SourceCode
INNER JOIN #ProgramMap pm ON pm.SourceCode = 'L117'
INNER JOIN #AcademicYearMap ay ON ay.SourceYear = CAST(ce.AcademicYearName AS INT)
INNER JOIN #SemesterMap sm ON sm.AcademicYearId = ay.NewId AND sm.Number = 1
INNER JOIN #SubjectOfferingMap som ON som.SubjectCatalogId = scm.NewId AND som.ProgramId = pm.NewId AND som.SemesterId = sm.NewId
LEFT JOIN #ExamScheduleMap esm ON esm.ProgramId = pm.NewId AND esm.AcademicYearId = ay.NewId AND esm.SemesterId = sm.NewId;

PRINT 'Step 16b: ExamSubjectResults for ComputerEngineering. Rows=' + CAST(@@ROWCOUNT AS VARCHAR);

-- Process CPM
INSERT INTO ExamSubjectResults (ExamRegistrationId, ExamTypeId, SubjectOfferingId, ExamScheduleId,
    ObtainedMarksTheory, ObtainedMarksPractical, ObtainedMarksTheoryInternal, ObtainedMarksPracticalInternal,
    ObtainedMarks, GradeLetter, Remarks, IsActive, IsSubmitted, TenantId, CreatedDate)
SELECT
    er.Id,
    CASE WHEN cpm.ExamTypeName = 'Partial' THEN @ExamTypePartialId ELSE @ExamTypeRegularId END,
    som.NewId,
    esm.NewId,
    CASE WHEN cpm.ObtainedMarks IS NOT NULL AND ISNUMERIC(cpm.ObtainedMarks) = 1 THEN CAST(cpm.ObtainedMarks AS FLOAT) ELSE NULL END,
    CASE WHEN cpm.PracticalMarks IS NOT NULL AND ISNUMERIC(cpm.PracticalMarks) = 1 THEN CAST(cpm.PracticalMarks AS FLOAT) ELSE NULL END,
    CASE WHEN cpm.InternalMarks IS NOT NULL AND ISNUMERIC(cpm.InternalMarks) = 1 THEN CAST(cpm.InternalMarks AS FLOAT) ELSE NULL END,
    CASE WHEN cpm.InternalMarksFinal IS NOT NULL AND ISNUMERIC(cpm.InternalMarksFinal) = 1 THEN CAST(cpm.InternalMarksFinal AS FLOAT) ELSE NULL END,
    CASE WHEN cpm.TotalOM IS NOT NULL AND ISNUMERIC(cpm.TotalOM) = 1 THEN CAST(cpm.TotalOM AS FLOAT) ELSE NULL END,
    CASE WHEN cpm.GradeLetter IS NOT NULL AND cpm.GradeLetter <> 'NULL' THEN LTRIM(RTRIM(cpm.GradeLetter)) ELSE NULL END,
    CASE WHEN cpm.Rem IS NOT NULL AND cpm.Rem <> 'NULL' THEN LTRIM(RTRIM(cpm.Rem)) ELSE NULL END,
    1, CASE WHEN TRY_CAST(LTRIM(RTRIM(cpm.IsResultConfirm)) AS INT) = 1 THEN 1 ELSE 0 END,
    @TenantId, GETDATE()
FROM [FWUExams.Legacy].dbo.CPM cpm
INNER JOIN #ExamRegMap erm ON erm.SourceExamRegId = TRY_CAST(cpm.ExamRegistrationID AS INT)
INNER JOIN ExamRegistrations er ON er.Id = erm.NewId
INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(cpm.SubjectCode)) = scm.SourceCode
INNER JOIN #ProgramMap pm ON pm.SourceCode = 'L131'
INNER JOIN #AcademicYearMap ay ON ay.SourceYear = CAST(cpm.AcademicYearName AS INT)
INNER JOIN #SemesterMap sm ON sm.AcademicYearId = ay.NewId AND sm.Number = 1
INNER JOIN #SubjectOfferingMap som ON som.SubjectCatalogId = scm.NewId AND som.ProgramId = pm.NewId AND som.SemesterId = sm.NewId
LEFT JOIN #ExamScheduleMap esm ON esm.ProgramId = pm.NewId AND esm.AcademicYearId = ay.NewId AND esm.SemesterId = sm.NewId;

PRINT 'Step 16c: ExamSubjectResults for CPM. Rows=' + CAST(@@ROWCOUNT AS VARCHAR);

-- ============================================================================
-- STEP 17: Update ExamRegistrations.SemesterEnrollmentId
-- ============================================================================

UPDATE er
SET er.SemesterEnrollmentId = sem.Id
FROM ExamRegistrations er
INNER JOIN ExamSchedules es ON er.ExamScheduleId = es.Id
INNER JOIN StudentAdmissions sa ON sa.ProgramsId = es.ProgramId AND sa.TenantId = @TenantId
INNER JOIN SemesterEnrollments sem ON sem.StudentAdmissionId = sa.Id AND sem.TenantId = @TenantId
INNER JOIN Semesters s ON sem.SemesterId = s.Id AND s.AcademicYearId = es.AcademicYearId
WHERE er.TenantId = @TenantId
  AND er.SemesterEnrollmentId IS NULL;

DECLARE @CntUpdated INT = @@ROWCOUNT;
PRINT 'Step 17 complete: ExamRegistrations updated. Rows=' + CAST(@CntUpdated AS VARCHAR);

-- ============================================================================
-- STEP 18: Verification
-- ============================================================================

PRINT '';
PRINT '========== MIGRATION COMPLETE ==========';

SELECT 'Tenants' AS TableName, COUNT(*) AS cnt FROM Tenants WHERE Id = 2
UNION ALL SELECT 'AcademicYears', COUNT(*) FROM AcademicYears WHERE AcademicYearCode BETWEEN '2014' AND '2025'
UNION ALL SELECT 'Batches', COUNT(*) FROM Batches WHERE AcademicYearId IN (SELECT NewId FROM #AcademicYearMap)
UNION ALL SELECT 'Programs', COUNT(*) FROM Programs WHERE ProgramCode IN ('L092','L117','L131')
UNION ALL SELECT 'Semesters', COUNT(*) FROM Semesters WHERE AcademicYearId IN (SELECT NewId FROM #AcademicYearMap)
UNION ALL SELECT 'StudentRegistrations', COUNT(*) FROM StudentRegistrations WHERE TenantId = @TenantId
UNION ALL SELECT 'StudentAdmissions', COUNT(*) FROM StudentAdmissions WHERE TenantId = @TenantId
UNION ALL SELECT 'SemesterEnrollments', COUNT(*) FROM SemesterEnrollments WHERE TenantId = @TenantId
UNION ALL SELECT 'SubjectCatalogs', COUNT(*) FROM SubjectCatalogs
UNION ALL SELECT 'SubjectOfferings', COUNT(*) FROM SubjectOfferings WHERE TenantId = @TenantId
UNION ALL SELECT 'ExamSchedules', COUNT(*) FROM ExamSchedules WHERE TenantId = @TenantId
UNION ALL SELECT 'ExamCenters', COUNT(*) FROM ExamCenters WHERE TenantId = @TenantId
UNION ALL SELECT 'ExamRegistrations', COUNT(*) FROM ExamRegistrations WHERE TenantId = @TenantId
UNION ALL SELECT 'ExamSubjectResults', COUNT(*) FROM ExamSubjectResults WHERE TenantId = @TenantId;

-- Cleanup temp tables
DROP TABLE #AcademicYearMap;
DROP TABLE #ProgramMap;
DROP TABLE #BatchMap;
DROP TABLE #SemesterMap;
DROP TABLE #StudentRegMap;
DROP TABLE #StudentAdmissionMap;
DROP TABLE #SemesterEnrollmentMap;
DROP TABLE #DistinctStudents;
DROP TABLE #SourceBatches;
DROP TABLE #SourceAdmissions;
DROP TABLE #SourceEnrollments;
DROP TABLE #SubjectCatalogMap;
DROP TABLE #SubjectOfferingMap;
DROP TABLE #ExamScheduleMap;
DROP TABLE #ExamCenterMap;
DROP TABLE #ExamRegMap;
DROP TABLE #DistinctSubjects;
DROP TABLE #DistinctExamRegs;

PRINT 'Migration completed successfully.';
