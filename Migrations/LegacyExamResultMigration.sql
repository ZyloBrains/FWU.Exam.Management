-- ============================================
-- FWUExams.Legacy -> FUExamsDb Migration Script
-- ============================================
-- Migrates student exam result data from 3 denormalized
-- legacy tables into normalized FUExamsDb schema.
--
-- Source:  CivilEngineering (43,615 rows)
--          ComputerEngineering (11,515 rows)
--          CPM (636 rows)
--          Total: ~55,766 rows
--
-- Target:  ExamSchedules (~130), ExamCenters (~130),
--          ExamRegistrations (~7,941),
--          ExamSubjectResults (~55,766)
--
-- Prerequisites:
--   - TenantId=2 (Engineering Exam Office) exists
--   - Programs L092(8), L117(9), L131(22) exist
--   - AcademicYears 2014-2025 exist
--   - Colleges SCH001(1), SCH007(8), SCH129(68) exist
--   - ExamTypes Regular(1), Partial(2), Supplementary(3), Chance(4) exist
--   - All 890 students already in StudentRegistrations
--   - All subject catalogs already exist
-- ============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRANSACTION;

-- ============================================
-- STEP 0: Pre-cleanup
-- Remove existing test data for tenant 2
-- ============================================

DELETE FROM HallTickets WHERE TenantId = 2;
DELETE FROM ExamSubjectResults WHERE TenantId = 2;
DELETE FROM ExamRegistrations WHERE TenantId = 2;

PRINT 'Step 0: Cleaned up existing test data.';

-- ============================================
-- STEP 1: Create temp mapping tables
-- ============================================

-- Semester: Year+Part -> SemesterId
CREATE TABLE #SemesterMap (
    YearPart NVARCHAR(10) PRIMARY KEY,
    SemesterId INT
);
INSERT INTO #SemesterMap VALUES
('I-I', 1), ('I-II', 2), ('II-I', 3), ('II-II', 4),
('III-I', 5), ('III-II', 6), ('IV-I', 7), ('IV-II', 8);

-- Program: ProgramCode -> ProgramId
CREATE TABLE #ProgramMap (
    ProgramCode NVARCHAR(50) PRIMARY KEY,
    ProgramId INT
);
INSERT INTO #ProgramMap VALUES
('L092', 8), ('L117', 9), ('L131', 22);

-- Level: ProgramCode -> LevelId
CREATE TABLE #LevelMap (
    ProgramCode NVARCHAR(50) PRIMARY KEY,
    LevelId INT
);
INSERT INTO #LevelMap VALUES
('L092', 1), ('L117', 1), ('L131', 2);

-- College: CollegeCode -> CollegeId
CREATE TABLE #CollegeMap (
    CollegeCode NVARCHAR(50) PRIMARY KEY,
    CollegeId INT
);
INSERT INTO #CollegeMap VALUES
('SCH001', 1), ('SCH007', 8), ('SCH129', 68);

-- ExamType: ExamTypeName -> ExamTypeId
CREATE TABLE #ExamTypeMap (
    ExamTypeName NVARCHAR(50) PRIMARY KEY,
    ExamTypeId INT
);
INSERT INTO #ExamTypeMap VALUES
('Regular', 1), ('Partial', 2), ('Supplementary', 3), ('Chance', 4);

-- AcademicYear: source float -> target Id
CREATE TABLE #AcademicYearMap (
    SourceYear FLOAT PRIMARY KEY,
    AcademicYearId INT
);
INSERT INTO #AcademicYearMap
SELECT DISTINCT src.AcademicYearName, ay.Id
FROM (
    SELECT DISTINCT CAST(AcademicYearName AS FLOAT) AS AcademicYearName
    FROM [FWUExams.Legacy].dbo.CivilEngineering
    UNION
    SELECT DISTINCT CAST(AcademicYearName AS FLOAT)
    FROM [FWUExams.Legacy].dbo.ComputerEngineering
    UNION
    SELECT DISTINCT CAST(AcademicYearName AS FLOAT)
    FROM [FWUExams.Legacy].dbo.CPM
) src
INNER JOIN AcademicYears ay ON ay.AcademicYearName = CAST(CAST(src.AcademicYearName AS INT) AS NVARCHAR(20));

-- Gender: GenderName -> GenderId
CREATE TABLE #GenderMap (
    GenderName NVARCHAR(50) PRIMARY KEY,
    GenderId INT
);
INSERT INTO #GenderMap VALUES ('Male', 1), ('Female', 2);

PRINT 'Step 1: Created temp mapping tables.';

-- ============================================
-- STEP 2: Ensure SemesterInstances exist
-- ============================================

