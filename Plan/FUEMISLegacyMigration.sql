-- ============================================================================
-- FUEMIS Legacy Data Migration Script
-- Source: [FUExamDBcopy].dbo.* (26 tables)
-- Target: FUExamsDb - normalized exam management database
-- ============================================================================
-- This script:
--   Step 0: Seeds base data (Tenants, Provinces, Districts, LocalLevels, Countries)
--   Steps 1-20: Migrates all available data from FUExamDBcopy
-- SKIPPED tables (don't exist in source):
--   ExamSubjectAndMarksRegistration, ExamScheduleDetail
-- ============================================================================

SET NOCOUNT ON;
SET XACT_ABORT OFF;
SET QUOTED_IDENTIFIER ON;

DECLARE @TenantId INT = 1;

-- ============================================================================
-- STEP 0a: Seed Tenant
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Id = @TenantId)
BEGIN
    SET IDENTITY_INSERT Tenants ON;
    INSERT INTO Tenants (Id, Name, OfficeCode, ContactNumber, Address, Email, TenantType, IsActive)
    VALUES (@TenantId, 'Far Western University', 'FWU', '01-2345678', 'Far Western University', 'info@fwu.edu.np', 1, 1);
    SET IDENTITY_INSERT Tenants OFF;
    PRINT 'Tenant seeded.';
END
ELSE
BEGIN
    UPDATE Tenants SET Name = 'Far Western University', OfficeCode = 'FWU',
        ContactNumber = '01-2345678', Address = 'Far Western University',
        Email = 'info@fwu.edu.np', TenantType = 1, IsActive = 1
    WHERE Id = @TenantId;
    PRINT 'Tenant updated.';
END

-- ============================================================================
-- STEP 0b: Seed Provinces (7 Nepal provinces)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM Provinces)
BEGIN
    SET IDENTITY_INSERT Provinces ON;
    INSERT INTO Provinces (Id, ProvinceName, ProvinceCode, IsActive) VALUES
    (1, 'Koshi', 'P1', 1),
    (2, 'Madhesh', 'P2', 1),
    (3, 'Bagmati', 'P3', 1),
    (4, 'Gandaki', 'P4', 1),
    (5, 'Lumbini', 'P5', 1),
    (6, 'Karnali', 'P6', 1),
    (7, 'Sudurpashchim', 'P7', 1);
    SET IDENTITY_INSERT Provinces OFF;
    PRINT 'Provinces seeded: 7 rows.';
END

-- ============================================================================
-- STEP 0c: Seed Districts from source (with Zone-to-Province mapping)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM Districts)
BEGIN
    SET IDENTITY_INSERT Districts ON;
    INSERT INTO Districts (Id, ProvinceId, DistrictCode, DistrictName, IsActive)
    SELECT
        d.DistrictID,
        CASE d.ZoneID
            WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1  -- Koshi
            WHEN 4 THEN 2 WHEN 5 THEN 2                   -- Madhesh
            WHEN 6 THEN 3 WHEN 7 THEN 3                   -- Bagmati
            WHEN 8 THEN 4 WHEN 9 THEN 4                   -- Gandaki
            WHEN 10 THEN 5 WHEN 11 THEN 5                  -- Lumbini
            WHEN 12 THEN 6 WHEN 13 THEN 6                  -- Karnali
            WHEN 14 THEN 7                                  -- Sudurpashchim
            ELSE 1
        END,
        d.DistrictCode,
        d.DistrictName,
        ISNULL(d.IsActive, 1)
    FROM [FUExamDBcopy].dbo.District d;
    SET IDENTITY_INSERT Districts OFF;
    PRINT 'Districts seeded from source.';
END

-- ============================================================================
-- STEP 0d: Seed LocalLevels from source
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM LocalLevels)
BEGIN
    SET IDENTITY_INSERT LocalLevels ON;
    INSERT INTO LocalLevels (Id, DistrictId, LocalLevelName, LocalLevelType, IsActive)
    SELECT
        ll.LocalLevelID,
        ll.DistrictID,
        ll.LocalLevelName,
        0,
        ISNULL(ll.IsActive, 1)
    FROM [FUExamDBcopy].dbo.LocalLevel ll;
    SET IDENTITY_INSERT LocalLevels OFF;
    PRINT 'LocalLevels seeded from source.';
END

-- ============================================================================
-- STEP 0e: Seed Countries from source
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM Countries)
BEGIN
    SET IDENTITY_INSERT Countries ON;
    INSERT INTO Countries (Id, CountryName, IsActive)
    SELECT CountryID, CountryName, ISNULL(IsActive, 1)
    FROM [FUExamDBcopy].dbo.Country;
    SET IDENTITY_INSERT Countries OFF;
    PRINT 'Countries seeded from source.';
END

PRINT 'Step 0 complete: Base data seeded.';

-- ============================================================================
-- STEP 0f: Seed default StudentCategory (required NOT NULL FK)
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM StudentCategories)
BEGIN
    SET IDENTITY_INSERT StudentCategories ON;
    INSERT INTO StudentCategories (Id, StudentCategoryName, IsActive) VALUES (1, 'General', 1);
    SET IDENTITY_INSERT StudentCategories OFF;
    PRINT 'Default StudentCategory seeded.';
END

-- ============================================================================
-- STEP 1: Create temp mapping tables
-- ============================================================================

CREATE TABLE #DistrictMap (
    SourceDistrictId INT,
    TargetDistrictId INT
);

CREATE TABLE #LocalLevelMap (
    SourceLocalLevelId INT,
    TargetLocalLevelId INT
);

CREATE TABLE #AcademicYearMap (
    SourceId INT,
    SourceCode NVARCHAR(50),
    NewId INT
);

CREATE TABLE #SubjectCatalogMap (
    SourceSubjectDetailId INT,
    SourceSubjectCode NVARCHAR(50),
    NewCatalogId INT,
    NewOfferingId INT
);

CREATE TABLE #StudentRegMap (
    SourceId INT,
    NewId INT
);

CREATE TABLE #ExamScheduleMap (
    SourceId INT,
    NewId INT
);

CREATE TABLE #ExamCenterMap (
    SourceId INT,
    NewId INT
);

CREATE TABLE #ExamRegMap (
    SourceId INT,
    NewId INT
);

CREATE TABLE #SemesterMap (
    SourceYear NVARCHAR(3),
    SourcePart NVARCHAR(2),
    AcademicYearId INT,
    NewId INT
);

CREATE TABLE #AYSemesterMap (
    SourceYear NVARCHAR(3),
    SourcePart NVARCHAR(2),
    AcademicYearId INT,
    NewId INT
);

PRINT 'Step 1 complete: Temp mapping tables created.';

-- ============================================================================
-- STEP 2: Create District mapping (Source ID -> Target ID by name)
-- ============================================================================

INSERT INTO #DistrictMap (SourceDistrictId, TargetDistrictId)
SELECT fd.DistrictID, d.Id
FROM [FUExamDBcopy].dbo.District fd
INNER JOIN Districts d ON fd.DistrictName = d.DistrictName;

