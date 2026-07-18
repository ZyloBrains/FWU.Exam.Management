-- ============================================================================
-- Legacy Data Map Script - FWUExams.Legacy -> FUExamsDb
-- ============================================================================
-- This script maps ALL data from the legacy database FWUExams.Legacy into the
-- normalized FUExamsDb schema, including:
--   1. Reference/Lookup data (Tenants, Faculties, Programs, Levels, etc.)
--   2. StudentRegistrations (all students)
--   3. StudentAdmissions (admission records for each student)
--   4. SemesterEnrollments (semester enrollment per student)
--   5. StudentGuardians (placeholder - legacy has no guardian data)
--   6. StudentQualifications (placeholder - legacy has no qualification data)
--   7. ExamRegistrations (exam registration records)
--   8. ExamSubjectResults (marks/result records)
--   9. Update ExamRegistrations.SemesterEnrollmentId
--
-- IMPORTANT: All inserts use IF NOT EXISTS / MERGE to protect seeded data.
-- Existing records from seeders are NEVER modified or deleted.
-- ============================================================================
--
-- HOW TO RUN:
--   1. Open SQL Server Management Studio (SSMS)
--   2. Connect to your SQL Server instance containing both:
--        - FWUExams.Legacy (source - legacy denormalized tables)
--        - FUExamsDb (target - new normalized database)
--   3. Select FUExamsDb as the target database
--   4. Execute this script
--
-- PREREQUISITES:
--   - The FUExamsDb must have the latest migration applied (all tables exist)
--   - FWUExams.Legacy must have these tables:
--        dbo.CivilEngineering
--        dbo.ComputerEngineering
--        dbo.CPM
--   - If seeders have already run, reference data will NOT be duplicated
-- ============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRANSACTION;

PRINT '============================================================';
PRINT '  FUExams DataMap - Legacy to Target Migration';
PRINT '  Started: ' + CONVERT(NVARCHAR(30), GETDATE(), 120);
PRINT '============================================================';
PRINT '';

-- ============================================================================
-- Declare constants for key IDs (these match seed data if already seeded)
-- ============================================================================
DECLARE @TenantId INT = 1; -- OCE
DECLARE @CollegeId INT;
DECLARE @EngineeringFacultyId INT;

-- ============================================================================
-- STEP 0: Create temp ID mapping tables
-- ============================================================================

-- AcademicYear ID mapping
CREATE TABLE #AcademicYearMap (
    SourceYear NVARCHAR(10),
    NewId INT
);

-- Program ID mapping
CREATE TABLE #ProgramMap (
    SourceCode NVARCHAR(50),
    NewId INT
);

-- SubjectCatalog ID mapping
CREATE TABLE #SubjectCatalogMap (
    SourceCode NVARCHAR(50),
    SourceName NVARCHAR(200),
    NewId INT
);

-- SubjectOffering ID mapping (composite key)
CREATE TABLE #SubjectOfferingMap (
    SubjectCatalogId INT,
    ProgramId INT,
    SemesterId INT,
    NewId INT
);

-- ExamSchedule ID mapping
CREATE TABLE #ExamScheduleMap (
    ProgramId INT,
    AcademicYearId INT,
    SemesterId INT,
    NewId INT
);

-- ExamCenter ID mapping
CREATE TABLE #ExamCenterMap (
    ExamScheduleId INT,
    CenterName NVARCHAR(200),
    NewId INT
);

-- StudentRegistration ID mapping
CREATE TABLE #StudentRegMap (
    RegistrationNo NVARCHAR(100),
    NewId INT
);

-- ExamRegistration ID mapping
CREATE TABLE #ExamRegMap (
    SourceExamRegId INT,
    NewId INT
);

-- StudentAdmission ID mapping
CREATE TABLE #StudentAdmissionMap (
    RegistrationNo NVARCHAR(100),
    AcademicYearId INT,
    SemesterId INT,
    NewId INT
);

-- SemesterEnrollment ID mapping
CREATE TABLE #SemesterEnrollmentMap (
    StudentAdmissionId INT,
    SemesterId INT,
    NewId INT
);

PRINT 'Step 0 complete: Temp mapping tables created.';

-- ============================================================================
-- STEP 1: Resolve/Ensure Reference/Lookup Data
-- ============================================================================
-- All of these use IF NOT EXISTS to protect seed data.

-- 1a. Ensure OCE Tenant
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Id = @TenantId)
BEGIN
    SET IDENTITY_INSERT Tenants ON;
    INSERT INTO Tenants (Id, Name, OfficeCode, ContactNumber, Address, Email, IsActive)
    VALUES (@TenantId, 'Office of Controller of Examinations', 'OCE', '01-2345678', 'Kathmandu, Nepal', 'info@oce.gov.np', 1);
    SET IDENTITY_INSERT Tenants OFF;
    PRINT '  Created Tenant: OCE (Id=1)';
END
ELSE
    PRINT '  Tenant OCE already exists (Id=1)';

-- 1b. Engineering Faculty (lookup by OfficeCode, create if missing)
IF NOT EXISTS (SELECT 1 FROM Faculties WHERE OfficeCode = 'ENG')
BEGIN
    INSERT INTO Faculties (OfficeCode, Name, ContactNumber, Address, Email, TenantId)
    VALUES ('ENG', 'Faculty of Engineering', 'N/A', 'N/A', 'N/A', @TenantId);
    PRINT '  Created Faculty: ENG';
END
ELSE
    PRINT '  Faculty ENG already exists';

SET @EngineeringFacultyId = (SELECT Id FROM Faculties WHERE OfficeCode = 'ENG');

-- 1c. Academic Years (create years 2014-2026 if missing)
DECLARE @AYYear INT = 2014;
WHILE @AYYear <= 2026
BEGIN
    IF NOT EXISTS (SELECT 1 FROM AcademicYears WHERE AcademicYearCode = CAST(@AYYear AS VARCHAR))
    BEGIN
        INSERT INTO AcademicYears (AcademicYearCode, AcademicYearName, IsActive)
        VALUES (CAST(@AYYear AS VARCHAR), CAST(@AYYear AS VARCHAR), 1);
    END

    DECLARE @AYId INT = (SELECT Id FROM AcademicYears WHERE AcademicYearCode = CAST(@AYYear AS VARCHAR));

    -- Avoid duplicates in mapping table
    IF NOT EXISTS (SELECT 1 FROM #AcademicYearMap WHERE SourceYear = CAST(@AYYear AS VARCHAR))
    BEGIN
        INSERT INTO #AcademicYearMap (SourceYear, NewId) VALUES (CAST(@AYYear AS VARCHAR), @AYId);
    END

    SET @AYYear = @AYYear + 1;
END
PRINT '  Step 1c complete: Academic Years ensured (2014-2026).';

-- Get specific AY IDs for later use
DECLARE @AY2014Id INT = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = '2014');
DECLARE @AY2021Id INT = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = '2021');
DECLARE @AY2023Id INT = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = '2023');

-- 1d. Resolve College
-- Try to find SCH001 (legacy uses this), fallback to first available college
IF EXISTS (SELECT 1 FROM Colleges WHERE Code = 'SCH001')
    SET @CollegeId = (SELECT Id FROM Colleges WHERE Code = 'SCH001');
ELSE IF EXISTS (SELECT 1 FROM Colleges WHERE Code = 'COC')
    SET @CollegeId = (SELECT Id FROM Colleges WHERE Code = 'COC');
ELSE
BEGIN
    INSERT INTO Colleges (Code, Name, ShortName, IsActive, TenantId)
    VALUES ('SCH001', 'University Central Campus (Legacy)', 'UCC', 1, @TenantId);
    SET @CollegeId = SCOPE_IDENTITY();
    PRINT '  Created College: SCH001 (Id=' + CAST(@CollegeId AS VARCHAR) + ')';
END
PRINT '  Using CollegeId: ' + CAST(@CollegeId AS VARCHAR);

-- 1e. Levels (Undergraduate=1, Graduate=2)
IF NOT EXISTS (SELECT 1 FROM Levels WHERE LevelCode = '1')
BEGIN
    SET IDENTITY_INSERT Levels ON;
    INSERT INTO Levels (Id, LevelCode, LevelName, IsActive) VALUES (1, '1', 'Undergraduate', 1);
    SET IDENTITY_INSERT Levels OFF;
    PRINT '  Created Level: Undergraduate (1)';
END

IF NOT EXISTS (SELECT 1 FROM Levels WHERE LevelCode = '2')
BEGIN
    SET IDENTITY_INSERT Levels ON;
    INSERT INTO Levels (Id, LevelCode, LevelName, IsActive) VALUES (2, '2', 'Graduate', 1);
    SET IDENTITY_INSERT Levels OFF;
    PRINT '  Created Level: Graduate (2)';
END

-- 1f. Programs - only create if they don't exist (seeded programs take priority)
DECLARE @ProgCivilId INT, @ProgCompId INT, @ProgCPMId INT;

-- L092 - Civil Engineering
IF NOT EXISTS (SELECT 1 FROM Programs WHERE ProgramCode = 'L092')
BEGIN
    INSERT INTO Programs (LevelId, ProgramCode, ProgramName, ShortName, Duration, IsActive)
    VALUES (1, 'L092', 'Bachelor''s Degree in Civil Engineering', 'BE Civil', 8, 1);
    SET @ProgCivilId = SCOPE_IDENTITY();
    PRINT '  Created Program: L092 (Id=' + CAST(@ProgCivilId AS VARCHAR) + ')';
END
ELSE
    SET @ProgCivilId = (SELECT Id FROM Programs WHERE ProgramCode = 'L092');

