-- ============================================================
-- SEED DATA for FWU.Exam.Management
-- Run this script against the FUExamsDb database
-- ============================================================

-- 1. ORGANIZATIONS
IF NOT EXISTS (SELECT 1 FROM Organizations)
BEGIN
    INSERT INTO Organizations ([Name], OfficeCode, ContactNumber, [Address], Email, LogoPath)
    VALUES ('Far Western University', 'FWU', '01-2345678', 'Mahendranagar, Kanchanpur', 'info@fwu.edu.np', NULL);
END
GO

-- 2. ACADEMIC YEARS (AcademicYearCode is INT)
IF NOT EXISTS (SELECT 1 FROM AcademicYears)
BEGIN
    INSERT INTO AcademicYears (AcademicYearCode, AcademicYearName, AcademicYearCodeNepali, AcademicYearNameNepali, Remark, IsRunning, IsActive)
    VALUES 
    (2076, '2076/77', N'२०७६/७७', N'२०७६/७७', NULL, 0, 1),
    (2077, '2077/78', N'२०७७/७८', N'२०७७/७८', NULL, 0, 1),
    (2078, '2078/79', N'२०७८/७९', N'२०७८/७९', NULL, 0, 1),
    (2079, '2079/80', N'२०७९/८०', N'२०७९/८०', NULL, 0, 1),
    (2080, '2080/81', N'२०८०/८१', N'२०८०/८१', NULL, 1, 1);
END
GO

-- 3. LEVELS (LevelCode is MAX 2 CHARS)
IF NOT EXISTS (SELECT 1 FROM Levels)
BEGIN
    INSERT INTO Levels (LevelCode, LevelName, LevelDisplayOrder, Remarks, IsRunning, IsActive)
    VALUES 
    ('BA', 'Bachelor', 1, NULL, 1, 1),
    ('MA', 'Master', 2, NULL, 1, 1),
    ('MP', 'M.Phil.', 3, NULL, 1, 1),
    ('PH', 'Ph.D.', 4, NULL, 1, 1),
    ('DI', 'Diploma', 5, NULL, 1, 1);
END
GO

-- 4. FACULTIES
IF NOT EXISTS (SELECT 1 FROM Faculties)
BEGIN
    INSERT INTO Faculties (FacultyCode, FacultyName, ShortName, Remarks, IsActive)
    VALUES 
    ('MGT', 'Management', 'MGT', NULL, 1),
    ('SCI', 'Science', 'SCI', NULL, 1),
    ('HUM', 'Humanities & Social Sciences', 'HUM', NULL, 1),
    ('EDU', 'Education', 'EDU', NULL, 1),
    ('LAW', 'Law', 'LAW', NULL, 1),
    ('AGR', 'Agriculture', 'AGR', NULL, 1),
    ('ENG', 'Engineering', 'ENG', NULL, 1);
END
GO

-- 5. COLLEGE TYPES (Code is MAX 2 CHARS)
IF NOT EXISTS (SELECT 1 FROM CollegeTypes)
BEGIN
    INSERT INTO CollegeTypes ([Code], [Name], Remarks, IsDefault, IsActive)
    VALUES 
    ('CO', 'Constituent Campus', NULL, 1, 1),
    ('AF', 'Affiliated Campus', NULL, 0, 1);
END
GO

-- 6. COLLEGES / CAMPUSES
IF NOT EXISTS (SELECT 1 FROM Colleges)
BEGIN
    DECLARE @ctConstituent INT = (SELECT Id FROM CollegeTypes WHERE Code = 'CO');
    INSERT INTO Colleges ([Code], [Name], ShortName, Email, Phone1, IsActive, IsExamCenterOnly, CollegeTypeId, DisplayOrder)
    VALUES 
    ('FWUCC', 'Far Western University Central Campus', 'FWUCC', 'central@fwu.edu.np', '099-523456', 1, 0, @ctConstituent, 1),
    ('FWUMAH', 'FWU Mahendranagar Campus', 'FWUMC', 'mc@fwu.edu.np', '099-523457', 1, 0, @ctConstituent, 2),
    ('FWUDH', 'FWU Dhangadhi Campus', 'FWUDC', 'dc@fwu.edu.np', '091-523458', 1, 0, @ctConstituent, 3);
END
GO

-- 7. GENDERS
IF NOT EXISTS (SELECT 1 FROM Genders)
BEGIN
    INSERT INTO Genders (GenderName, IsActive)
    VALUES 
    ('Male', 1),
    ('Female', 1),
    ('Others', 1);
END
GO

-- 8. ETHNICITIES
IF NOT EXISTS (SELECT 1 FROM Ethnicities)
BEGIN
    INSERT INTO Ethnicities (EthnicityName, IsDefault, IsActive) VALUES ('Brahmin', 0, 1);
    INSERT INTO Ethnicities (EthnicityName, IsDefault, IsActive) VALUES ('Chhetri', 0, 1);
    INSERT INTO Ethnicities (EthnicityName, IsDefault, IsActive) VALUES ('Janajati', 0, 1);
    INSERT INTO Ethnicities (EthnicityName, IsDefault, IsActive) VALUES ('Dalit', 0, 1);
    INSERT INTO Ethnicities (EthnicityName, IsDefault, IsActive) VALUES ('Madhesi', 0, 1);
    INSERT INTO Ethnicities (EthnicityName, IsDefault, IsActive) VALUES ('Muslim', 0, 1);
    INSERT INTO Ethnicities (EthnicityName, IsDefault, IsActive) VALUES ('Others', 1, 1);
