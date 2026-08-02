-- ============================================================================
-- Migrate Legacy Exam Data: FWUExams.Legacy -> FUExamsDb
-- ============================================================================
-- Source : [FWUExams.Legacy].dbo.CivilEngineering / ComputerEngineering / CPM
--          (3 tables with an identical schema; Civil 43,615 / Comp 11,515 / CPM 636 rows)
-- Target : FUExamsDb -> ExamRegistrations + ExamSubjectResults
-- Tenant : 2 (Engineering)   |   Status : 4 (Registered)
--
-- This script also creates the exam-domain reference data required by the
-- foreign keys of the two destination tables (ExamTypes, SubjectTypes,
-- Semesters, SubjectCatalogs, SubjectOfferings, ExamSchedules, ExamCenters),
-- because those tables are empty in a fresh FUExamsDb.
--
-- Source IDs are preserved (IDENTITY_INSERT): ExamRegistrationID and
-- ExamSubjectAndMarksRegistrationID are unique across all three source tables,
-- which also makes this script re-runnable.
--
-- Run against: (localdb)\MSSQLLocalDB / FUExamsDb
-- ============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @TenantId INT = 2;
DECLARE @MigratedRegs INT = 0;
DECLARE @MigratedResults INT = 0;
DECLARE @OK BIT = 0;

BEGIN TRY
BEGIN TRANSACTION;

-- ============================================================================
-- STEP 0: Temp mapping tables
-- ============================================================================
CREATE TABLE #AcademicYearMap (SourceYear NVARCHAR(10), NewId INT);
CREATE TABLE #ProgramMap (ProgramCode NVARCHAR(50), NewId INT);
CREATE TABLE #CollegeMap (CollegeCode NVARCHAR(50), NewId INT);
CREATE TABLE #ExamTypeMap (ExamTypeName NVARCHAR(50), NewId INT);
CREATE TABLE #SemesterMap (Number INT, NewId INT);
CREATE TABLE #SubjectCatalogMap (SubjectCode NVARCHAR(100), NewId INT);
CREATE TABLE #SubjectOfferingMap (SubjectCatalogId INT, ProgramId INT, SemesterId INT, NewId INT);
CREATE TABLE #ScheduleMap (ProgramId INT, AcademicYearId INT, SemesterId INT, ExamTypeId INT, NewId INT);
CREATE TABLE #CenterMap (ScheduleId INT, NewId INT);

-- ============================================================================
-- STEP 1: Reference data (idempotent)
-- ============================================================================

-- 1a. ExamTypes - IDs 1-4 match source ExamTypeID (1=Regular, 2=Partial, 3=Supplementary, 4=Chance)
IF NOT EXISTS (SELECT 1 FROM ExamTypes WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT ExamTypes ON;
    INSERT INTO ExamTypes (Id, Name, Remarks, IsActive, Code) VALUES (1, 'Regular', 'Regular examination', 1, '1');
    SET IDENTITY_INSERT ExamTypes OFF;
END

IF NOT EXISTS (SELECT 1 FROM ExamTypes WHERE Id = 2)
BEGIN
    SET IDENTITY_INSERT ExamTypes ON;
    INSERT INTO ExamTypes (Id, Name, Remarks, IsActive, Code) VALUES (2, 'Partial', 'Partial examination', 1, '2');
    SET IDENTITY_INSERT ExamTypes OFF;
END

IF NOT EXISTS (SELECT 1 FROM ExamTypes WHERE Id = 3)
BEGIN
    SET IDENTITY_INSERT ExamTypes ON;
    INSERT INTO ExamTypes (Id, Name, Remarks, IsActive, Code) VALUES (3, 'Supplementary', 'Supplementary examination', 1, '3');
    SET IDENTITY_INSERT ExamTypes OFF;
END

IF NOT EXISTS (SELECT 1 FROM ExamTypes WHERE Id = 4)
BEGIN
    SET IDENTITY_INSERT ExamTypes ON;
    INSERT INTO ExamTypes (Id, Name, Remarks, IsActive, Code) VALUES (4, 'Chance', 'Chance examination', 1, '4');
    SET IDENTITY_INSERT ExamTypes OFF;
END

-- 1b. SubjectTypes (mapped from source SubjectTypeShortName)
IF NOT EXISTS (SELECT 1 FROM SubjectTypes WHERE Code = 'COMP')
    INSERT INTO SubjectTypes (Code, Name, IsDefault, IsActive) VALUES ('COMP', 'Compulsory', 1, 1);

IF NOT EXISTS (SELECT 1 FROM SubjectTypes WHERE Code = 'DISS')
    INSERT INTO SubjectTypes (Code, Name, IsDefault, IsActive) VALUES ('DISS', 'Dissertation', 0, 1);

IF NOT EXISTS (SELECT 1 FROM SubjectTypes WHERE Code = 'OPT1')
    INSERT INTO SubjectTypes (Code, Name, IsDefault, IsActive) VALUES ('OPT1', 'Option I', 0, 1);

IF NOT EXISTS (SELECT 1 FROM SubjectTypes WHERE Code = 'OPT2')
    INSERT INTO SubjectTypes (Code, Name, IsDefault, IsActive) VALUES ('OPT2', 'Option II', 0, 1);

IF NOT EXISTS (SELECT 1 FROM SubjectTypes WHERE Code = 'OPT3')
    INSERT INTO SubjectTypes (Code, Name, IsDefault, IsActive) VALUES ('OPT3', 'Option III', 0, 1);

IF NOT EXISTS (SELECT 1 FROM SubjectTypes WHERE Code = 'OPT4')
    INSERT INTO SubjectTypes (Code, Name, IsDefault, IsActive) VALUES ('OPT4', 'Option IV', 0, 1);

-- 1c. Semesters - global SEM1..SEM8 (Bachelor Year/Part I/I..IV/II; Master uses SEM1..SEM4)
DECLARE @Ay2014 INT = (SELECT Id FROM AcademicYears WHERE AcademicYearCode = '2014');
IF @Ay2014 IS NULL
    THROW 50001, 'AcademicYear 2014 not found in target. Required reference data missing.', 1;

IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM1')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (1, 1, 'Semester 1', 'SEM1', '2014-01-01', '2014-06-30', @Ay2014);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM2')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (2, 1, 'Semester 2', 'SEM2', '2014-07-01', '2014-12-31', @Ay2014);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM3')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (3, 2, 'Semester 3', 'SEM3', '2015-01-01', '2015-06-30', @Ay2014);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM4')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (4, 2, 'Semester 4', 'SEM4', '2015-07-01', '2015-12-31', @Ay2014);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM5')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (5, 3, 'Semester 5', 'SEM5', '2016-01-01', '2016-06-30', @Ay2014);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM6')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (6, 3, 'Semester 6', 'SEM6', '2016-07-01', '2016-12-31', @Ay2014);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM7')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (7, 4, 'Semester 7', 'SEM7', '2017-01-01', '2017-06-30', @Ay2014);
IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = 'SEM8')
    INSERT INTO Semesters (Number, Year, Name, Code, StartDate, EndDate, AcademicYearId) VALUES (8, 4, 'Semester 8', 'SEM8', '2017-07-01', '2017-12-31', @Ay2014);