UPDATE dm SET dm.TargetDistrictId = d.Id
FROM #DistrictMap dm
INNER JOIN [FUExamDBcopy].dbo.District fd ON dm.SourceDistrictId = fd.DistrictID
INNER JOIN Districts d ON d.DistrictName = 'Chitwan'
WHERE fd.DistrictName = 'Chitawan';

UPDATE dm SET dm.TargetDistrictId = d.Id
FROM #DistrictMap dm
INNER JOIN [FUExamDBcopy].dbo.District fd ON dm.SourceDistrictId = fd.DistrictID
INNER JOIN Districts d ON d.DistrictName = 'Dhanusha'
WHERE fd.DistrictName = 'Dhanusa';

UPDATE dm SET dm.TargetDistrictId = d.Id
FROM #DistrictMap dm
INNER JOIN [FUExamDBcopy].dbo.District fd ON dm.SourceDistrictId = fd.DistrictID
INNER JOIN Districts d ON d.DistrictName = 'Tanahun'
WHERE fd.DistrictName = 'Tanahu';

UPDATE dm SET dm.TargetDistrictId = d.Id
FROM #DistrictMap dm
INNER JOIN [FUExamDBcopy].dbo.District fd ON dm.SourceDistrictId = fd.DistrictID
INNER JOIN Districts d ON d.DistrictName = 'Kapilvastu'
WHERE fd.DistrictName = 'Kapilbastu';

UPDATE dm SET dm.TargetDistrictId = d.Id
FROM #DistrictMap dm
INNER JOIN [FUExamDBcopy].dbo.District fd ON dm.SourceDistrictId = fd.DistrictID
INNER JOIN Districts d ON d.DistrictName = 'Eastern Rukum'
WHERE fd.DistrictName = 'East Rukum';

UPDATE dm SET dm.TargetDistrictId = d.Id
FROM #DistrictMap dm
INNER JOIN [FUExamDBcopy].dbo.District fd ON dm.SourceDistrictId = fd.DistrictID
INNER JOIN Districts d ON d.DistrictName = 'Western Rukum'
WHERE fd.DistrictName = 'West Rukum';

UPDATE dm SET dm.TargetDistrictId = d.Id
FROM #DistrictMap dm
INNER JOIN [FUExamDBcopy].dbo.District fd ON dm.SourceDistrictId = fd.DistrictID
INNER JOIN Districts d ON d.DistrictName = 'Nawalparasi West'
WHERE fd.DistrictName = 'Nawalparasi East';