IF NOT EXISTS (SELECT 1 FROM #ProgramMap WHERE SourceCode = 'L092')
    INSERT INTO #ProgramMap (SourceCode, NewId) VALUES ('L092', @ProgCivilId);

-- L117 - Computer Engineering
IF NOT EXISTS (SELECT 1 FROM Programs WHERE ProgramCode = 'L117')
BEGIN
    INSERT INTO Programs (LevelId, ProgramCode, ProgramName, ShortName, Duration, IsActive)
    VALUES (1, 'L117', 'Bachelor''s Degree in Computer Engineering', 'BE Computer', 8, 1);
    SET @ProgCompId = SCOPE_IDENTITY();
    PRINT '  Created Program: L117 (Id=' + CAST(@ProgCompId AS VARCHAR) + ')';
END
ELSE
    SET @ProgCompId = (SELECT Id FROM Programs WHERE ProgramCode = 'L117');

IF NOT EXISTS (SELECT 1 FROM #ProgramMap WHERE SourceCode = 'L117')
    INSERT INTO #ProgramMap (SourceCode, NewId) VALUES ('L117', @ProgCompId);

-- L131 - M.Sc. CPM
IF NOT EXISTS (SELECT 1 FROM Programs WHERE ProgramCode = 'L131')
BEGIN
    INSERT INTO Programs (LevelId, ProgramCode, ProgramName, ShortName, Duration, IsActive)
    VALUES (2, 'L131', 'Master of Science (M.Sc.) in Construction Project Management', 'M.Sc. CPM', 4, 1);
    SET @ProgCPMId = SCOPE_IDENTITY();
    PRINT '  Created Program: L131 (Id=' + CAST(@ProgCPMId AS VARCHAR) + ')';
END
ELSE
    SET @ProgCPMId = (SELECT Id FROM Programs WHERE ProgramCode = 'L131');

IF NOT EXISTS (SELECT 1 FROM #ProgramMap WHERE SourceCode = 'L131')
    INSERT INTO #ProgramMap (SourceCode, NewId) VALUES ('L131', @ProgCPMId);

PRINT '  Step 1f complete: Programs resolved.';

-- 1g. ExamTypes
DECLARE @ExamTypeRegularId INT, @ExamTypePartialId INT;

IF NOT EXISTS (SELECT 1 FROM ExamTypes WHERE Code = '1')
BEGIN
    INSERT INTO ExamTypes (Code, Name, Remarks, IsActive)
    VALUES ('1', 'Regular', 'Regular examination', 1);
    PRINT '  Created ExamType: Regular';
END
SET @ExamTypeRegularId = (SELECT Id FROM ExamTypes WHERE Code = '1');

IF NOT EXISTS (SELECT 1 FROM ExamTypes WHERE Code = '2')
BEGIN
    INSERT INTO ExamTypes (Code, Name, Remarks, IsActive)
    VALUES ('2', 'Partial', 'Partial examination', 1);
    PRINT '  Created ExamType: Partial';
END
SET @ExamTypePartialId = (SELECT Id FROM ExamTypes WHERE Code = '2');

-- 1h. SubjectType (Compulsory)
DECLARE @SubjectTypeCompId INT;
IF NOT EXISTS (SELECT 1 FROM SubjectTypes WHERE Code = 'COMP')
BEGIN
    INSERT INTO SubjectTypes (Code, Name, IsDefault, IsActive)
    VALUES ('COMP', 'Compulsory', 1, 1);
    PRINT '  Created SubjectType: COMP';
END
SET @SubjectTypeCompId = (SELECT Id FROM SubjectTypes WHERE Code = 'COMP');

-- 1i. Genders (Male=1, Female=2, Other=3 - auto-increment may vary)
-- Use lookup by name
DECLARE @GenderMaleId INT = (SELECT Id FROM Genders WHERE GenderName = 'Male');
DECLARE @GenderFemaleId INT = (SELECT Id FROM Genders WHERE GenderName = 'Female');
DECLARE @GenderOtherId INT = (SELECT Id FROM Genders WHERE GenderName = 'Other');

IF @GenderMaleId IS NULL
BEGIN
    INSERT INTO Genders (GenderName, IsActive) VALUES ('Male', 1);
    SET @GenderMaleId = SCOPE_IDENTITY();
END
IF @GenderFemaleId IS NULL
BEGIN
    INSERT INTO Genders (GenderName, IsActive) VALUES ('Female', 1);
    SET @GenderFemaleId = SCOPE_IDENTITY();
END
IF @GenderOtherId IS NULL
BEGIN
    INSERT INTO Genders (GenderName, IsActive) VALUES ('Other', 1);
    SET @GenderOtherId = SCOPE_IDENTITY();
END
PRINT '  Step 1i complete: Genders resolved.';

-- 1j. StudentCategory (Regular)
IF NOT EXISTS (SELECT 1 FROM StudentCategories WHERE StudentCategoryName = 'Regular')
BEGIN
    INSERT INTO StudentCategories (StudentCategoryName, IsActive)
    VALUES ('Regular', 1);
    PRINT '  Created StudentCategory: Regular';
END
DECLARE @StudentCategoryRegularId INT = (SELECT Id FROM StudentCategories WHERE StudentCategoryName = 'Regular');

-- 1k. Semesters - one-time creation per program
-- Only create if they don't exist (seed data takes priority)
-- L092 Civil Engineering: SEM1-SEM6 (Year I-III, Part I-II)
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM1')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (1, 1, 'Semester 1', 'SEM1', '2014-01-01', '2014-06-30', @AY2014Id);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM2')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (2, 1, 'Semester 2', 'SEM2', '2014-07-01', '2014-12-31', @AY2014Id);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM3')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (3, 2, 'Semester 3', 'SEM3', '2015-01-01', '2015-06-30', @AY2014Id);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM4')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (4, 2, 'Semester 4', 'SEM4', '2015-07-01', '2015-12-31', @AY2014Id);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM5')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (5, 3, 'Semester 5', 'SEM5', '2016-01-01', '2016-06-30', @AY2014Id);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM6')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (6, 3, 'Semester 6', 'SEM6', '2016-07-01', '2016-12-31', @AY2014Id);

-- L117 Computer Engineering: CESEM1-CESEM2
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'CESEM1')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (1, 1, 'CE Semester 1', 'CESEM1', '2021-01-01', '2021-06-30', @AY2021Id);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'CESEM2')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (2, 1, 'CE Semester 2', 'CESEM2', '2021-07-01', '2021-12-31', @AY2021Id);

-- L131 CPM: CPMSEM1-CPMSEM2
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'CPMSEM1')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (1, 1, 'CPM Semester 1', 'CPMSEM1', '2023-01-01', '2023-06-30', @AY2023Id);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'CPMSEM2')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (2, 1, 'CPM Semester 2', 'CPMSEM2', '2023-07-01', '2023-12-31', @AY2023Id);

-- Semester IDs
DECLARE @SemCiv1 INT = (SELECT Id FROM Semesters WHERE Code = 'SEM1');
DECLARE @SemCiv2 INT = (SELECT Id FROM Semesters WHERE Code = 'SEM2');
DECLARE @SemCiv3 INT = (SELECT Id FROM Semesters WHERE Code = 'SEM3');
DECLARE @SemCiv4 INT = (SELECT Id FROM Semesters WHERE Code = 'SEM4');
DECLARE @SemCiv5 INT = (SELECT Id FROM Semesters WHERE Code = 'SEM5');
DECLARE @SemCiv6 INT = (SELECT Id FROM Semesters WHERE Code = 'SEM6');
DECLARE @SemCE1 INT = (SELECT Id FROM Semesters WHERE Code = 'CESEM1');
DECLARE @SemCE2 INT = (SELECT Id FROM Semesters WHERE Code = 'CESEM2');
DECLARE @SemCPM1 INT = (SELECT Id FROM Semesters WHERE Code = 'CPMSEM1');
DECLARE @SemCPM2 INT = (SELECT Id FROM Semesters WHERE Code = 'CPMSEM2');

PRINT 'Step 1 complete: All reference data ensured.';

-- ============================================================================
-- STEP 2: Create SubjectCatalogs (distinct subjects from all 3 source tables)
-- ============================================================================

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
WHERE SubjectCode IS NOT NULL AND SubjectCode <> 'NULL';