-- Create missing SemesterInstances for needed (Semester, AcademicYear, Program) combos
;WITH NeededSemesterInstances AS (
    SELECT DISTINCT
        sm.SemesterId,
        aym.AcademicYearId,
        pm.ProgramId
    FROM (
        SELECT DISTINCT Year, Part, AcademicYearName, ProgramCode
        FROM [FWUExams.Legacy].dbo.CivilEngineering
        UNION
        SELECT DISTINCT Year, Part, AcademicYearName, ProgramCode
        FROM [FWUExams.Legacy].dbo.ComputerEngineering
        UNION
        SELECT DISTINCT Year, Part, AcademicYearName, ProgramCode
        FROM [FWUExams.Legacy].dbo.CPM
    ) src
    INNER JOIN #SemesterMap sm ON sm.YearPart = src.Year + '-' + src.Part
    INNER JOIN #AcademicYearMap aym ON aym.SourceYear = CAST(src.AcademicYearName AS FLOAT)
    INNER JOIN #ProgramMap pm ON pm.ProgramCode = src.ProgramCode
)
INSERT INTO SemesterInstances (TenantId, SemesterId, AcademicYearId, ProgramId)
SELECT 2, nsi.SemesterId, nsi.AcademicYearId, nsi.ProgramId
FROM NeededSemesterInstances nsi
WHERE NOT EXISTS (
    SELECT 1 FROM SemesterInstances si
    WHERE si.TenantId = 2
      AND si.SemesterId = nsi.SemesterId
      AND si.AcademicYearId = nsi.AcademicYearId
      AND si.ProgramId = nsi.ProgramId
);

PRINT 'Step 2: Ensured SemesterInstances exist.';

-- ============================================
-- STEP 3: Create ExamSchedules
-- ============================================

-- Collect distinct schedule combinations from source
CREATE TABLE #SourceSchedules (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AcademicYearName FLOAT,
    ProgramCode NVARCHAR(50),
    Year_ NVARCHAR(10),
    Part_ NVARCHAR(10),
    ExamTypeName NVARCHAR(50)
);

INSERT INTO #SourceSchedules (AcademicYearName, ProgramCode, Year_, Part_, ExamTypeName)
SELECT DISTINCT
    CAST(src.AcademicYearName AS FLOAT),
    src.ProgramCode,
    src.Year,
    src.Part,
    src.ExamTypeName
FROM (
    SELECT DISTINCT AcademicYearName, ProgramCode, Year, Part, ExamTypeName
    FROM [FWUExams.Legacy].dbo.CivilEngineering
    UNION
    SELECT DISTINCT AcademicYearName, ProgramCode, Year, Part, ExamTypeName
    FROM [FWUExams.Legacy].dbo.ComputerEngineering
    UNION
    SELECT DISTINCT AcademicYearName, ProgramCode, Year, Part, ExamTypeName
    FROM [FWUExams.Legacy].dbo.CPM
) src;

-- Create ExamSchedules
DECLARE @ScheduleCount INT = 0;
DECLARE @TotalSchedules INT = (SELECT COUNT(*) FROM #SourceSchedules);

-- Temp table to capture ExamSchedule ID mappings
CREATE TABLE #ExamScheduleMap (
    SourceId INT PRIMARY KEY,
    NewExamScheduleId INT
);

DECLARE @CurrentId INT = 1;
DECLARE @MaxId INT = (SELECT MAX(Id) FROM #SourceSchedules);

WHILE @CurrentId <= @MaxId
BEGIN
    DECLARE @AyName FLOAT, @ProgCode NVARCHAR(50), @Yr NVARCHAR(10), @Pt NVARCHAR(10), @ExType NVARCHAR(50);
    DECLARE @SemId INT, @ProgId INT, @LvId INT, @ExTypeId INT, @AyId INT, @SemInstId INT;

    SELECT @AyName = AcademicYearName, @ProgCode = ProgramCode,
           @Yr = Year_, @Pt = Part_, @ExType = ExamTypeName
    FROM #SourceSchedules WHERE Id = @CurrentId;

    SELECT @SemId = SemesterId FROM #SemesterMap WHERE YearPart = @Yr + '-' + @Pt;
    SELECT @ProgId = ProgramId FROM #ProgramMap WHERE ProgramCode = @ProgCode;
    SELECT @LvId = LevelId FROM #LevelMap WHERE ProgramCode = @ProgCode;
    SELECT @ExTypeId = ExamTypeId FROM #ExamTypeMap WHERE ExamTypeName = @ExType;
    SELECT @AyId = AcademicYearId FROM #AcademicYearMap WHERE SourceYear = @AyName;

    SELECT @SemInstId = si.Id
    FROM SemesterInstances si
    WHERE si.TenantId = 2
      AND si.SemesterId = @SemId
      AND si.AcademicYearId = @AyId
      AND si.ProgramId = @ProgId;

    DECLARE @ScheduleName NVARCHAR(50) = LEFT(CONCAT(@ProgCode, '-', CAST(CAST(@AyName AS INT) AS NVARCHAR), '-', @Yr, '-', @Pt, '-', LEFT(@ExType, 4)), 50);
    DECLARE @ScheduleCode NVARCHAR(50) = CONCAT(@ProgCode, '_', CAST(CAST(@AyName AS INT) AS NVARCHAR), '_', @Yr, '_', @Pt, '_', LEFT(@ExType, 3));

    DECLARE @NewScheduleId INT;

    -- Check if schedule already exists for this tenant+code
    SELECT TOP 1 @NewScheduleId = Id FROM ExamSchedules WHERE TenantId = 2 AND ExamScheduleCode = @ScheduleCode;

    IF @NewScheduleId IS NULL
    BEGIN
        INSERT INTO ExamSchedules (
            TenantId, ExamScheduleName, ExamScheduleCode,
            ProgramId, SemesterInstanceId, ExamTypeId, LevelId,
            IsActive, StartDateBs, EndDateBs, StartDate, EndDate, StartTime, EndTime
        )
        VALUES (
            2, @ScheduleName, @ScheduleCode,
            @ProgId, @SemInstId, @ExTypeId, @LvId,
            1, '2080-01-01', '2080-01-01', '2023-01-01', '2023-12-31', '09:00:00', '17:00:00'
        );

        SET @NewScheduleId = SCOPE_IDENTITY();
        SET @ScheduleCount = @ScheduleCount + 1;
    END

    INSERT INTO #ExamScheduleMap (SourceId, NewExamScheduleId)
    VALUES (@CurrentId, @NewScheduleId);
    SET @CurrentId = @CurrentId + 1;
END

PRINT CONCAT('Step 3: Created ', @ScheduleCount, ' ExamSchedules.');

-- ============================================
-- STEP 4: Create ExamCenters (one default per schedule)
-- ============================================

INSERT INTO ExamCenters (TenantId, ExamScheduleId, CollegeId, Code, IsActive)
SELECT 2, esm.NewExamScheduleId, 1, CONCAT('DEF-', es.Id), 1
FROM #ExamScheduleMap esm
INNER JOIN #SourceSchedules es ON es.Id = esm.SourceId
WHERE NOT EXISTS (
    SELECT 1 FROM ExamCenters ec
    WHERE ec.TenantId = 2 AND ec.ExamScheduleId = esm.NewExamScheduleId
);

PRINT CONCAT('Step 4: Created ', @ScheduleCount, ' ExamCenters.');

-- ============================================
-- STEP 5: Ensure SubjectOfferings exist
-- ============================================

-- Find distinct (SubjectCode, ProgramId, SemesterId) from source
CREATE TABLE #NeededSubjectOfferings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SubjectCode NVARCHAR(255),
    ProgramId INT,
    SemesterId INT,
    TheoryFullMarks FLOAT,
    PracticalFullMarks FLOAT,
    InternalFullMarks FLOAT,
    SubjectTypeName NVARCHAR(255)
);