DECLARE @MappedDist INT = (SELECT COUNT(*) FROM #DistrictMap WHERE TargetDistrictId IS NOT NULL);
PRINT 'Step 2 complete: District mapping created. Mapped=' + CAST(@MappedDist AS VARCHAR);

-- ============================================================================
-- STEP 3: Create LocalLevel mapping
-- ============================================================================

INSERT INTO #LocalLevelMap (SourceLocalLevelId, TargetLocalLevelId)
SELECT fll.LocalLevelID, ll.Id
FROM [FUExamDBcopy].dbo.LocalLevel fll
INNER JOIN LocalLevels ll ON fll.LocalLevelName = ll.LocalLevelName;

DECLARE @MappedLL INT = (SELECT COUNT(*) FROM #LocalLevelMap);
PRINT 'Step 3 complete: LocalLevel mapping created. Mapped=' + CAST(@MappedLL AS VARCHAR);

-- ============================================================================
-- STEP 4: Migrate reference data
-- ============================================================================

-- Levels (4 rows)
SET IDENTITY_INSERT Levels ON;
INSERT INTO Levels (Id, LevelCode, LevelName, LevelDisplayOrder, Remarks, IsActive)
SELECT LevelID, LevelCode, LevelName, LevelDisplayOrder, Remarks, ISNULL(IsActive, 1)
FROM [FUExamDBcopy].dbo.Level
WHERE NOT EXISTS (SELECT 1 FROM Levels WHERE Id = LevelID);
SET IDENTITY_INSERT Levels OFF;
PRINT 'Step 4a: Levels migrated.';

-- Genders (3 rows)
SET IDENTITY_INSERT Genders ON;
INSERT INTO Genders (Id, GenderName, IsActive)
SELECT GenderID, GenderName, ISNULL(IsActive, 1)
FROM [FUExamDBcopy].dbo.Gender
WHERE NOT EXISTS (SELECT 1 FROM Genders WHERE Id = GenderID);
SET IDENTITY_INSERT Genders OFF;
PRINT 'Step 4b: Genders migrated.';

-- CollegeTypes (2 rows)
SET IDENTITY_INSERT CollegeTypes ON;
INSERT INTO CollegeTypes (Id, Code, Name, Remarks, IsDefault, IsActive)
SELECT CollegeTypeID, CollegeTypeCode, CollegeTypeName, Remarks, ISNULL(IsDefault, 0), ISNULL(IsActive, 1)
FROM [FUExamDBcopy].dbo.CollegeType
WHERE NOT EXISTS (SELECT 1 FROM CollegeTypes WHERE Id = CollegeTypeID);
SET IDENTITY_INSERT CollegeTypes OFF;
PRINT 'Step 4c: CollegeTypes migrated.';

-- PreviousLevels (4 rows)
SET IDENTITY_INSERT PreviousLevels ON;
INSERT INTO PreviousLevels (Id, PreviousLevelName, LevelDisplayOrder, Remarks, IsActive)
SELECT PreviousLevelID, PreviousLevelName, DisplayOrder, Remarks, ISNULL(IsActive, 1)
FROM [FUExamDBcopy].dbo.PreviousLevel
WHERE NOT EXISTS (SELECT 1 FROM PreviousLevels WHERE Id = PreviousLevelID);
SET IDENTITY_INSERT PreviousLevels OFF;
PRINT 'Step 4d: PreviousLevels migrated.';

-- SubjectTypes (11 rows) - handle duplicate Code by using ROW_NUMBER
SET IDENTITY_INSERT SubjectTypes ON;
;WITH src AS (
    SELECT SubjectTypeID, SubjectTypeName, SubjectTypeShortName, IsActive,
        ROW_NUMBER() OVER (PARTITION BY SubjectTypeShortName ORDER BY SubjectTypeID) AS rn
    FROM [FUExamDBcopy].dbo.SubjectType
)
INSERT INTO SubjectTypes (Id, Code, Name, IsDefault, IsActive)
SELECT
    SubjectTypeID,
    CASE
        WHEN rn > 1 THEN SubjectTypeShortName + CAST(rn AS VARCHAR)
        WHEN SubjectTypeShortName = 'Com.' THEN 'COMP'
        WHEN SubjectTypeShortName = 'OptI' THEN 'OPT1'
        WHEN SubjectTypeShortName = 'OptII' THEN 'OPT2'
        WHEN SubjectTypeShortName = 'OptIII' THEN 'OPT3'
        WHEN SubjectTypeShortName = 'OptIV' THEN 'OPT4'
        WHEN SubjectTypeShortName = 'OptV' THEN 'OPT5'
        WHEN SubjectTypeShortName = 'OptVI' THEN 'OPT6'
        WHEN SubjectTypeShortName = 'Dissertation' THEN 'DISS'
        WHEN SubjectTypeShortName = 'BC1' THEN 'BC1'
        WHEN SubjectTypeShortName = 'BC2' THEN 'BC2'
        ELSE SubjectTypeShortName
    END,
    SubjectTypeName,
    0,
    ISNULL(IsActive, 1)
FROM src
WHERE NOT EXISTS (SELECT 1 FROM SubjectTypes WHERE Id = SubjectTypeID);
SET IDENTITY_INSERT SubjectTypes OFF;
PRINT 'Step 4e: SubjectTypes migrated.';

-- ExamTypes (5 rows)
SET IDENTITY_INSERT ExamTypes ON;
INSERT INTO ExamTypes (Id, Code, Name, Remarks, IsActive)
SELECT ExamTypeID,
    CASE
        WHEN ExamTypeCode = 0 THEN '1'
        WHEN ExamTypeCode = 1 THEN '2'
        WHEN ExamTypeCode = 2 THEN '3'
        WHEN ExamTypeCode = 3 THEN '4'
        WHEN ExamTypeCode = 4 THEN '5'
        ELSE CAST(ExamTypeCode AS VARCHAR)
    END,
    ExamTypeName,
    Remarks,
    ISNULL(IsActive, 1)
FROM [FUExamDBcopy].dbo.ExamType
WHERE NOT EXISTS (SELECT 1 FROM ExamTypes WHERE Id = ExamTypeID);
SET IDENTITY_INSERT ExamTypes OFF;
PRINT 'Step 4f: ExamTypes migrated.';

-- Banks (2 rows)
SET IDENTITY_INSERT Banks ON;
INSERT INTO Banks (Id, BankName, BankCode, Remarks, IsActive)
SELECT BankID, BankName, BankCode, Remarks, ISNULL(IsActive, 1)
FROM [FUExamDBcopy].dbo.Bank
WHERE NOT EXISTS (SELECT 1 FROM Banks WHERE Id = BankID);
SET IDENTITY_INSERT Banks OFF;
PRINT 'Step 4g: Banks migrated.';

-- Boards (8 rows) - use CountryId=1 (Nepal) as default for NULL CountryID
SET IDENTITY_INSERT Boards ON;
INSERT INTO Boards (Id, CountryId, BoardName, Remarks, IsActive)
SELECT BoardID, ISNULL(CountryID, 1), BoardName, Remarks, ISNULL(IsActive, 1)
FROM [FUExamDBcopy].dbo.Board
WHERE NOT EXISTS (SELECT 1 FROM Boards WHERE Id = BoardID);
SET IDENTITY_INSERT Boards OFF;
PRINT 'Step 4h: Boards migrated.';

-- Ethnicities (6 rows)
SET IDENTITY_INSERT Ethnicities ON;
INSERT INTO Ethnicities (Id, EthnicityName, IsDefault, IsActive)
SELECT EthnicGroupID, EthnicGroupName, ISNULL(IsDefault, 0), ISNULL(IsActive, 1)
FROM [FUExamDBcopy].dbo.EthnicGroup
WHERE NOT EXISTS (SELECT 1 FROM Ethnicities WHERE Id = EthnicGroupID);
SET IDENTITY_INSERT Ethnicities OFF;
PRINT 'Step 4i: Ethnicities migrated.';

PRINT 'Step 4 complete: Reference data migrated.';

-- ============================================================================
-- STEP 5: Migrate AcademicYears (14 rows)
-- ============================================================================

SET IDENTITY_INSERT AcademicYears ON;
INSERT INTO AcademicYears (Id, AcademicYearCode, AcademicYearCodeNepali, AcademicYearName, AcademicYearNameNepali, IsRunning, IsActive)
SELECT AcademicYearID,
    AcademicYearCode,
    ISNULL(AcademicYearCode, ''),
    AcademicYearName,
    ISNULL(AcademicYearNameNepali, AcademicYearName),
    ISNULL(Running, 0),
    ISNULL(IsActive, 1)
FROM [FUExamDBcopy].dbo.AcademicYear
WHERE NOT EXISTS (SELECT 1 FROM AcademicYears WHERE Id = AcademicYearID);
SET IDENTITY_INSERT AcademicYears OFF;

INSERT INTO #AcademicYearMap (SourceId, SourceCode, NewId)
SELECT AcademicYearID, AcademicYearCode, AcademicYearID
FROM [FUExamDBcopy].dbo.AcademicYear;

PRINT 'Step 5 complete: AcademicYears migrated.';

-- ============================================================================
-- STEP 6: Migrate Faculties (9 rows)
-- ============================================================================

SET IDENTITY_INSERT Faculties ON;
INSERT INTO Faculties (Id, Name, OfficeCode, ShortName, ContactNumber, Address, Email, TenantId)
SELECT FacultyID,
    FacultyName,
    ISNULL(FacultyCode, 'N/A'),
    ShortName,
    'N/A',
    'N/A',
    'N/A',
    @TenantId
FROM [FUExamDBcopy].dbo.Faculty
WHERE NOT EXISTS (SELECT 1 FROM Faculties WHERE Id = FacultyID);
SET IDENTITY_INSERT Faculties OFF;
PRINT 'Step 6 complete: Faculties migrated.';

-- ============================================================================
-- STEP 7: Migrate Colleges (53 rows)
-- ============================================================================

SET IDENTITY_INSERT Colleges ON;
INSERT INTO Colleges (Id, TenantId, Code, Name, CollegeNameNepali, ShortName, EstablishedDate, Website, Email, Phone1, Phone2, PrincipalName, PrincipalContactNumber, Fax, Remarks, IsExamCenterOnly, IsActive, CollegeTypeId)
SELECT
    CollegeID,
    @TenantId,
    CollegeCode,
    CollegeName,
    CollegeNameNepali,
    ShortName,
    EstablishedDate,
    WebAddress,
    ISNULL(NULLIF(LTRIM(RTRIM(EmailAddress)), ''), 'N/A'),
    Phone1,
    Phone2,
    ISNULL(NULLIF(LTRIM(RTRIM(ChairmanName)), ''), 'N/A'),
    ISNULL(NULLIF(LTRIM(RTRIM(ChairmanContactNo)), ''), 'N/A'),
    Fax,
    Remarks,
    ISNULL(IsCentreOnly, 0),
    ISNULL(IsActive, 1),
    CollegeTypeID
FROM [FUExamDBcopy].dbo.College
WHERE NOT EXISTS (SELECT 1 FROM Colleges WHERE Id = CollegeID);
SET IDENTITY_INSERT Colleges OFF;
PRINT 'Step 7 complete: Colleges migrated.';

-- ============================================================================
-- STEP 8: Migrate Programs (45 rows)
-- ============================================================================

SET IDENTITY_INSERT Programs ON;
INSERT INTO Programs (Id, LevelId, FacultyId, ProgramCode, ProgramName, ShortName, Duration, GrandTotalMarks, HasMultipleIntakes, IsActive, NumberOfSeats, ScholarshipSeats)
SELECT
    ProgramID,
    LevelID,
    FacultyID,
    ProgramCode,
    ProgramName,
    ShortName,
    ISNULL(CAST(Duration AS INT), 4),
    ISNULL(GrandFullMark, 4000),
    ISNULL(HasMultipleIntakes, 0),
    ISNULL(IsActive, 1),
    '0',
    0
FROM [FUExamDBcopy].dbo.Program
WHERE NOT EXISTS (SELECT 1 FROM Programs WHERE Id = ProgramID);
SET IDENTITY_INSERT Programs OFF;
PRINT 'Step 8 complete: Programs migrated.';

-- ============================================================================
-- STEP 9: Migrate CollegePrograms (194 rows)
-- ============================================================================

SET IDENTITY_INSERT CollegePrograms ON;
INSERT INTO CollegePrograms (Id, TenantId, CollegeId, ProgramId, AffiliationDate, NumberOfStudents, Remarks, IsActive)
SELECT
    CollegeProgramID,
    @TenantId,
    CollegeID,
    ProgramID,
    CASE WHEN AffiliationDate IS NOT NULL AND ISDATE(AffiliationDate) = 1
         THEN CAST(AffiliationDate AS DATETIME2)
         ELSE NULL END,
    ISNULL(NoOfSeat, 0),
    NULL,
    1
FROM [FUExamDBcopy].dbo.CollegeProgram
WHERE NOT EXISTS (SELECT 1 FROM CollegePrograms WHERE Id = CollegeProgramID);
SET IDENTITY_INSERT CollegePrograms OFF;
PRINT 'Step 9 complete: CollegePrograms migrated.';

-- ============================================================================
-- STEP 10: Migrate Batches (14 rows)
-- ============================================================================

SET IDENTITY_INSERT Batches ON;
INSERT INTO Batches (Id, AcademicYearId, BatchName, Remarks, IsActive)
SELECT
    BatchID,
    AcademicYearID,
    BatchName,
    Remarks,
    ISNULL(IsActive, 1)
FROM [FUExamDBcopy].dbo.Batch
WHERE NOT EXISTS (SELECT 1 FROM Batches WHERE Id = BatchID);
SET IDENTITY_INSERT Batches OFF;
PRINT 'Step 10 complete: Batches migrated.';

-- ============================================================================
-- STEP 11a: Create Semesters (MUST be before SubjectOfferings for FK)
-- ============================================================================

-- AcademicYearId remains NOT NULL (required by entity model)

-- Create per-AY semesters for ExamSchedules
DECLARE @SemAYId INT, @SemNum INT, @SemStart DATE, @SemEnd DATE, @SemCode NVARCHAR(30), @SemNewId INT;
DECLARE sem_cursor CURSOR FOR
    SELECT DISTINCT ay.Id
    FROM AcademicYears ay
    WHERE EXISTS (SELECT 1 FROM [FUExamDBcopy].dbo.ExamSchedule es WHERE es.AcademicYearID = ay.Id)
       OR EXISTS (SELECT 1 FROM [FUExamDBcopy].dbo.ExamRegistration er WHERE er.AcademicYearID = ay.Id);
OPEN sem_cursor;
FETCH NEXT FROM sem_cursor INTO @SemAYId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SemNum = 1;
    WHILE @SemNum <= 8
    BEGIN
        SET @SemCode = 'SEM' + CAST(@SemNum AS NVARCHAR(10)) + '-AY' + CAST(@SemAYId AS NVARCHAR(10));
        IF NOT EXISTS (SELECT 1 FROM Semesters WHERE Code = @SemCode)
        BEGIN
            SET @SemStart = DATEADD(MONTH, (@SemNum - 1) * 6, '2020-01-01');
            SET @SemEnd = DATEADD(MONTH, @SemNum * 6, '2020-01-01');
            INSERT INTO Semesters (Number, Year, Name, Code, AcademicYearId, FacultyId, StartDate, EndDate)
            VALUES (@SemNum, CEILING(CAST(@SemNum AS FLOAT) / 2),
                'Semester ' + CAST(@SemNum AS NVARCHAR(10)),
                @SemCode, @SemAYId, NULL, @SemStart, @SemEnd);
        END

        SET @SemNewId = (SELECT Id FROM Semesters WHERE Code = @SemCode);

        INSERT INTO #AYSemesterMap (SourceYear, SourcePart, AcademicYearId, NewId)
        SELECT y.YearVal, p.PartVal, @SemAYId, @SemNewId
        FROM (VALUES ('I'), ('II'), ('III'), ('IV'), ('V')) y(YearVal)
        CROSS JOIN (VALUES ('I'), ('II')) p(PartVal)
        WHERE (
            (y.YearVal = 'I' AND p.PartVal = 'I' AND @SemNum = 1) OR
            (y.YearVal = 'I' AND p.PartVal = 'II' AND @SemNum = 2) OR
            (y.YearVal = 'II' AND p.PartVal = 'I' AND @SemNum = 3) OR
            (y.YearVal = 'II' AND p.PartVal = 'II' AND @SemNum = 4) OR
            (y.YearVal = 'III' AND p.PartVal = 'I' AND @SemNum = 5) OR
            (y.YearVal = 'III' AND p.PartVal = 'II' AND @SemNum = 6) OR
            (y.YearVal = 'IV' AND p.PartVal = 'I' AND @SemNum = 7) OR
            (y.YearVal = 'IV' AND p.PartVal = 'II' AND @SemNum = 8) OR
            (y.YearVal = 'V' AND p.PartVal = 'I' AND @SemNum = 9) OR
            (y.YearVal = 'V' AND p.PartVal = 'II' AND @SemNum = 10)
        );

        SET @SemNum = @SemNum + 1;
    END
    FETCH NEXT FROM sem_cursor INTO @SemAYId;
END
CLOSE sem_cursor;
DEALLOCATE sem_cursor;

PRINT '  Per-AY semesters created for ExamSchedules.';

-- #SemesterMap: maps source (Year, Part) → per-AY SemesterId (for SubjectOfferings)
INSERT INTO #SemesterMap (SourceYear, SourcePart, AcademicYearId, NewId)
SELECT y.YearVal, p.PartVal, s.AcademicYearId, MIN(s.Id)
FROM (VALUES ('I'), ('II'), ('III'), ('IV'), ('V')) y(YearVal)
CROSS JOIN (VALUES ('I'), ('II')) p(PartVal)
INNER JOIN Semesters s ON s.Number = (
    CASE
        WHEN y.YearVal = 'I'  AND p.PartVal = 'I'  THEN 1
        WHEN y.YearVal = 'I'  AND p.PartVal = 'II' THEN 2
        WHEN y.YearVal = 'II' AND p.PartVal = 'I'  THEN 3
        WHEN y.YearVal = 'II' AND p.PartVal = 'II' THEN 4
        WHEN y.YearVal = 'III' AND p.PartVal = 'I'  THEN 5
        WHEN y.YearVal = 'III' AND p.PartVal = 'II' THEN 6
        WHEN y.YearVal = 'IV'  AND p.PartVal = 'I'  THEN 7
        WHEN y.YearVal = 'IV'  AND p.PartVal = 'II' THEN 8
        WHEN y.YearVal = 'V'   AND p.PartVal = 'I'  THEN 9
        WHEN y.YearVal = 'V'   AND p.PartVal = 'II' THEN 10
    END
)
WHERE s.AcademicYearId IS NOT NULL
GROUP BY y.YearVal, p.PartVal, s.AcademicYearId;

PRINT 'Step 11a: Semesters created.';

-- ============================================================================
-- STEP 11b: Migrate SubjectCatalogs + SubjectOfferings (split from SubjectDetail)
-- ============================================================================

-- SubjectCatalogs (deduplicated by SubjectCode)
SET IDENTITY_INSERT SubjectCatalogs ON;
INSERT INTO SubjectCatalogs (Id, SubjectCode, SubjectName, ShortName, CreditHours, SubjectTypeId, IsActive)
SELECT
    MIN(sd.SubjectDetailID),
    sd.SubjectCode,
    MAX(sd.SubjectName),
    MAX(sd.SubjectShortName),
    MAX(sd.CreditHour),
    ISNULL(MAX(sd.SubjectTypeID), 1),
    1
FROM [FUExamDBcopy].dbo.SubjectDetail sd
WHERE sd.SubjectCode IS NOT NULL AND LTRIM(RTRIM(sd.SubjectCode)) <> ''
GROUP BY sd.SubjectCode
HAVING NOT EXISTS (SELECT 1 FROM SubjectCatalogs WHERE SubjectCode = sd.SubjectCode);
SET IDENTITY_INSERT SubjectCatalogs OFF;

INSERT INTO #SubjectCatalogMap (SourceSubjectDetailId, SourceSubjectCode, NewCatalogId, NewOfferingId)
SELECT MIN(sd.SubjectDetailID), sd.SubjectCode, MIN(sd.SubjectDetailID), 0
FROM [FUExamDBcopy].dbo.SubjectDetail sd
WHERE sd.SubjectCode IS NOT NULL AND LTRIM(RTRIM(sd.SubjectCode)) <> ''
GROUP BY sd.SubjectCode;

PRINT 'Step 11b: SubjectCatalogs created.';

-- SubjectOfferings (one per SubjectDetail row)
DECLARE @SemMapTemp TABLE (
    SourceSubjectDetailId INT,
    SemesterId INT
);

INSERT INTO @SemMapTemp (SourceSubjectDetailId, SemesterId)
SELECT sd.SubjectDetailID,
    MIN(sm.NewId)
FROM [FUExamDBcopy].dbo.SubjectDetail sd
INNER JOIN #SemesterMap sm ON sm.SourceYear = sd.Year AND sm.SourcePart = sd.Part
GROUP BY sd.SubjectDetailID;

SET IDENTITY_INSERT SubjectOfferings ON;
;WITH cte AS (
    SELECT
        sd.SubjectDetailID,
        @TenantId AS TenantId,
        scm.NewCatalogId AS SubjectCatalogId,
        sd.ProgramID AS ProgramId,
        sm.SemesterId,
        CASE WHEN st.SubjectTypeName = 'Comp' THEN 1 ELSE 0 END AS IsCompulsory,
        ISNULL(sd.DisplayOrder, 0) AS DisplayOrder,
        CASE WHEN sd.TheoryFullMark > 0 THEN 1 ELSE 0 END AS HasTheory,
        sd.HasPractical,
        sd.HasInternal,
        ISNULL(sd.TheoryFullMark, 0) AS TheoryFullMarks,
        ISNULL(sd.TheoryPassMark, 0) AS TheoryPassMarks,
        ISNULL(sd.PracticalFullMark, 0) AS PracticalFullMarks,
        ISNULL(sd.PracticalPassMark, 0) AS PracticalPassMarks,
        ISNULL(sd.InternalFullMark, 0) AS InternalTheoryFullMarks,
        ISNULL(sd.InternalPassMark, 0) AS InternalTheoryPassMarks,
        ROW_NUMBER() OVER (PARTITION BY scm.NewCatalogId, sd.ProgramID, sm.SemesterId ORDER BY sd.SubjectDetailID) AS rn
    FROM [FUExamDBcopy].dbo.SubjectDetail sd
    INNER JOIN #SubjectCatalogMap scm ON sd.SubjectCode = scm.SourceSubjectCode
    INNER JOIN @SemMapTemp sm ON sd.SubjectDetailID = sm.SourceSubjectDetailId
    LEFT JOIN [FUExamDBcopy].dbo.SubjectType st ON sd.SubjectTypeID = st.SubjectTypeID
    WHERE sm.SemesterId IS NOT NULL
)
INSERT INTO SubjectOfferings (Id, TenantId, SubjectCatalogId, ProgramId, SemesterId, IsCompulsory, DisplayOrder,
    HasTheory, HasPractical, HasInternal,
    TheoryFullMarks, TheoryPassMarks, PracticalFullMarks, PracticalPassMarks,
    InternalTheoryFullMarks, InternalTheoryPassMarks, InternalPracticalFullMarks, InternalPracticalPassMarks)
SELECT SubjectDetailID, TenantId, SubjectCatalogId, ProgramId, SemesterId, IsCompulsory, DisplayOrder,
    HasTheory, HasPractical, HasInternal,
    TheoryFullMarks, TheoryPassMarks, PracticalFullMarks, PracticalPassMarks,
    InternalTheoryFullMarks, 0, InternalTheoryPassMarks, 0
FROM cte WHERE rn = 1;
SET IDENTITY_INSERT SubjectOfferings OFF;

UPDATE scm SET scm.NewOfferingId = sd.SubjectDetailID
FROM #SubjectCatalogMap scm
INNER JOIN [FUExamDBcopy].dbo.SubjectDetail sd ON scm.SourceSubjectCode = sd.SubjectCode;

PRINT 'Step 11 complete: SubjectCatalogs + SubjectOfferings migrated.';

-- ============================================================================
-- STEP 12: Migrate StudentRegistrations (44,640 rows)
-- ============================================================================

SET IDENTITY_INSERT StudentRegistrations ON;
;WITH cte AS (
    SELECT
        sr.StudentRegistrationID,
        @TenantId AS TenantId,
        sr.LevelID AS LevelId,
        sr.CollegeID AS CollegeId,
        sr.FacultyID AS FacultyId,
        NULL AS ProgramId,
        sr.RegistrationNo AS RegistrationNumber,
        sr.FirstName,
        NULLIF(LTRIM(RTRIM(sr.MiddleName)), '') AS MiddleName,
        sr.LastName,
        sr.FullNameNepali AS NepaliName,
        LEFT(NULLIF(LTRIM(RTRIM(sr.ContactNo)), ''), 15) AS ContactNumber,
        NULLIF(LTRIM(RTRIM(sr.Email)), '') AS Email,
        ISNULL(sr.BirthDateBS, '') AS DateOfBirthBS,
        CASE WHEN sr.BirthDateAD IS NOT NULL THEN CONVERT(NVARCHAR(10), sr.BirthDateAD, 23) ELSE NULL END AS DateOfBirthAD,
        ISNULL(NULLIF(sr.GenderID, 0), 1) AS GenderId,
        1 AS StudentCategoryId,
        sr.AcademicYearID AS AcademicYearId,
        ISNULL(sr.IsActive, 1) AS IsActive,
        sr.StudentRegistrationIndex,
        NULLIF(sr.EthnicGroupID, 0) AS EthnicityId,
        dm.TargetDistrictId AS DistrictId,
        lm.TargetLocalLevelId AS LocalLevelId,
        ROW_NUMBER() OVER (PARTITION BY sr.StudentRegistrationID ORDER BY sr.StudentRegistrationID) AS rn
    FROM [FUExamDBcopy].dbo.StudentRegistration sr
    LEFT JOIN #DistrictMap dm ON sr.DistrictID = dm.SourceDistrictId
    LEFT JOIN #LocalLevelMap lm ON sr.LocalLevelID = lm.SourceLocalLevelId
)
INSERT INTO StudentRegistrations (Id, TenantId, LevelId, CollegeId, FacultyId, ProgramId, RegistrationNumber,
    FirstName, MiddleName, LastName, NepaliName, ContactNumber, Email,
    DateOfBirthBS, DateOfBirthAD, GenderId, StudentCategoryId, AcademicYearId,
    IsActive, StudentRegistrationIndex, EthnicityId, DistrictId, LocalLevelId)
SELECT StudentRegistrationID, TenantId, LevelId, CollegeId, FacultyId, ProgramId, RegistrationNumber,
    FirstName, MiddleName, LastName, NepaliName, ContactNumber, Email,
    DateOfBirthBS, DateOfBirthAD, GenderId, StudentCategoryId, AcademicYearId,
    IsActive, StudentRegistrationIndex, EthnicityId, DistrictId, LocalLevelId
FROM cte WHERE rn = 1;
SET IDENTITY_INSERT StudentRegistrations OFF;

INSERT INTO #StudentRegMap (SourceId, NewId)
SELECT StudentRegistrationID, StudentRegistrationID
FROM [FUExamDBcopy].dbo.StudentRegistration;

PRINT 'Step 12 complete: StudentRegistrations migrated.';

-- ============================================================================
-- STEP 13: Migrate StudentAdmissions (46,860 rows) + link to registrations
-- ============================================================================

CREATE TABLE #AdmissionMap (SourceAdmissionId INT, SourceStudentRegId INT, NewId INT);

SET IDENTITY_INSERT StudentAdmissions ON;
INSERT INTO StudentAdmissions (Id, TenantId, ProgramsId, CollegeId, AcademicYearId,
    AdmissionDate, CheckedBy, IsCompleted, IsActive, HasFeeExemption)
SELECT
    sa.StudentAdmissionID,
    @TenantId,
    sa.ProgramID,
    sa.CollegeID,
    ISNULL(sa.AcademicYearID, b.AcademicYearID),
    ISNULL(sa.AdmissionDate, '2016-01-01'),
    sa.CheckedBy,
    ISNULL(sa.IsCompleted, 0),
    ISNULL(sa.IsActive, 1),
    0
FROM [FUExamDBcopy].dbo.StudentAdmission sa
LEFT JOIN [FUExamDBcopy].dbo.Batch b ON sa.BatchID = b.BatchID;
SET IDENTITY_INSERT StudentAdmissions OFF;

INSERT INTO #AdmissionMap (SourceAdmissionId, SourceStudentRegId, NewId)
SELECT sa.StudentAdmissionID, sa.StudentRegistrationID, sa.StudentAdmissionID
FROM [FUExamDBcopy].dbo.StudentAdmission sa;

PRINT 'Step 13a: StudentAdmissions created.';

-- Link StudentRegistrations → StudentAdmissions + fill ProgramId
UPDATE sr
SET sr.StudentAdmissionId = am.NewId,
    sr.ProgramId = sa.ProgramID
FROM StudentRegistrations sr
INNER JOIN #AdmissionMap am ON sr.Id = am.SourceStudentRegId
INNER JOIN [FUExamDBcopy].dbo.StudentAdmission sa ON sa.StudentAdmissionID = am.SourceAdmissionId;

PRINT 'Step 13b: StudentRegistrations linked to admissions + ProgramId filled.';

DECLARE @CntAdm INT = (SELECT COUNT(*) FROM StudentAdmissions);
PRINT 'Step 13 complete: StudentAdmissions migrated. Count=' + CAST(@CntAdm AS VARCHAR);

DROP TABLE #AdmissionMap;

-- ============================================================================
-- STEP 14: Migrate ExamSchedules (98 rows) + ExamCenters
-- ============================================================================

-- Derive correct ProgramId for each ExamSchedule from source data:
-- ExamSchedule → ExamRegistration → StudentAdmission → ProgramID
CREATE TABLE #ExamScheduleProgramMap (
    ExamScheduleId INT PRIMARY KEY,
    ProgramId INT NOT NULL
);

INSERT INTO #ExamScheduleProgramMap (ExamScheduleId, ProgramId)
SELECT ExamScheduleId, ProgramId
FROM (
    SELECT er.ExamScheduleID, sa.ProgramID,
        ROW_NUMBER() OVER (PARTITION BY er.ExamScheduleID ORDER BY COUNT(*) DESC) AS rn
    FROM [FUExamDBcopy].dbo.ExamRegistration er
    INNER JOIN [FUExamDBcopy].dbo.StudentAdmission sa ON sa.CollegeID = er.CollegeID AND sa.AcademicYearID = er.AcademicYearID
    WHERE sa.ProgramID IS NOT NULL
    GROUP BY er.ExamScheduleID, sa.ProgramID
) ranked
WHERE rn = 1;

-- Fallback: for ExamSchedules with no ExamRegistrations, use LevelId → first active program
INSERT INTO #ExamScheduleProgramMap (ExamScheduleId, ProgramId)
SELECT es.ExamScheduleID,
    ISNULL((SELECT TOP 1 p.Id FROM Programs p WHERE p.LevelId = es.LevelID AND p.IsActive = 1),
           (SELECT TOP 1 p.Id FROM Programs p WHERE p.LevelId = es.LevelID))
FROM [FUExamDBcopy].dbo.ExamSchedule es
WHERE NOT EXISTS (SELECT 1 FROM #ExamScheduleProgramMap WHERE ExamScheduleId = es.ExamScheduleID);

SET IDENTITY_INSERT ExamSchedules ON;
INSERT INTO ExamSchedules (Id, TenantId, ExamScheduleName, AcademicYearId, LevelId, IsActive,
    StartTime, EndTime, StartDate, EndDate, StartDateBs, EndDateBs, PublishedDate, Remarks,
    ProgramId, SemesterId, ExamTypeId)
SELECT
    es.ExamScheduleID,
    @TenantId,
    es.ExamScheduleName,
    es.AcademicYearID,
    es.LevelID,
    ISNULL(es.Active, 1),
    ISNULL(es.StartTime, '08:00:00'),
    ISNULL(es.EndTime, '11:00:00'),
    es.StartFromAD,
    es.EndToAD,
    es.StartFromBS,
    es.EndToBS,
    es.PublishedDate,
    es.Remarks,
    ISNULL(pgm.ProgramId, (SELECT TOP 1 p.Id FROM Programs p WHERE p.LevelId = es.LevelID AND p.IsActive = 1)),
    ISNULL(sm.NewId, (SELECT TOP 1 s.Id FROM Semesters s WHERE s.AcademicYearId = es.AcademicYearID)),
    CASE
        WHEN es.ExamScheduleName LIKE '%Partial%' THEN 2
        WHEN es.ExamScheduleName LIKE '%Supplementary%' THEN 3
        WHEN es.ExamScheduleName LIKE '%Chance%' THEN 4
        WHEN es.ExamScheduleName LIKE '%Special%' THEN 5
        ELSE 1
    END
FROM [FUExamDBcopy].dbo.ExamSchedule es
LEFT JOIN #AYSemesterMap sm ON sm.SourceYear = es.Year AND sm.SourcePart = es.Part AND sm.AcademicYearId = es.AcademicYearID
LEFT JOIN #ExamScheduleProgramMap pgm ON pgm.ExamScheduleId = es.ExamScheduleID
WHERE NOT EXISTS (SELECT 1 FROM ExamSchedules WHERE Id = es.ExamScheduleID);
SET IDENTITY_INSERT ExamSchedules OFF;

INSERT INTO #ExamScheduleMap (SourceId, NewId)
SELECT ExamScheduleID, ExamScheduleID
FROM [FUExamDBcopy].dbo.ExamSchedule;

PRINT 'Step 14b: ExamSchedules migrated.';

-- Create placeholder ExamSchedules for AYs that have ExamRegistrations but no ExamSchedules
-- Derive ProgramId from ExamRegistrations → StudentAdmissions chain
SET IDENTITY_INSERT ExamSchedules ON;
INSERT INTO ExamSchedules (Id, TenantId, ExamScheduleName, AcademicYearId, LevelId, IsActive,
    StartTime, EndTime, StartDate, EndDate, StartDateBs, EndDateBs, Remarks, ProgramId, SemesterId, ExamTypeId)
SELECT
    100 + ay.Id,
    @TenantId,
    'Regular ' + ay.AcademicYearName,
    ay.Id,
    ISNULL(ph.ProgramLevelId, (SELECT TOP 1 LevelId FROM Programs WHERE IsActive = 1)),
    1,
    '08:00:00',
    '11:00:00',
    NULL,
    NULL,
    NULL,
    NULL,
    'Auto-created for exam registrations',
    ISNULL(ph.ProgramId, (SELECT TOP 1 Id FROM Programs WHERE IsActive = 1)),
    (SELECT TOP 1 Id FROM Semesters s WHERE s.AcademicYearId = ay.Id),
    (SELECT TOP 1 Id FROM ExamTypes)
FROM AcademicYears ay
LEFT JOIN (
    SELECT er.AcademicYearID, sa.ProgramID, p.LevelId AS ProgramLevelId,
        ROW_NUMBER() OVER (PARTITION BY er.AcademicYearID ORDER BY COUNT(*) DESC) AS rn
    FROM [FUExamDBcopy].dbo.ExamRegistration er
    INNER JOIN [FUExamDBcopy].dbo.StudentAdmission sa ON sa.CollegeID = er.CollegeID AND sa.AcademicYearID = er.AcademicYearID
    INNER JOIN Programs p ON p.Id = sa.ProgramID
    WHERE sa.ProgramID IS NOT NULL
    GROUP BY er.AcademicYearID, sa.ProgramID, p.LevelId
) ph ON ph.AcademicYearID = ay.Id AND ph.rn = 1
WHERE NOT EXISTS (SELECT 1 FROM ExamSchedules WHERE AcademicYearId = ay.Id)
  AND EXISTS (SELECT 1 FROM [FUExamDBcopy].dbo.ExamRegistration WHERE AcademicYearID = ay.Id);
SET IDENTITY_INSERT ExamSchedules OFF;

-- Also create ExamCenters for these new schedules
SET IDENTITY_INSERT ExamCenters ON;
INSERT INTO ExamCenters (Id, TenantId, ExamScheduleId, CollegeId, Code, Remark, IsActive)
SELECT
    100 + es.Id,
    @TenantId,
    es.Id,
    NULL,
    'EC' + CAST(es.Id AS NVARCHAR(10)),
    'Auto-created',
    1
FROM ExamSchedules es
WHERE es.Id > 100
  AND NOT EXISTS (SELECT 1 FROM ExamCenters WHERE ExamScheduleId = es.Id);
SET IDENTITY_INSERT ExamCenters OFF;

PRINT 'Step 14c: Placeholder ExamSchedules + ExamCenters created.';

-- ============================================================================
-- STEP 14d: Verify + Fix ExamSchedule ProgramId
-- ============================================================================
-- Re-derive correct ProgramId for ALL ExamSchedules (including placeholders)
-- using ExamRegistration → StudentAdmission → ProgramID via CollegeID + AcademicYearID.
-- This catches any mismatches from earlier steps.

UPDATE es
SET es.ProgramId = cpm.CorrectProgramId
FROM ExamSchedules es
INNER JOIN (
    SELECT ExamScheduleId, CorrectProgramId
    FROM (
        SELECT er.ExamScheduleID, sa.ProgramID AS CorrectProgramId,
            ROW_NUMBER() OVER (PARTITION BY er.ExamScheduleID ORDER BY COUNT(*) DESC) AS rn
        FROM [FUExamDBcopy].dbo.ExamRegistration er
        INNER JOIN [FUExamDBcopy].dbo.StudentAdmission sa
            ON sa.CollegeID = er.CollegeID AND sa.AcademicYearID = er.AcademicYearID
        WHERE sa.ProgramID IS NOT NULL
        GROUP BY er.ExamScheduleID, sa.ProgramID
    ) ranked
    WHERE rn = 1
) cpm ON cpm.ExamScheduleId = es.Id
WHERE es.ProgramId != cpm.CorrectProgramId;

DECLARE @FixedCnt INT = @@ROWCOUNT;
PRINT 'Step 14d: ExamSchedule ProgramIds verified. Fixed=' + CAST(@FixedCnt AS VARCHAR);

-- ============================================================================
-- STEP 15: Migrate ExamRegistrations (248,797 rows)
-- ============================================================================

SET IDENTITY_INSERT ExamRegistrations ON;
INSERT INTO ExamRegistrations (Id, TenantId, AcademicYearId, ExamCenterId, CollegeId,
    ExamRollNumber, ExamRollNumberCoding, FeeEnclosed, AttendancePercentage,
    RegistrationDate, Sgpa, IsActive, ExamScheduleId, ProgramsId, Status)
SELECT
    er.ExamRegistrationID,
    @TenantId,
    er.AcademicYearID,
    NULL AS ExamCenterId,
    er.CollegeID,
    er.ExamRollNo,
    er.ExamRollNoCoding,
    er.FeeEnclosed,
    er.AttendancePercentage,
    er.RegistrationDate,
    er.SGPA,
    ISNULL(er.IsActive, 1),
    ISNULL(esm.NewId, (SELECT TOP 1 Id FROM ExamSchedules WHERE AcademicYearId = er.AcademicYearID)),
    NULL,
    CASE
        WHEN er.ResultStatus = 'Pass' THEN 1
        WHEN er.ResultStatus = 'Fail' THEN 2
        ELSE 0
    END
FROM [FUExamDBcopy].dbo.ExamRegistration er
LEFT JOIN #ExamScheduleMap esm ON er.ExamScheduleID = esm.SourceId AND esm.SourceId > 0
WHERE NOT EXISTS (SELECT 1 FROM ExamRegistrations WHERE Id = er.ExamRegistrationID);
SET IDENTITY_INSERT ExamRegistrations OFF;

INSERT INTO #ExamRegMap (SourceId, NewId)
SELECT ExamRegistrationID, ExamRegistrationID
FROM [FUExamDBcopy].dbo.ExamRegistration;

PRINT 'Step 15 complete: ExamRegistrations migrated.';

-- ============================================================================
-- STEP 16: Migrate StudentQualifications (8,427 rows)
-- ============================================================================

SET IDENTITY_INSERT StudentQualifications ON;
INSERT INTO StudentQualifications (Id, TenantId, StudentRegistrationId, BoardId, PreviousLevelId,
    InstituteName, PassedYear, Specialization, Percentage, Remarks, IsHigherDegree, IsActive)
SELECT
    sq.StudentQualificationID,
    @TenantId,
    sq.StudentRegistrationID,
    sq.BoardID,
    sq.PreviousLevelID,
    sq.InstituteName,
    sq.PassedYear,
    sq.Specialization,
    CASE WHEN sq.Percentage IS NOT NULL AND ISNUMERIC(sq.Percentage) = 1
         AND CAST(sq.Percentage AS DECIMAL(10,2)) BETWEEN 0 AND 100
         THEN CAST(sq.Percentage AS DECIMAL(10,2))
         ELSE NULL END,
    sq.Remarks,
    ISNULL(sq.IsHigherDegree, 0),
    ISNULL(sq.IsActive, 1)
FROM [FUExamDBcopy].dbo.StudentQualification sq
INNER JOIN StudentRegistrations sr ON sr.Id = sq.StudentRegistrationID
WHERE NOT EXISTS (SELECT 1 FROM StudentQualifications WHERE Id = sq.StudentQualificationID);
SET IDENTITY_INSERT StudentQualifications OFF;

PRINT 'Step 16 complete: StudentQualifications migrated.';

-- ============================================================================
-- STEP 17: SKIPPED - ExamSubjectAndMarksRegistration not in source
-- ============================================================================
PRINT 'Step 17 SKIPPED: ExamSubjectAndMarksRegistration not in source.';

-- ============================================================================
-- STEP 18: SKIPPED - ExamScheduleDetail not in source
-- ============================================================================
PRINT 'Step 18 SKIPPED: ExamScheduleDetail not in source.';

-- ============================================================================
-- STEP 19: Verification
-- ============================================================================

PRINT '';
PRINT '========================================';
PRINT 'VERIFICATION: Row counts';
PRINT '========================================';

SELECT 'Tenants' AS Tbl, COUNT(*) AS Cnt FROM Tenants
UNION ALL SELECT 'Provinces', COUNT(*) FROM Provinces
UNION ALL SELECT 'Districts', COUNT(*) FROM Districts
UNION ALL SELECT 'LocalLevels', COUNT(*) FROM LocalLevels
UNION ALL SELECT 'Countries', COUNT(*) FROM Countries
UNION ALL SELECT 'AcademicYears', COUNT(*) FROM AcademicYears
UNION ALL SELECT 'Levels', COUNT(*) FROM Levels
UNION ALL SELECT 'Genders', COUNT(*) FROM Genders
UNION ALL SELECT 'CollegeTypes', COUNT(*) FROM CollegeTypes
UNION ALL SELECT 'PreviousLevels', COUNT(*) FROM PreviousLevels
UNION ALL SELECT 'SubjectTypes', COUNT(*) FROM SubjectTypes
UNION ALL SELECT 'ExamTypes', COUNT(*) FROM ExamTypes
UNION ALL SELECT 'Banks', COUNT(*) FROM Banks
UNION ALL SELECT 'Boards', COUNT(*) FROM Boards
UNION ALL SELECT 'Ethnicities', COUNT(*) FROM Ethnicities
UNION ALL SELECT 'Faculties', COUNT(*) FROM Faculties
UNION ALL SELECT 'Colleges', COUNT(*) FROM Colleges
UNION ALL SELECT 'Programs', COUNT(*) FROM Programs
UNION ALL SELECT 'CollegePrograms', COUNT(*) FROM CollegePrograms
UNION ALL SELECT 'Batches', COUNT(*) FROM Batches
UNION ALL SELECT 'Semesters', COUNT(*) FROM Semesters
UNION ALL SELECT 'SubjectCatalogs', COUNT(*) FROM SubjectCatalogs
UNION ALL SELECT 'SubjectOfferings', COUNT(*) FROM SubjectOfferings
UNION ALL SELECT 'StudentRegistrations', COUNT(*) FROM StudentRegistrations
UNION ALL SELECT 'StudentAdmissions', COUNT(*) FROM StudentAdmissions
UNION ALL SELECT 'ExamSchedules', COUNT(*) FROM ExamSchedules
UNION ALL SELECT 'ExamCenters', COUNT(*) FROM ExamCenters
UNION ALL SELECT 'ExamRegistrations', COUNT(*) FROM ExamRegistrations
UNION ALL SELECT 'StudentQualifications', COUNT(*) FROM StudentQualifications;

-- ============================================================================
-- STEP 20: Cleanup
-- ============================================================================

DROP TABLE #DistrictMap;
DROP TABLE #LocalLevelMap;
DROP TABLE #AcademicYearMap;
DROP TABLE #SubjectCatalogMap;
DROP TABLE #StudentRegMap;
DROP TABLE #ExamScheduleMap;
DROP TABLE #ExamCenterMap;
DROP TABLE #ExamRegMap;
DROP TABLE #SemesterMap;
DROP TABLE #AYSemesterMap;
DROP TABLE #ExamScheduleProgramMap;

PRINT '';
PRINT '========================================';
PRINT 'MIGRATION COMPLETE';
PRINT '========================================';