-- Insert new SubjectCatalogs only if they don't exist (protect seeded subjects)
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
        DECLARE @NewSubId INT = SCOPE_IDENTITY();
        INSERT INTO #SubjectCatalogMap (SourceCode, SourceName, NewId) VALUES (@SubCode, @SubName, @NewSubId);
    END
    ELSE
    BEGIN
        DECLARE @ExistingSubId INT = (SELECT Id FROM SubjectCatalogs WHERE SubjectCode = @SubCode);
        IF NOT EXISTS (SELECT 1 FROM #SubjectCatalogMap WHERE SourceCode = @SubCode AND SourceName = @SubName)
            INSERT INTO #SubjectCatalogMap (SourceCode, SourceName, NewId) VALUES (@SubCode, @SubName, @ExistingSubId);
    END

    FETCH NEXT FROM sub_cursor INTO @SubCode, @SubName, @CreditH;
END
CLOSE sub_cursor;
DEALLOCATE sub_cursor;

DECLARE @Cnt2 INT = (SELECT COUNT(*) FROM #SubjectCatalogMap);
PRINT 'Step 2 complete: SubjectCatalogs mapped. Count=' + CAST(@Cnt2 AS VARCHAR);

-- ============================================================================
-- STEP 3: Create SubjectOfferings (per Subject + Program + Semester)
-- ============================================================================

-- Civil Engineering (L092)
DECLARE @SoSubjectCatalogId INT, @SoProgramId INT, @SoSemesterId INT;
DECLARE @TheoryFM FLOAT, @TheoryPM FLOAT, @PracFM FLOAT, @PracPM FLOAT, @IntFM FLOAT, @IntPM FLOAT;
DECLARE @DisplayOrd INT;

DECLARE so_civ_cursor CURSOR FOR
    SELECT DISTINCT
        scm.NewId AS SubjectCatalogId,
        @ProgCivilId AS ProgramId,
        CASE
            WHEN Year = 'I' AND Part = 'I' THEN @SemCiv1
            WHEN Year = 'I' AND Part = 'II' THEN @SemCiv2
            WHEN Year = 'II' AND Part = 'I' THEN @SemCiv3
            WHEN Year = 'II' AND Part = 'II' THEN @SemCiv4
            WHEN Year = 'III' AND Part = 'I' THEN @SemCiv5
            WHEN Year = 'III' AND Part = 'II' THEN @SemCiv6
            ELSE @SemCiv1
        END AS SemesterId,
        MAX(CAST(TotalFM AS FLOAT)) AS TheoryFM,
        MAX(CAST(TotalPM AS FLOAT)) AS TheoryPM,
        MAX(CAST(TheoryFullMark AS FLOAT)) AS PracFM,
        MAX(CAST(TheoryPassMark AS FLOAT)) AS PracPM,
        MAX(CAST(InternalFullMark AS FLOAT)) AS IntFM,
        MAX(CAST(InternalPassMark AS FLOAT)) AS IntPM,
        MAX(CAST(DisplayOrder AS INT)) AS DisplayOrd
    FROM [FWUExams.Legacy].dbo.CivilEngineering ce
    INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(ce.SubjectCode)) = scm.SourceCode
    GROUP BY scm.NewId,
        CASE
            WHEN Year = 'I' AND Part = 'I' THEN @SemCiv1
            WHEN Year = 'I' AND Part = 'II' THEN @SemCiv2
            WHEN Year = 'II' AND Part = 'I' THEN @SemCiv3
            WHEN Year = 'II' AND Part = 'II' THEN @SemCiv4
            WHEN Year = 'III' AND Part = 'I' THEN @SemCiv5
            WHEN Year = 'III' AND Part = 'II' THEN @SemCiv6
            ELSE @SemCiv1
        END;

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
        DECLARE @NewSoId INT = SCOPE_IDENTITY();
        INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId) VALUES (@SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @NewSoId);
    END
    ELSE
    BEGIN
        DECLARE @ExistingSoId INT = (SELECT Id FROM SubjectOfferings WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId);
        IF NOT EXISTS (SELECT 1 FROM #SubjectOfferingMap WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId)
            INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId) VALUES (@SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @ExistingSoId);
    END

    FETCH NEXT FROM so_civ_cursor INTO @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @TheoryFM, @TheoryPM, @PracFM, @PracPM, @IntFM, @IntPM, @DisplayOrd;
END
CLOSE so_civ_cursor;
DEALLOCATE so_civ_cursor;

-- Computer Engineering (L117) - all subjects in CESEM1
DECLARE so_comp_cursor CURSOR FOR
    SELECT DISTINCT
        scm.NewId,
        @ProgCompId,
        @SemCE1,
        MAX(CAST(TotalFM AS FLOAT)),
        MAX(CAST(TotalPM AS FLOAT)),
        MAX(CAST(TheoryFullMark AS FLOAT)),
        MAX(CAST(TheoryPassMark AS FLOAT)),
        MAX(CAST(InternalFullMark AS FLOAT)),
        MAX(CAST(InternalPassMark AS FLOAT)),
        MAX(CAST(DisplayOrder AS INT))
    FROM [FWUExams.Legacy].dbo.ComputerEngineering ce
    INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(ce.SubjectCode)) = scm.SourceCode
    GROUP BY scm.NewId;

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
        DECLARE @ExistingSoId2 INT = (SELECT Id FROM SubjectOfferings WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId);
        IF NOT EXISTS (SELECT 1 FROM #SubjectOfferingMap WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId)
            INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId) VALUES (@SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @ExistingSoId2);
    END

    FETCH NEXT FROM so_comp_cursor INTO @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @TheoryFM, @TheoryPM, @PracFM, @PracPM, @IntFM, @IntPM, @DisplayOrd;
END
CLOSE so_comp_cursor;
DEALLOCATE so_comp_cursor;

-- CPM (L131)
DECLARE so_cpm_cursor CURSOR FOR
    SELECT DISTINCT
        scm.NewId,
        @ProgCPMId,
        CASE WHEN Year = 'I' AND Part = 'II' THEN @SemCPM2 ELSE @SemCPM1 END,
        MAX(CAST(TotalFM AS FLOAT)),
        MAX(CAST(TotalPM AS FLOAT)),
        MAX(CAST(TheoryFullMark AS FLOAT)),
        MAX(CAST(TheoryPassMark AS FLOAT)),
        MAX(CAST(InternalFullMark AS FLOAT)),
        MAX(CAST(InternalPassMark AS FLOAT)),
        MAX(CAST(DisplayOrder AS INT))
    FROM [FWUExams.Legacy].dbo.CPM cpm
    INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(cpm.SubjectCode)) = scm.SourceCode
    GROUP BY scm.NewId,
        CASE WHEN Year = 'I' AND Part = 'II' THEN @SemCPM2 ELSE @SemCPM1 END;

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
        DECLARE @ExistingSoId3 INT = (SELECT Id FROM SubjectOfferings WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId);
        IF NOT EXISTS (SELECT 1 FROM #SubjectOfferingMap WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId)
            INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId) VALUES (@SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @ExistingSoId3);
    END

    FETCH NEXT FROM so_cpm_cursor INTO @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @TheoryFM, @TheoryPM, @PracFM, @PracPM, @IntFM, @IntPM, @DisplayOrd;
END
CLOSE so_cpm_cursor;
DEALLOCATE so_cpm_cursor;

PRINT 'Step 3 complete: SubjectOfferings mapped.';

-- ============================================================================
-- STEP 4: Create ExamSchedules (per Program + AcademicYear + Semester)
-- ============================================================================

DECLARE es_cursor CURSOR FOR
    SELECT DISTINCT
        pm.NewId AS ProgramId,
        ay.NewId AS AcademicYearId,
        s.Id AS SemesterId,
        pm.SourceCode + ' ' + ay.SourceYear + ' Sem' + CAST(s.Number AS VARCHAR) AS EsName
    FROM #ProgramMap pm
    INNER JOIN #AcademicYearMap ay ON 1=1
    INNER JOIN Semesters s ON 1=1
    WHERE (pm.SourceCode = 'L092' AND s.Code IN ('SEM1','SEM2','SEM3','SEM4','SEM5','SEM6'))
       OR (pm.SourceCode = 'L117' AND s.Code IN ('CESEM1','CESEM2'))
       OR (pm.SourceCode = 'L131' AND s.Code IN ('CPMSEM1','CPMSEM2'));

