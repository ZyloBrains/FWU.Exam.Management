-- ============================================================================
-- Legacy Exam Data Migration Script
-- Source: [FWUExams.Legacy].dbo.CivilEngineering, [FWUExams.Legacy].dbo.ComputerEngineering, [FWUExams.Legacy].dbo.CPM
-- Target: FUExamsDb normalized schema
-- TenantId: 2 (Engineering)
-- ============================================================================

SET NOCOUNT ON;
SET XACT_ABORT OFF;
SET QUOTED_IDENTIFIER ON;

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
    SourceStudentRegId INT,
    RegistrationNo NVARCHAR(100),
    NewId INT
);

-- ExamRegistration ID mapping
CREATE TABLE #ExamRegMap (
    SourceExamRegId INT,
    NewId INT
);

-- ============================================================================
-- STEP 1: Create Reference/Lookup Data
-- ============================================================================

-- 0. Tenant (required for FK)
DECLARE @TenantId INT = 2;
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Id = @TenantId)
BEGIN
    SET IDENTITY_INSERT Tenants ON;
    INSERT INTO Tenants (Id, Name, OfficeCode, ContactNumber, Address, Email, TenantType, IsActive)
    VALUES (@TenantId, 'Engineering', 'ENG', 'N/A', 'N/A', 'N/A', 1, 1);
    SET IDENTITY_INSERT Tenants OFF;
    PRINT 'Created Tenant: Engineering (Id=' + CAST(@TenantId AS VARCHAR) + ')';
END

-- 1a. Engineering Faculty (if not exists)
IF NOT EXISTS (SELECT 1 FROM Faculties WHERE OfficeCode = 'ENG')
BEGIN
    SET IDENTITY_INSERT Faculties ON;
    INSERT INTO Faculties (Id, OfficeCode, Name, ContactNumber, Address, Email, TenantId)
    VALUES (100, 'ENG', 'Faculty of Engineering', 'N/A', 'N/A', 'N/A', @TenantId);
    SET IDENTITY_INSERT Faculties OFF;
    PRINT 'Created Faculty: ENG (Id=100)';
END
DECLARE @EngineeringFacultyId INT = (SELECT Id FROM Faculties WHERE OfficeCode = 'ENG');

-- 1b. Academic Years (create all years 2014-2025 from source data)
DECLARE @AYYear INT = 2014;
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
    DECLARE @AYId INT = (SELECT Id FROM AcademicYears WHERE AcademicYearCode = CAST(@AYYear AS VARCHAR));
    INSERT INTO #AcademicYearMap (SourceYear, NewId) VALUES (CAST(@AYYear AS VARCHAR), @AYId);
    SET @AYYear = @AYYear + 1;
END

DECLARE @AY2014Id INT = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = '2014');
DECLARE @AY2021Id INT = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = '2021');
DECLARE @AY2023Id INT = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = '2023');

-- 1c. College (SCH001)
DECLARE @CollegeId INT;
IF NOT EXISTS (SELECT 1 FROM Colleges WHERE Code = 'SCH001')
BEGIN
    INSERT INTO Colleges (Code, Name, CollegeNameNepali, ShortName, EstablishedDate, Email, PrincipalName, PrincipalContactNumber, IsExamCenterOnly, IsActive, TenantId)
    VALUES ('SCH001', 'UNIVERSITY CENTRAL CAMPUS', NULL, 'UCC', '1900-01-01', 'info@fwu.edu.np', 'N/A', 'N/A', 0, 1, @TenantId);
    SET @CollegeId = SCOPE_IDENTITY();
    PRINT 'Created College: SCH001 (Id=' + CAST(@CollegeId AS VARCHAR) + ')';
END
ELSE
    SET @CollegeId = (SELECT Id FROM Colleges WHERE Code = 'SCH001');