-- Populate lookup maps from existing reference data
INSERT INTO #AcademicYearMap (SourceYear, NewId)
SELECT AcademicYearCode, Id FROM AcademicYears;

INSERT INTO #ProgramMap (ProgramCode, NewId)
SELECT ProgramCode, Id FROM Programs WHERE ProgramCode IN ('L092', 'L117', 'L131');

INSERT INTO #CollegeMap (CollegeCode, NewId)
SELECT Code, Id FROM Colleges WHERE Code IN ('SCH001', 'SCH007', 'SCH129');

INSERT INTO #ExamTypeMap (ExamTypeName, NewId)
SELECT Name, Id FROM ExamTypes;

INSERT INTO #SemesterMap (Number, NewId)
SELECT Number, Id FROM Semesters WHERE Code IN ('SEM1', 'SEM2', 'SEM3', 'SEM4', 'SEM5', 'SEM6', 'SEM7', 'SEM8');

DECLARE @CollegeDefault INT = (SELECT NewId FROM #CollegeMap WHERE CollegeCode = 'SCH001');

PRINT 'Step 1 complete: reference data ensured.';

-- ============================================================================
-- STEP 2: Normalize the 3 source tables into #LegacySource
-- ============================================================================
CREATE TABLE #LegacySource (
    SourceTable NVARCHAR(30),
    ExamRegistrationID BIGINT,
    ExamSubjectAndMarksRegistrationID BIGINT,
    RegistrationNo NVARCHAR(255),
    ExamRollNo NVARCHAR(20),
    ExamRollNoCoding NVARCHAR(50),
    AcademicYearName NVARCHAR(10),
    ExamTypeID INT,
    ExamTypeName NVARCHAR(50),
    ProgramCode NVARCHAR(50),
    CollegeCode NVARCHAR(50),
    Year NVARCHAR(10),
    Part NVARCHAR(10),
    SubjectCode NVARCHAR(100),
    SubjectName NVARCHAR(300),
    SubjectTypeShortName NVARCHAR(100),
    CreditHour FLOAT,
    TotalFM FLOAT,
    TotalPM FLOAT,
    TheoryFullMark FLOAT,
    TheoryPassMark FLOAT,
    PracticalFullMark FLOAT,
    PracticalPassMark FLOAT,
    InternalFullMark FLOAT,
    InternalPassMark FLOAT,
    DisplayOrder FLOAT,
    ObtainedMarks FLOAT,
    ObtainedMarksConfirm FLOAT,
    PracticalMarks FLOAT,
    PracticalMarksConfirm FLOAT,
    InternalMarks FLOAT,
    InternalMarksFinal FLOAT,
    TotalOM FLOAT,
    GradeLetter NVARCHAR(50),
    Rem NVARCHAR(255),
    IsLooseEntry INT,
    IsResultConfirm INT,
    SGPA NVARCHAR(50),
    SemesterNumber INT
);