OPEN es_cursor;
DECLARE @EsProgramId INT, @EsAcademicYearId INT, @EsSemesterId INT, @EsName NVARCHAR(50);
FETCH NEXT FROM es_cursor INTO @EsProgramId, @EsAcademicYearId, @EsSemesterId, @EsName;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @EsExamTypeId INT = @ExamTypeRegularId;
    DECLARE @EsLevelId INT = (SELECT LevelId FROM Programs WHERE Id = @EsProgramId);

    IF NOT EXISTS (SELECT 1 FROM ExamSchedules WHERE ProgramId = @EsProgramId AND AcademicYearId = @EsAcademicYearId AND SemesterId = @EsSemesterId)
    BEGIN
        INSERT INTO ExamSchedules (ExamScheduleName, AcademicYearId, ProgramId, SemesterId, ExamTypeId, LevelId, IsActive, StartTime, EndTime, TenantId)
        VALUES (@EsName, @EsAcademicYearId, @EsProgramId, @EsSemesterId, @EsExamTypeId, @EsLevelId, 1, '08:00', '11:00', @TenantId);
        DECLARE @EsId INT = SCOPE_IDENTITY();
        INSERT INTO #ExamScheduleMap (ProgramId, AcademicYearId, SemesterId, NewId) VALUES (@EsProgramId, @EsAcademicYearId, @EsSemesterId, @EsId);
    END
    ELSE
    BEGIN
        DECLARE @ExistingEsId INT = (SELECT Id FROM ExamSchedules WHERE ProgramId = @EsProgramId AND AcademicYearId = @EsAcademicYearId AND SemesterId = @EsSemesterId);
        IF NOT EXISTS (SELECT 1 FROM #ExamScheduleMap WHERE ProgramId = @EsProgramId AND AcademicYearId = @EsAcademicYearId AND SemesterId = @EsSemesterId)
            INSERT INTO #ExamScheduleMap (ProgramId, AcademicYearId, SemesterId, NewId) VALUES (@EsProgramId, @EsAcademicYearId, @EsSemesterId, @ExistingEsId);
    END

    FETCH NEXT FROM es_cursor INTO @EsProgramId, @EsAcademicYearId, @EsSemesterId, @EsName;
END
CLOSE es_cursor;
DEALLOCATE es_cursor;

PRINT 'Step 4 complete: ExamSchedules mapped.';

-- ============================================================================
-- STEP 5: Create ExamCenters (per ExamSchedule)
-- ============================================================================

INSERT INTO #ExamCenterMap (ExamScheduleId, CenterName, NewId)
SELECT DISTINCT esm.NewId, 'Kanchanpur', 0
FROM #ExamScheduleMap esm
WHERE NOT EXISTS (SELECT 1 FROM #ExamCenterMap ec WHERE ec.ExamScheduleId = esm.NewId);

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
        DECLARE @ExistingEcId INT = (SELECT Id FROM ExamCenters WHERE ExamScheduleId = @EcExamScheduleId AND Code = 1);
        UPDATE #ExamCenterMap SET NewId = @ExistingEcId WHERE ExamScheduleId = @EcExamScheduleId AND CenterName = @EcCenterName;
    END

    FETCH NEXT FROM ec_cursor INTO @EcExamScheduleId, @EcCenterName;
END
CLOSE ec_cursor;
DEALLOCATE ec_cursor;

PRINT 'Step 5 complete: ExamCenters created.';

-- ============================================================================
-- STEP 6: Create StudentRegistrations (deduplicated by RegistrationNo)
-- ============================================================================
-- PROTECTION: Uses IF NOT EXISTS to skip already-inserted students (seed data).

SELECT DISTINCT
    LTRIM(RTRIM(RegistrationNo)) AS RegistrationNo,
    LTRIM(RTRIM(FirstName)) AS FirstName,
    LTRIM(RTRIM(MiddleName)) AS MiddleName,
    LTRIM(RTRIM(LastName)) AS LastName,
    LTRIM(RTRIM(ContactNo)) AS ContactNo,
    LTRIM(RTRIM(Email)) AS Email,
    BirthDateAD,
    BirthDateBS,
    LTRIM(RTRIM(GenderName)) AS GenderName,
    CollegeID AS SourceCollegeId,
    LevelID AS SourceLevelId,
    FacultyID AS SourceFacultyId,
    AcademicYearName,
    LTRIM(RTRIM(FullNameNepali)) AS FullNameNepali,
    CASE WHEN IsCompleted = 0 THEN 1 ELSE 0 END AS IsActive
INTO #DistinctStudents
FROM (
    SELECT RegistrationNo, FirstName, MiddleName, LastName, ContactNo, Email, BirthDateAD, BirthDateBS, GenderName, CollegeID, LevelId, FacultyId, AcademicYearName, FullNameNepali, IsCompleted FROM [FWUExams.Legacy].dbo.CivilEngineering
    UNION
    SELECT RegistrationNo, FirstName, MiddleName, LastName, ContactNo, Email, BirthDateAD, BirthDateBS, GenderName, CollegeID, LevelId, FacultyId, AcademicYearName, FullNameNepali, IsCompleted FROM [FWUExams.Legacy].dbo.ComputerEngineering
    UNION
    SELECT RegistrationNo, FirstName, MiddleName, LastName, ContactNo, Email, BirthDateAD, BirthDateBS, GenderName, CollegeID, LevelId, FacultyId, AcademicYearName, FullNameNepali, IsCompleted FROM [FWUExams.Legacy].dbo.CPM
) AS AllStudents
WHERE RegistrationNo IS NOT NULL AND RegistrationNo <> 'NULL';

DECLARE @SrRegNo NVARCHAR(100), @SrFirstName NVARCHAR(80), @SrMiddleName NVARCHAR(30), @SrLastName NVARCHAR(30);
DECLARE @SrContact NVARCHAR(15), @SrEmail NVARCHAR(50), @SrDobAD NVARCHAR(50), @SrDobBS NVARCHAR(10);
DECLARE @SrGender NVARCHAR(50), @SrAyName NVARCHAR(50), @SrNepaliName NVARCHAR(100), @SrIsActive BIT;
DECLARE @SrGenderId INT, @SrAyId INT, @SrLevelId INT;

DECLARE sr_cursor CURSOR FOR
    SELECT RegistrationNo, FirstName, MiddleName, LastName, ContactNo, Email, BirthDateAD, BirthDateBS, GenderName, AcademicYearName, FullNameNepali, IsActive
    FROM #DistinctStudents;

OPEN sr_cursor;
FETCH NEXT FROM sr_cursor INTO @SrRegNo, @SrFirstName, @SrMiddleName, @SrLastName, @SrContact, @SrEmail, @SrDobAD, @SrDobBS, @SrGender, @SrAyName, @SrNepaliName, @SrIsActive;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SrGenderId = ISNULL((SELECT Id FROM Genders WHERE GenderName = @SrGender), 1);
    DECLARE @SrAyNameClean NVARCHAR(10) = CASE WHEN @SrAyName LIKE '%.0' THEN LEFT(@SrAyName, LEN(@SrAyName) - 2) ELSE @SrAyName END;
    SET @SrAyId = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = @SrAyNameClean);
    SET @SrLevelId = CASE WHEN @SrAyNameClean = '2023' THEN 2 ELSE 1 END;

    -- Only insert if registration number doesn't already exist
    IF NOT EXISTS (SELECT 1 FROM StudentRegistrations WHERE RegistrationNumber = @SrRegNo)
    BEGIN
        DECLARE @DobAdFormatted NVARCHAR(50) = NULL;
        IF @SrDobAD IS NOT NULL AND @SrDobAD <> 'NULL' AND @SrDobAD <> ''
        BEGIN
            SET @DobAdFormatted = CONVERT(NVARCHAR(50), TRY_CONVERT(DATE, @SrDobAD), 121);
        END

        INSERT INTO StudentRegistrations (LevelId, CollegeId, RegistrationNumber, FirstName, MiddleName, LastName,
            ContactNumber, Email, DateOfBirthBS, DateOfBirthAD, GenderId, StudentCategoryId, AcademicYearId,
            IsActive, NepaliName, TenantId)
        VALUES (@SrLevelId, @CollegeId, @SrRegNo, @SrFirstName, NULLIF(@SrMiddleName, 'NULL'),
            ISNULL(NULLIF(@SrLastName, 'NULL'), 'N/A'), NULLIF(@SrContact, 'NULL'), NULLIF(@SrEmail, 'NULL'),
            ISNULL(CONVERT(NVARCHAR(10), TRY_CONVERT(DATE, @SrDobBS), 103), 'N/A'),
            @DobAdFormatted,
            @SrGenderId, @StudentCategoryRegularId, @SrAyId,
            @SrIsActive, NULLIF(@SrNepaliName, 'NULL'), @TenantId);
        DECLARE @SrNewId INT = SCOPE_IDENTITY();
        INSERT INTO #StudentRegMap (RegistrationNo, NewId) VALUES (@SrRegNo, @SrNewId);
    END
    ELSE
    BEGIN
        DECLARE @SrExistingId INT = (SELECT Id FROM StudentRegistrations WHERE RegistrationNumber = @SrRegNo);
        IF NOT EXISTS (SELECT 1 FROM #StudentRegMap WHERE RegistrationNo = @SrRegNo)
            INSERT INTO #StudentRegMap (RegistrationNo, NewId) VALUES (@SrRegNo, @SrExistingId);
    END

    FETCH NEXT FROM sr_cursor INTO @SrRegNo, @SrFirstName, @SrMiddleName, @SrLastName, @SrContact, @SrEmail, @SrDobAD, @SrDobBS, @SrGender, @SrAyName, @SrNepaliName, @SrIsActive;
END
CLOSE sr_cursor;
DEALLOCATE sr_cursor;

DECLARE @Cnt6 INT = (SELECT COUNT(DISTINCT NewId) FROM #StudentRegMap);
PRINT 'Step 6 complete: StudentRegistrations mapped. Count=' + CAST(@Cnt6 AS VARCHAR);

-- ============================================================================
-- STEP 7: Create Batch record for legacy students
-- ============================================================================
-- Create one batch for each academic year found in legacy data.
-- This is needed for StudentAdmission records.

DECLARE @Batch2014Id INT, @Batch2021Id INT, @Batch2023Id INT;

IF NOT EXISTS (SELECT 1 FROM Batches WHERE BatchName = 'Legacy 2014 Batch')
BEGIN
    INSERT INTO Batches (AcademicYearId, BatchName, Remarks, IsActive)
    VALUES (@AY2014Id, 'Legacy 2014 Batch', 'Migrated from legacy CivilEngineering data', 1);
    SET @Batch2014Id = SCOPE_IDENTITY();
END
ELSE
    SET @Batch2014Id = (SELECT Id FROM Batches WHERE BatchName = 'Legacy 2014 Batch');

IF NOT EXISTS (SELECT 1 FROM Batches WHERE BatchName = 'Legacy 2021 Batch')
BEGIN
    INSERT INTO Batches (AcademicYearId, BatchName, Remarks, IsActive)
    VALUES (@AY2021Id, 'Legacy 2021 Batch', 'Migrated from legacy ComputerEngineering data', 1);
    SET @Batch2021Id = SCOPE_IDENTITY();
END
ELSE
    SET @Batch2021Id = (SELECT Id FROM Batches WHERE BatchName = 'Legacy 2021 Batch');

IF NOT EXISTS (SELECT 1 FROM Batches WHERE BatchName = 'Legacy 2023 Batch')
BEGIN
    INSERT INTO Batches (AcademicYearId, BatchName, Remarks, IsActive)
    VALUES (@AY2023Id, 'Legacy 2023 Batch', 'Migrated from legacy CPM data', 1);
    SET @Batch2023Id = SCOPE_IDENTITY();
END
ELSE
    SET @Batch2023Id = (SELECT Id FROM Batches WHERE BatchName = 'Legacy 2023 Batch');

PRINT 'Step 7 complete: Legacy batches created.';

-- ============================================================================
-- STEP 8: Create StudentAdmission records for each student
-- ============================================================================
-- Each legacy student gets a StudentAdmission record linked to their Registration.
-- The StudentRegistrationId FK on StudentAdmissions ties them together.
-- PROTECTION: Skips if admission already exists for that student + program + college.

-- Civil Engineering students (AY 2014, Program L092)
INSERT INTO #StudentAdmissionMap (RegistrationNo, AcademicYearId, SemesterId, NewId)
SELECT DISTINCT
    srm.RegistrationNo,
    @AY2014Id,
    @SemCiv1, -- Default semester
    0
FROM #StudentRegMap srm
INNER JOIN #DistinctStudents ds ON ds.RegistrationNo = srm.RegistrationNo
WHERE ds.AcademicYearName LIKE '2014%'
  AND NOT EXISTS (
      SELECT 1 FROM StudentAdmissions sa
      WHERE sa.StudentRegistrationId = srm.NewId
  );

-- Computer Engineering students (AY 2021, Program L117)
INSERT INTO #StudentAdmissionMap (RegistrationNo, AcademicYearId, SemesterId, NewId)
SELECT DISTINCT
    srm.RegistrationNo,
    @AY2021Id,
    @SemCE1,
    0
FROM #StudentRegMap srm
INNER JOIN #DistinctStudents ds ON ds.RegistrationNo = srm.RegistrationNo
WHERE ds.AcademicYearName LIKE '2021%'
  AND NOT EXISTS (
      SELECT 1 FROM StudentAdmissions sa
      WHERE sa.StudentRegistrationId = srm.NewId
  );

-- CPM students (AY 2023, Program L131)
INSERT INTO #StudentAdmissionMap (RegistrationNo, AcademicYearId, SemesterId, NewId)
SELECT DISTINCT
    srm.RegistrationNo,
    @AY2023Id,
    @SemCPM1,
    0
FROM #StudentRegMap srm
INNER JOIN #DistinctStudents ds ON ds.RegistrationNo = srm.RegistrationNo
WHERE ds.AcademicYearName LIKE '2023%'
  AND NOT EXISTS (
      SELECT 1 FROM StudentAdmissions sa
      WHERE sa.StudentRegistrationId = srm.NewId
  );

-- Now insert the actual StudentAdmission records
DECLARE @AdmRegNo NVARCHAR(100), @AdmAyId INT, @AdmSemId INT;
DECLARE @AdmProgId INT, @AdmBatchId INT;

DECLARE adm_cursor CURSOR FOR
    SELECT sam.RegistrationNo, sam.AcademicYearId, sam.SemesterId
    FROM #StudentAdmissionMap sam;

OPEN adm_cursor;
FETCH NEXT FROM adm_cursor INTO @AdmRegNo, @AdmAyId, @AdmSemId;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Determine program and batch based on academic year
    SET @AdmProgId = CASE @AdmAyId
        WHEN @AY2014Id THEN @ProgCivilId
        WHEN @AY2021Id THEN @ProgCompId
        WHEN @AY2023Id THEN @ProgCPMId
        ELSE @ProgCivilId
    END;

    SET @AdmBatchId = CASE @AdmAyId
        WHEN @AY2014Id THEN @Batch2014Id
        WHEN @AY2021Id THEN @Batch2021Id
        WHEN @AY2023Id THEN @Batch2023Id
        ELSE @Batch2014Id
    END;

    DECLARE @StudentRegId INT = (SELECT NewId FROM #StudentRegMap WHERE RegistrationNo = @AdmRegNo);

    IF NOT EXISTS (SELECT 1 FROM StudentAdmissions WHERE StudentRegistrationId = @StudentRegId AND ProgramsId = @AdmProgId AND CollegeId = @CollegeId)
    BEGIN
        INSERT INTO StudentAdmissions (
            TenantId, ProgramsId, CollegeId, AdmissionDate, IsCompleted, IsActive,
            HasFeeExemption, BatchId, StudentRegistrationId
        )
        VALUES (
            @TenantId, @AdmProgId, @CollegeId, GETDATE(), 1, 1,
            0, @AdmBatchId, @StudentRegId
        );

        UPDATE #StudentAdmissionMap
        SET NewId = SCOPE_IDENTITY()
        WHERE RegistrationNo = @AdmRegNo AND AcademicYearId = @AdmAyId AND SemesterId = @AdmSemId;
    END
    ELSE
    BEGIN
        UPDATE #StudentAdmissionMap
        SET NewId = (SELECT Id FROM StudentAdmissions WHERE StudentRegistrationId = @StudentRegId AND ProgramsId = @AdmProgId AND CollegeId = @CollegeId)
        WHERE RegistrationNo = @AdmRegNo AND AcademicYearId = @AdmAyId AND SemesterId = @AdmSemId;
    END

    FETCH NEXT FROM adm_cursor INTO @AdmRegNo, @AdmAyId, @AdmSemId;
END
CLOSE adm_cursor;
DEALLOCATE adm_cursor;

DECLARE @Cnt8 INT = (SELECT COUNT(DISTINCT NewId) FROM #StudentAdmissionMap WHERE NewId > 0);
PRINT 'Step 8 complete: StudentAdmissions created. Count=' + CAST(@Cnt8 AS VARCHAR);

-- ============================================================================
-- STEP 9: Create SemesterEnrollment records
-- ============================================================================
-- Each student gets enrolled in the semester matching their Year/Part from source.
-- PROTECTION: Skips if enrollment already exists.

-- For Civil Engineering: map each source row's Year+Part to the correct Semester
INSERT INTO #SemesterEnrollmentMap (StudentAdmissionId, SemesterId, NewId)
SELECT DISTINCT
    sam.NewId AS StudentAdmissionId,
    CASE
        WHEN ce.Year = 'I' AND ce.Part = 'I' THEN @SemCiv1
        WHEN ce.Year = 'I' AND ce.Part = 'II' THEN @SemCiv2
        WHEN ce.Year = 'II' AND ce.Part = 'I' THEN @SemCiv3
        WHEN ce.Year = 'II' AND ce.Part = 'II' THEN @SemCiv4
        WHEN ce.Year = 'III' AND ce.Part = 'I' THEN @SemCiv5
        WHEN ce.Year = 'III' AND ce.Part = 'II' THEN @SemCiv6
        ELSE @SemCiv1
    END AS SemesterId,
    0 AS NewId
FROM #StudentAdmissionMap sam
INNER JOIN #StudentRegMap srm ON srm.RegistrationNo = sam.RegistrationNo
INNER JOIN [FWUExams.Legacy].dbo.CivilEngineering ce ON LTRIM(RTRIM(ce.RegistrationNo)) = srm.RegistrationNo
WHERE sam.NewId > 0
  AND NOT EXISTS (
      SELECT 1 FROM SemesterEnrollments se
      WHERE se.StudentAdmissionId = sam.NewId
  );

-- For Computer Engineering: all in CESEM1
INSERT INTO #SemesterEnrollmentMap (StudentAdmissionId, SemesterId, NewId)
SELECT DISTINCT
    sam.NewId,
    @SemCE1,
    0
FROM #StudentAdmissionMap sam
INNER JOIN #StudentRegMap srm ON srm.RegistrationNo = sam.RegistrationNo
INNER JOIN [FWUExams.Legacy].dbo.ComputerEngineering ce ON LTRIM(RTRIM(ce.RegistrationNo)) = srm.RegistrationNo
WHERE sam.NewId > 0
  AND sam.AcademicYearId = @AY2021Id
  AND NOT EXISTS (
      SELECT 1 FROM SemesterEnrollments se
      WHERE se.StudentAdmissionId = sam.NewId
  );

-- For CPM: map by Year+Part
INSERT INTO #SemesterEnrollmentMap (StudentAdmissionId, SemesterId, NewId)
SELECT DISTINCT
    sam.NewId,
    CASE
        WHEN cpm.Year = 'I' AND cpm.Part = 'II' THEN @SemCPM2
        ELSE @SemCPM1
    END,
    0
FROM #StudentAdmissionMap sam
INNER JOIN #StudentRegMap srm ON srm.RegistrationNo = sam.RegistrationNo
INNER JOIN [FWUExams.Legacy].dbo.CPM cpm ON LTRIM(RTRIM(cpm.RegistrationNo)) = srm.RegistrationNo
WHERE sam.NewId > 0
  AND sam.AcademicYearId = @AY2023Id
  AND NOT EXISTS (
      SELECT 1 FROM SemesterEnrollments se
      WHERE se.StudentAdmissionId = sam.NewId
  );

-- Insert SemesterEnrollment records
DECLARE @EnrollAdmId INT, @EnrollSemId INT;

DECLARE enr_cursor CURSOR FOR
    SELECT StudentAdmissionId, SemesterId FROM #SemesterEnrollmentMap WHERE NewId = 0;
OPEN enr_cursor;
FETCH NEXT FROM enr_cursor INTO @EnrollAdmId, @EnrollSemId;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM SemesterEnrollments WHERE StudentAdmissionId = @EnrollAdmId AND SemesterId = @EnrollSemId)
    BEGIN
        INSERT INTO SemesterEnrollments (
            StudentAdmissionId, SemesterId, EnrollmentStatus, EnrollmentType,
            PaymentStatus, EnrolledDate, TotalCredits, GradePoints, TotalFee, PaidAmount,
            Deficiency, ResultStatus, TenantId
        )
        VALUES (
            @EnrollAdmId, @EnrollSemId, 0 /* Enrolled */, 0 /* Regular */,
            0 /* Pending */, GETDATE(), 0, 0, 0, 0,
            0 /* Not deficient */, 0 /* Unknown */, @TenantId
        );

        UPDATE #SemesterEnrollmentMap
        SET NewId = SCOPE_IDENTITY()
        WHERE StudentAdmissionId = @EnrollAdmId AND SemesterId = @EnrollSemId;
    END
    ELSE
    BEGIN
        UPDATE #SemesterEnrollmentMap
        SET NewId = (SELECT Id FROM SemesterEnrollments WHERE StudentAdmissionId = @EnrollAdmId AND SemesterId = @EnrollSemId)
        WHERE StudentAdmissionId = @EnrollAdmId AND SemesterId = @EnrollSemId;
    END

    FETCH NEXT FROM enr_cursor INTO @EnrollAdmId, @EnrollSemId;
END
CLOSE enr_cursor;
DEALLOCATE enr_cursor;

DECLARE @Cnt9 INT = (SELECT COUNT(DISTINCT NewId) FROM #SemesterEnrollmentMap WHERE NewId > 0);
PRINT 'Step 9 complete: SemesterEnrollments created. Count=' + CAST(@Cnt9 AS VARCHAR);

-- ============================================================================
-- STEP 10: Create StudentGuardians (placeholder)
-- ============================================================================
-- Legacy data does NOT contain guardian information.
-- We create basic placeholder records with NULL values to establish the FK,
-- ONLY for students who don't already have a guardian record.
-- PROTECTION: Skips if guardian already exists.

INSERT INTO StudentGuardians (
    TenantId, StudentRegistrationId,
    FatherName, MotherName, GuardianName
)
SELECT
    @TenantId, srm.NewId,
    'N/A (Legacy)', 'N/A (Legacy)', 'N/A (Legacy)'
FROM #StudentRegMap srm
WHERE NOT EXISTS (
    SELECT 1 FROM StudentGuardians sg WHERE sg.StudentRegistrationId = srm.NewId
);

DECLARE @Cnt10 INT = @@ROWCOUNT;
PRINT 'Step 10 complete: StudentGuardians created. Count=' + CAST(@Cnt10 AS VARCHAR);

-- ============================================================================
-- STEP 11: Create StudentQualifications (placeholder)
-- ============================================================================
-- Legacy data does NOT contain qualification information.
-- We skip this since it requires BoardId and PreviousLevelId which vary.
-- Admin should enter qualifications manually for legacy students.
-- PROTECTION: No insert - qualifications are optional and better entered manually.

PRINT 'Step 11 skipped: StudentQualifications - no legacy data available. Enter manually.';

-- ============================================================================
-- STEP 12: Create ExamRegistrations (deduplicated by ExamRegistrationID)
-- ============================================================================
-- PROTECTION: Uses identity insert with IF NOT EXISTS.

SELECT DISTINCT
    CAST(ExamRegistrationID AS INT) AS SourceExamRegId,
    LTRIM(RTRIM(RegistrationNo)) AS RegistrationNo,
    NULLIF(LTRIM(RTRIM(CAST(ExamRollNo AS NVARCHAR(50)))), '') AS ExamRollNo,
    NULLIF(LTRIM(RTRIM(CAST(ExamRollNoCoding AS NVARCHAR(50)))), '') AS ExamRollNoCoding,
    AcademicYearName,
    LTRIM(RTRIM(ExamTypeName)) AS ExamTypeName,
    LTRIM(RTRIM(ExamCenterName)) AS ExamCenterName,
    NULLIF(LTRIM(RTRIM(CAST(SGPA AS NVARCHAR(50)))), '') AS SGPA,
    LTRIM(RTRIM(GradeLetter)) AS GradeLetter,
    ProgramCode
INTO #DistinctExamRegs
FROM (
    SELECT CAST(ExamRegistrationID AS INT) AS ExamRegistrationID, RegistrationNo, CAST(ExamRollNo AS NVARCHAR(50)) AS ExamRollNo, ExamRollNoCoding, AcademicYearName, ExamTypeName, ExamCenterName, CAST(SGPA AS NVARCHAR(50)) AS SGPA, GradeLetter, ProgramCode FROM [FWUExams.Legacy].dbo.CivilEngineering
    UNION
    SELECT CAST(ExamRegistrationID AS INT) AS ExamRegistrationID, RegistrationNo, CAST(ExamRollNo AS NVARCHAR(50)) AS ExamRollNo, ExamRollNoCoding, AcademicYearName, ExamTypeName, ExamCenterName, CAST(SGPA AS NVARCHAR(50)) AS SGPA, GradeLetter, ProgramCode FROM [FWUExams.Legacy].dbo.ComputerEngineering
    UNION
    SELECT CAST(ExamRegistrationID AS INT) AS ExamRegistrationID, RegistrationNo, CAST(ExamRollNo AS NVARCHAR(50)) AS ExamRollNo, CAST(ExamRollNoCoding AS NVARCHAR(50)) AS ExamRollNoCoding, AcademicYearName, ExamTypeName, ExamCenterName, CAST(SGPA AS NVARCHAR(50)) AS SGPA, GradeLetter, ProgramCode FROM [FWUExams.Legacy].dbo.CPM
) AS AllExamRegs
WHERE ExamRegistrationID IS NOT NULL;

DECLARE @ErSourceId INT, @ErRegNo NVARCHAR(100), @ErRollNo NVARCHAR(50), @ErRollNoCoding NVARCHAR(50);
DECLARE @ErAyName NVARCHAR(50), @ErExamType NVARCHAR(50), @ErCenterName NVARCHAR(200);
DECLARE @ErSgpa NVARCHAR(50), @ErGrade NVARCHAR(50), @ErProgCode NVARCHAR(50);
DECLARE @ErStudentRegId INT, @ErAyId INT, @ErExamTypeId INT, @ErProgId INT, @ErEsId INT, @ErEcId INT;
DECLARE @ErEnrollmentId INT;

DECLARE er_cursor CURSOR FOR
    SELECT SourceExamRegId, RegistrationNo, ExamRollNo, ExamRollNoCoding, AcademicYearName, ExamTypeName, ExamCenterName, SGPA, GradeLetter, ProgramCode
    FROM #DistinctExamRegs;

OPEN er_cursor;
FETCH NEXT FROM er_cursor INTO @ErSourceId, @ErRegNo, @ErRollNo, @ErRollNoCoding, @ErAyName, @ErExamType, @ErCenterName, @ErSgpa, @ErGrade, @ErProgCode;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @ErStudentRegId = (SELECT TOP 1 NewId FROM #StudentRegMap WHERE RegistrationNo = @ErRegNo);
    DECLARE @ErAyNameClean NVARCHAR(10) = CASE WHEN @ErAyName LIKE '%.0' THEN LEFT(@ErAyName, LEN(@ErAyName) - 2) ELSE @ErAyName END;
    SET @ErAyId = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = @ErAyNameClean);
    SET @ErExamTypeId = CASE WHEN @ErExamType = 'Partial' THEN @ExamTypePartialId ELSE @ExamTypeRegularId END;
    SET @ErProgId = (SELECT NewId FROM #ProgramMap WHERE SourceCode = @ErProgCode);
    SET @ErEsId = (SELECT TOP 1 NewId FROM #ExamScheduleMap WHERE ProgramId = @ErProgId AND AcademicYearId = @ErAyId);
    SET @ErEcId = (SELECT TOP 1 NewId FROM #ExamCenterMap WHERE ExamScheduleId = @ErEsId);

    -- Find SemesterEnrollmentId for this student's schedule
    DECLARE @ErAdmId INT = (SELECT TOP 1 NewId FROM #StudentAdmissionMap WHERE RegistrationNo = @ErRegNo);
    SET @ErEnrollmentId = (SELECT TOP 1 NewId FROM #SemesterEnrollmentMap WHERE StudentAdmissionId = @ErAdmId);

    IF @ErStudentRegId IS NOT NULL AND @ErAyId IS NOT NULL AND @ErProgId IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM ExamRegistrations WHERE Id = @ErSourceId)
        BEGIN
            SET IDENTITY_INSERT ExamRegistrations ON;
            INSERT INTO ExamRegistrations (
                Id, AcademicYearId, ExamCenterId, CollegeId, ExamRollNumber,
                Sgpa, IsActive, ExamScheduleId, ProgramsId, TenantId, Status,
                SemesterEnrollmentId
            )
            VALUES (
                @ErSourceId, @ErAyId, @ErEcId, @CollegeId, NULLIF(@ErRollNo, 'NULL'),
                NULLIF(@ErSgpa, 'NULL'), 1, @ErEsId, @ErProgId, @TenantId, 1,
                @ErEnrollmentId
            );
            SET IDENTITY_INSERT ExamRegistrations OFF;
            INSERT INTO #ExamRegMap (SourceExamRegId, NewId) VALUES (@ErSourceId, @ErSourceId);
        END
        ELSE
        BEGIN
            -- Update existing exam registration with SemesterEnrollmentId if null
            UPDATE ExamRegistrations
            SET SemesterEnrollmentId = COALESCE(SemesterEnrollmentId, @ErEnrollmentId)
            WHERE Id = @ErSourceId AND SemesterEnrollmentId IS NULL;

            IF NOT EXISTS (SELECT 1 FROM #ExamRegMap WHERE SourceExamRegId = @ErSourceId)
                INSERT INTO #ExamRegMap (SourceExamRegId, NewId) VALUES (@ErSourceId, @ErSourceId);
        END
    END

    FETCH NEXT FROM er_cursor INTO @ErSourceId, @ErRegNo, @ErRollNo, @ErRollNoCoding, @ErAyName, @ErExamType, @ErCenterName, @ErSgpa, @ErGrade, @ErProgCode;
END
CLOSE er_cursor;
DEALLOCATE er_cursor;

DECLARE @Cnt12 INT = (SELECT COUNT(*) FROM #ExamRegMap);
PRINT 'Step 12 complete: ExamRegistrations mapped. Count=' + CAST(@Cnt12 AS VARCHAR);

-- ============================================================================
-- STEP 13: Create ExamSubjectResults (one per source row)
-- ============================================================================
-- PROTECTION: Uses WHERE NOT EXISTS to skip already-inserted results.
-- Check if results already exist to avoid duplicates.

-- Civil Engineering
INSERT INTO ExamSubjectResults (ExamRegistrationId, ExamTypeId, SubjectOfferingId, ExamScheduleId,
    ObtainedMarksTheory, ObtainedMarksPractical, ObtainedMarksTheoryInternal, ObtainedMarksPracticalInternal,
    ObtainedMarks, GradeLetter, Remarks, IsActive, IsSubmitted, TenantId, CreatedDate)
SELECT
    er.Id AS ExamRegistrationId,
    CASE WHEN ce.ExamTypeName = 'Partial' THEN @ExamTypePartialId ELSE @ExamTypeRegularId END AS ExamTypeId,
    som.NewId AS SubjectOfferingId,
    esm.NewId AS ExamScheduleId,
    CASE WHEN ce.ObtainedMarks IS NOT NULL AND ISNUMERIC(ce.ObtainedMarks) = 1 THEN CAST(ce.ObtainedMarks AS FLOAT) ELSE NULL END AS ObtainedMarksTheory,
    CASE WHEN ce.PracticalMarks IS NOT NULL AND ISNUMERIC(ce.PracticalMarks) = 1 THEN CAST(ce.PracticalMarks AS FLOAT) ELSE NULL END AS ObtainedMarksPractical,
    CASE WHEN ce.InternalMarks IS NOT NULL AND ISNUMERIC(ce.InternalMarks) = 1 THEN CAST(ce.InternalMarks AS FLOAT) ELSE NULL END AS ObtainedMarksTheoryInternal,
    CASE WHEN ce.InternalMarksFinal IS NOT NULL AND ISNUMERIC(ce.InternalMarksFinal) = 1 THEN CAST(ce.InternalMarksFinal AS FLOAT) ELSE NULL END AS ObtainedMarksPracticalInternal,
    CASE WHEN ce.TotalOM IS NOT NULL AND ISNUMERIC(ce.TotalOM) = 1 THEN CAST(ce.TotalOM AS FLOAT) ELSE NULL END AS ObtainedMarks,
    CASE WHEN ce.GradeLetter IS NOT NULL AND ce.GradeLetter <> 'NULL' THEN LTRIM(RTRIM(ce.GradeLetter)) ELSE NULL END AS GradeLetter,
    CASE WHEN ce.Rem IS NOT NULL AND ce.Rem <> 'NULL' THEN LTRIM(RTRIM(ce.Rem)) ELSE NULL END AS Remarks,
    1 AS IsActive,
    CASE WHEN TRY_CAST(LTRIM(RTRIM(ce.IsResultConfirm)) AS INT) = 1 THEN 1 ELSE 0 END AS IsSubmitted,
    @TenantId AS TenantId,
    GETDATE() AS CreatedDate
FROM [FWUExams.Legacy].dbo.CivilEngineering ce
INNER JOIN ExamRegistrations er ON TRY_CAST(ce.ExamRegistrationID AS INT) = er.Id
INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(ce.SubjectCode)) = scm.SourceCode
INNER JOIN #SubjectOfferingMap som ON som.SubjectCatalogId = scm.NewId AND som.ProgramId = @ProgCivilId
    AND som.SemesterId = CASE
        WHEN ce.Year = 'I' AND ce.Part = 'I' THEN @SemCiv1
        WHEN ce.Year = 'I' AND ce.Part = 'II' THEN @SemCiv2
        WHEN ce.Year = 'II' AND ce.Part = 'I' THEN @SemCiv3
        WHEN ce.Year = 'II' AND ce.Part = 'II' THEN @SemCiv4
        WHEN ce.Year = 'III' AND ce.Part = 'I' THEN @SemCiv5
        WHEN ce.Year = 'III' AND ce.Part = 'II' THEN @SemCiv6
        ELSE @SemCiv1
    END
INNER JOIN #AcademicYearMap ay_src ON ay_src.SourceYear = LEFT(CAST(ce.AcademicYearName AS NVARCHAR(50)), CHARINDEX('.', CAST(ce.AcademicYearName AS NVARCHAR(50)) + '.') - 1)
LEFT JOIN #ExamScheduleMap esm ON esm.ProgramId = @ProgCivilId AND esm.AcademicYearId = ay_src.NewId
    AND esm.SemesterId = som.SemesterId
WHERE NOT EXISTS (
    SELECT 1 FROM ExamSubjectResults esr
    WHERE esr.ExamRegistrationId = er.Id
      AND esr.SubjectOfferingId = som.NewId
);

PRINT 'Step 13a: ExamSubjectResults for CivilEngineering inserted. Rows=' + CAST(@@ROWCOUNT AS VARCHAR);

-- Computer Engineering
INSERT INTO ExamSubjectResults (ExamRegistrationId, ExamTypeId, SubjectOfferingId, ExamScheduleId,
    ObtainedMarksTheory, ObtainedMarksPractical, ObtainedMarksTheoryInternal, ObtainedMarksPracticalInternal,
    ObtainedMarks, GradeLetter, Remarks, IsActive, IsSubmitted, TenantId, CreatedDate)
SELECT
    er.Id AS ExamRegistrationId,
    CASE WHEN ce.ExamTypeName = 'Partial' THEN @ExamTypePartialId ELSE @ExamTypeRegularId END AS ExamTypeId,
    som.NewId AS SubjectOfferingId,
    esm.NewId AS ExamScheduleId,
    CASE WHEN ce.ObtainedMarks IS NOT NULL AND ISNUMERIC(ce.ObtainedMarks) = 1 THEN CAST(ce.ObtainedMarks AS FLOAT) ELSE NULL END AS ObtainedMarksTheory,
    CASE WHEN ce.PracticalMarks IS NOT NULL AND ISNUMERIC(ce.PracticalMarks) = 1 THEN CAST(ce.PracticalMarks AS FLOAT) ELSE NULL END AS ObtainedMarksPractical,
    CASE WHEN ce.InternalMarks IS NOT NULL AND ISNUMERIC(ce.InternalMarks) = 1 THEN CAST(ce.InternalMarks AS FLOAT) ELSE NULL END AS ObtainedMarksTheoryInternal,
    CASE WHEN ce.InternalMarksFinal IS NOT NULL AND ISNUMERIC(ce.InternalMarksFinal) = 1 THEN CAST(ce.InternalMarksFinal AS FLOAT) ELSE NULL END AS ObtainedMarksPracticalInternal,
    CASE WHEN ce.TotalOM IS NOT NULL AND ISNUMERIC(ce.TotalOM) = 1 THEN CAST(ce.TotalOM AS FLOAT) ELSE NULL END AS ObtainedMarks,
    CASE WHEN ce.GradeLetter IS NOT NULL AND ce.GradeLetter <> 'NULL' THEN LTRIM(RTRIM(ce.GradeLetter)) ELSE NULL END AS GradeLetter,
    CASE WHEN ce.Rem IS NOT NULL AND ce.Rem <> 'NULL' THEN LTRIM(RTRIM(ce.Rem)) ELSE NULL END AS Remarks,
    1 AS IsActive,
    CASE WHEN TRY_CAST(LTRIM(RTRIM(ce.IsResultConfirm)) AS INT) = 1 THEN 1 ELSE 0 END AS IsSubmitted,
    @TenantId AS TenantId,
    GETDATE() AS CreatedDate
FROM [FWUExams.Legacy].dbo.ComputerEngineering ce
INNER JOIN ExamRegistrations er ON ce.ExamRegistrationID = er.Id
INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(ce.SubjectCode)) = scm.SourceCode
INNER JOIN #SubjectOfferingMap som ON som.SubjectCatalogId = scm.NewId AND som.ProgramId = @ProgCompId
    AND som.SemesterId = @SemCE1
INNER JOIN #AcademicYearMap ay_src ON ay_src.SourceYear = LEFT(CAST(ce.AcademicYearName AS NVARCHAR(50)), CHARINDEX('.', CAST(ce.AcademicYearName AS NVARCHAR(50)) + '.') - 1)
LEFT JOIN #ExamScheduleMap esm ON esm.ProgramId = @ProgCompId AND esm.AcademicYearId = ay_src.NewId
    AND esm.SemesterId = @SemCE1
WHERE NOT EXISTS (
    SELECT 1 FROM ExamSubjectResults esr
    WHERE esr.ExamRegistrationId = er.Id
      AND esr.SubjectOfferingId = som.NewId
);

PRINT 'Step 13b: ExamSubjectResults for ComputerEngineering inserted. Rows=' + CAST(@@ROWCOUNT AS VARCHAR);

-- CPM
INSERT INTO ExamSubjectResults (ExamRegistrationId, ExamTypeId, SubjectOfferingId, ExamScheduleId,
    ObtainedMarksTheory, ObtainedMarksPractical, ObtainedMarksTheoryInternal, ObtainedMarksPracticalInternal,
    ObtainedMarks, GradeLetter, Remarks, IsActive, IsSubmitted, TenantId, CreatedDate)
SELECT
    er.Id AS ExamRegistrationId,
    CASE WHEN cpm.ExamTypeName = 'Partial' THEN @ExamTypePartialId ELSE @ExamTypeRegularId END AS ExamTypeId,
    som.NewId AS SubjectOfferingId,
    esm.NewId AS ExamScheduleId,
    CASE WHEN cpm.ObtainedMarks IS NOT NULL AND ISNUMERIC(cpm.ObtainedMarks) = 1 THEN CAST(cpm.ObtainedMarks AS FLOAT) ELSE NULL END AS ObtainedMarksTheory,
    CASE WHEN cpm.PracticalMarks IS NOT NULL AND ISNUMERIC(cpm.PracticalMarks) = 1 THEN CAST(cpm.PracticalMarks AS FLOAT) ELSE NULL END AS ObtainedMarksPractical,
    CASE WHEN cpm.InternalMarks IS NOT NULL AND ISNUMERIC(cpm.InternalMarks) = 1 THEN CAST(cpm.InternalMarks AS FLOAT) ELSE NULL END AS ObtainedMarksTheoryInternal,
    CASE WHEN cpm.InternalMarksFinal IS NOT NULL AND ISNUMERIC(cpm.InternalMarksFinal) = 1 THEN CAST(cpm.InternalMarksFinal AS FLOAT) ELSE NULL END AS ObtainedMarksPracticalInternal,
    CASE WHEN cpm.TotalOM IS NOT NULL AND ISNUMERIC(cpm.TotalOM) = 1 THEN CAST(cpm.TotalOM AS FLOAT) ELSE NULL END AS ObtainedMarks,
    CASE WHEN cpm.GradeLetter IS NOT NULL AND cpm.GradeLetter <> 'NULL' THEN LTRIM(RTRIM(cpm.GradeLetter)) ELSE NULL END AS GradeLetter,
    CASE WHEN cpm.Rem IS NOT NULL AND cpm.Rem <> 'NULL' THEN LTRIM(RTRIM(cpm.Rem)) ELSE NULL END AS Remarks,
    1 AS IsActive,
    CASE WHEN TRY_CAST(LTRIM(RTRIM(cpm.IsResultConfirm)) AS INT) = 1 THEN 1 ELSE 0 END AS IsSubmitted,
    @TenantId AS TenantId,
    GETDATE() AS CreatedDate
FROM [FWUExams.Legacy].dbo.CPM cpm
INNER JOIN ExamRegistrations er ON cpm.ExamRegistrationID = er.Id
INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(cpm.SubjectCode)) = scm.SourceCode
INNER JOIN #SubjectOfferingMap som ON som.SubjectCatalogId = scm.NewId AND som.ProgramId = @ProgCPMId
    AND som.SemesterId = CASE
        WHEN cpm.Year = 'I' AND cpm.Part = 'II' THEN @SemCPM2
        ELSE @SemCPM1
    END
INNER JOIN #AcademicYearMap ay_src ON ay_src.SourceYear = LEFT(CAST(cpm.AcademicYearName AS NVARCHAR(50)), CHARINDEX('.', CAST(cpm.AcademicYearName AS NVARCHAR(50)) + '.') - 1)
LEFT JOIN #ExamScheduleMap esm ON esm.ProgramId = @ProgCPMId AND esm.AcademicYearId = ay_src.NewId
    AND esm.SemesterId = som.SemesterId
WHERE NOT EXISTS (
    SELECT 1 FROM ExamSubjectResults esr
    WHERE esr.ExamRegistrationId = er.Id
      AND esr.SubjectOfferingId = som.NewId
);

PRINT 'Step 13c: ExamSubjectResults for CPM inserted. Rows=' + CAST(@@ROWCOUNT AS VARCHAR);

-- ============================================================================
-- STEP 14: Link StudentAdmission to AppUser (if matching email exists)
-- ============================================================================
-- If a legacy student has an email that matches an existing AppUser,
-- link their StudentAdmission to that AppUser.
-- PROTECTION: Only updates if AppUserId is null.

UPDATE sa
SET sa.AppUserId = u.Id
FROM StudentAdmissions sa
INNER JOIN #StudentAdmissionMap sam ON sam.NewId = sa.Id
INNER JOIN #StudentRegMap srm ON srm.RegistrationNo = sam.RegistrationNo
INNER JOIN StudentRegistrations sr ON sr.Id = srm.NewId
INNER JOIN [Users] u ON u.Email = sr.Email
WHERE sa.AppUserId IS NULL;

PRINT 'Step 14 complete: StudentAdmission-AppUser links updated. Rows=' + CAST(@@ROWCOUNT AS VARCHAR);

-- ============================================================================
-- VERIFICATION
-- ============================================================================

PRINT '';
PRINT '============================================================';
PRINT '  VERIFICATION REPORT';
PRINT '============================================================';

PRINT '';

-- Source counts
SELECT 'Source: CivilEngineering' AS DataSource, COUNT(*) AS RecordCount FROM [FWUExams.Legacy].dbo.CivilEngineering
UNION ALL
SELECT 'Source: ComputerEngineering', COUNT(*) FROM [FWUExams.Legacy].dbo.ComputerEngineering
UNION ALL
SELECT 'Source: CPM', COUNT(*) FROM [FWUExams.Legacy].dbo.CPM;

-- Target counts (legacy-related only)
SELECT 'Target: StudentRegistrations (Legacy AY)' AS DataSource, COUNT(*) AS RecordCount FROM StudentRegistrations WHERE AcademicYearId IN (@AY2014Id, @AY2021Id, @AY2023Id)
UNION ALL
SELECT 'Target: StudentAdmissions (Legacy)', COUNT(*) FROM StudentAdmissions sa INNER JOIN #StudentAdmissionMap sam ON sam.NewId = sa.Id
UNION ALL
SELECT 'Target: SemesterEnrollments (Legacy)', COUNT(*) FROM SemesterEnrollments se INNER JOIN #SemesterEnrollmentMap sem ON sem.NewId = se.Id
UNION ALL
SELECT 'Target: StudentGuardians (Legacy)', COUNT(*) FROM StudentGuardians sg INNER JOIN #StudentRegMap srm ON srm.NewId = sg.StudentRegistrationId
UNION ALL
SELECT 'Target: ExamRegistrations (Legacy)', COUNT(*) FROM ExamRegistrations WHERE Id IN (SELECT SourceExamRegId FROM #DistinctExamRegs)
UNION ALL
SELECT 'Target: ExamSubjectResults (All)', COUNT(*) FROM ExamSubjectResults WHERE TenantId = @TenantId;

PRINT '';

-- Total distinct legacy students
SELECT COUNT(DISTINCT RegistrationNo) AS DistinctLegacyStudents FROM #DistinctStudents;

-- Spot check
PRINT '';
PRINT '--- SPOT CHECK: Sample student from each source ---';
PRINT '';

SELECT TOP 3 'CivilEngineering' AS Source, RegistrationNo, FirstName, LastName FROM [FWUExams.Legacy].dbo.CivilEngineering ORDER BY RegistrationNo;
SELECT TOP 3 'ComputerEngineering' AS Source, RegistrationNo, FirstName, LastName FROM [FWUExams.Legacy].dbo.ComputerEngineering ORDER BY RegistrationNo;
SELECT TOP 3 'CPM' AS Source, RegistrationNo, FirstName, LastName FROM [FWUExams.Legacy].dbo.CPM ORDER BY RegistrationNo;

PRINT '';
PRINT '--- STUDENT REGISTRATION -> ADMISSION -> ENROLLMENT VERIFICATION ---';
SELECT TOP 5
    sr.RegistrationNumber,
    sr.FirstName + ' ' + sr.LastName AS StudentName,
    CASE WHEN sa.Id IS NOT NULL THEN 'YES' ELSE 'NO' END AS HasAdmission,
    CASE WHEN se.Id IS NOT NULL THEN 'YES' ELSE 'NO' END AS HasEnrollment,
    CASE WHEN sg.Id IS NOT NULL THEN 'YES' ELSE 'NO' END AS HasGuardian
FROM StudentRegistrations sr
LEFT JOIN #StudentRegMap srm ON srm.NewId = sr.Id
LEFT JOIN #StudentAdmissionMap sam ON sam.RegistrationNo = srm.RegistrationNo
LEFT JOIN StudentAdmissions sa ON sa.Id = sam.NewId
LEFT JOIN #SemesterEnrollmentMap sem ON sem.StudentAdmissionId = sam.NewId
LEFT JOIN SemesterEnrollments se ON se.Id = sem.NewId
LEFT JOIN StudentGuardians sg ON sg.StudentRegistrationId = sr.Id;

-- ============================================================================
-- CLEANUP
-- ============================================================================

DROP TABLE #AcademicYearMap;
DROP TABLE #ProgramMap;
DROP TABLE #SubjectCatalogMap;
DROP TABLE #SubjectOfferingMap;
DROP TABLE #ExamScheduleMap;
DROP TABLE #ExamCenterMap;
DROP TABLE #StudentRegMap;
DROP TABLE #ExamRegMap;
DROP TABLE #StudentAdmissionMap;
DROP TABLE #SemesterEnrollmentMap;
DROP TABLE #DistinctSubjects;
DROP TABLE #DistinctStudents;
DROP TABLE #DistinctExamRegs;

PRINT '';
PRINT '============================================================';
PRINT '  Migration completed successfully!';
PRINT '  Finished: ' + CONVERT(NVARCHAR(30), GETDATE(), 120);
PRINT '============================================================';

COMMIT TRANSACTION;
GO