INSERT INTO #NeededSubjectOfferings (SubjectCode, ProgramId, SemesterId, TheoryFullMarks, PracticalFullMarks, InternalFullMarks, SubjectTypeName)
SELECT
    src.SubjectCode,
    pm.ProgramId,
    sm.SemesterId,
    src.TheoryFullMark,
    TRY_CAST(src.PracticalFullMark AS FLOAT),
    src.InternalFullMark,
    src.SubjectTypeName
FROM (
    SELECT DISTINCT SubjectCode, SubjectTypeName, ProgramCode, AcademicYearName, Year, Part,
           TheoryFullMark, TRY_CAST(PracticalFullMark AS FLOAT) AS PracticalFullMark, InternalFullMark
    FROM [FWUExams.Legacy].dbo.CivilEngineering
    UNION
    SELECT DISTINCT SubjectCode, SubjectTypeName, ProgramCode, AcademicYearName, Year, Part,
           TheoryFullMark, TRY_CAST(PracticalFullMark AS FLOAT), InternalFullMark
    FROM [FWUExams.Legacy].dbo.ComputerEngineering
    UNION
    SELECT DISTINCT SubjectCode, SubjectTypeName, ProgramCode, AcademicYearName, Year, Part,
           TheoryFullMark, PracticalFullMark, InternalFullMark
    FROM [FWUExams.Legacy].dbo.CPM
) src
INNER JOIN #ProgramMap pm ON pm.ProgramCode = src.ProgramCode
INNER JOIN #SemesterMap sm ON sm.YearPart = src.Year + '-' + src.Part;

-- SubjectOffering mapping: track which offering to use for each needed combo
CREATE TABLE #SubjectOfferingMap (
    NeededId INT PRIMARY KEY,
    SubjectOfferingId INT
);

-- Try to find existing SubjectOfferings
INSERT INTO #SubjectOfferingMap (NeededId, SubjectOfferingId)
SELECT NeededId, SubjectOfferingId
FROM (
    SELECT nso.Id AS NeededId, so.Id AS SubjectOfferingId,
           ROW_NUMBER() OVER (PARTITION BY nso.Id ORDER BY so.Id) AS rn
    FROM #NeededSubjectOfferings nso
    INNER JOIN SubjectCatalogs sc ON LTRIM(RTRIM(sc.SubjectCode)) = LTRIM(RTRIM(nso.SubjectCode))
    INNER JOIN (
        SELECT Id, SubjectCatalogId, ProgramId, SemesterId,
               ROW_NUMBER() OVER (PARTITION BY SubjectCatalogId, ProgramId, SemesterId ORDER BY Id) AS rn
        FROM SubjectOfferings
        WHERE TenantId = 2
    ) so ON so.SubjectCatalogId = sc.Id
         AND so.ProgramId = nso.ProgramId
         AND so.SemesterId = nso.SemesterId
         AND so.rn = 1
) ranked
WHERE rn = 1;