INSERT INTO #LegacySource (
    SourceTable, ExamRegistrationID, ExamSubjectAndMarksRegistrationID, RegistrationNo, ExamRollNo, ExamRollNoCoding,
    AcademicYearName, ExamTypeID, ExamTypeName, ProgramCode, CollegeCode, Year, Part,
    SubjectCode, SubjectName, SubjectTypeShortName, CreditHour,
    TotalFM, TotalPM, TheoryFullMark, TheoryPassMark, PracticalFullMark, PracticalPassMark,
    InternalFullMark, InternalPassMark, DisplayOrder,
    ObtainedMarks, ObtainedMarksConfirm, PracticalMarks, PracticalMarksConfirm,
    InternalMarks, InternalMarksFinal, TotalOM, GradeLetter, Rem, IsLooseEntry, IsResultConfirm, SGPA
)
SELECT
    'CivilEngineering',
    CAST(ExamRegistrationID AS BIGINT),
    CAST(ExamSubjectAndMarksRegistrationID AS BIGINT),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(RegistrationNo AS NVARCHAR(255)))), ''), 'NULL'),
    CASE WHEN ExamRollNo IS NOT NULL THEN CAST(CAST(ROUND(ExamRollNo, 0) AS BIGINT) AS NVARCHAR(20)) ELSE NULL END,
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(ExamRollNoCoding AS NVARCHAR(50)))), ''), 'NULL'),
    CAST(TRY_CAST(AcademicYearName AS INT) AS NVARCHAR(10)),
    CAST(ExamTypeID AS INT),
    COALESCE(
        NULLIF(NULLIF(LTRIM(RTRIM(CAST(ExamTypeName AS NVARCHAR(50)))), ''), 'NULL'),
        CASE CAST(ExamTypeID AS INT) WHEN 1 THEN 'Regular' WHEN 2 THEN 'Partial' WHEN 3 THEN 'Supplementary' WHEN 4 THEN 'Chance' END
    ),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(ProgramCode AS NVARCHAR(50)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(CollegeCode AS NVARCHAR(50)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(Year AS NVARCHAR(10)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(Part AS NVARCHAR(10)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SubjectCode AS NVARCHAR(100)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SubjectName AS NVARCHAR(300)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SubjectTypeShortName AS NVARCHAR(100)))), ''), 'NULL'),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(CreditHour AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TotalFM AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TotalPM AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TheoryFullMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TheoryPassMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalFullMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalPassMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalFullMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalPassMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(DisplayOrder AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(ObtainedMarks AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(ObtainedMarksConfirm AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalMarks AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalMarksConfirm AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalMarks AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalMarksFinal AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TotalOM AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(GradeLetter AS NVARCHAR(50)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(Rem AS NVARCHAR(255)))), ''), 'NULL'),
    CASE
        WHEN NULLIF(NULLIF(LTRIM(RTRIM(CAST(IsLooseEntry AS NVARCHAR(20)))), ''), 'NULL') IS NULL THEN NULL
        WHEN TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(IsLooseEntry AS NVARCHAR(20)))), ''), 'NULL') AS FLOAT) = 1 THEN 1
        ELSE 0
    END,
    CASE WHEN TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(IsResultConfirm AS NVARCHAR(20)))), ''), 'NULL') AS FLOAT) = 1 THEN 1 ELSE 0 END,
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SGPA AS NVARCHAR(50)))), ''), 'NULL')
FROM [FWUExams.Legacy].dbo.CivilEngineering

UNION ALL

SELECT
    'ComputerEngineering',
    CAST(ExamRegistrationID AS BIGINT),
    CAST(ExamSubjectAndMarksRegistrationID AS BIGINT),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(RegistrationNo AS NVARCHAR(255)))), ''), 'NULL'),
    CASE WHEN ExamRollNo IS NOT NULL THEN CAST(CAST(ROUND(ExamRollNo, 0) AS BIGINT) AS NVARCHAR(20)) ELSE NULL END,
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(ExamRollNoCoding AS NVARCHAR(50)))), ''), 'NULL'),
    CAST(TRY_CAST(AcademicYearName AS INT) AS NVARCHAR(10)),
    CAST(ExamTypeID AS INT),
    COALESCE(
        NULLIF(NULLIF(LTRIM(RTRIM(CAST(ExamTypeName AS NVARCHAR(50)))), ''), 'NULL'),
        CASE CAST(ExamTypeID AS INT) WHEN 1 THEN 'Regular' WHEN 2 THEN 'Partial' WHEN 3 THEN 'Supplementary' WHEN 4 THEN 'Chance' END
    ),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(ProgramCode AS NVARCHAR(50)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(CollegeCode AS NVARCHAR(50)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(Year AS NVARCHAR(10)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(Part AS NVARCHAR(10)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SubjectCode AS NVARCHAR(100)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SubjectName AS NVARCHAR(300)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SubjectTypeShortName AS NVARCHAR(100)))), ''), 'NULL'),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(CreditHour AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TotalFM AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TotalPM AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TheoryFullMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TheoryPassMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalFullMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalPassMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalFullMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalPassMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(DisplayOrder AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(ObtainedMarks AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(ObtainedMarksConfirm AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalMarks AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalMarksConfirm AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalMarks AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalMarksFinal AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TotalOM AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(GradeLetter AS NVARCHAR(50)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(Rem AS NVARCHAR(255)))), ''), 'NULL'),
    CASE
        WHEN NULLIF(NULLIF(LTRIM(RTRIM(CAST(IsLooseEntry AS NVARCHAR(20)))), ''), 'NULL') IS NULL THEN NULL
        WHEN TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(IsLooseEntry AS NVARCHAR(20)))), ''), 'NULL') AS FLOAT) = 1 THEN 1
        ELSE 0
    END,
    CASE WHEN TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(IsResultConfirm AS NVARCHAR(20)))), ''), 'NULL') AS FLOAT) = 1 THEN 1 ELSE 0 END,
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SGPA AS NVARCHAR(50)))), ''), 'NULL')
FROM [FWUExams.Legacy].dbo.ComputerEngineering

UNION ALL