-- 1d. Levels (Bachelor and Master)
IF NOT EXISTS (SELECT 1 FROM Levels WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT Levels ON;
    INSERT INTO Levels (Id, LevelCode, LevelName, IsActive) VALUES (1, 'UG', 'Undergraduate', 1);
    INSERT INTO Levels (Id, LevelCode, LevelName, IsActive) VALUES (2, 'PG', 'Graduate', 2);
    SET IDENTITY_INSERT Levels OFF;
    PRINT 'Created Levels: Undergraduate (1), Graduate (2)';
END

-- 1e. Programs
DECLARE @ProgCivilId INT, @ProgCompId INT, @ProgCPMId INT;

IF NOT EXISTS (SELECT 1 FROM Programs WHERE ProgramCode = 'L092')
BEGIN
    INSERT INTO Programs (LevelId, ProgramCode, ProgramName, ShortName, Duration, HasMultipleIntakes, IsActive)
    VALUES (1, 'L092', 'Bachelor''s Degree in Civil Engineering', 'BE Civil', 4, 0, 1);
    SET @ProgCivilId = SCOPE_IDENTITY();
    PRINT 'Created Program: L092 (Id=' + CAST(@ProgCivilId AS VARCHAR) + ')';
END
ELSE
    SET @ProgCivilId = (SELECT Id FROM Programs WHERE ProgramCode = 'L092');
INSERT INTO #ProgramMap (SourceCode, NewId) VALUES ('L092', @ProgCivilId);

IF NOT EXISTS (SELECT 1 FROM Programs WHERE ProgramCode = 'L117')
BEGIN
    INSERT INTO Programs (LevelId, ProgramCode, ProgramName, ShortName, Duration, HasMultipleIntakes, IsActive)
    VALUES (1, 'L117', 'Bachelor''s Degree in Computer Engineering', 'BE Computer', 4, 0, 1);
    SET @ProgCompId = SCOPE_IDENTITY();
    PRINT 'Created Program: L117 (Id=' + CAST(@ProgCompId AS VARCHAR) + ')';
END
ELSE
    SET @ProgCompId = (SELECT Id FROM Programs WHERE ProgramCode = 'L117');
INSERT INTO #ProgramMap (SourceCode, NewId) VALUES ('L117', @ProgCompId);

IF NOT EXISTS (SELECT 1 FROM Programs WHERE ProgramCode = 'L131')
BEGIN
    INSERT INTO Programs (LevelId, ProgramCode, ProgramName, ShortName, Duration, HasMultipleIntakes, IsActive)
    VALUES (2, 'L131', 'Master of Science (M.Sc.) in Construction Project Management', 'M.Sc. CPM', 2, 0, 1);
    SET @ProgCPMId = SCOPE_IDENTITY();
    PRINT 'Created Program: L131 (Id=' + CAST(@ProgCPMId AS VARCHAR) + ')';
END
ELSE
    SET @ProgCPMId = (SELECT Id FROM Programs WHERE ProgramCode = 'L131');
INSERT INTO #ProgramMap (SourceCode, NewId) VALUES ('L131', @ProgCPMId);

-- 1e. ExamTypes
DECLARE @ExamTypeRegularId INT, @ExamTypePartialId INT;

IF NOT EXISTS (SELECT 1 FROM ExamTypes WHERE Code = '1')
BEGIN
    SET IDENTITY_INSERT ExamTypes ON;
    INSERT INTO ExamTypes (Id, Code, Name, Remarks, IsActive)
    VALUES (1, '1', 'Regular', 'Regular examination', 1);
    SET IDENTITY_INSERT ExamTypes OFF;
    PRINT 'Created ExamType: Regular (Id=1)';
END
SET @ExamTypeRegularId = (SELECT Id FROM ExamTypes WHERE Code = '1');

IF NOT EXISTS (SELECT 1 FROM ExamTypes WHERE Code = '2')
BEGIN
    SET IDENTITY_INSERT ExamTypes ON;
    INSERT INTO ExamTypes (Id, Code, Name, Remarks, IsActive)
    VALUES (2, '2', 'Partial', 'Partial examination', 1);
    SET IDENTITY_INSERT ExamTypes OFF;
    PRINT 'Created ExamType: Partial (Id=2)';
END
SET @ExamTypePartialId = (SELECT Id FROM ExamTypes WHERE Code = '2');

-- 1f. SubjectType (Compulsory)
DECLARE @SubjectTypeCompId INT;
IF NOT EXISTS (SELECT 1 FROM SubjectTypes WHERE Code = 'COMP')
BEGIN
    SET IDENTITY_INSERT SubjectTypes ON;
    INSERT INTO SubjectTypes (Id, Code, Name, IsDefault, IsActive)
    VALUES (1, 'COMP', 'Compulsory', 1, 1);
    SET IDENTITY_INSERT SubjectTypes OFF;
    PRINT 'Created SubjectType: COMP (Id=1)';
END
SET @SubjectTypeCompId = (SELECT Id FROM SubjectTypes WHERE Code = 'COMP');

-- 1g0. Genders (if empty)
IF NOT EXISTS (SELECT 1 FROM Genders)
BEGIN
    SET IDENTITY_INSERT Genders ON;
    INSERT INTO Genders (Id, GenderName, IsActive) VALUES (1, 'Male', 1);
    INSERT INTO Genders (Id, GenderName, IsActive) VALUES (2, 'Female', 1);
    INSERT INTO Genders (Id, GenderName, IsActive) VALUES (3, 'Other', 1);
    SET IDENTITY_INSERT Genders OFF;
    PRINT 'Created Genders: Male(1), Female(2), Other(3)';
END

-- 1g. StudentCategory (Regular)
IF NOT EXISTS (SELECT 1 FROM StudentCategories WHERE StudentCategoryName = 'Regular')
BEGIN
    SET IDENTITY_INSERT StudentCategories ON;
    INSERT INTO StudentCategories (Id, StudentCategoryName, IsActive)
    VALUES (1, 'Regular', 1);
    SET IDENTITY_INSERT StudentCategories OFF;
    PRINT 'Created StudentCategory: Regular (Id=1)';
END

-- 1h. Semesters - global (unique Code), create one set per program type
-- L092 uses SEM1-SEM6 (linked to AY 2014)
INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES
(1, 1, 'Semester 1', 'SEM1', '2014-01-01', '2014-06-30', @AY2014Id),
(2, 1, 'Semester 2', 'SEM2', '2014-07-01', '2014-12-31', @AY2014Id),
(3, 2, 'Semester 3', 'SEM3', '2015-01-01', '2015-06-30', @AY2014Id),
(4, 2, 'Semester 4', 'SEM4', '2015-07-01', '2015-12-31', @AY2014Id),
(5, 3, 'Semester 5', 'SEM5', '2016-01-01', '2016-06-30', @AY2014Id),
(6, 3, 'Semester 6', 'SEM6', '2016-07-01', '2016-12-31', @AY2014Id);
PRINT 'Created 6 Semesters for L092 (SEM1-SEM6)';

-- L117 uses CESEM1-CESEM2 (linked to AY 2021)
INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES
(1, 1, 'CE Semester 1', 'CESEM1', '2021-01-01', '2021-06-30', @AY2021Id),
(2, 1, 'CE Semester 2', 'CESEM2', '2021-07-01', '2021-12-31', @AY2021Id);
PRINT 'Created 2 Semesters for L117 (CESEM1-CESEM2)';

-- L131 uses CPMSEM1-CPMSEM2 (linked to AY 2023)
INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES
(1, 1, 'CPM Semester 1', 'CPMSEM1', '2023-01-01', '2023-06-30', @AY2023Id),
(2, 1, 'CPM Semester 2', 'CPMSEM2', '2023-07-01', '2023-12-31', @AY2023Id);
PRINT 'Created 2 Semesters for L131 (CPMSEM1-CPMSEM2)';

-- Resolve semester IDs
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
PRINT 'Step 1h complete: Semesters created for all AYs.';

PRINT 'Step 1 complete: Reference data created.';

-- ============================================================================
-- STEP 2: Create SubjectCatalogs (distinct subjects from all 3 source tables)
-- ============================================================================

-- Collect distinct subjects from all source tables
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

-- Insert new SubjectCatalogs
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
        INSERT INTO #SubjectCatalogMap (SourceCode, SourceName, NewId) VALUES (@SubCode, @SubName, @ExistingSubId);
    END

    FETCH NEXT FROM sub_cursor INTO @SubCode, @SubName, @CreditH;
END
CLOSE sub_cursor;
DEALLOCATE sub_cursor;

DECLARE @Cnt2 INT = (SELECT COUNT(*) FROM #SubjectCatalogMap);
PRINT 'Step 2 complete: SubjectCatalogs created. Count=' + CAST(@Cnt2 AS VARCHAR);

-- ============================================================================
-- STEP 3: Create SubjectOfferings (per Subject + Program + Semester)
-- ============================================================================

-- For each distinct subject+program+year+part combination, create a SubjectOffering
-- We need to figure out which program and semester each subject belongs to

-- Civil Engineering (L092) subjects - Year I Part I -> Sem1, etc.
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
        INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId) VALUES (@SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @ExistingSoId);
    END

    FETCH NEXT FROM so_civ_cursor INTO @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @TheoryFM, @TheoryPM, @PracFM, @PracPM, @IntFM, @IntPM, @DisplayOrd;
END
CLOSE so_civ_cursor;
DEALLOCATE so_civ_cursor;

-- Create SubjectOfferings for Computer Engineering
DECLARE so_comp_cursor CURSOR FOR
    SELECT DISTINCT
        scm.NewId AS SubjectCatalogId,
        @ProgCompId AS ProgramId,
        @SemCE1 AS SemesterId,
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
        INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId)
        SELECT @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, Id FROM SubjectOfferings WHERE SubjectCatalogId = @SoSubjectCatalogId AND ProgramId = @SoProgramId AND SemesterId = @SoSemesterId;
    END

    FETCH NEXT FROM so_comp_cursor INTO @SoSubjectCatalogId, @SoProgramId, @SoSemesterId, @TheoryFM, @TheoryPM, @PracFM, @PracPM, @IntFM, @IntPM, @DisplayOrd;