-- Create missing SubjectOfferings
DECLARE @NsoCount INT = 0;
DECLARE @NsoMaxId INT = (SELECT MAX(Id) FROM #NeededSubjectOfferings);
DECLARE @NsoCurrentId INT = 1;

WHILE @NsoCurrentId <= @NsoMaxId
BEGIN
    DECLARE @NeededSubId INT, @NeededProgId INT, @NeededSemId INT;
    DECLARE @NeededThFM FLOAT, @NeededPrFM FLOAT, @NeededInFM FLOAT;
    DECLARE @NeededSubType NVARCHAR(255);
    DECLARE @NeededSubjectCode NVARCHAR(255);
    DECLARE @CatalogId INT;

    SELECT @NeededSubjectCode = SubjectCode, @NeededProgId = ProgramId, @NeededSemId = SemesterId,
           @NeededThFM = TheoryFullMarks, @NeededPrFM = PracticalFullMarks,
           @NeededInFM = InternalFullMarks, @NeededSubType = SubjectTypeName
    FROM #NeededSubjectOfferings WHERE Id = @NsoCurrentId;

    -- Check if already mapped
    IF NOT EXISTS (SELECT 1 FROM #SubjectOfferingMap WHERE NeededId = @NsoCurrentId)
    BEGIN
        -- Find SubjectCatalog (deduplicated by trimmed code)
        SELECT TOP 1 @CatalogId = sc.Id
        FROM (
            SELECT Id, LTRIM(RTRIM(SubjectCode)) AS SubjectCode
            FROM SubjectCatalogs
        ) sc
        WHERE sc.SubjectCode = LTRIM(RTRIM(@NeededSubjectCode))
        ORDER BY sc.Id;

        IF @CatalogId IS NOT NULL
        BEGIN
            -- Check if a SubjectOffering already exists for this combo
            DECLARE @ExistingOfferingId INT;
            SELECT TOP 1 @ExistingOfferingId = Id
            FROM SubjectOfferings
            WHERE TenantId = 2
              AND SubjectCatalogId = @CatalogId
              AND ProgramId = @NeededProgId
              AND SemesterId = @NeededSemId;

            DECLARE @NewOfferingId INT;

            IF @ExistingOfferingId IS NOT NULL
            BEGIN
                SET @NewOfferingId = @ExistingOfferingId;
            END
            ELSE
            BEGIN
                DECLARE @HasTh BIT = CASE WHEN @NeededThFM > 0 THEN 1 ELSE 0 END;
                DECLARE @HasPr BIT = CASE WHEN @NeededPrFM > 0 THEN 1 ELSE 0 END;
                DECLARE @HasIn BIT = CASE WHEN @NeededInFM > 0 THEN 1 ELSE 0 END;

                INSERT INTO SubjectOfferings (
                    TenantId, SubjectCatalogId, ProgramId, SemesterId,
                    IsCompulsory, DisplayOrder, HasTheory, HasPractical, HasInternal,
                    TheoryFullMarks, PracticalFullMarks, InternalTheoryFullMarks,
                    IsActive
                )
                VALUES (
                    2, @CatalogId, @NeededProgId, @NeededSemId,
                    1, 0, @HasTh, @HasPr, @HasIn,
                    ISNULL(@NeededThFM, 0), ISNULL(@NeededPrFM, 0), ISNULL(@NeededInFM, 0),
                    1
                );

                SET @NewOfferingId = SCOPE_IDENTITY();
                SET @NsoCount = @NsoCount + 1;
            END

            INSERT INTO #SubjectOfferingMap (NeededId, SubjectOfferingId)
            VALUES (@NsoCurrentId, @NewOfferingId);
        END
    END

    SET @NsoCurrentId = @NsoCurrentId + 1;
END

PRINT CONCAT('Step 5: Created ', @NsoCount, ' new SubjectOfferings. Existing matched.');

-- ============================================
-- STEP 5b: Ensure all needed SubjectOfferings exist
-- (catch any combos the loop may have missed due to duplicate catalogs)
-- ============================================

DECLARE @ExtraOfferingsCount INT = 0;
DECLARE @ExtraCatId INT, @ExtraProgId INT, @ExtraSemId INT;

DECLARE extraCur CURSOR FAST_FORWARD FOR
SELECT sc.Id, src.ProgramId, src.SemesterId
FROM (
    SELECT DISTINCT src.SubjectCode, pm.ProgramId, sm.SemesterId
    FROM (
        SELECT DISTINCT SubjectCode, ProgramCode, Year, Part
        FROM [FWUExams.Legacy].dbo.CivilEngineering
        UNION ALL
        SELECT DISTINCT SubjectCode, ProgramCode, Year, Part
        FROM [FWUExams.Legacy].dbo.ComputerEngineering
        UNION ALL
        SELECT DISTINCT SubjectCode, ProgramCode, Year, Part
        FROM [FWUExams.Legacy].dbo.CPM
    ) src
    INNER JOIN #ProgramMap pm ON pm.ProgramCode = src.ProgramCode
    INNER JOIN #SemesterMap sm ON sm.YearPart = src.Year + '-' + src.Part
) src
INNER JOIN SubjectCatalogs sc ON LTRIM(RTRIM(sc.SubjectCode)) = LTRIM(RTRIM(src.SubjectCode))
WHERE NOT EXISTS (
    SELECT 1 FROM SubjectOfferings so
    WHERE so.TenantId = 2
      AND so.SubjectCatalogId = sc.Id
      AND so.ProgramId = src.ProgramId
      AND so.SemesterId = src.SemesterId
);

OPEN extraCur;
FETCH NEXT FROM extraCur INTO @ExtraCatId, @ExtraProgId, @ExtraSemId;
WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO SubjectOfferings (
        TenantId, SubjectCatalogId, ProgramId, SemesterId,
        IsCompulsory, DisplayOrder, HasTheory, HasPractical, HasInternal,
        TheoryFullMarks, PracticalFullMarks, InternalTheoryFullMarks,
        IsActive
    )
    VALUES (
        2, @ExtraCatId, @ExtraProgId, @ExtraSemId,
        1, 0, 1, 0, 1,
        0, 0, 0,
        1
    );
    SET @ExtraOfferingsCount = @ExtraOfferingsCount + 1;
    FETCH NEXT FROM extraCur INTO @ExtraCatId, @ExtraProgId, @ExtraSemId;
END
CLOSE extraCur;
DEALLOCATE extraCur;

PRINT CONCAT('Step 5b: Created ', @ExtraOfferingsCount, ' additional SubjectOfferings for completeness.');

-- ============================================
-- STEP 5c: Set CurriculumVersionId on NULL-CV offerings
-- Match to an existing CV-linked offering with same code (ignoring spaces) + program + semester
-- ============================================

UPDATE so
SET so.CurriculumVersionId = cvRef.CurriculumVersionId
FROM SubjectOfferings so
INNER JOIN SubjectCatalogs sc ON sc.Id = so.SubjectCatalogId
INNER JOIN (
    SELECT REPLACE(LTRIM(RTRIM(sc2.SubjectCode)), ' ', '') AS Code, so2.ProgramId, so2.SemesterId, so2.CurriculumVersionId,
           ROW_NUMBER() OVER (PARTITION BY REPLACE(LTRIM(RTRIM(sc2.SubjectCode)), ' ', ''), so2.ProgramId, so2.SemesterId ORDER BY so2.Id) AS rn
    FROM SubjectOfferings so2
    INNER JOIN SubjectCatalogs sc2 ON sc2.Id = so2.SubjectCatalogId
    WHERE so2.TenantId = 2 AND so2.CurriculumVersionId IS NOT NULL
) cvRef ON cvRef.Code = REPLACE(LTRIM(RTRIM(sc.SubjectCode)), ' ', '')
    AND cvRef.ProgramId = so.ProgramId
    AND cvRef.SemesterId = so.SemesterId
    AND cvRef.rn = 1
WHERE so.TenantId = 2
  AND so.CurriculumVersionId IS NULL
  AND EXISTS (
    SELECT 1 FROM SubjectOfferings so3
    INNER JOIN SubjectCatalogs sc3 ON sc3.Id = so3.SubjectCatalogId
    WHERE so3.TenantId = 2 AND so3.CurriculumVersionId IS NOT NULL
      AND REPLACE(LTRIM(RTRIM(sc3.SubjectCode)), ' ', '') = REPLACE(LTRIM(RTRIM(sc.SubjectCode)), ' ', '')
      AND so3.ProgramId = so.ProgramId AND so3.SemesterId = so.SemesterId
  );

PRINT CONCAT('Step 5c: Set CurriculumVersionId on ', @@ROWCOUNT, ' NULL-CV SubjectOfferings.');

-- ============================================
-- STEP 6: Create ExamRegistrations
-- ============================================

-- Distinct student-exam combos from all source tables (grouped by meaningful key, not ExamRegistrationID)
CREATE TABLE #DistinctExamRegs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RegistrationNo NVARCHAR(255),
    CollegeCode NVARCHAR(255),
    AcademicYearName FLOAT,
    ProgramCode NVARCHAR(255),
    Year_ NVARCHAR(255),
    Part_ NVARCHAR(255),
    ExamTypeName NVARCHAR(255)
);

INSERT INTO #DistinctExamRegs (RegistrationNo, CollegeCode, AcademicYearName, ProgramCode, Year_, Part_, ExamTypeName)
SELECT DISTINCT RegistrationNo, CollegeCode, AcademicYearName, ProgramCode, Year, Part, ExamTypeName
FROM [FWUExams.Legacy].dbo.CivilEngineering
UNION
SELECT DISTINCT RegistrationNo, CollegeCode, AcademicYearName, ProgramCode, Year, Part, ExamTypeName
FROM [FWUExams.Legacy].dbo.ComputerEngineering
UNION
SELECT DISTINCT RegistrationNo, CollegeCode, AcademicYearName, ProgramCode, Year, Part, ExamTypeName
FROM [FWUExams.Legacy].dbo.CPM;

-- Map (RegistrationNo, AY, Program, Year, Part, ExamType) to new ExamRegistration Ids
CREATE TABLE #ExamRegMap (
    RegistrationNo NVARCHAR(255),
    AcademicYearName FLOAT,
    ProgramCode NVARCHAR(255),
    Year_ NVARCHAR(255),
    Part_ NVARCHAR(255),
    ExamTypeName NVARCHAR(255),
    NewExamRegistrationId INT,
    PRIMARY KEY (RegistrationNo, AcademicYearName, ProgramCode, Year_, Part_, ExamTypeName)
);