SELECT
    'CPM',
    CAST(ExamRegistrationID AS BIGINT),
    CAST(ExamSubjectAndMarksRegistrationID AS BIGINT),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(RegistrationNo AS NVARCHAR(255)))), ''), 'NULL'),
    CASE WHEN ExamRollNo IS NOT NULL THEN CAST(CAST(ROUND(ExamRollNo, 0) AS BIGINT) AS NVARCHAR(20)) ELSE NULL END,
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(ExamRollNoCoding AS NVARCHAR(50)))), ''), 'NULL'),
    CAST(TRY_CAST(AcademicYearName AS INT) AS NVARCHAR(10)),
    CAST(ExamTypeID AS INT),
    COALESCE(
        NULLIF(NULLIF(LTRIM(RTRIM(CAST(ExamTypeName AS NVARCHAR(50)))), ''), 'NULL'),
        CASE CAST(ExamTypeID AS INT) WHEN 1 THEN 'Regular' WHEN 2 THEN 'Partial' WHEN 3 THEN 'Supplementary' WHEN 4 THEN 'Chance' END
    ),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(ProgramCode AS NVARCHAR(50)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(CollegeCode AS NVARCHAR(50)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(Year AS NVARCHAR(10)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(Part AS NVARCHAR(10)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SubjectCode AS NVARCHAR(100)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SubjectName AS NVARCHAR(300)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SubjectTypeShortName AS NVARCHAR(100)))), ''), 'NULL'),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(CreditHour AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TotalFM AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TotalPM AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TheoryFullMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TheoryPassMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalFullMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalPassMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalFullMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalPassMark AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(DisplayOrder AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(ObtainedMarks AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(ObtainedMarksConfirm AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalMarks AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(PracticalMarksConfirm AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalMarks AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(InternalMarksFinal AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(TotalOM AS NVARCHAR(50)))), ''), 'NULL') AS FLOAT),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(GradeLetter AS NVARCHAR(50)))), ''), 'NULL'),
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(Rem AS NVARCHAR(255)))), ''), 'NULL'),
    CASE
        WHEN NULLIF(NULLIF(LTRIM(RTRIM(CAST(IsLooseEntry AS NVARCHAR(20)))), ''), 'NULL') IS NULL THEN NULL
        WHEN TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(IsLooseEntry AS NVARCHAR(20)))), ''), 'NULL') AS FLOAT) = 1 THEN 1
        ELSE 0
    END,
    CASE WHEN TRY_CAST(NULLIF(NULLIF(LTRIM(RTRIM(CAST(IsResultConfirm AS NVARCHAR(20)))), ''), 'NULL') AS FLOAT) = 1 THEN 1 ELSE 0 END,
    NULLIF(NULLIF(LTRIM(RTRIM(CAST(SGPA AS NVARCHAR(50)))), ''), 'NULL')
FROM [FWUExams.Legacy].dbo.CPM;

-- Derive SemesterNumber from Year/Part: Bachelor I/I..IV/II -> 1..8, Master (L131) I/I..II/II -> 1..4
UPDATE #LegacySource
SET SemesterNumber = CASE
    WHEN ProgramCode = 'L131' THEN
        CASE Year
            WHEN 'I'  THEN CASE Part WHEN 'I' THEN 1 WHEN 'II' THEN 2 ELSE NULL END
            WHEN 'II' THEN CASE Part WHEN 'I' THEN 3 WHEN 'II' THEN 4 ELSE NULL END
            ELSE NULL
        END
    ELSE
        CASE Year
            WHEN 'I'   THEN CASE Part WHEN 'I' THEN 1 WHEN 'II' THEN 2 ELSE NULL END
            WHEN 'II'  THEN CASE Part WHEN 'I' THEN 3 WHEN 'II' THEN 4 ELSE NULL END
            WHEN 'III' THEN CASE Part WHEN 'I' THEN 5 WHEN 'II' THEN 6 ELSE NULL END
            WHEN 'IV'  THEN CASE Part WHEN 'I' THEN 7 WHEN 'II' THEN 8 ELSE NULL END
            ELSE NULL
        END
END;