END
GO

-- 9. STUDENT CATEGORIES
IF NOT EXISTS (SELECT 1 FROM StudentCategories)
BEGIN
    INSERT INTO StudentCategories (StudentCategoryName, IsActive, Remarks)
    VALUES 
    ('Regular', 1, NULL),
    ('Partial', 1, NULL),
    ('Full Free', 1, NULL);
END
GO

-- 10. BOARDS / UNIVERSITIES
IF NOT EXISTS (SELECT 1 FROM Boards)
BEGIN
    INSERT INTO Boards (CountryId, BoardName, Remarks, IsActive)
    VALUES 
    (1, 'NEB (National Examination Board)', NULL, 1),
    (1, 'SEE (Secondary Education Examination)', NULL, 1),
    (1, 'Tribhuvan University', NULL, 1),
    (1, 'Far Western University', NULL, 1),
    (1, 'Kathmandu University', NULL, 1),
    (1, 'Pokhara University', NULL, 1),
    (1, 'Purbanchal University', NULL, 1),
    (1, 'Mid-Western University', NULL, 1),
    (1, 'Nepal Sanskrit University', NULL, 1),
    (1, 'CTEVT', NULL, 1);
END
GO

-- 11. PREVIOUS LEVELS
IF NOT EXISTS (SELECT 1 FROM PreviousLevels)
BEGIN
    DECLARE @bachLevel INT = (SELECT Id FROM Levels WHERE LevelCode = 'BA');
    DECLARE @mastLevel INT = (SELECT Id FROM Levels WHERE LevelCode = 'MA');
    DECLARE @mphilLevel INT = (SELECT Id FROM Levels WHERE LevelCode = 'MP');
    DECLARE @phdLevel INT = (SELECT Id FROM Levels WHERE LevelCode = 'PH');

    INSERT INTO PreviousLevels (PreviousLevelName, LevelId, LevelDisplayOrder, Remarks, IsActive)
    VALUES 
    ('SLC/SEE', @bachLevel, 1, NULL, 1),
    ('10+2 or Diploma', @bachLevel, 2, NULL, 1),
    ('Bachelor', @mastLevel, 3, NULL, 1),
    ('Master', @mphilLevel, 4, NULL, 1),
    ('M.Phil.', @phdLevel, 5, NULL, 1);
END
GO

-- 12. PROGRAMS
IF NOT EXISTS (SELECT 1 FROM Programs)
BEGIN
    DECLARE @mgtFaculty INT = (SELECT Id FROM Faculties WHERE FacultyCode = 'MGT');
    DECLARE @sciFaculty INT = (SELECT Id FROM Faculties WHERE FacultyCode = 'SCI');
    DECLARE @humFaculty INT = (SELECT Id FROM Faculties WHERE FacultyCode = 'HUM');
    DECLARE @eduFaculty INT = (SELECT Id FROM Faculties WHERE FacultyCode = 'EDU');
    DECLARE @lawFaculty INT = (SELECT Id FROM Faculties WHERE FacultyCode = 'LAW');
    DECLARE @bachLvl INT = (SELECT Id FROM Levels WHERE LevelCode = 'BA');
    DECLARE @mastLvl INT = (SELECT Id FROM Levels WHERE LevelCode = 'MA');

    -- Bachelor Programs
    INSERT INTO Programs (LevelId, FacultyId, BoardId, ProgramCode, ProgramName, ShortName, Duration, GrandTotalMarks, HasMultipleIntakes, Remarks, IsActive, RollNumberPrefix)
    VALUES 
    (@bachLvl, @mgtFaculty, 3, 'BBA', 'Bachelor of Business Administration', 'BBA', 4, 1000, 0, NULL, 1, 'BBA'),
    (@bachLvl, @mgtFaculty, 3, 'BBS', 'Bachelor of Business Studies', 'BBS', 4, 1000, 0, NULL, 1, 'BBS'),
    (@bachLvl, @sciFaculty, 3, 'BSC', 'Bachelor of Science (General)', 'B.Sc.', 4, 1000, 0, NULL, 1, 'BSC'),
    (@bachLvl, @sciFaculty, 3, 'BSC-CSIT', 'Bachelor of Science in Computer Science & IT', 'B.Sc. CSIT', 4, 1000, 0, NULL, 1, 'CSIT'),
    (@bachLvl, @eduFaculty, 3, 'BED', 'Bachelor of Education', 'B.Ed.', 4, 1000, 0, NULL, 1, 'BED'),
    (@bachLvl, @humFaculty, 3, 'BA', 'Bachelor of Arts', 'B.A.', 4, 1000, 0, NULL, 1, 'BA'),
    (@bachLvl, @lawFaculty, 3, 'LLB', 'Bachelor of Laws', 'LL.B.', 5, 1200, 0, NULL, 1, 'LLB'),
    (@bachLvl, @sciFaculty, 3, 'BScAg', 'Bachelor of Science in Agriculture', 'B.Sc. Ag.', 4, 1000, 0, NULL, 1, 'BSCAG');

    -- Master Programs
    INSERT INTO Programs (LevelId, FacultyId, BoardId, ProgramCode, ProgramName, ShortName, Duration, GrandTotalMarks, HasMultipleIntakes, Remarks, IsActive, RollNumberPrefix)
    VALUES 
    (@mastLvl, @mgtFaculty, 3, 'MBA', 'Master of Business Administration', 'MBA', 2, 1000, 0, NULL, 1, 'MBA'),
    (@mastLvl, @mgtFaculty, 3, 'MBS', 'Master of Business Studies', 'MBS', 2, 1000, 0, NULL, 1, 'MBS'),
    (@mastLvl, @sciFaculty, 3, 'MSC', 'Master of Science (General)', 'M.Sc.', 2, 1000, 0, NULL, 1, 'MSC'),
    (@mastLvl, @eduFaculty, 3, 'MED', 'Master of Education', 'M.Ed.', 2, 1000, 0, NULL, 1, 'MED'),
    (@mastLvl, @humFaculty, 3, 'MA', 'Master of Arts', 'M.A.', 2, 1000, 0, NULL, 1, 'MA');