DECLARE @RegCount INT = 0;
DECLARE @RegMaxId INT = (SELECT MAX(Id) FROM #DistinctExamRegs);
DECLARE @RegCurrentId INT = 1;

WHILE @RegCurrentId <= @RegMaxId
BEGIN
    DECLARE @RegNo NVARCHAR(255);
    DECLARE @SrcCollegeCode NVARCHAR(255), @SrcAyName FLOAT;
    DECLARE @SrcProgCode NVARCHAR(255), @SrcYear NVARCHAR(255), @SrcPart NVARCHAR(255);
    DECLARE @SrcExamType NVARCHAR(255);

    SELECT @RegNo = RegistrationNo,
           @SrcCollegeCode = CollegeCode, @SrcAyName = AcademicYearName,
           @SrcProgCode = ProgramCode, @SrcYear = Year_, @SrcPart = Part_,
           @SrcExamType = ExamTypeName
    FROM #DistinctExamRegs WHERE Id = @RegCurrentId;

    -- Find ExamSchedule
    DECLARE @TargetExamSchedId INT;

    SELECT TOP 1 @TargetExamSchedId = esm.NewExamScheduleId
    FROM #ExamScheduleMap esm
    INNER JOIN #SourceSchedules ss ON ss.Id = esm.SourceId
    WHERE ss.AcademicYearName = @SrcAyName
      AND ss.ProgramCode = @SrcProgCode
      AND ss.Year_ = @SrcYear
      AND ss.Part_ = @SrcPart
      AND ss.ExamTypeName = @SrcExamType;

    DECLARE @TargetCollegeId INT;
    SELECT @TargetCollegeId = CollegeId FROM #CollegeMap WHERE CollegeCode = @SrcCollegeCode;

    DECLARE @TargetProgId INT;
    SELECT @TargetProgId = ProgramId FROM #ProgramMap WHERE ProgramCode = @SrcProgCode;

    DECLARE @TargetAyId INT;
    SELECT @TargetAyId = AcademicYearId FROM #AcademicYearMap WHERE SourceYear = @SrcAyName;

    DECLARE @NewExamRegId INT;

    INSERT INTO ExamRegistrations (
        TenantId, AcademicYearId, CollegeId, ExamScheduleId,
        ExamRollNumber, Sgpa,
        Status, IsActive, ProgramsId, IsSupplementary
    )
    VALUES (
        2,
        @TargetAyId,
        @TargetCollegeId,
        @TargetExamSchedId,
        @RegNo,
        NULL,
        4,  -- Registered
        1,
        @TargetProgId,
        CASE WHEN @SrcExamType = 'Supplementary' THEN 1 ELSE 0 END
    );

    SET @NewExamRegId = SCOPE_IDENTITY();

    INSERT INTO #ExamRegMap (RegistrationNo, AcademicYearName, ProgramCode, Year_, Part_, ExamTypeName, NewExamRegistrationId)
    VALUES (@RegNo, @SrcAyName, @SrcProgCode, @SrcYear, @SrcPart, @SrcExamType, @NewExamRegId);

    SET @RegCount = @RegCount + 1;
    SET @RegCurrentId = @RegCurrentId + 1;

    IF @RegCurrentId % 1000 = 0
        PRINT CONCAT('  ExamRegistrations: ', @RegCurrentId, ' / ', @RegMaxId);
END

PRINT CONCAT('Step 6: Created ', @RegCount, ' ExamRegistrations.');

-- ============================================
-- STEP 7: Create ExamSubjectResults
-- ============================================

-- Source result rows: deduplicated by ExamRegistrationID + SubjectCode + ExamTypeName + AcademicYearName + Year + Part
-- Each source row becomes one ExamSubjectResult
DECLARE @ResultCount INT = 0;

-- Batch insert using UNION ALL from all 3 source tables
-- We use a staging approach to handle the large volume

-- First, collect all source result rows with their mappings
CREATE TABLE #SourceResults (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RegistrationNo NVARCHAR(255),
    ExamRegistrationID FLOAT,
    SubjectCode NVARCHAR(255),
    SubjectTypeName NVARCHAR(255),
    ObtainedMarks FLOAT,
    PracticalMarksFloat FLOAT,
    InternalMarks FLOAT,
    GradeLetter NVARCHAR(255),
    IsLooseEntry NVARCHAR(255),
    Year_ NVARCHAR(255),
    Part_ NVARCHAR(255),
    ExamTypeName NVARCHAR(255),
    AcademicYearName FLOAT,
    ProgramCode NVARCHAR(255),
    CollegeCode NVARCHAR(255),
    ResultStatus NVARCHAR(255),
    Remarks NVARCHAR(255)
);