DECLARE @Cnt INT = (SELECT COUNT(*) FROM #LegacySource);
PRINT 'Step 2 complete: source normalized. Rows=' + CAST(@Cnt AS VARCHAR);

-- ============================================================================
-- STEP 3: SubjectCatalogs (205 distinct subjects)
-- ============================================================================
SELECT
    SubjectCode,
    ISNULL(MAX(SubjectName), MAX(SubjectCode)) AS SubjectName,
    MAX(CreditHour) AS CreditHour,
    MAX(SubjectTypeShortName) AS SubjectTypeShortName
INTO #DistinctSubjects
FROM #LegacySource
WHERE SubjectCode IS NOT NULL
GROUP BY SubjectCode;

DECLARE @SubCode NVARCHAR(100), @SubName NVARCHAR(300), @SubCredit FLOAT, @SubType NVARCHAR(100);
DECLARE @SubTypeCode NVARCHAR(10), @SubTypeId INT;

DECLARE sub_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT SubjectCode, SubjectName, CreditHour, SubjectTypeShortName FROM #DistinctSubjects;

OPEN sub_cursor;
FETCH NEXT FROM sub_cursor INTO @SubCode, @SubName, @SubCredit, @SubType;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SubTypeCode = CASE @SubType
        WHEN 'Dissertation' THEN 'DISS'
        WHEN 'OptI' THEN 'OPT1' WHEN 'OptII' THEN 'OPT2' WHEN 'OptIII' THEN 'OPT3' WHEN 'OptIV' THEN 'OPT4'
        ELSE 'COMP'
    END;
    SET @SubTypeId = (SELECT Id FROM SubjectTypes WHERE Code = @SubTypeCode);

    IF NOT EXISTS (SELECT 1 FROM SubjectCatalogs WHERE SubjectCode = @SubCode)
    BEGIN
        INSERT INTO SubjectCatalogs (SubjectCode, SubjectName, CreditHours, SubjectTypeId, IsActive)
        VALUES (@SubCode, @SubName, CASE WHEN @SubCredit > 0 THEN CAST(@SubCredit AS INT) ELSE NULL END, ISNULL(@SubTypeId, 1), 1);
        INSERT INTO #SubjectCatalogMap (SubjectCode, NewId) VALUES (@SubCode, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        INSERT INTO #SubjectCatalogMap (SubjectCode, NewId)
        SELECT @SubCode, Id FROM SubjectCatalogs WHERE SubjectCode = @SubCode;
    END

    FETCH NEXT FROM sub_cursor INTO @SubCode, @SubName, @SubCredit, @SubType;
END
CLOSE sub_cursor;
DEALLOCATE sub_cursor;

SET @Cnt = (SELECT COUNT(*) FROM #SubjectCatalogMap);
PRINT 'Step 3 complete: SubjectCatalogs. Count=' + CAST(@Cnt AS VARCHAR);

-- ============================================================================
-- STEP 4: SubjectOfferings (per Subject + Program + Semester)
-- ============================================================================
SELECT
    scm.NewId AS SubjectCatalogId,
    pm.NewId AS ProgramId,
    sm.NewId AS SemesterId,
    MAX(ISNULL(ls.TotalFM, 0)) AS TotalFM,
    MAX(ISNULL(ls.TotalPM, 0)) AS TotalPM,
    MAX(ISNULL(ls.TheoryFullMark, 0)) AS TheoryFullMark,
    MAX(ISNULL(ls.TheoryPassMark, 0)) AS TheoryPassMark,
    MAX(ISNULL(ls.PracticalFullMark, 0)) AS PracticalFullMark,
    MAX(ISNULL(ls.PracticalPassMark, 0)) AS PracticalPassMark,
    MAX(ISNULL(ls.InternalFullMark, 0)) AS InternalFullMark,
    MAX(ISNULL(ls.InternalPassMark, 0)) AS InternalPassMark,
    MAX(ISNULL(ls.DisplayOrder, 0)) AS DisplayOrder
INTO #DistinctOfferings
FROM #LegacySource ls
INNER JOIN #SubjectCatalogMap scm ON ls.SubjectCode = scm.SubjectCode
INNER JOIN #ProgramMap pm ON ls.ProgramCode = pm.ProgramCode
INNER JOIN #SemesterMap sm ON ls.SemesterNumber = sm.Number
GROUP BY scm.NewId, pm.NewId, sm.NewId;

DECLARE @SoSC INT, @SoP INT, @SoS INT;
DECLARE @SoTotalFM FLOAT, @SoTotalPM FLOAT, @SoTF FLOAT, @SoTP FLOAT, @SoPF FLOAT, @SoPP FLOAT, @SoIF FLOAT, @SoIP FLOAT, @SoDisp FLOAT;

DECLARE so_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT SubjectCatalogId, ProgramId, SemesterId, TotalFM, TotalPM, TheoryFullMark, TheoryPassMark,
           PracticalFullMark, PracticalPassMark, InternalFullMark, InternalPassMark, DisplayOrder
    FROM #DistinctOfferings;

OPEN so_cursor;
FETCH NEXT FROM so_cursor INTO @SoSC, @SoP, @SoS, @SoTotalFM, @SoTotalPM, @SoTF, @SoTP, @SoPF, @SoPP, @SoIF, @SoIP, @SoDisp;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM SubjectOfferings WHERE SubjectCatalogId = @SoSC AND ProgramId = @SoP AND SemesterId = @SoS)
    BEGIN
        INSERT INTO SubjectOfferings (
            TenantId, SubjectCatalogId, ProgramId, SemesterId, IsCompulsory, DisplayOrder,
            HasTheory, HasPractical, HasInternal,
            TheoryFullMarks, TheoryPassMarks, PracticalFullMarks, PracticalPassMarks,
            InternalTheoryFullMarks, InternalTheoryPassMarks, InternalPracticalFullMarks, InternalPracticalPassMarks
        )
        VALUES (
            @TenantId, @SoSC, @SoP, @SoS, 1, CAST(@SoDisp AS INT),
            1,
            CASE WHEN @SoPF > 0 THEN 1 ELSE 0 END,
            CASE WHEN @SoIF > 0 THEN 1 ELSE 0 END,
            ISNULL(@SoTF, ISNULL(@SoTotalFM, 0)), ISNULL(@SoTP, ISNULL(@SoTotalPM, 0)),
            CASE WHEN @SoPF > 0 THEN @SoPF ELSE NULL END,
            CASE WHEN @SoPP > 0 THEN @SoPP ELSE NULL END,
            CASE WHEN @SoIF > 0 THEN @SoIF ELSE NULL END,
            CASE WHEN @SoIP > 0 THEN @SoIP ELSE NULL END,
            NULL, NULL
        );
        INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId)
        VALUES (@SoSC, @SoP, @SoS, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        INSERT INTO #SubjectOfferingMap (SubjectCatalogId, ProgramId, SemesterId, NewId)
        SELECT @SoSC, @SoP, @SoS, Id FROM SubjectOfferings WHERE SubjectCatalogId = @SoSC AND ProgramId = @SoP AND SemesterId = @SoS;
    END

    FETCH NEXT FROM so_cursor INTO @SoSC, @SoP, @SoS, @SoTotalFM, @SoTotalPM, @SoTF, @SoTP, @SoPF, @SoPP, @SoIF, @SoIP, @SoDisp;
END
CLOSE so_cursor;
DEALLOCATE so_cursor;

SET @Cnt = (SELECT COUNT(*) FROM #SubjectOfferingMap);
PRINT 'Step 4 complete: SubjectOfferings. Count=' + CAST(@Cnt AS VARCHAR);

-- ============================================================================
-- STEP 5: ExamSchedules (per Program + AcademicYear + Semester + ExamType)
-- ============================================================================
SELECT
    d.ProgramCode,
    d.AcademicYearName,
    d.SemesterNumber,
    d.ExamTypeName,
    pm.NewId AS ProgramId,
    ay.NewId AS AcademicYearId,
    sm.NewId AS SemesterId,
    et.NewId AS ExamTypeId
INTO #DistinctSchedules
FROM (
    SELECT DISTINCT ProgramCode, AcademicYearName, SemesterNumber, ExamTypeName
    FROM #LegacySource
    WHERE ExamTypeName IS NOT NULL AND AcademicYearName IS NOT NULL AND SemesterNumber IS NOT NULL
) d
INNER JOIN #ProgramMap pm ON d.ProgramCode = pm.ProgramCode
INNER JOIN #AcademicYearMap ay ON d.AcademicYearName = ay.SourceYear
INNER JOIN #SemesterMap sm ON d.SemesterNumber = sm.Number
INNER JOIN #ExamTypeMap et ON d.ExamTypeName = et.ExamTypeName;

DECLARE @PCode NVARCHAR(50), @AYVal NVARCHAR(10), @SemNum INT, @EType NVARCHAR(50);
DECLARE @PId INT, @AYId INT, @SemId INT, @ETypeId INT, @LevId INT;
DECLARE @SchedCode NVARCHAR(50), @SchedName NVARCHAR(50);

DECLARE sch_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT ProgramCode, AcademicYearName, SemesterNumber, ExamTypeName, ProgramId, AcademicYearId, SemesterId, ExamTypeId
    FROM #DistinctSchedules;

OPEN sch_cursor;
FETCH NEXT FROM sch_cursor INTO @PCode, @AYVal, @SemNum, @EType, @PId, @AYId, @SemId, @ETypeId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SchedCode = @PCode + '-' + @AYVal + '-SEM' + CAST(@SemNum AS NVARCHAR(2)) + '-T' + CAST(@ETypeId AS NVARCHAR(2));
    SET @SchedName = @PCode + ' ' + @AYVal + ' Semester ' + CAST(@SemNum AS NVARCHAR(2)) + ' ' + @EType;

    IF NOT EXISTS (SELECT 1 FROM ExamSchedules WHERE TenantId = @TenantId AND ExamScheduleCode = @SchedCode)
    BEGIN
        SET @LevId = (SELECT LevelId FROM Programs WHERE Id = @PId);
        INSERT INTO ExamSchedules (TenantId, ExamScheduleName, StartTime, EndTime, IsActive, ExamScheduleCode,
            AcademicYearId, ProgramId, SemesterId, ExamTypeId, LevelId)
        VALUES (@TenantId, @SchedName, '08:00', '11:00', 1, @SchedCode, @AYId, @PId, @SemId, @ETypeId, @LevId);
        INSERT INTO #ScheduleMap (ProgramId, AcademicYearId, SemesterId, ExamTypeId, NewId)
        VALUES (@PId, @AYId, @SemId, @ETypeId, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        INSERT INTO #ScheduleMap (ProgramId, AcademicYearId, SemesterId, ExamTypeId, NewId)
        SELECT @PId, @AYId, @SemId, @ETypeId, Id
        FROM ExamSchedules WHERE TenantId = @TenantId AND ExamScheduleCode = @SchedCode;
    END

    FETCH NEXT FROM sch_cursor INTO @PCode, @AYVal, @SemNum, @EType, @PId, @AYId, @SemId, @ETypeId;
END
CLOSE sch_cursor;
DEALLOCATE sch_cursor;

SET @Cnt = (SELECT COUNT(*) FROM #ScheduleMap);
PRINT 'Step 5 complete: ExamSchedules. Count=' + CAST(@Cnt AS VARCHAR);

-- ============================================================================
-- STEP 6: ExamCenters (one "Kanchanpur" center per schedule)
-- ============================================================================
INSERT INTO ExamCenters (TenantId, ExamScheduleId, CollegeId, Remark, IsActive, Code)
SELECT @TenantId, sm.NewId, @CollegeDefault, 'Kanchanpur', 1, 1
FROM #ScheduleMap sm
WHERE NOT EXISTS (SELECT 1 FROM ExamCenters ec WHERE ec.ExamScheduleId = sm.NewId AND ec.Code = 1);

INSERT INTO #CenterMap (ScheduleId, NewId)
SELECT sm.NewId, MIN(ec.Id)
FROM #ScheduleMap sm
INNER JOIN ExamCenters ec ON ec.ExamScheduleId = sm.NewId
GROUP BY sm.NewId;

PRINT 'Step 6 complete: ExamCenters.';

-- ============================================================================
-- STEP 7: ExamRegistrations (deduplicated by ExamRegistrationID)
-- ============================================================================
SELECT
    ExamRegistrationID, AcademicYearName, ExamTypeName, SemesterNumber, ProgramCode, CollegeCode,
    ExamRollNo, ExamRollNoCoding, SGPA
INTO #DistinctRegs
FROM (
    SELECT
        ExamRegistrationID, AcademicYearName, ExamTypeName, SemesterNumber, ProgramCode, CollegeCode,
        ExamRollNo, ExamRollNoCoding, SGPA,
        ROW_NUMBER() OVER (PARTITION BY ExamRegistrationID ORDER BY SemesterNumber, ExamTypeID) AS rn
    FROM #LegacySource
    WHERE ExamRegistrationID IS NOT NULL
) reg
WHERE rn = 1;

SET IDENTITY_INSERT ExamRegistrations ON;

INSERT INTO ExamRegistrations (
    Id, TenantId, AcademicYearId, ExamCenterId, CollegeId, ExamRollNumber, ExamRollNumberCoding,
    Sgpa, IsActive, ExamScheduleId, ProgramsId, Status
)
SELECT
    dr.ExamRegistrationID,
    @TenantId,
    ay.NewId,
    cm.NewId,
    col.NewId,
    NULLIF(dr.ExamRollNo, ''),
    CASE WHEN dr.ExamRollNoCoding IS NOT NULL THEN TRY_CONVERT(BIGINT, ROUND(TRY_CAST(dr.ExamRollNoCoding AS FLOAT), 0)) ELSE NULL END,
    NULLIF(dr.SGPA, ''),
    1,
    sch.NewId,
    pm.NewId,
    4
FROM #DistinctRegs dr
INNER JOIN #AcademicYearMap ay ON dr.AcademicYearName = ay.SourceYear
INNER JOIN #ProgramMap pm ON dr.ProgramCode = pm.ProgramCode
INNER JOIN #CollegeMap col ON dr.CollegeCode = col.CollegeCode
INNER JOIN #SemesterMap sm ON dr.SemesterNumber = sm.Number
INNER JOIN #ExamTypeMap et ON dr.ExamTypeName = et.ExamTypeName
INNER JOIN #ScheduleMap sch ON sch.ProgramId = pm.NewId AND sch.AcademicYearId = ay.NewId
                            AND sch.SemesterId = sm.NewId AND sch.ExamTypeId = et.NewId
LEFT JOIN #CenterMap cm ON cm.ScheduleId = sch.NewId
WHERE NOT EXISTS (SELECT 1 FROM ExamRegistrations e2 WHERE e2.Id = dr.ExamRegistrationID);

SET @MigratedRegs = @@ROWCOUNT;
SET IDENTITY_INSERT ExamRegistrations OFF;

PRINT 'Step 7 complete: ExamRegistrations inserted. Count=' + CAST(@MigratedRegs AS VARCHAR);

-- ============================================================================
-- STEP 8: ExamSubjectResults (one per source row)
-- ============================================================================
SET IDENTITY_INSERT ExamSubjectResults ON;

INSERT INTO ExamSubjectResults (
    Id, TenantId, ExamRegistrationId, ExamTypeId, SubjectOfferingId, ExamScheduleId,
    ObtainedMarksTheory, ObtainedMarksTheoryConfirm, ObtainedMarksPractical, ObtainedMarksPracticalConfirm,
    ObtainedMarksTheoryInternal, ObtainedMarksPracticalInternal, GradeLetter, Remarks, IsActive, IsLooseEntry,
    IsSubmitted, ObtainedMarks, CreatedDate
)
SELECT
    ls.ExamSubjectAndMarksRegistrationID,
    @TenantId,
    er.Id,
    et.NewId,
    som.NewId,
    sch.NewId,
    ls.ObtainedMarks,
    ls.ObtainedMarksConfirm,
    ls.PracticalMarks,
    ls.PracticalMarksConfirm,
    ls.InternalMarks,
    ls.InternalMarksFinal,
    CASE WHEN ls.GradeLetter IS NULL OR ls.GradeLetter = '' OR ls.GradeLetter = 'NULL' THEN NULL ELSE LEFT(ls.GradeLetter, 3) END,
    CASE WHEN ls.Rem IS NULL OR ls.Rem = '' OR ls.Rem = 'NULL' THEN NULL ELSE ls.Rem END,
    1,
    CASE WHEN ls.IsLooseEntry = 1 THEN 1 ELSE NULL END,
    ISNULL(ls.IsResultConfirm, 0),
    ls.TotalOM,
    GETDATE()
FROM #LegacySource ls
INNER JOIN ExamRegistrations er ON er.Id = ls.ExamRegistrationID
INNER JOIN #ExamTypeMap et ON ls.ExamTypeName = et.ExamTypeName
INNER JOIN #SubjectCatalogMap scm ON ls.SubjectCode = scm.SubjectCode
INNER JOIN #ProgramMap pm ON ls.ProgramCode = pm.ProgramCode
INNER JOIN #SemesterMap sm ON ls.SemesterNumber = sm.Number
INNER JOIN #SubjectOfferingMap som ON som.SubjectCatalogId = scm.NewId AND som.ProgramId = pm.NewId AND som.SemesterId = sm.NewId
LEFT JOIN #AcademicYearMap ay ON ls.AcademicYearName = ay.SourceYear
LEFT JOIN #ScheduleMap sch ON sch.ProgramId = pm.NewId AND sch.AcademicYearId = ay.NewId
                            AND sch.SemesterId = sm.NewId AND sch.ExamTypeId = et.NewId
WHERE ls.ExamSubjectAndMarksRegistrationID IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM ExamSubjectResults r WHERE r.Id = ls.ExamSubjectAndMarksRegistrationID);

SET @MigratedResults = @@ROWCOUNT;
SET IDENTITY_INSERT ExamSubjectResults OFF;

PRINT 'Step 8 complete: ExamSubjectResults inserted. Count=' + CAST(@MigratedResults AS VARCHAR);

COMMIT;
SET @OK = 1;
PRINT 'Transaction committed.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    PRINT 'ERROR: ' + ERROR_MESSAGE();
END CATCH;

-- ============================================================================
-- STEP 9: Verification (runs only if the transaction committed)
-- ============================================================================
IF @OK = 1
BEGIN
    PRINT '';
    PRINT '========== MIGRATION SUMMARY ==========';
    PRINT '';

    SELECT SourceTable, COUNT(*) AS SourceRows FROM #LegacySource GROUP BY SourceTable;

    PRINT '';
    PRINT 'Reference data created / ensured:';
    SELECT 'ExamTypes' AS TableName, COUNT(*) AS Cnt FROM ExamTypes
    UNION ALL SELECT 'SubjectTypes', COUNT(*) FROM SubjectTypes
    UNION ALL SELECT 'Semesters (SEM1-8)', COUNT(*) FROM Semesters WHERE Code IN ('SEM1','SEM2','SEM3','SEM4','SEM5','SEM6','SEM7','SEM8')
    UNION ALL SELECT 'SubjectCatalogs', COUNT(*) FROM SubjectCatalogs
    UNION ALL SELECT 'SubjectOfferings', COUNT(*) FROM SubjectOfferings WHERE TenantId = @TenantId
    UNION ALL SELECT 'ExamSchedules', COUNT(*) FROM ExamSchedules WHERE TenantId = @TenantId
    UNION ALL SELECT 'ExamCenters', COUNT(*) FROM ExamCenters WHERE TenantId = @TenantId;

    PRINT '';
    PRINT 'Migration results:';
    SELECT 'Distinct registrations (source)' AS Item, COUNT(*) AS Cnt FROM #DistinctRegs
    UNION ALL SELECT 'ExamRegistrations inserted', @MigratedRegs
    UNION ALL SELECT 'Registrations NOT migrated (missing FK)', (
        SELECT COUNT(*) FROM #DistinctRegs dr
        WHERE NOT EXISTS (SELECT 1 FROM ExamRegistrations e WHERE e.Id = dr.ExamRegistrationID)
    )
    UNION ALL SELECT 'ExamSubjectResults inserted', @MigratedResults
    UNION ALL SELECT 'Subject results NOT migrated', (
        SELECT COUNT(*) FROM #LegacySource ls
        WHERE ls.ExamSubjectAndMarksRegistrationID IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM ExamSubjectResults r WHERE r.Id = ls.ExamSubjectAndMarksRegistrationID)
    );

    PRINT '';
    PRINT 'Total rows in destination tables:';
    SELECT 'ExamRegistrations' AS TableName, COUNT(*) AS Cnt FROM ExamRegistrations
    UNION ALL SELECT 'ExamSubjectResults', COUNT(*) FROM ExamSubjectResults;

    PRINT '';
    PRINT 'Spot check - sample registrations:';
    SELECT TOP 5 er.Id, er.ExamRollNumber, er.Sgpa, er.Status, es.ExamScheduleName
    FROM ExamRegistrations er
    LEFT JOIN ExamSchedules es ON es.Id = er.ExamScheduleId
    ORDER BY er.Id;

    PRINT '';
    PRINT 'Spot check - sample subject results:';
    SELECT TOP 5 r.Id, r.ExamRegistrationId, r.ExamTypeId, r.SubjectOfferingId, r.GradeLetter,
           r.ObtainedMarks, r.ObtainedMarksTheory, r.ObtainedMarksPractical, r.ObtainedMarksTheoryInternal, r.ObtainedMarksPracticalInternal
    FROM ExamSubjectResults r
    ORDER BY r.Id;

    PRINT '';
    PRINT '========== MIGRATION COMPLETE ==========';
END
ELSE
BEGIN
    PRINT 'Migration did not complete - all changes rolled back. Review the error above.';
END

-- ============================================================================
-- STEP 10: Cleanup
-- ============================================================================
DROP TABLE IF EXISTS #AcademicYearMap;
DROP TABLE IF EXISTS #ProgramMap;
DROP TABLE IF EXISTS #CollegeMap;
DROP TABLE IF EXISTS #ExamTypeMap;
DROP TABLE IF EXISTS #SemesterMap;
DROP TABLE IF EXISTS #SubjectCatalogMap;
DROP TABLE IF EXISTS #SubjectOfferingMap;
DROP TABLE IF EXISTS #ScheduleMap;
DROP TABLE IF EXISTS #CenterMap;
DROP TABLE IF EXISTS #LegacySource;
DROP TABLE IF EXISTS #DistinctSubjects;
DROP TABLE IF EXISTS #DistinctOfferings;
DROP TABLE IF EXISTS #DistinctSchedules;
DROP TABLE IF EXISTS #DistinctRegs;