END
CLOSE so_comp_cursor;
DEALLOCATE so_comp_cursor;

-- Create SubjectOfferings for CPM
DECLARE so_cpm_cursor CURSOR FOR
    SELECT DISTINCT
        scm.NewId AS SubjectCatalogId,
        @ProgCPMId AS ProgramId,
        @SemCPM1 AS SemesterId,
        MAX(CAST(TotalFM AS FLOAT)),
        MAX(CAST(TotalPM AS FLOAT)),
        MAX(CAST(TheoryFullMark AS FLOAT)),
        MAX(CAST(TheoryPassMark AS FLOAT)),
        MAX(CAST(InternalFullMark AS FLOAT)),
        MAX(CAST(InternalPassMark AS FLOAT)),
        MAX(CAST(DisplayOrder AS INT))
    FROM [FWUExams.Legacy].dbo.CPM cpm
    INNER JOIN #SubjectCatalogMap scm ON LTRIM(RTRIM(cpm.SubjectCode)) = scm.SourceCode
    GROUP BY scm.NewId;

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

DECLARE @Cnt3 INT = (SELECT COUNT(*) FROM #SubjectOfferingMap);
PRINT 'Step 3 complete: SubjectOfferings created. Count=' + CAST(@Cnt3 AS VARCHAR);

-- ============================================================================
-- STEP 4: Create ExamSchedules (per Program + AcademicYear + Semester)
-- ============================================================================

-- Create ExamSchedules for all Program + AcademicYear + Semester combos
-- Semesters are global, so cross join with all AYs
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

    FETCH NEXT FROM es_cursor INTO @EsProgramId, @EsAcademicYearId, @EsSemesterId, @EsName;
END
CLOSE es_cursor;
DEALLOCATE es_cursor;

DECLARE @Cnt4 INT = (SELECT COUNT(*) FROM #ExamScheduleMap);
PRINT 'Step 4 complete: ExamSchedules created. Count=' + CAST(@Cnt4 AS VARCHAR);

-- ============================================================================
-- STEP 5: Create ExamCenters (per ExamSchedule)
-- ============================================================================

INSERT INTO #ExamCenterMap (ExamScheduleId, CenterName, NewId)
SELECT DISTINCT esm.NewId, 'Kanchanpur', 0
FROM #ExamScheduleMap esm
WHERE NOT EXISTS (SELECT 1 FROM ExamCenters ec WHERE ec.ExamScheduleId = esm.NewId AND ec.Code = 1);

-- Create the actual ExamCenter records
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

PRINT 'Step 5 complete: ExamCenters created.';

-- ============================================================================
-- STEP 6: Create StudentRegistrations (deduplicated by RegistrationNo)
-- ============================================================================

-- Get distinct students from all source tables
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

-- Insert students in batches
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
    -- Look up GenderId
    SET @SrGenderId = ISNULL((SELECT Id FROM Genders WHERE GenderName = @SrGender), 1);
    -- Strip trailing .0 from AcademicYearName (source has '2014.0', map has '2014')
    DECLARE @SrAyNameClean NVARCHAR(10) = CASE WHEN @SrAyName LIKE '%.0' THEN LEFT(@SrAyName, LEN(@SrAyName) - 2) ELSE @SrAyName END;
    -- Look up AcademicYearId
    SET @SrAyId = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = @SrAyNameClean);
    -- Determine LevelId based on AcademicYear (2014/2021 = Bachelor, 2023 = Master)
    SET @SrLevelId = CASE WHEN @SrAyNameClean = '2023' THEN 2 ELSE 1 END;

    IF NOT EXISTS (SELECT 1 FROM StudentRegistrations WHERE RegistrationNumber = @SrRegNo)
    BEGIN
        -- Format BirthDateAD properly
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
            @SrGenderId, 1, @SrAyId,
            @SrIsActive, NULLIF(@SrNepaliName, 'NULL'), @TenantId);
        DECLARE @SrNewId INT = SCOPE_IDENTITY();
        INSERT INTO #StudentRegMap (SourceStudentRegId, RegistrationNo, NewId) VALUES (0, @SrRegNo, @SrNewId);
    END
    ELSE
    BEGIN
        DECLARE @SrExistingId INT = (SELECT Id FROM StudentRegistrations WHERE RegistrationNumber = @SrRegNo);
        INSERT INTO #StudentRegMap (SourceStudentRegId, RegistrationNo, NewId) VALUES (0, @SrRegNo, @SrExistingId);
    END

    FETCH NEXT FROM sr_cursor INTO @SrRegNo, @SrFirstName, @SrMiddleName, @SrLastName, @SrContact, @SrEmail, @SrDobAD, @SrDobBS, @SrGender, @SrAyName, @SrNepaliName, @SrIsActive;