INSERT INTO #SourceResults (RegistrationNo, ExamRegistrationID, SubjectCode, SubjectTypeName, ObtainedMarks, PracticalMarksFloat, InternalMarks, GradeLetter, IsLooseEntry, Year_, Part_, ExamTypeName, AcademicYearName, ProgramCode, CollegeCode, ResultStatus, Remarks)
SELECT
    RegistrationNo,
    ExamRegistrationID,
    SubjectCode,
    SubjectTypeName,
    ObtainedMarks,
    TRY_CAST(PracticalMarks AS FLOAT),
    InternalMarks,
    GradeLetter,
    IsLooseEntry,
    Year,
    Part,
    ExamTypeName,
    AcademicYearName,
    ProgramCode,
    CollegeCode,
    ResultStatus,
    Rem
FROM [FWUExams.Legacy].dbo.CivilEngineering
UNION ALL
SELECT
    RegistrationNo,
    ExamRegistrationID, SubjectCode, SubjectTypeName, ObtainedMarks,
    TRY_CAST(PracticalMarks AS FLOAT), InternalMarks, GradeLetter, IsLooseEntry,
    Year, Part, ExamTypeName, AcademicYearName, ProgramCode, CollegeCode,
    ResultStatus, Rem
FROM [FWUExams.Legacy].dbo.ComputerEngineering
UNION ALL
SELECT
    RegistrationNo,
    ExamRegistrationID, SubjectCode, SubjectTypeName, ObtainedMarks,
    TRY_CAST(PracticalMarks AS FLOAT), InternalMarks, GradeLetter, IsLooseEntry,
    Year, Part, ExamTypeName, AcademicYearName, ProgramCode, CollegeCode,
    ResultStatus, Rem