END
GO

-- 13. EXAM CENTERS (requires ExamSchedules to be seeded first — skipped for now)
-- IF NOT EXISTS (SELECT 1 FROM ExamCenters)
-- BEGIN
--     DECLARE @espId INT = (SELECT Id FROM ExamScheduleParent);
--     INSERT INTO ExamCenters (ExamScheduleId, CollegeId, Remark, IsActive, [Code])
--     VALUES 
--     (@espId, (SELECT Id FROM Colleges WHERE Code = 'FWUCC'), 'Mahendranagar - Main Campus', 1, 1),
--     (@espId, (SELECT Id FROM Colleges WHERE Code = 'FWUMAH'), 'Mahendranagar', 1, 2),
--     (@espId, (SELECT Id FROM Colleges WHERE Code = 'FWUDH'), 'Dhangadhi', 1, 3);
-- END
-- GO

-- 14. EXAM TYPES (Code is INT)
IF NOT EXISTS (SELECT 1 FROM ExamTypes)
BEGIN
    INSERT INTO ExamTypes ([Name], Remarks, IsActive, [Code])
    VALUES 
    ('Regular', NULL, 1, 1),
    ('Partial', NULL, 1, 2),
    ('Full', NULL, 1, 3),
    ('Supplement', NULL, 1, 4),
    ('Grade Improvement', NULL, 1, 5);
END
GO

-- 15. EXAM SCHEDULE PARENT (table name is ExamScheduleParent)
IF NOT EXISTS (SELECT 1 FROM ExamScheduleParent)
BEGIN
    INSERT INTO ExamScheduleParent (ExamScheduleParentName, IsActive)
    VALUES ('Default Schedule', 1);
END
GO

-- 16. PAYMENT TYPES (table name is PaymentType, not PaymentTypes)
IF NOT EXISTS (SELECT 1 FROM PaymentType)
BEGIN
    INSERT INTO PaymentType (PaymentTypeName, IsActive)
    VALUES 
    ('eSewa', 1),
    ('Khalti', 1),
    ('ConnectIPS', 1),
    ('Bank Deposit', 1);
END
GO

-- 17. BANKS
IF NOT EXISTS (SELECT 1 FROM Banks)
BEGIN
    INSERT INTO Banks (BankName, BankCode, Remarks, IsActive)
    VALUES 
    ('Nepal Bank Limited', 'NBL', NULL, 1),
    ('Rastriya Banijya Bank', 'RBB', NULL, 1),
    ('Agricultural Development Bank', 'ADBL', NULL, 1),
    ('Nabil Bank', 'NABIL', NULL, 1),
    ('Global IME Bank', 'GBIME', NULL, 1);
END
GO

-- 18. SUBJECT TYPES
IF NOT EXISTS (SELECT 1 FROM SubjectTypes)
BEGIN
    INSERT INTO SubjectTypes ([Code], [Name], MaxAllowedSubjects, IsDefault, IsActive)
    VALUES 
    ('TH', 'Theory', 99, 1, 1),
    ('PR', 'Practical', 99, 0, 1),
    ('TP', 'Theory & Practical', 99, 0, 1);
END
GO

-- 19. ENTRY FORMATS
IF NOT EXISTS (SELECT 1 FROM EntryFormats)
BEGIN
    INSERT INTO EntryFormats (EntryFormatName, Remarks, IsActive)
    VALUES 
    ('New Admission', NULL, 1),
    ('Re-Admission', NULL, 1);
END
GO

PRINT 'Seed data inserted successfully!';
GO