END
CLOSE sr_cursor;
DEALLOCATE sr_cursor;

DECLARE @Cnt6 INT = (SELECT COUNT(DISTINCT NewId) FROM #StudentRegMap);
PRINT 'Step 6 complete: StudentRegistrations created. Count=' + CAST(@Cnt6 AS VARCHAR);

-- ============================================================================
-- STEP 7: Create ExamRegistrations (deduplicated by ExamRegistrationID)
-- ============================================================================

-- Get distinct exam registrations from all source tables
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

DECLARE er_cursor CURSOR FOR
    SELECT SourceExamRegId, RegistrationNo, ExamRollNo, ExamRollNoCoding, AcademicYearName, ExamTypeName, ExamCenterName, SGPA, GradeLetter, ProgramCode
    FROM #DistinctExamRegs;

OPEN er_cursor;
FETCH NEXT FROM er_cursor INTO @ErSourceId, @ErRegNo, @ErRollNo, @ErRollNoCoding, @ErAyName, @ErExamType, @ErCenterName, @ErSgpa, @ErGrade, @ErProgCode;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Resolve FKs
    SET @ErStudentRegId = (SELECT TOP 1 NewId FROM #StudentRegMap WHERE RegistrationNo = @ErRegNo);
    DECLARE @ErAyNameClean NVARCHAR(10) = CASE WHEN @ErAyName LIKE '%.0' THEN LEFT(@ErAyName, LEN(@ErAyName) - 2) ELSE @ErAyName END;
    SET @ErAyId = (SELECT NewId FROM #AcademicYearMap WHERE SourceYear = @ErAyNameClean);
    SET @ErExamTypeId = CASE WHEN @ErExamType = 'Partial' THEN @ExamTypePartialId ELSE @ExamTypeRegularId END;
    SET @ErProgId = (SELECT NewId FROM #ProgramMap WHERE SourceCode = @ErProgCode);
    SET @ErEsId = (SELECT TOP 1 NewId FROM #ExamScheduleMap WHERE ProgramId = @ErProgId AND AcademicYearId = @ErAyId);
    SET @ErEcId = (SELECT TOP 1 NewId FROM #ExamCenterMap WHERE ExamScheduleId = @ErEsId);

    IF @ErStudentRegId IS NOT NULL AND @ErAyId IS NOT NULL AND @ErProgId IS NOT NULL
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

    FETCH NEXT FROM er_cursor INTO @ErSourceId, @ErRegNo, @ErRollNo, @ErRollNoCoding, @ErAyName, @ErExamType, @ErCenterName, @ErSgpa, @ErGrade, @ErProgCode;
END
CLOSE er_cursor;
DEALLOCATE er_cursor;

DECLARE @Cnt7 INT = (SELECT COUNT(*) FROM #ExamRegMap);
PRINT 'Step 7 complete: ExamRegistrations created. Count=' + CAST(@Cnt7 AS VARCHAR);

-- ============================================================================
-- STEP 8: Create ExamSubjectResults (one per source row)
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
    AND esm.SemesterId = som.SemesterId;

PRINT 'Step 8a: ExamSubjectResults for CivilEngineering inserted. Rows=' + CAST(@@ROWCOUNT AS VARCHAR);

-- Process ComputerEngineering
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
    AND esm.SemesterId = @SemCE1;

PRINT 'Step 8b: ExamSubjectResults for ComputerEngineering inserted. Rows=' + CAST(@@ROWCOUNT AS VARCHAR);

-- Process CPM
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
        WHEN cpm.Year = 'I' AND cpm.Part = 'I' THEN @SemCPM1
        WHEN cpm.Year = 'I' AND cpm.Part = 'II' THEN @SemCPM2
        ELSE @SemCPM1
    END
INNER JOIN #AcademicYearMap ay_src ON ay_src.SourceYear = LEFT(CAST(cpm.AcademicYearName AS NVARCHAR(50)), CHARINDEX('.', CAST(cpm.AcademicYearName AS NVARCHAR(50)) + '.') - 1)
LEFT JOIN #ExamScheduleMap esm ON esm.ProgramId = @ProgCPMId AND esm.AcademicYearId = ay_src.NewId
    AND esm.SemesterId = som.SemesterId;

PRINT 'Step 8c: ExamSubjectResults for CPM inserted. Rows=' + CAST(@@ROWCOUNT AS VARCHAR);

-- ============================================================================
-- STEP 9: Verification
-- ============================================================================

PRINT '';
PRINT '========== MIGRATION COMPLETE ==========';

SELECT 'AcademicYears' AS TableName, COUNT(*) AS cnt FROM AcademicYears WHERE AcademicYearCode IN ('2014','2021','2023')
UNION ALL SELECT 'Colleges', COUNT(*) FROM Colleges WHERE Code = 'SCH001'
UNION ALL SELECT 'Programs', COUNT(*) FROM Programs WHERE ProgramCode IN ('L092','L117','L131')
UNION ALL SELECT 'SubjectCatalogs', COUNT(*) FROM SubjectCatalogs
UNION ALL SELECT 'SubjectOfferings', COUNT(*) FROM SubjectOfferings WHERE TenantId = @TenantId
UNION ALL SELECT 'Semesters', COUNT(*) FROM Semesters WHERE AcademicYearId IN (@AY2014Id, @AY2021Id, @AY2023Id)
UNION ALL SELECT 'ExamSchedules', COUNT(*) FROM ExamSchedules WHERE AcademicYearId IN (@AY2014Id, @AY2021Id, @AY2023Id)
UNION ALL SELECT 'ExamCenters', COUNT(*) FROM ExamCenters WHERE TenantId = @TenantId
UNION ALL SELECT 'StudentRegistrations', COUNT(*) FROM StudentRegistrations WHERE AcademicYearId IN (@AY2014Id, @AY2021Id, @AY2023Id)
UNION ALL SELECT 'ExamRegistrations', COUNT(*) FROM ExamRegistrations WHERE AcademicYearId IN (@AY2014Id, @AY2021Id, @AY2023Id)
UNION ALL SELECT 'ExamSubjectResults', COUNT(*) FROM ExamSubjectResults WHERE TenantId = @TenantId;

-- Spot check: verify a specific student
PRINT '';
PRINT 'Spot check - Student EG-2014-1-1-1438:';
SELECT sr.Id, sr.RegistrationNumber, sr.FirstName, sr.LastName, sr.ContactNumber
FROM StudentRegistrations sr WHERE sr.RegistrationNumber = 'EG-2014-1-1-1438';

SELECT er.Id, er.ExamRollNumber, er.Sgpa
FROM ExamRegistrations er
INNER JOIN #StudentRegMap srm ON er.Id = srm.NewId
WHERE srm.RegistrationNo = 'EG-2014-1-1-1438';

-- Cleanup temp tables
DROP TABLE #AcademicYearMap;
DROP TABLE #ProgramMap;
DROP TABLE #SubjectCatalogMap;
DROP TABLE #SubjectOfferingMap;
DROP TABLE #ExamScheduleMap;
DROP TABLE #ExamCenterMap;
DROP TABLE #StudentRegMap;
DROP TABLE #ExamRegMap;
DROP TABLE #DistinctSubjects;
DROP TABLE #DistinctStudents;
DROP TABLE #DistinctExamRegs;

PRINT 'Migration completed successfully.';