FROM [FWUExams.Legacy].dbo.CPM;

DECLARE @SourceCount INT = (SELECT COUNT(*) FROM #SourceResults);
PRINT CONCAT('Step 7a: Staged ', @SourceCount, ' source result rows.');

-- Now batch insert ExamSubjectResults
-- Process in chunks for memory efficiency
DECLARE @BatchSize INT = 5000;
DECLARE @Offset INT = 0;
DECLARE @TotalRows INT = @SourceCount;

WHILE @Offset < @TotalRows
BEGIN
    INSERT INTO ExamSubjectResults (
        TenantId, ExamRegistrationId, ExamTypeId, SubjectOfferingId,
        ObtainedMarksTheory, ObtainedMarksPractical, ObtainedMarksTheoryInternal,
        GradeLetter, Remarks, IsActive, IsLooseEntry, IsSubmitted, IsSupplementary
    )
    SELECT
        2,
        erm.NewExamRegistrationId,
        etm.ExamTypeId,
        so.SubjectOfferingId,
        sr.ObtainedMarks,
        sr.PracticalMarksFloat,
        sr.InternalMarks,
        LEFT(sr.GradeLetter, 3),
        LEFT(sr.Remarks, 255),
        1,
        CASE WHEN sr.IsLooseEntry = '1' THEN 1 ELSE 0 END,
        1,
        CASE WHEN sr.ExamTypeName = 'Supplementary' THEN 1 ELSE 0 END
    FROM (
        SELECT *, ROW_NUMBER() OVER (ORDER BY Id) AS RowNum
        FROM #SourceResults
    ) sr
    INNER JOIN #ExamRegMap erm ON erm.RegistrationNo = sr.RegistrationNo
        AND erm.AcademicYearName = sr.AcademicYearName
        AND erm.ProgramCode = sr.ProgramCode
        AND erm.Year_ = sr.Year_
        AND erm.Part_ = sr.Part_
        AND erm.ExamTypeName = sr.ExamTypeName
    INNER JOIN #ExamTypeMap etm ON etm.ExamTypeName = sr.ExamTypeName
    INNER JOIN #ProgramMap pm ON pm.ProgramCode = sr.ProgramCode
    INNER JOIN #SemesterMap sm ON sm.YearPart = sr.Year_ + '-' + sr.Part_
    INNER JOIN (
        SELECT sc_nospace AS SubjectCode, ProgramId, SemesterId, Id AS SubjectOfferingId
        FROM (
            SELECT REPLACE(LTRIM(RTRIM(sc.SubjectCode)), ' ', '') AS sc_nospace, so2.ProgramId, so2.SemesterId, so2.Id,
                   ROW_NUMBER() OVER (PARTITION BY REPLACE(LTRIM(RTRIM(sc.SubjectCode)), ' ', ''), so2.ProgramId, so2.SemesterId ORDER BY CASE WHEN so2.CurriculumVersionId IS NOT NULL THEN 0 ELSE 1 END, so2.Id) AS rn
            FROM SubjectOfferings so2
            INNER JOIN SubjectCatalogs sc ON sc.Id = so2.SubjectCatalogId
            WHERE so2.TenantId = 2
        ) ranked WHERE rn = 1
    ) so ON so.SubjectCode = REPLACE(LTRIM(RTRIM(sr.SubjectCode)), ' ', '')
         AND so.ProgramId = pm.ProgramId
         AND so.SemesterId = sm.SemesterId
    WHERE sr.RowNum > @Offset AND sr.RowNum <= @Offset + @BatchSize;

    SET @ResultCount = @ResultCount + @@ROWCOUNT;
    SET @Offset = @Offset + @BatchSize;

    IF @Offset % 10000 = 0
        PRINT CONCAT('  ExamSubjectResults: ', @Offset, ' / ', @TotalRows, ' processed, ', @ResultCount, ' inserted.');
END

PRINT CONCAT('Step 7b: Created ', @ResultCount, ' ExamSubjectResults.');

-- ============================================
-- STEP 7c: Clean up remaining NULL-CV SubjectOfferings
-- Step 7 preferred CV-linked offerings, so any NULL-CV ones are orphaned
-- ============================================

DELETE esr FROM ExamSubjectResults esr
INNER JOIN SubjectOfferings so ON so.Id = esr.SubjectOfferingId
WHERE so.TenantId = 2 AND so.CurriculumVersionId IS NULL;

DECLARE @NullCvDeleted INT = @@ROWCOUNT;

DELETE FROM SubjectOfferings WHERE TenantId = 2 AND CurriculumVersionId IS NULL;

SET @NullCvDeleted = @NullCvDeleted + @@ROWCOUNT;
PRINT CONCAT('Step 7c: Cleaned up ', @NullCvDeleted, ' NULL-CV records (results + offerings).');

-- ============================================
-- STEP 8: Verification
-- ============================================

PRINT '';
PRINT '=== VERIFICATION ===';

-- Source counts
PRINT '';
PRINT 'Source row counts:';
SELECT 'CivilEngineering' AS [Table], COUNT(*) AS Rows FROM [FWUExams.Legacy].dbo.CivilEngineering
UNION ALL
SELECT 'ComputerEngineering', COUNT(*) FROM [FWUExams.Legacy].dbo.ComputerEngineering
UNION ALL
SELECT 'CPM', COUNT(*) FROM [FWUExams.Legacy].dbo.CPM;

-- Target counts (tenant 2 only)
PRINT '';
PRINT 'Target row counts (tenant 2):';
SELECT 'ExamRegistrations' AS [Table], COUNT(*) AS Rows FROM ExamRegistrations WHERE TenantId = 2
UNION ALL
SELECT 'ExamSubjectResults', COUNT(*) FROM ExamSubjectResults WHERE TenantId = 2
UNION ALL
SELECT 'ExamSchedules', COUNT(*) FROM ExamSchedules WHERE TenantId = 2
UNION ALL
SELECT 'ExamCenters', COUNT(*) FROM ExamCenters WHERE TenantId = 2;

-- Verify no orphan FKs in ExamSubjectResults
PRINT '';
PRINT 'Orphan checks:';
SELECT 'ExamSubjectResults -> ExamRegistration (orphan)' AS Check_, COUNT(*) AS OrphanCount
FROM ExamSubjectResults esr
WHERE esr.TenantId = 2
  AND NOT EXISTS (SELECT 1 FROM ExamRegistrations er WHERE er.Id = esr.ExamRegistrationId);

SELECT 'ExamSubjectResults -> SubjectOffering (orphan)' AS Check_, COUNT(*) AS OrphanCount
FROM ExamSubjectResults esr
WHERE esr.TenantId = 2
  AND NOT EXISTS (SELECT 1 FROM SubjectOfferings so WHERE so.Id = esr.SubjectOfferingId);

SELECT 'ExamSubjectResults -> ExamType (orphan)' AS Check_, COUNT(*) AS OrphanCount
FROM ExamSubjectResults esr
WHERE esr.TenantId = 2
  AND NOT EXISTS (SELECT 1 FROM ExamTypes et WHERE et.Id = esr.ExamTypeId);

SELECT 'ExamRegistrations -> ExamSchedule (orphan)' AS Check_, COUNT(*) AS OrphanCount
FROM ExamRegistrations er
WHERE er.TenantId = 2
  AND NOT EXISTS (SELECT 1 FROM ExamSchedules es WHERE es.Id = er.ExamScheduleId);

SELECT 'ExamRegistrations -> College (orphan)' AS Check_, COUNT(*) AS OrphanCount
FROM ExamRegistrations er
WHERE er.TenantId = 2
  AND NOT EXISTS (SELECT 1 FROM Colleges c WHERE c.Id = er.CollegeId);

-- Spot check: sample students
PRINT '';
PRINT 'Spot check - first 5 students with results:';
SELECT TOP 5
    er.ExamRollNumber,
    sr.RegistrationNumber,
    sr.FirstName + ' ' + ISNULL(sr.MiddleName, '') + ' ' + sr.LastName AS FullName,
    es.ExamScheduleName,
    esr.GradeLetter,
    esr.ObtainedMarksTheory,
    esr.ObtainedMarksPractical,
    esr.ObtainedMarksTheoryInternal
FROM ExamSubjectResults esr
INNER JOIN ExamRegistrations er ON er.Id = esr.ExamRegistrationId
INNER JOIN ExamSchedules es ON es.Id = er.ExamScheduleId
LEFT JOIN StudentRegistrations sr ON sr.RegistrationNumber = er.ExamRollNumber
WHERE esr.TenantId = 2
ORDER BY er.Id, esr.Id;

COMMIT TRANSACTION;

PRINT '';
PRINT '=== MIGRATION COMPLETE ===';
PRINT CONCAT('Total ExamRegistrations created: ', @RegCount);
PRINT CONCAT('Total ExamSubjectResults created: ', @ResultCount);
