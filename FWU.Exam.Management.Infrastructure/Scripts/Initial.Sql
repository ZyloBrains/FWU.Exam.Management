IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AcademicYears] (
    [Id] int NOT NULL IDENTITY,
    [AcademicYearCode] nvarchar(30) NOT NULL,
    [AcademicYearCodeNepali] nvarchar(50) NULL,
    [AcademicYearName] nvarchar(50) NOT NULL,
    [AcademicYearNameNepali] nvarchar(50) NOT NULL,
    [Remark] nvarchar(50) NULL,
    [IsRunning] bit NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_AcademicYears] PRIMARY KEY ([Id])
);

CREATE TABLE [Banks] (
    [Id] int NOT NULL IDENTITY,
    [BankName] nvarchar(100) NOT NULL,
    [BankCode] nvarchar(30) NULL,
    [Remarks] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Banks] PRIMARY KEY ([Id])
);

CREATE TABLE [Boards] (
    [Id] int NOT NULL IDENTITY,
    [CountryId] int NOT NULL,
    [BoardName] nvarchar(50) NOT NULL,
    [Remarks] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Boards] PRIMARY KEY ([Id])
);

CREATE TABLE [CollegeTypes] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Remarks] nvarchar(1024) NULL,
    [IsDefault] bit NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_CollegeTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [ConnectIpsPaymentConfiguration] (
    [Id] int NOT NULL IDENTITY,
    [GatewayUrl] nvarchar(1024) NOT NULL,
    [MerchantId] nvarchar(1024) NOT NULL,
    [AppId] nvarchar(1024) NOT NULL,
    [AppName] nvarchar(1024) NOT NULL,
    [ValidationApiUrl] nvarchar(1024) NOT NULL,
    [UsernameForValidationApi] nvarchar(1024) NOT NULL,
    [PasswordForValidationApi] nvarchar(1024) NOT NULL,
    [PasswordForCreditorPfx] nvarchar(1024) NOT NULL,
    [TransactionCurrency] nvarchar(10) NULL,
    CONSTRAINT [PK_ConnectIpsPaymentConfiguration] PRIMARY KEY ([Id])
);

CREATE TABLE [EntryFormats] (
    [Id] int NOT NULL IDENTITY,
    [EntryFormatName] nvarchar(100) NOT NULL,
    [Remarks] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_EntryFormats] PRIMARY KEY ([Id])
);

CREATE TABLE [ESewaConfiguration] (
    [Id] int NOT NULL IDENTITY,
    [PostUrl] nvarchar(256) NOT NULL,
    [ProductCode] nvarchar(50) NOT NULL,
    [SecretKey] nvarchar(256) NOT NULL,
    [SuccessUrl] nvarchar(256) NOT NULL,
    [ServiceChargeAmount] decimal(18,2) NOT NULL,
    [VerifyUrl] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_ESewaConfiguration] PRIMARY KEY ([Id])
);

CREATE TABLE [Ethnicities] (
    [Id] int NOT NULL IDENTITY,
    [EthnicityName] nvarchar(50) NOT NULL,
    [IsDefault] bit NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Ethnicities] PRIMARY KEY ([Id])
);

CREATE TABLE [ExamTypes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [Remarks] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    [Code] nvarchar(30) NOT NULL,
    CONSTRAINT [PK_ExamTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [FiscalYears] (
    [Id] int NOT NULL IDENTITY,
    [FiscalYearName] nvarchar(50) NOT NULL,
    [StartDate] nvarchar(10) NOT NULL,
    [EndDate] nvarchar(10) NOT NULL,
    [IsRunning] bit NOT NULL,
    [Remarks] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    [FiscalYearCode] nvarchar(30) NULL,
    CONSTRAINT [PK_FiscalYears] PRIMARY KEY ([Id])
);

CREATE TABLE [Genders] (
    [Id] int NOT NULL IDENTITY,
    [GenderName] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Genders] PRIMARY KEY ([Id])
);

CREATE TABLE [IndexGroups] (
    [Id] int NOT NULL IDENTITY,
    [IndexGroupName] nvarchar(100) NOT NULL,
    [Remarks] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_IndexGroups] PRIMARY KEY ([Id])
);

CREATE TABLE [KhaltiConfigurations] (
    [Id] int NOT NULL IDENTITY,
    [ReturnUrl] nvarchar(400) NOT NULL,
    [WebsiteUrl] nvarchar(400) NOT NULL,
    [Amount] decimal(18,2) NULL,
    [ProductName] nvarchar(400) NOT NULL,
    [AuthorizationKey] nvarchar(400) NOT NULL,
    [ServiceCharge] int NOT NULL,
    [PostUrl] nvarchar(400) NOT NULL,
    [VerifyUrl] nvarchar(400) NOT NULL,
    CONSTRAINT [PK_KhaltiConfigurations] PRIMARY KEY ([Id])
);

CREATE TABLE [Levels] (
    [Id] int NOT NULL IDENTITY,
    [LevelCode] nvarchar(30) NULL,
    [LevelName] nvarchar(50) NOT NULL,
    [LevelDisplayOrder] int NULL,
    [Remarks] nvarchar(255) NULL,
    [IsRunning] bit NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Levels] PRIMARY KEY ([Id])
);

CREATE TABLE [NepaliDates] (
    [Id] int NOT NULL IDENTITY,
    [GregorianDate] datetime2 NULL,
    [NepaliDateShort] nvarchar(10) NULL,
    [NepaliDateFull] nvarchar(50) NULL,
    [NepaliDateString] nvarchar(50) NULL,
    CONSTRAINT [PK_NepaliDates] PRIMARY KEY ([Id])
);

CREATE TABLE [PaymentType] (
    [Id] int NOT NULL IDENTITY,
    [PaymentTypeName] nvarchar(255) NOT NULL,
    [LogoUrl] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_PaymentType] PRIMARY KEY ([Id])
);

CREATE TABLE [PeriodTypes] (
    [Id] int NOT NULL IDENTITY,
    [PeriodTypeName] nvarchar(50) NOT NULL,
    [NumberOfMonths] decimal(5,2) NULL,
    [IsActive] bit NULL,
    [Remarks] nvarchar(255) NULL,
    CONSTRAINT [PK_PeriodTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [Permissions] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(256) NOT NULL,
    [DisplayName] nvarchar(256) NULL,
    [Description] nvarchar(500) NULL,
    [Group] nvarchar(128) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
);

CREATE TABLE [Provinces] (
    [Id] int NOT NULL IDENTITY,
    [ProvinceName] nvarchar(50) NOT NULL,
    [ProvinceCode] nvarchar(10) NULL,
    [IsActive] bit NOT NULL,
    [Remarks] nvarchar(255) NULL,
    CONSTRAINT [PK_Provinces] PRIMARY KEY ([Id])
);

CREATE TABLE [Roles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);

CREATE TABLE [SmsConfigurations] (
    [Id] int NOT NULL IDENTITY,
    [ApiUrl] nvarchar(1024) NOT NULL,
    [ApiKey] nvarchar(2048) NOT NULL,
    [Mode] nvarchar(50) NULL,
    [Tags] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_SmsConfigurations] PRIMARY KEY ([Id])
);

CREATE TABLE [SmtpConfigurations] (
    [Id] int NOT NULL IDENTITY,
    [Host] nvarchar(1024) NOT NULL,
    [From] nvarchar(1024) NOT NULL,
    [Port] int NOT NULL,
    [UserName] nvarchar(1024) NOT NULL,
    [Password] nvarchar(1024) NOT NULL,
    [EnableSsl] bit NOT NULL,
    CONSTRAINT [PK_SmtpConfigurations] PRIMARY KEY ([Id])
);

CREATE TABLE [StudentCategories] (
    [Id] int NOT NULL IDENTITY,
    [StudentCategoryName] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL,
    [Remarks] nvarchar(255) NULL,
    CONSTRAINT [PK_StudentCategories] PRIMARY KEY ([Id])
);

CREATE TABLE [SubjectTypes] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [MaxAllowedSubjects] int NULL,
    [IsDefault] bit NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_SubjectTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [Tenants] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [OfficeCode] nvarchar(30) NOT NULL,
    [ContactNumber] nvarchar(50) NOT NULL,
    [Address] nvarchar(255) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [LogoPath] nvarchar(500) NULL,
    [TenantType] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
);

CREATE TABLE [UserAttachments] (
    [Id] int NOT NULL IDENTITY,
    [FileName] nvarchar(255) NOT NULL,
    [FilePath] nvarchar(1024) NOT NULL,
    [ContentType] nvarchar(100) NULL,
    [FileSize] bigint NULL,
    [UploadedByUserId] nvarchar(max) NULL,
    [UploadedDate] datetime2 NOT NULL,
    [Remarks] nvarchar(255) NULL,
    CONSTRAINT [PK_UserAttachments] PRIMARY KEY ([Id])
);

CREATE TABLE [Batches] (
    [Id] int NOT NULL IDENTITY,
    [AcademicYearId] int NOT NULL,
    [BatchName] nvarchar(50) NOT NULL,
    [Remarks] nvarchar(50) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Batches] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Batches_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Semesters] (
    [Id] int NOT NULL IDENTITY,
    [Number] int NOT NULL,
    [Year] int NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Code] nvarchar(30) NOT NULL,
    [Remark] nvarchar(50) NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [AcademicYearId] int NOT NULL,
    CONSTRAINT [PK_Semesters] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Semesters_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PreviousLevels] (
    [Id] int NOT NULL IDENTITY,
    [PreviousLevelName] nvarchar(100) NOT NULL,
    [LevelId] int NULL,
    [LevelDisplayOrder] int NULL,
    [Remarks] nvarchar(1024) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_PreviousLevels] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PreviousLevels_Levels_LevelId] FOREIGN KEY ([LevelId]) REFERENCES [Levels] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Districts] (
    [Id] int NOT NULL IDENTITY,
    [ProvinceId] int NOT NULL,
    [DistrictCode] nvarchar(30) NULL,
    [DistrictName] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL,
    [Remarks] nvarchar(255) NULL,
    CONSTRAINT [PK_Districts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Districts_Provinces_ProvinceId] FOREIGN KEY ([ProvinceId]) REFERENCES [Provinces] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [RoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_RoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RoleClaims_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RolePermissions] (
    [RoleId] nvarchar(450) NOT NULL,
    [PermissionId] int NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
    CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [SubjectCatalogs] (
    [Id] int NOT NULL IDENTITY,
    [SubjectCode] nvarchar(30) NOT NULL,
    [SubjectName] nvarchar(150) NOT NULL,
    [ShortName] nvarchar(50) NULL,
    [Description] nvarchar(500) NULL,
    [CreditHours] int NULL,
    [SubjectTypeId] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_SubjectCatalogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SubjectCatalogs_SubjectTypes_SubjectTypeId] FOREIGN KEY ([SubjectTypeId]) REFERENCES [SubjectTypes] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Faculties] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [OfficeCode] nvarchar(30) NOT NULL,
    [ContactNumber] nvarchar(50) NOT NULL,
    [Address] nvarchar(255) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [LogoPath] nvarchar(max) NULL,
    [TenantId] int NULL,
    CONSTRAINT [PK_Faculties] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Faculties_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id])
);

CREATE TABLE [Notices] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [NoticeTitle] nvarchar(1024) NOT NULL,
    [NoticePreview] nvarchar(1024) NOT NULL,
    [PublishedDate] datetime2 NULL,
    [NoticeContent] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Notices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notices_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [QuestionSets] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [QuestionSetName] nvarchar(255) NOT NULL,
    [Description] nvarchar(1024) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_QuestionSets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuestionSets_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SchoolTypes] (
    [Id] int NOT NULL IDENTITY,
    [PreviousLevelId] int NOT NULL,
    [SchoolTypeName] nvarchar(255) NOT NULL,
    CONSTRAINT [PK_SchoolTypes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SchoolTypes_PreviousLevels_PreviousLevelId] FOREIGN KEY ([PreviousLevelId]) REFERENCES [PreviousLevels] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [LocalLevels] (
    [Id] int NOT NULL IDENTITY,
    [DistrictId] int NOT NULL,
    [LocalLevelName] nvarchar(100) NOT NULL,
    [LocalLevelType] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_LocalLevels] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LocalLevels_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Departments] (
    [Id] int NOT NULL IDENTITY,
    [DepartmentCode] nvarchar(30) NOT NULL,
    [DepartmentName] nvarchar(200) NOT NULL,
    [ShortName] nvarchar(50) NULL,
    [Remarks] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [FacultyId] int NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Departments_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Addresses] (
    [Id] int NOT NULL IDENTITY,
    [LocalLevelId] int NOT NULL,
    [WardNumber] int NULL,
    [HouseNumber] nvarchar(50) NULL,
    [ToleStreet] nvarchar(255) NULL,
    [FullAddress] nvarchar(500) NULL,
    [AddressType] int NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Addresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Addresses_LocalLevels_LocalLevelId] FOREIGN KEY ([LocalLevelId]) REFERENCES [LocalLevels] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Programs] (
    [Id] int NOT NULL IDENTITY,
    [LevelId] int NOT NULL,
    [DepartmentId] int NOT NULL,
    [BoardId] int NULL,
    [ProgramCode] nvarchar(50) NOT NULL,
    [ProgramName] nvarchar(255) NOT NULL,
    [ShortName] nvarchar(50) NOT NULL,
    [Duration] int NOT NULL,
    [GrandTotalMarks] int NULL,
    [HasMultipleIntakes] bit NOT NULL,
    [NumberOfSeats] nvarchar(50) NULL,
    [ScholarshipSeats] int NULL,
    [Remarks] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    [RollNumberPrefix] nvarchar(10) NULL,
    CONSTRAINT [PK_Programs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Programs_Boards_BoardId] FOREIGN KEY ([BoardId]) REFERENCES [Boards] ([Id]),
    CONSTRAINT [FK_Programs_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Programs_Levels_LevelId] FOREIGN KEY ([LevelId]) REFERENCES [Levels] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Colleges] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(500) NOT NULL,
    [CollegeNameNepali] nvarchar(500) NULL,
    [ShortName] nvarchar(500) NULL,
    [EstablishedDate] datetime2 NOT NULL,
    [ClosedDate] datetime2 NULL,
    [Website] nvarchar(50) NULL,
    [Email] nvarchar(50) NOT NULL,
    [Phone1] nvarchar(20) NULL,
    [Phone2] nvarchar(20) NULL,
    [PrincipalName] nvarchar(255) NOT NULL,
    [PrincipalContactNumber] nvarchar(50) NOT NULL,
    [Fax] nvarchar(20) NULL,
    [Remarks] nvarchar(255) NULL,
    [IsExamCenterOnly] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [AllocatedAmount] decimal(18,2) NULL,
    [DisplayOrder] int NULL,
    [AddressId] int NULL,
    [CollegeTypeId] int NULL,
    [CollegeProfileId] int NULL,
    [DistrictId] int NULL,
    [QuestionSetId] int NULL,
    CONSTRAINT [PK_Colleges] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Colleges_Addresses_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Addresses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Colleges_CollegeTypes_CollegeTypeId] FOREIGN KEY ([CollegeTypeId]) REFERENCES [CollegeTypes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Colleges_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]),
    CONSTRAINT [FK_Colleges_QuestionSets_QuestionSetId] FOREIGN KEY ([QuestionSetId]) REFERENCES [QuestionSets] ([Id]),
    CONSTRAINT [FK_Colleges_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [CurriculumVersions] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [ProgramId] int NOT NULL,
    [EffectiveAcademicYearId] int NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_CurriculumVersions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CurriculumVersions_AcademicYears_EffectiveAcademicYearId] FOREIGN KEY ([EffectiveAcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CurriculumVersions_Programs_ProgramId] FOREIGN KEY ([ProgramId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CurriculumVersions_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [GradingSchemes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [ProgramId] int NOT NULL,
    [AcademicYearId] int NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedDate] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [UpdatedDate] datetime2 NULL,
    CONSTRAINT [PK_GradingSchemes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GradingSchemes_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_GradingSchemes_Programs_ProgramId] FOREIGN KEY ([ProgramId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ProgramSubjectPracticalCharge] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ProgramsId] int NOT NULL,
    [PracticalSubjectCharge] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_ProgramSubjectPracticalCharge] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProgramSubjectPracticalCharge_Programs_ProgramsId] FOREIGN KEY ([ProgramsId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProgramSubjectPracticalCharge_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SubjectOfferings] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [SubjectCatalogId] int NOT NULL,
    [ProgramId] int NOT NULL,
    [SemesterId] int NOT NULL,
    [IsCompulsory] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    [HasTheory] bit NOT NULL,
    [HasPractical] bit NOT NULL,
    [HasInternal] bit NOT NULL,
    [TheoryFullMarks] real NOT NULL,
    [TheoryPassMarks] real NOT NULL,
    [PracticalFullMarks] real NULL,
    [PracticalPassMarks] real NULL,
    [InternalTheoryFullMarks] real NULL,
    [InternalTheoryPassMarks] real NULL,
    [InternalPracticalFullMarks] real NULL,
    [InternalPracticalPassMarks] real NULL,
    CONSTRAINT [PK_SubjectOfferings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SubjectOfferings_Programs_ProgramId] FOREIGN KEY ([ProgramId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SubjectOfferings_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SubjectOfferings_SubjectCatalogs_SubjectCatalogId] FOREIGN KEY ([SubjectCatalogId]) REFERENCES [SubjectCatalogs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SubjectOfferings_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [CollegeFaculty] (
    [CollegesId] int NOT NULL,
    [FacultiesId] int NOT NULL,
    CONSTRAINT [PK_CollegeFaculty] PRIMARY KEY ([CollegesId], [FacultiesId]),
    CONSTRAINT [FK_CollegeFaculty_Colleges_CollegesId] FOREIGN KEY ([CollegesId]) REFERENCES [Colleges] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CollegeFaculty_Faculties_FacultiesId] FOREIGN KEY ([FacultiesId]) REFERENCES [Faculties] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CollegeProfiles] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [BankName] nvarchar(1024) NOT NULL,
    [BankBranchName] nvarchar(1024) NOT NULL,
    [BankAccountNumber] nvarchar(1024) NOT NULL,
    [ContactPersonName] nvarchar(1024) NOT NULL,
    [ContactPersonMobileNumber] nvarchar(1024) NOT NULL,
    [ContactPersonEmail] nvarchar(1024) NOT NULL,
    [Status] int NULL,
    [CollegeId] int NOT NULL,
    [BlankChequeUserAttachmentId] int NOT NULL,
    [AuditReportUserAttachmentId] int NOT NULL,
    CONSTRAINT [PK_CollegeProfiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CollegeProfiles_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CollegeProfiles_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CollegeProfiles_UserAttachments_AuditReportUserAttachmentId] FOREIGN KEY ([AuditReportUserAttachmentId]) REFERENCES [UserAttachments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CollegeProfiles_UserAttachments_BlankChequeUserAttachmentId] FOREIGN KEY ([BlankChequeUserAttachmentId]) REFERENCES [UserAttachments] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [CollegePrograms] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [AffiliationDate] datetime2 NULL,
    [NumberOfStudents] int NOT NULL,
    [Remarks] nvarchar(1024) NULL,
    [IsActive] bit NOT NULL,
    [CollegeId] int NOT NULL,
    [ProgramId] int NOT NULL,
    CONSTRAINT [PK_CollegePrograms] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CollegePrograms_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CollegePrograms_Programs_ProgramId] FOREIGN KEY ([ProgramId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CollegePrograms_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExamSchedules] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [CollegeId] int NULL,
    [ExamScheduleName] nvarchar(50) NOT NULL,
    [StartDateBs] nvarchar(10) NULL,
    [EndDateBs] nvarchar(10) NULL,
    [StartDate] date NULL,
    [EndDate] date NULL,
    [PublishedDate] datetime2 NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [Remarks] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    [ExtendedDate] datetime2 NULL,
    [ExtendedDateCharge] decimal(18,2) NULL,
    [ExamFee] decimal(18,2) NULL,
    [PracticalSubjectFee] decimal(18,2) NULL,
    [AdmissionCardReleaseDate] datetime2 NULL,
    [ExamScheduleCode] nvarchar(50) NULL,
    [AcademicYearId] int NOT NULL,
    [ProgramId] int NOT NULL,
    [SemesterId] int NOT NULL,
    [ExamTypeId] int NOT NULL,
    [LevelId] int NULL,
    CONSTRAINT [PK_ExamSchedules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExamSchedules_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamSchedules_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]),
    CONSTRAINT [FK_ExamSchedules_ExamTypes_ExamTypeId] FOREIGN KEY ([ExamTypeId]) REFERENCES [ExamTypes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamSchedules_Levels_LevelId] FOREIGN KEY ([LevelId]) REFERENCES [Levels] ([Id]),
    CONSTRAINT [FK_ExamSchedules_Programs_ProgramId] FOREIGN KEY ([ProgramId]) REFERENCES [Programs] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ExamSchedules_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ExamSchedules_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [StudentRegistrations] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [LevelId] int NOT NULL,
    [DepartmentId] int NOT NULL,
    [CollegeId] int NOT NULL,
    [FacultyId] int NULL,
    [ProgramId] int NULL,
    [RegistrationNumber] nvarchar(450) NULL,
    [FirstName] nvarchar(80) NOT NULL,
    [MiddleName] nvarchar(30) NULL,
    [LastName] nvarchar(30) NOT NULL,
    [NepaliName] nvarchar(100) NULL,
    [ContactNumber] nvarchar(15) NULL,
    [Phone] nvarchar(15) NULL,
    [Email] nvarchar(50) NULL,
    [DateOfBirthBS] nvarchar(10) NOT NULL,
    [DateOfBirthAD] nvarchar(max) NULL,
    [GenderId] int NOT NULL,
    [BloodGroup] nvarchar(5) NULL,
    [Nationality] nvarchar(50) NULL,
    [Religion] nvarchar(50) NULL,
    [PermanentAddressId] int NULL,
    [CurrentAddressId] int NULL,
    [IsActive] bit NOT NULL,
    [StudentCategoryId] int NOT NULL,
    [VerifiedBy] int NULL,
    [VerifiedDate] datetime2 NULL,
    [EthnicityId] int NULL,
    [EntranceRollNumber] nvarchar(50) NULL,
    [IsRegistrationNumberGenerated] bit NULL,
    [AcademicYearId] int NOT NULL,
    [DistrictId] int NULL,
    [EntryFormatId] int NULL,
    [IndexGroupId] int NULL,
    [LocalLevelId] int NULL,
    CONSTRAINT [PK_StudentRegistrations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentRegistrations_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentRegistrations_Addresses_CurrentAddressId] FOREIGN KEY ([CurrentAddressId]) REFERENCES [Addresses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentRegistrations_Addresses_PermanentAddressId] FOREIGN KEY ([PermanentAddressId]) REFERENCES [Addresses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentRegistrations_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentRegistrations_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentRegistrations_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]),
    CONSTRAINT [FK_StudentRegistrations_EntryFormats_EntryFormatId] FOREIGN KEY ([EntryFormatId]) REFERENCES [EntryFormats] ([Id]),
    CONSTRAINT [FK_StudentRegistrations_Ethnicities_EthnicityId] FOREIGN KEY ([EthnicityId]) REFERENCES [Ethnicities] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentRegistrations_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentRegistrations_Genders_GenderId] FOREIGN KEY ([GenderId]) REFERENCES [Genders] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentRegistrations_IndexGroups_IndexGroupId] FOREIGN KEY ([IndexGroupId]) REFERENCES [IndexGroups] ([Id]),
    CONSTRAINT [FK_StudentRegistrations_Levels_LevelId] FOREIGN KEY ([LevelId]) REFERENCES [Levels] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentRegistrations_LocalLevels_LocalLevelId] FOREIGN KEY ([LocalLevelId]) REFERENCES [LocalLevels] ([Id]),
    CONSTRAINT [FK_StudentRegistrations_Programs_ProgramId] FOREIGN KEY ([ProgramId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentRegistrations_StudentCategories_StudentCategoryId] FOREIGN KEY ([StudentCategoryId]) REFERENCES [StudentCategories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentRegistrations_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Users] (
    [Id] nvarchar(450) NOT NULL,
    [ProfilePath] nvarchar(max) NULL,
    [SignaturePath] nvarchar(max) NULL,
    [FullName] nvarchar(max) NULL,
    [Designation] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [ValidFrom] datetime2 NULL,
    [ValidTo] datetime2 NULL,
    [Remarks] nvarchar(max) NULL,
    [FacultyId] int NULL,
    [CollegeId] int NULL,
    [DepartmentId] int NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedDate] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [UpdatedDate] datetime2 NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Users_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Users_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Users_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [GradeDefinitions] (
    [Id] int NOT NULL IDENTITY,
    [GradeLetter] nvarchar(10) NOT NULL,
    [MinPercentage] decimal(5,2) NOT NULL,
    [MaxPercentage] decimal(5,2) NOT NULL,
    [GradePoint] decimal(5,2) NOT NULL,
    [Remark] nvarchar(50) NULL,
    [IsPass] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    [GradingSchemeId] int NOT NULL,
    CONSTRAINT [PK_GradeDefinitions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GradeDefinitions_GradingSchemes_GradingSchemeId] FOREIGN KEY ([GradingSchemeId]) REFERENCES [GradingSchemes] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [BillTitle] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [BillTitleName] nvarchar(255) NOT NULL,
    [Category] nvarchar(1024) NULL,
    [IsActive] bit NOT NULL,
    [Amount] decimal(18,2) NULL,
    [ThroughDate] datetime2 NULL,
    [ApplicableDate] datetime2 NULL,
    [ExamScheduleId] int NULL,
    [PracticalFee] decimal(18,2) NULL,
    [ProgramsId] int NULL,
    CONSTRAINT [PK_BillTitle] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BillTitle_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BillTitle_Programs_ProgramsId] FOREIGN KEY ([ProgramsId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BillTitle_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExamCenters] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ExamScheduleId] int NOT NULL,
    [CollegeId] int NULL,
    [Remark] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [Code] nvarchar(30) NOT NULL,
    CONSTRAINT [PK_ExamCenters] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExamCenters_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]),
    CONSTRAINT [FK_ExamCenters_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamCenters_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExamFees] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [Name] nvarchar(400) NOT NULL,
    [ExamScheduleId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [CollegeTypeId] int NULL,
    [ExamTypeId] int NULL,
    [ThroughDate] datetime2 NULL,
    [ApplicableDate] datetime2 NULL,
    [IsCollegeFee] bit NOT NULL,
    CONSTRAINT [PK_ExamFees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExamFees_CollegeTypes_CollegeTypeId] FOREIGN KEY ([CollegeTypeId]) REFERENCES [CollegeTypes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamFees_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamFees_ExamTypes_ExamTypeId] FOREIGN KEY ([ExamTypeId]) REFERENCES [ExamTypes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamFees_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExamRollNumberSetup] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ExamScheduleId] int NOT NULL,
    [FirstExamRollNumber] int NOT NULL,
    [Prefix] nvarchar(50) NULL,
    [Suffix] nvarchar(50) NULL,
    [DetailsJson] nvarchar(4000) NULL,
    [MinimumRollNumberLength] int NOT NULL,
    [Round] int NOT NULL,
    [MinimumGap] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ExamRollNumberSetup] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExamRollNumberSetup_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamRollNumberSetup_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ResultRecords] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [AcademicYearId] int NOT NULL,
    [ProgramsId] int NOT NULL,
    [ExamTypeId] int NOT NULL,
    [CollegeId] int NOT NULL,
    [Year] nvarchar(3) NOT NULL,
    [Part] nvarchar(2) NOT NULL,
    [RegistrationNumber] nvarchar(50) NULL,
    [SymbolNumber] nvarchar(50) NOT NULL,
    [Alphabet] nvarchar(1) NULL,
    [DateOfBirthBs] nvarchar(10) NOT NULL,
    [Sex] nvarchar(10) NULL,
    [TheoryObtainedMarks] nvarchar(5) NULL,
    [InternalObtainedMarks] nvarchar(5) NULL,
    [PracticalObtainedMarks] nvarchar(5) NULL,
    [TheoryObtainedGrade] nvarchar(5) NULL,
    [InternalObtainedGrade] nvarchar(5) NULL,
    [PracticalObtainedGrade] nvarchar(5) NULL,
    [TotalObtainedMarks] nvarchar(5) NULL,
    [TotalObtainedGrade] nvarchar(5) NULL,
    [TotalGradePoints] nvarchar(5) NULL,
    [Gpa] nvarchar(4) NULL,
    [Result] nvarchar(50) NULL,
    [StudentName] nvarchar(255) NULL,
    [ResultRecordMasterId] int NOT NULL,
    [ExamScheduleId] int NULL,
    [CreatedDate] datetime2 NULL,
    CONSTRAINT [PK_ResultRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ResultRecords_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResultRecords_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResultRecords_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResultRecords_ExamTypes_ExamTypeId] FOREIGN KEY ([ExamTypeId]) REFERENCES [ExamTypes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResultRecords_Programs_ProgramsId] FOREIGN KEY ([ProgramsId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ResultRecords_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ApplicationVouchers] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [VoucherNumber] nvarchar(50) NOT NULL,
    [StudentName] nvarchar(1024) NOT NULL,
    [DateOfBirthAd] date NULL,
    [DateOfBirthBs] nvarchar(50) NULL,
    [Amount] decimal(18,2) NOT NULL,
    [VoucherDate] datetime2 NULL,
    [Timestamp] datetime2 NULL,
    [ContactNumber] nvarchar(1024) NOT NULL,
    [Branch] nvarchar(1024) NULL,
    [ExamScheduleId] int NOT NULL,
    [StudentRegistrationId] int NULL,
    CONSTRAINT [PK_ApplicationVouchers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApplicationVouchers_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ApplicationVouchers_StudentRegistrations_StudentRegistrationId] FOREIGN KEY ([StudentRegistrationId]) REFERENCES [StudentRegistrations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ApplicationVouchers_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PaymentRequestLogs] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [PaymentRequestLogStatus] int NULL,
    [InvoiceNumber] nvarchar(50) NOT NULL,
    [ForwardedTimestamp] datetime2 NOT NULL,
    [DateOfBirthAd] datetime2 NULL,
    [MobileNumber] nvarchar(20) NULL,
    [Email] nvarchar(100) NULL,
    [FullName] nvarchar(255) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [FullRequestContent] nvarchar(max) NOT NULL,
    [PaymentTypeId] int NOT NULL,
    [StudentRegistrationId] int NULL,
    [ExamScheduleId] int NOT NULL,
    [TransactionId] nvarchar(50) NULL,
    [CollegeId] int NULL,
    [StudentCount] int NOT NULL,
    [SelectedSubjectIds] nvarchar(1000) NULL,
    CONSTRAINT [PK_PaymentRequestLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentRequestLogs_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PaymentRequestLogs_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PaymentRequestLogs_PaymentType_PaymentTypeId] FOREIGN KEY ([PaymentTypeId]) REFERENCES [PaymentType] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PaymentRequestLogs_StudentRegistrations_StudentRegistrationId] FOREIGN KEY ([StudentRegistrationId]) REFERENCES [StudentRegistrations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PaymentRequestLogs_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [StudentGuardians] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [StudentRegistrationId] int NOT NULL,
    [FatherName] nvarchar(50) NOT NULL,
    [FatherContactNumber] nvarchar(50) NULL,
    [FatherPhone] nvarchar(50) NULL,
    [FatherEmail] nvarchar(50) NULL,
    [FatherQualification] nvarchar(50) NULL,
    [FatherProfession] nvarchar(50) NULL,
    [FatherAddress] nvarchar(100) NULL,
    [FatherOrganization] nvarchar(50) NULL,
    [FatherOrganizationAddress] nvarchar(50) NULL,
    [MotherName] nvarchar(50) NOT NULL,
    [MotherContactNumber] nvarchar(50) NULL,
    [MotherPhone] nvarchar(50) NULL,
    [MotherEmail] nvarchar(50) NULL,
    [MotherQualification] nvarchar(50) NULL,
    [MotherProfession] nvarchar(50) NULL,
    [MotherAddress] nvarchar(100) NULL,
    [MotherOrganization] nvarchar(50) NULL,
    [MotherOrganizationAddress] nvarchar(50) NULL,
    [GuardianName] nvarchar(50) NOT NULL,
    [GuardianContactNumber] nvarchar(50) NULL,
    [GuardianPhone] nvarchar(50) NULL,
    [GuardianEmail] nvarchar(50) NULL,
    [GuardianQualification] nvarchar(50) NULL,
    [GuardianProfession] nvarchar(50) NULL,
    [GuardianAddress] nvarchar(100) NULL,
    [GuardianOrganization] nvarchar(50) NULL,
    [GuardianOrganizationAddress] nvarchar(50) NULL,
    [RelationWithStudent] nvarchar(50) NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedDate] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [UpdatedDate] datetime2 NULL,
    CONSTRAINT [PK_StudentGuardians] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentGuardians_StudentRegistrations_StudentRegistrationId] FOREIGN KEY ([StudentRegistrationId]) REFERENCES [StudentRegistrations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentGuardians_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [StudentQualifications] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [StudentRegistrationId] int NOT NULL,
    [BoardId] int NOT NULL,
    [PreviousLevelId] int NOT NULL,
    [ProgramName] nvarchar(255) NULL,
    [InstituteName] nvarchar(255) NOT NULL,
    [PassedYear] nvarchar(50) NULL,
    [Specialization] nvarchar(255) NULL,
    [Percentage] decimal(5,2) NULL,
    [TotalCredits] nvarchar(50) NULL,
    [Remarks] nvarchar(50) NULL,
    [IsHigherDegree] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [DocumentPath] nvarchar(500) NULL,
    [ExamRollNumber] nvarchar(500) NULL,
    CONSTRAINT [PK_StudentQualifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentQualifications_Boards_BoardId] FOREIGN KEY ([BoardId]) REFERENCES [Boards] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentQualifications_PreviousLevels_PreviousLevelId] FOREIGN KEY ([PreviousLevelId]) REFERENCES [PreviousLevels] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentQualifications_StudentRegistrations_StudentRegistrationId] FOREIGN KEY ([StudentRegistrationId]) REFERENCES [StudentRegistrations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentQualifications_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [StudentAdmissions] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ProgramsId] int NOT NULL,
    [CollegeId] int NOT NULL,
    [AdmissionDate] datetime2 NOT NULL,
    [CheckedBy] int NULL,
    [IsCompleted] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CollegeRollNumber] nvarchar(50) NULL,
    [HasFeeExemption] bit NOT NULL,
    [AppUserId] nvarchar(450) NULL,
    [BatchId] int NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedDate] datetime2 NULL,
    [StudentRegistrationId] int NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [UpdatedDate] datetime2 NULL,
    CONSTRAINT [PK_StudentAdmissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentAdmissions_Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [Batches] ([Id]),
    CONSTRAINT [FK_StudentAdmissions_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentAdmissions_Programs_ProgramsId] FOREIGN KEY ([ProgramsId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentAdmissions_StudentRegistrations_StudentRegistrationId] FOREIGN KEY ([StudentRegistrationId]) REFERENCES [StudentRegistrations] ([Id]),
    CONSTRAINT [FK_StudentAdmissions_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StudentAdmissions_Users_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [TeacherSubjectAssignments] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [TeacherUserId] nvarchar(450) NOT NULL,
    [SubjectOfferingId] int NOT NULL,
    [ExamScheduleId] int NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_TeacherSubjectAssignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TeacherSubjectAssignments_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TeacherSubjectAssignments_SubjectOfferings_SubjectOfferingId] FOREIGN KEY ([SubjectOfferingId]) REFERENCES [SubjectOfferings] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TeacherSubjectAssignments_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TeacherSubjectAssignments_Users_TeacherUserId] FOREIGN KEY ([TeacherUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [UserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_UserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [UserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_UserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_UserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [UserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [UserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_UserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [BankVoucher] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [AcademicYearId] int NOT NULL,
    [CollegeId] int NOT NULL,
    [BillTitleId] int NOT NULL,
    [BankId] int NOT NULL,
    [BankAddress] nvarchar(100) NULL,
    [VoucherDate] datetime2 NOT NULL,
    [VoucherNumber] nvarchar(50) NULL,
    [VoucherAmount] decimal(18,2) NOT NULL,
    [Remarks] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    [BankVoucherUserAttachmentId] int NULL,
    CONSTRAINT [PK_BankVoucher] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BankVoucher_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BankVoucher_Banks_BankId] FOREIGN KEY ([BankId]) REFERENCES [Banks] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BankVoucher_BillTitle_BillTitleId] FOREIGN KEY ([BillTitleId]) REFERENCES [BillTitle] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BankVoucher_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BankVoucher_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BankVoucher_UserAttachments_BankVoucherUserAttachmentId] FOREIGN KEY ([BankVoucherUserAttachmentId]) REFERENCES [UserAttachments] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExamCenterColleges] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ExamCenterId] int NOT NULL,
    [CollegeId] int NOT NULL,
    CONSTRAINT [PK_ExamCenterColleges] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExamCenterColleges_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamCenterColleges_ExamCenters_ExamCenterId] FOREIGN KEY ([ExamCenterId]) REFERENCES [ExamCenters] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamCenterColleges_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExamCenterSymbolRanges] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ExamScheduleId] int NOT NULL,
    [ExamCenterId] int NOT NULL,
    [FromSymbolNumber] bigint NOT NULL,
    [ToSymbolNumber] bigint NOT NULL,
    CONSTRAINT [PK_ExamCenterSymbolRanges] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExamCenterSymbolRanges_ExamCenters_ExamCenterId] FOREIGN KEY ([ExamCenterId]) REFERENCES [ExamCenters] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamCenterSymbolRanges_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamCenterSymbolRanges_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExamCenterVenues] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ExamCenterId] int NOT NULL,
    [CollegeId] int NOT NULL,
    CONSTRAINT [PK_ExamCenterVenues] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExamCenterVenues_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamCenterVenues_ExamCenters_ExamCenterId] FOREIGN KEY ([ExamCenterId]) REFERENCES [ExamCenters] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamCenterVenues_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExamSlots] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ExamScheduleId] int NOT NULL,
    [SubjectOfferingId] int NOT NULL,
    [BatchId] int NOT NULL,
    [ExamCenterId] int NOT NULL,
    [ExamDate] nvarchar(10) NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [RoomNumber] nvarchar(50) NULL,
    [Remarks] nvarchar(255) NULL,
    CONSTRAINT [PK_ExamSlots] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExamSlots_Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [Batches] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamSlots_ExamCenters_ExamCenterId] FOREIGN KEY ([ExamCenterId]) REFERENCES [ExamCenters] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamSlots_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamSlots_SubjectOfferings_SubjectOfferingId] FOREIGN KEY ([SubjectOfferingId]) REFERENCES [SubjectOfferings] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamSlots_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [EntranceExamApplications] (
    [Id] int NOT NULL IDENTITY,
    [AcademicYearId] int NOT NULL,
    [CollegeId] int NOT NULL,
    [ProgramId] int NOT NULL,
    [FirstName] nvarchar(80) NOT NULL,
    [MiddleName] nvarchar(30) NULL,
    [LastName] nvarchar(30) NOT NULL,
    [NepaliName] nvarchar(100) NULL,
    [DateOfBirthBS] nvarchar(10) NOT NULL,
    [DateOfBirthAD] nvarchar(max) NULL,
    [GenderId] int NOT NULL,
    [Email] nvarchar(50) NULL,
    [ContactNumber] nvarchar(15) NULL,
    [Phone] nvarchar(15) NULL,
    [PermanentAddressId] int NULL,
    [FatherName] nvarchar(100) NULL,
    [FatherContact] nvarchar(15) NULL,
    [MotherName] nvarchar(100) NULL,
    [MotherContact] nvarchar(15) NULL,
    [GuardianEmail] nvarchar(100) NULL,
    [FatherProfession] nvarchar(100) NULL,
    [MotherProfession] nvarchar(100) NULL,
    [CitizenshipNo] nvarchar(50) NULL,
    [CitizenshipDistrictId] int NULL,
    [CitizenshipIssueDateBs] nvarchar(10) NULL,
    [CitizenshipIssueDateAd] nvarchar(max) NULL,
    [BloodGroup] nvarchar(5) NULL,
    [BirthPlace] nvarchar(100) NULL,
    [Country] nvarchar(100) NULL,
    [PostalCode] nvarchar(20) NULL,
    [PhotoPath] nvarchar(500) NULL,
    [DocumentsPath] nvarchar(500) NULL,
    [VoucherPath] nvarchar(500) NULL,
    [PreviousSchoolCollege] nvarchar(200) NULL,
    [PreviousLevelId] int NULL,
    [PreviousPassedYear] nvarchar(10) NULL,
    [PreviousSymbolNumber] nvarchar(50) NULL,
    [PreviousGPA] decimal(5,2) NULL,
    [PreviousDivision] nvarchar(10) NULL,
    [PreviousLevel2Id] int NULL,
    [PreviousSchoolCollege2] nvarchar(200) NULL,
    [PreviousBoard2] nvarchar(50) NULL,
    [PreviousSymbolNumber2] nvarchar(50) NULL,
    [PreviousPassedYear2] nvarchar(10) NULL,
    [PreviousGPA2] decimal(5,2) NULL,
    [PreviousDivision2] nvarchar(10) NULL,
    [PreviousLevel3Id] int NULL,
    [PreviousSchoolCollege3] nvarchar(200) NULL,
    [PreviousBoard3] nvarchar(50) NULL,
    [PreviousSymbolNumber3] nvarchar(50) NULL,
    [PreviousPassedYear3] nvarchar(10) NULL,
    [PreviousGPA3] decimal(5,2) NULL,
    [PreviousDivision3] nvarchar(10) NULL,
    [ApplicationVoucherId] int NULL,
    [PaymentVerified] bit NOT NULL,
    [Status] int NOT NULL,
    [ReviewedBy] nvarchar(max) NULL,
    [ReviewDate] datetime2 NULL,
    [ReviewRemarks] nvarchar(500) NULL,
    [TenantId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedDate] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [UpdatedDate] datetime2 NULL,
    CONSTRAINT [PK_EntranceExamApplications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EntranceExamApplications_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntranceExamApplications_Addresses_PermanentAddressId] FOREIGN KEY ([PermanentAddressId]) REFERENCES [Addresses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntranceExamApplications_ApplicationVouchers_ApplicationVoucherId] FOREIGN KEY ([ApplicationVoucherId]) REFERENCES [ApplicationVouchers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntranceExamApplications_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntranceExamApplications_Districts_CitizenshipDistrictId] FOREIGN KEY ([CitizenshipDistrictId]) REFERENCES [Districts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntranceExamApplications_Genders_GenderId] FOREIGN KEY ([GenderId]) REFERENCES [Genders] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntranceExamApplications_PreviousLevels_PreviousLevel2Id] FOREIGN KEY ([PreviousLevel2Id]) REFERENCES [PreviousLevels] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntranceExamApplications_PreviousLevels_PreviousLevel3Id] FOREIGN KEY ([PreviousLevel3Id]) REFERENCES [PreviousLevels] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntranceExamApplications_PreviousLevels_PreviousLevelId] FOREIGN KEY ([PreviousLevelId]) REFERENCES [PreviousLevels] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntranceExamApplications_Programs_ProgramId] FOREIGN KEY ([ProgramId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EntranceExamApplications_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PaymentPracticalSubjects] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [PaymentRequestLogId] int NOT NULL,
    [PracticalSubjectsCount] int NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_PaymentPracticalSubjects] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentPracticalSubjects_PaymentRequestLogs_PaymentRequestLogId] FOREIGN KEY ([PaymentRequestLogId]) REFERENCES [PaymentRequestLogs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PaymentPracticalSubjects_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PaymentResponseLogs] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [PaymentRequestLogId] int NOT NULL,
    [ResponseTimestamp] datetime2 NOT NULL,
    [IsSuccess] bit NOT NULL,
    [ResponseMessage] nvarchar(1024) NOT NULL,
    [FullResponse] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_PaymentResponseLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentResponseLogs_PaymentRequestLogs_PaymentRequestLogId] FOREIGN KEY ([PaymentRequestLogId]) REFERENCES [PaymentRequestLogs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PaymentResponseLogs_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SemesterEnrollments] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [StudentAdmissionId] int NOT NULL,
    [SemesterId] int NOT NULL,
    [EnrollmentStatus] int NOT NULL,
    [EnrollmentType] int NOT NULL,
    [PaymentStatus] int NOT NULL,
    [EnrolledDate] datetime2 NOT NULL,
    [DropDate] datetime2 NULL,
    [DropReason] nvarchar(500) NULL,
    [SemesterResultDate] datetime2 NULL,
    [TotalCredits] float NOT NULL,
    [GradePoints] float NOT NULL,
    [TotalFee] float NOT NULL,
    [PaidAmount] float NOT NULL,
    [Deficiency] bit NOT NULL,
    [ResultStatus] int NOT NULL,
    CONSTRAINT [PK_SemesterEnrollments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SemesterEnrollments_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SemesterEnrollments_StudentAdmissions_StudentAdmissionId] FOREIGN KEY ([StudentAdmissionId]) REFERENCES [StudentAdmissions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SemesterEnrollments_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExamRegistrations] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [AcademicYearId] int NOT NULL,
    [ExamCenterId] int NULL,
    [CollegeId] int NOT NULL,
    [ExamRollNumber] nvarchar(20) NULL,
    [ExamRollNumberCoding] bigint NULL,
    [FeeEnclosed] decimal(18,2) NULL,
    [AttendancePercentage] decimal(5,2) NULL,
    [RegistrationDate] datetime2 NULL,
    [Status] int NOT NULL,
    [VerifiedByUsername] nvarchar(100) NULL,
    [VerifiedDate] datetime2 NULL,
    [Sgpa] nvarchar(50) NULL,
    [Remarks] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    [ExamScheduleId] int NOT NULL,
    [RollNumberIndex] int NULL,
    [IsAppliedByStudent] bit NULL,
    [ProgramsId] int NULL,
    [ApplicationVoucherId] int NULL,
    [AdminVerifiedByUsername] nvarchar(100) NULL,
    [SymbolNumber] nvarchar(50) NULL,
    [AdminVerifiedDate] datetime2 NULL,
    [SemesterEnrollmentId] int NULL,
    CONSTRAINT [PK_ExamRegistrations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExamRegistrations_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamRegistrations_ApplicationVouchers_ApplicationVoucherId] FOREIGN KEY ([ApplicationVoucherId]) REFERENCES [ApplicationVouchers] ([Id]),
    CONSTRAINT [FK_ExamRegistrations_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamRegistrations_ExamCenters_ExamCenterId] FOREIGN KEY ([ExamCenterId]) REFERENCES [ExamCenters] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamRegistrations_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamRegistrations_Programs_ProgramsId] FOREIGN KEY ([ProgramsId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamRegistrations_SemesterEnrollments_SemesterEnrollmentId] FOREIGN KEY ([SemesterEnrollmentId]) REFERENCES [SemesterEnrollments] ([Id]),
    CONSTRAINT [FK_ExamRegistrations_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExamSubjectResults] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ExamRegistrationId] int NOT NULL,
    [ExamTypeId] int NOT NULL,
    [SubjectOfferingId] int NOT NULL,
    [ExamScheduleId] int NULL,
    [ObtainedMarksTheory] real NULL,
    [ObtainedMarksTheoryConfirm] real NULL,
    [ObtainedMarksPractical] real NULL,
    [ObtainedMarksPracticalConfirm] real NULL,
    [ObtainedMarksTheoryInternal] real NULL,
    [ObtainedMarksPracticalInternal] real NULL,
    [GradeLetter] nvarchar(3) NULL,
    [Remarks] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    [IsLooseEntry] bit NULL,
    [IsTheoryRegistered] bit NULL,
    [IsPracticalRegistered] bit NULL,
    [IsExtra] bit NULL,
    [ExamStartedDateTime] datetime2 NULL,
    [IsSubmitted] bit NOT NULL,
    [ObtainedMarks] real NULL,
    [ExamSubmittedDateTime] datetime2 NULL,
    [IsAutoSubmitted] bit NULL,
    [LastStatusSyncDateTime] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedDate] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [UpdatedDate] datetime2 NULL,
    CONSTRAINT [PK_ExamSubjectResults] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExamSubjectResults_ExamRegistrations_ExamRegistrationId] FOREIGN KEY ([ExamRegistrationId]) REFERENCES [ExamRegistrations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamSubjectResults_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamSubjectResults_ExamTypes_ExamTypeId] FOREIGN KEY ([ExamTypeId]) REFERENCES [ExamTypes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamSubjectResults_SubjectOfferings_SubjectOfferingId] FOREIGN KEY ([SubjectOfferingId]) REFERENCES [SubjectOfferings] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExamSubjectResults_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [HallTickets] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ExamRegistrationId] int NOT NULL,
    [ExamScheduleId] int NOT NULL,
    [StudentRegistrationId] int NULL,
    [HallTicketNumber] nvarchar(max) NULL,
    [GeneratedDate] datetime2 NOT NULL,
    [IsDownloaded] bit NOT NULL,
    [DownloadedDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_HallTickets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HallTickets_ExamRegistrations_ExamRegistrationId] FOREIGN KEY ([ExamRegistrationId]) REFERENCES [ExamRegistrations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HallTickets_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HallTickets_StudentRegistrations_StudentRegistrationId] FOREIGN KEY ([StudentRegistrationId]) REFERENCES [StudentRegistrations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_HallTickets_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [RetotalRequests] (
    [Id] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [ExamSubjectResultId] int NOT NULL,
    [StudentRegistrationId] int NOT NULL,
    [ExamRegistrationId] int NOT NULL,
    [RequestedDate] datetime2 NOT NULL,
    [Reason] nvarchar(max) NULL,
    [Status] int NOT NULL,
    [OriginalGradeLetter] nvarchar(max) NULL,
    [OriginalObtainedMarks] real NULL,
    [RetotalledGradeLetter] nvarchar(max) NULL,
    [RetotalledObtainedMarks] real NULL,
    [ReviewedByUsername] nvarchar(max) NULL,
    [ReviewedDate] datetime2 NULL,
    [AdminRemarks] nvarchar(max) NULL,
    [FeeAmount] decimal(18,2) NULL,
    [FeePaid] bit NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_RetotalRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RetotalRequests_ExamRegistrations_ExamRegistrationId] FOREIGN KEY ([ExamRegistrationId]) REFERENCES [ExamRegistrations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RetotalRequests_ExamSubjectResults_ExamSubjectResultId] FOREIGN KEY ([ExamSubjectResultId]) REFERENCES [ExamSubjectResults] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RetotalRequests_StudentRegistrations_StudentRegistrationId] FOREIGN KEY ([StudentRegistrationId]) REFERENCES [StudentRegistrations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RetotalRequests_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_AcademicYears_AcademicYearCode] ON [AcademicYears] ([AcademicYearCode]);

CREATE INDEX [IX_Addresses_LocalLevelId] ON [Addresses] ([LocalLevelId]);

CREATE INDEX [IX_ApplicationVouchers_ExamScheduleId] ON [ApplicationVouchers] ([ExamScheduleId]);

CREATE INDEX [IX_ApplicationVouchers_StudentRegistrationId] ON [ApplicationVouchers] ([StudentRegistrationId]);

CREATE INDEX [IX_ApplicationVouchers_TenantId] ON [ApplicationVouchers] ([TenantId]);

CREATE UNIQUE INDEX [IX_Banks_BankCode] ON [Banks] ([BankCode]) WHERE [BankCode] IS NOT NULL;

CREATE INDEX [IX_BankVoucher_AcademicYearId] ON [BankVoucher] ([AcademicYearId]);

CREATE INDEX [IX_BankVoucher_BankId] ON [BankVoucher] ([BankId]);

CREATE INDEX [IX_BankVoucher_BankVoucherUserAttachmentId] ON [BankVoucher] ([BankVoucherUserAttachmentId]);

CREATE INDEX [IX_BankVoucher_BillTitleId] ON [BankVoucher] ([BillTitleId]);

CREATE INDEX [IX_BankVoucher_CollegeId] ON [BankVoucher] ([CollegeId]);

CREATE INDEX [IX_BankVoucher_TenantId] ON [BankVoucher] ([TenantId]);

CREATE INDEX [IX_Batches_AcademicYearId] ON [Batches] ([AcademicYearId]);

CREATE INDEX [IX_BillTitle_ExamScheduleId] ON [BillTitle] ([ExamScheduleId]);

CREATE INDEX [IX_BillTitle_ProgramsId] ON [BillTitle] ([ProgramsId]);

CREATE INDEX [IX_BillTitle_TenantId] ON [BillTitle] ([TenantId]);

CREATE UNIQUE INDEX [IX_Boards_BoardName] ON [Boards] ([BoardName]) WHERE [BoardName] IS NOT NULL;

CREATE INDEX [IX_CollegeFaculty_FacultiesId] ON [CollegeFaculty] ([FacultiesId]);

CREATE INDEX [IX_CollegeProfiles_AuditReportUserAttachmentId] ON [CollegeProfiles] ([AuditReportUserAttachmentId]);

CREATE INDEX [IX_CollegeProfiles_BlankChequeUserAttachmentId] ON [CollegeProfiles] ([BlankChequeUserAttachmentId]);

CREATE UNIQUE INDEX [IX_CollegeProfiles_CollegeId] ON [CollegeProfiles] ([CollegeId]);

CREATE INDEX [IX_CollegeProfiles_TenantId] ON [CollegeProfiles] ([TenantId]);

CREATE INDEX [IX_CollegePrograms_CollegeId] ON [CollegePrograms] ([CollegeId]);

CREATE INDEX [IX_CollegePrograms_ProgramId] ON [CollegePrograms] ([ProgramId]);

CREATE INDEX [IX_CollegePrograms_TenantId] ON [CollegePrograms] ([TenantId]);

CREATE INDEX [IX_Colleges_AddressId] ON [Colleges] ([AddressId]);

CREATE INDEX [IX_Colleges_CollegeTypeId] ON [Colleges] ([CollegeTypeId]);

CREATE INDEX [IX_Colleges_DistrictId] ON [Colleges] ([DistrictId]);

CREATE INDEX [IX_Colleges_QuestionSetId] ON [Colleges] ([QuestionSetId]);

CREATE UNIQUE INDEX [IX_Colleges_TenantId_Code] ON [Colleges] ([TenantId], [Code]);

CREATE UNIQUE INDEX [IX_CollegeTypes_Code] ON [CollegeTypes] ([Code]);

CREATE INDEX [IX_CurriculumVersions_EffectiveAcademicYearId] ON [CurriculumVersions] ([EffectiveAcademicYearId]);

CREATE INDEX [IX_CurriculumVersions_ProgramId] ON [CurriculumVersions] ([ProgramId]);

CREATE INDEX [IX_CurriculumVersions_TenantId] ON [CurriculumVersions] ([TenantId]);

CREATE UNIQUE INDEX [IX_Departments_DepartmentCode] ON [Departments] ([DepartmentCode]);

CREATE INDEX [IX_Departments_FacultyId] ON [Departments] ([FacultyId]);

CREATE UNIQUE INDEX [IX_Districts_DistrictCode] ON [Districts] ([DistrictCode]) WHERE [DistrictCode] IS NOT NULL;

CREATE INDEX [IX_Districts_ProvinceId] ON [Districts] ([ProvinceId]);

CREATE INDEX [IX_EntranceExamApplications_AcademicYearId] ON [EntranceExamApplications] ([AcademicYearId]);

CREATE INDEX [IX_EntranceExamApplications_ApplicationVoucherId] ON [EntranceExamApplications] ([ApplicationVoucherId]);

CREATE INDEX [IX_EntranceExamApplications_CitizenshipDistrictId] ON [EntranceExamApplications] ([CitizenshipDistrictId]);

CREATE INDEX [IX_EntranceExamApplications_CollegeId] ON [EntranceExamApplications] ([CollegeId]);

CREATE INDEX [IX_EntranceExamApplications_GenderId] ON [EntranceExamApplications] ([GenderId]);

CREATE INDEX [IX_EntranceExamApplications_PermanentAddressId] ON [EntranceExamApplications] ([PermanentAddressId]);

CREATE INDEX [IX_EntranceExamApplications_PreviousLevel2Id] ON [EntranceExamApplications] ([PreviousLevel2Id]);

CREATE INDEX [IX_EntranceExamApplications_PreviousLevel3Id] ON [EntranceExamApplications] ([PreviousLevel3Id]);

CREATE INDEX [IX_EntranceExamApplications_PreviousLevelId] ON [EntranceExamApplications] ([PreviousLevelId]);

CREATE INDEX [IX_EntranceExamApplications_ProgramId] ON [EntranceExamApplications] ([ProgramId]);

CREATE INDEX [IX_EntranceExamApplications_TenantId] ON [EntranceExamApplications] ([TenantId]);

CREATE UNIQUE INDEX [IX_EntryFormats_EntryFormatName] ON [EntryFormats] ([EntryFormatName]) WHERE [EntryFormatName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Ethnicities_EthnicityName] ON [Ethnicities] ([EthnicityName]) WHERE [EthnicityName] IS NOT NULL;

CREATE INDEX [IX_ExamCenterColleges_CollegeId] ON [ExamCenterColleges] ([CollegeId]);

CREATE INDEX [IX_ExamCenterColleges_ExamCenterId] ON [ExamCenterColleges] ([ExamCenterId]);

CREATE INDEX [IX_ExamCenterColleges_TenantId] ON [ExamCenterColleges] ([TenantId]);

CREATE INDEX [IX_ExamCenters_CollegeId] ON [ExamCenters] ([CollegeId]);

CREATE INDEX [IX_ExamCenters_ExamScheduleId] ON [ExamCenters] ([ExamScheduleId]);

CREATE INDEX [IX_ExamCenters_TenantId] ON [ExamCenters] ([TenantId]);

CREATE INDEX [IX_ExamCenterSymbolRanges_ExamCenterId] ON [ExamCenterSymbolRanges] ([ExamCenterId]);

CREATE INDEX [IX_ExamCenterSymbolRanges_ExamScheduleId] ON [ExamCenterSymbolRanges] ([ExamScheduleId]);

CREATE INDEX [IX_ExamCenterSymbolRanges_TenantId] ON [ExamCenterSymbolRanges] ([TenantId]);

CREATE INDEX [IX_ExamCenterVenues_CollegeId] ON [ExamCenterVenues] ([CollegeId]);

CREATE INDEX [IX_ExamCenterVenues_ExamCenterId] ON [ExamCenterVenues] ([ExamCenterId]);

CREATE INDEX [IX_ExamCenterVenues_TenantId] ON [ExamCenterVenues] ([TenantId]);

CREATE INDEX [IX_ExamFees_CollegeTypeId] ON [ExamFees] ([CollegeTypeId]);

CREATE INDEX [IX_ExamFees_ExamScheduleId] ON [ExamFees] ([ExamScheduleId]);

CREATE INDEX [IX_ExamFees_ExamTypeId] ON [ExamFees] ([ExamTypeId]);

CREATE INDEX [IX_ExamFees_TenantId] ON [ExamFees] ([TenantId]);

CREATE INDEX [IX_ExamRegistrations_AcademicYearId] ON [ExamRegistrations] ([AcademicYearId]);

CREATE INDEX [IX_ExamRegistrations_ApplicationVoucherId] ON [ExamRegistrations] ([ApplicationVoucherId]);

CREATE INDEX [IX_ExamRegistrations_CollegeId] ON [ExamRegistrations] ([CollegeId]);

CREATE INDEX [IX_ExamRegistrations_ExamCenterId] ON [ExamRegistrations] ([ExamCenterId]);

CREATE INDEX [IX_ExamRegistrations_ExamScheduleId] ON [ExamRegistrations] ([ExamScheduleId]);

CREATE INDEX [IX_ExamRegistrations_ProgramsId] ON [ExamRegistrations] ([ProgramsId]);

CREATE INDEX [IX_ExamRegistrations_SemesterEnrollmentId] ON [ExamRegistrations] ([SemesterEnrollmentId]);

CREATE INDEX [IX_ExamRegistrations_TenantId] ON [ExamRegistrations] ([TenantId]);

CREATE INDEX [IX_ExamRollNumberSetup_ExamScheduleId] ON [ExamRollNumberSetup] ([ExamScheduleId]);

CREATE INDEX [IX_ExamRollNumberSetup_TenantId] ON [ExamRollNumberSetup] ([TenantId]);

CREATE INDEX [IX_ExamSchedules_AcademicYearId] ON [ExamSchedules] ([AcademicYearId]);

CREATE INDEX [IX_ExamSchedules_CollegeId] ON [ExamSchedules] ([CollegeId]);

CREATE INDEX [IX_ExamSchedules_ExamTypeId] ON [ExamSchedules] ([ExamTypeId]);

CREATE INDEX [IX_ExamSchedules_LevelId] ON [ExamSchedules] ([LevelId]);

CREATE INDEX [IX_ExamSchedules_ProgramId] ON [ExamSchedules] ([ProgramId]);

CREATE INDEX [IX_ExamSchedules_SemesterId] ON [ExamSchedules] ([SemesterId]);

CREATE UNIQUE INDEX [IX_ExamSchedules_TenantId_ExamScheduleCode] ON [ExamSchedules] ([TenantId], [ExamScheduleCode]) WHERE [ExamScheduleCode] IS NOT NULL;

CREATE INDEX [IX_ExamSlots_BatchId] ON [ExamSlots] ([BatchId]);

CREATE INDEX [IX_ExamSlots_ExamCenterId] ON [ExamSlots] ([ExamCenterId]);

CREATE INDEX [IX_ExamSlots_ExamScheduleId] ON [ExamSlots] ([ExamScheduleId]);

CREATE INDEX [IX_ExamSlots_SubjectOfferingId] ON [ExamSlots] ([SubjectOfferingId]);

CREATE INDEX [IX_ExamSlots_TenantId] ON [ExamSlots] ([TenantId]);

CREATE INDEX [IX_ExamSubjectResults_ExamRegistrationId] ON [ExamSubjectResults] ([ExamRegistrationId]);

CREATE INDEX [IX_ExamSubjectResults_ExamScheduleId] ON [ExamSubjectResults] ([ExamScheduleId]);

CREATE INDEX [IX_ExamSubjectResults_ExamTypeId] ON [ExamSubjectResults] ([ExamTypeId]);

CREATE INDEX [IX_ExamSubjectResults_SubjectOfferingId] ON [ExamSubjectResults] ([SubjectOfferingId]);

CREATE INDEX [IX_ExamSubjectResults_TenantId] ON [ExamSubjectResults] ([TenantId]);

CREATE UNIQUE INDEX [IX_ExamTypes_Name] ON [ExamTypes] ([Name]) WHERE [Name] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Faculties_OfficeCode] ON [Faculties] ([OfficeCode]) WHERE [OfficeCode] IS NOT NULL;

CREATE INDEX [IX_Faculties_TenantId] ON [Faculties] ([TenantId]);

CREATE UNIQUE INDEX [IX_FiscalYears_FiscalYearCode] ON [FiscalYears] ([FiscalYearCode]) WHERE [FiscalYearCode] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Genders_GenderName] ON [Genders] ([GenderName]) WHERE [GenderName] IS NOT NULL;

CREATE INDEX [IX_GradeDefinitions_GradingSchemeId] ON [GradeDefinitions] ([GradingSchemeId]);

CREATE INDEX [IX_GradingSchemes_AcademicYearId] ON [GradingSchemes] ([AcademicYearId]);

CREATE INDEX [IX_GradingSchemes_ProgramId] ON [GradingSchemes] ([ProgramId]);

CREATE INDEX [IX_HallTickets_ExamRegistrationId] ON [HallTickets] ([ExamRegistrationId]);

CREATE INDEX [IX_HallTickets_ExamScheduleId] ON [HallTickets] ([ExamScheduleId]);

CREATE INDEX [IX_HallTickets_StudentRegistrationId] ON [HallTickets] ([StudentRegistrationId]);

CREATE INDEX [IX_HallTickets_TenantId] ON [HallTickets] ([TenantId]);

CREATE UNIQUE INDEX [IX_IndexGroups_IndexGroupName] ON [IndexGroups] ([IndexGroupName]) WHERE [IndexGroupName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Levels_LevelCode] ON [Levels] ([LevelCode]) WHERE [LevelCode] IS NOT NULL;

CREATE INDEX [IX_LocalLevels_DistrictId] ON [LocalLevels] ([DistrictId]);

CREATE INDEX [IX_Notices_TenantId] ON [Notices] ([TenantId]);

CREATE INDEX [IX_PaymentPracticalSubjects_PaymentRequestLogId] ON [PaymentPracticalSubjects] ([PaymentRequestLogId]);

CREATE INDEX [IX_PaymentPracticalSubjects_TenantId] ON [PaymentPracticalSubjects] ([TenantId]);

CREATE INDEX [IX_PaymentRequestLogs_CollegeId] ON [PaymentRequestLogs] ([CollegeId]);

CREATE INDEX [IX_PaymentRequestLogs_ExamScheduleId] ON [PaymentRequestLogs] ([ExamScheduleId]);

CREATE INDEX [IX_PaymentRequestLogs_PaymentTypeId] ON [PaymentRequestLogs] ([PaymentTypeId]);

CREATE INDEX [IX_PaymentRequestLogs_StudentRegistrationId] ON [PaymentRequestLogs] ([StudentRegistrationId]);

CREATE INDEX [IX_PaymentRequestLogs_TenantId] ON [PaymentRequestLogs] ([TenantId]);

CREATE INDEX [IX_PaymentResponseLogs_PaymentRequestLogId] ON [PaymentResponseLogs] ([PaymentRequestLogId]);

CREATE INDEX [IX_PaymentResponseLogs_TenantId] ON [PaymentResponseLogs] ([TenantId]);

CREATE UNIQUE INDEX [IX_PaymentType_PaymentTypeName] ON [PaymentType] ([PaymentTypeName]) WHERE [PaymentTypeName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_PeriodTypes_PeriodTypeName] ON [PeriodTypes] ([PeriodTypeName]) WHERE [PeriodTypeName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Permissions_Name] ON [Permissions] ([Name]);

CREATE INDEX [IX_PreviousLevels_LevelId] ON [PreviousLevels] ([LevelId]);

CREATE INDEX [IX_Programs_BoardId] ON [Programs] ([BoardId]);

CREATE INDEX [IX_Programs_DepartmentId] ON [Programs] ([DepartmentId]);

CREATE INDEX [IX_Programs_LevelId] ON [Programs] ([LevelId]);

CREATE UNIQUE INDEX [IX_Programs_ProgramCode] ON [Programs] ([ProgramCode]) WHERE [ProgramCode] IS NOT NULL;

CREATE INDEX [IX_ProgramSubjectPracticalCharge_ProgramsId] ON [ProgramSubjectPracticalCharge] ([ProgramsId]);

CREATE INDEX [IX_ProgramSubjectPracticalCharge_TenantId] ON [ProgramSubjectPracticalCharge] ([TenantId]);

CREATE UNIQUE INDEX [IX_Provinces_ProvinceCode] ON [Provinces] ([ProvinceCode]) WHERE [ProvinceCode] IS NOT NULL;

CREATE INDEX [IX_QuestionSets_TenantId] ON [QuestionSets] ([TenantId]);

CREATE INDEX [IX_ResultRecords_AcademicYearId] ON [ResultRecords] ([AcademicYearId]);

CREATE INDEX [IX_ResultRecords_CollegeId] ON [ResultRecords] ([CollegeId]);

CREATE INDEX [IX_ResultRecords_ExamScheduleId] ON [ResultRecords] ([ExamScheduleId]);

CREATE INDEX [IX_ResultRecords_ExamTypeId] ON [ResultRecords] ([ExamTypeId]);

CREATE INDEX [IX_ResultRecords_ProgramsId] ON [ResultRecords] ([ProgramsId]);

CREATE INDEX [IX_ResultRecords_TenantId] ON [ResultRecords] ([TenantId]);

CREATE INDEX [IX_RetotalRequests_ExamRegistrationId] ON [RetotalRequests] ([ExamRegistrationId]);

CREATE INDEX [IX_RetotalRequests_ExamSubjectResultId] ON [RetotalRequests] ([ExamSubjectResultId]);

CREATE INDEX [IX_RetotalRequests_StudentRegistrationId] ON [RetotalRequests] ([StudentRegistrationId]);

CREATE INDEX [IX_RetotalRequests_TenantId] ON [RetotalRequests] ([TenantId]);

CREATE INDEX [IX_RoleClaims_RoleId] ON [RoleClaims] ([RoleId]);

CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_SchoolTypes_PreviousLevelId] ON [SchoolTypes] ([PreviousLevelId]);

CREATE UNIQUE INDEX [IX_SchoolTypes_SchoolTypeName] ON [SchoolTypes] ([SchoolTypeName]);

CREATE INDEX [IX_SemesterEnrollments_SemesterId] ON [SemesterEnrollments] ([SemesterId]);

CREATE INDEX [IX_SemesterEnrollments_StudentAdmissionId] ON [SemesterEnrollments] ([StudentAdmissionId]);

CREATE INDEX [IX_SemesterEnrollments_TenantId] ON [SemesterEnrollments] ([TenantId]);

CREATE INDEX [IX_Semesters_AcademicYearId] ON [Semesters] ([AcademicYearId]);

CREATE UNIQUE INDEX [IX_Semesters_Code] ON [Semesters] ([Code]) WHERE [Code] IS NOT NULL;

CREATE INDEX [IX_StudentAdmissions_AppUserId] ON [StudentAdmissions] ([AppUserId]);

CREATE INDEX [IX_StudentAdmissions_BatchId] ON [StudentAdmissions] ([BatchId]);

CREATE INDEX [IX_StudentAdmissions_CollegeId] ON [StudentAdmissions] ([CollegeId]);

CREATE INDEX [IX_StudentAdmissions_ProgramsId] ON [StudentAdmissions] ([ProgramsId]);

CREATE INDEX [IX_StudentAdmissions_StudentRegistrationId] ON [StudentAdmissions] ([StudentRegistrationId]);

CREATE INDEX [IX_StudentAdmissions_TenantId] ON [StudentAdmissions] ([TenantId]);

CREATE INDEX [IX_StudentGuardians_StudentRegistrationId] ON [StudentGuardians] ([StudentRegistrationId]);

CREATE INDEX [IX_StudentGuardians_TenantId] ON [StudentGuardians] ([TenantId]);

CREATE INDEX [IX_StudentQualifications_BoardId] ON [StudentQualifications] ([BoardId]);

CREATE INDEX [IX_StudentQualifications_PreviousLevelId] ON [StudentQualifications] ([PreviousLevelId]);

CREATE INDEX [IX_StudentQualifications_StudentRegistrationId] ON [StudentQualifications] ([StudentRegistrationId]);

CREATE INDEX [IX_StudentQualifications_TenantId] ON [StudentQualifications] ([TenantId]);

CREATE INDEX [IX_StudentRegistrations_AcademicYearId] ON [StudentRegistrations] ([AcademicYearId]);

CREATE INDEX [IX_StudentRegistrations_CollegeId] ON [StudentRegistrations] ([CollegeId]);

CREATE INDEX [IX_StudentRegistrations_CurrentAddressId] ON [StudentRegistrations] ([CurrentAddressId]);

CREATE INDEX [IX_StudentRegistrations_DepartmentId] ON [StudentRegistrations] ([DepartmentId]);

CREATE INDEX [IX_StudentRegistrations_DistrictId] ON [StudentRegistrations] ([DistrictId]);

CREATE UNIQUE INDEX [IX_StudentRegistrations_Email] ON [StudentRegistrations] ([Email]) WHERE [Email] IS NOT NULL;

CREATE INDEX [IX_StudentRegistrations_EntryFormatId] ON [StudentRegistrations] ([EntryFormatId]);

CREATE INDEX [IX_StudentRegistrations_EthnicityId] ON [StudentRegistrations] ([EthnicityId]);

CREATE INDEX [IX_StudentRegistrations_FacultyId] ON [StudentRegistrations] ([FacultyId]);

CREATE INDEX [IX_StudentRegistrations_GenderId] ON [StudentRegistrations] ([GenderId]);

CREATE INDEX [IX_StudentRegistrations_IndexGroupId] ON [StudentRegistrations] ([IndexGroupId]);

CREATE INDEX [IX_StudentRegistrations_LevelId] ON [StudentRegistrations] ([LevelId]);

CREATE INDEX [IX_StudentRegistrations_LocalLevelId] ON [StudentRegistrations] ([LocalLevelId]);

CREATE INDEX [IX_StudentRegistrations_PermanentAddressId] ON [StudentRegistrations] ([PermanentAddressId]);

CREATE INDEX [IX_StudentRegistrations_ProgramId] ON [StudentRegistrations] ([ProgramId]);

CREATE INDEX [IX_StudentRegistrations_StudentCategoryId] ON [StudentRegistrations] ([StudentCategoryId]);

CREATE UNIQUE INDEX [IX_StudentRegistrations_TenantId_RegistrationNumber] ON [StudentRegistrations] ([TenantId], [RegistrationNumber]) WHERE [RegistrationNumber] IS NOT NULL;

CREATE UNIQUE INDEX [IX_SubjectCatalogs_SubjectCode] ON [SubjectCatalogs] ([SubjectCode]);

CREATE INDEX [IX_SubjectCatalogs_SubjectTypeId] ON [SubjectCatalogs] ([SubjectTypeId]);

CREATE INDEX [IX_SubjectOfferings_ProgramId] ON [SubjectOfferings] ([ProgramId]);

CREATE INDEX [IX_SubjectOfferings_SemesterId] ON [SubjectOfferings] ([SemesterId]);

CREATE UNIQUE INDEX [IX_SubjectOfferings_SubjectCatalogId_ProgramId] ON [SubjectOfferings] ([SubjectCatalogId], [ProgramId]);

CREATE INDEX [IX_SubjectOfferings_TenantId] ON [SubjectOfferings] ([TenantId]);

CREATE UNIQUE INDEX [IX_SubjectTypes_Code] ON [SubjectTypes] ([Code]);

CREATE INDEX [IX_TeacherSubjectAssignments_ExamScheduleId] ON [TeacherSubjectAssignments] ([ExamScheduleId]);

CREATE INDEX [IX_TeacherSubjectAssignments_SubjectOfferingId] ON [TeacherSubjectAssignments] ([SubjectOfferingId]);

CREATE INDEX [IX_TeacherSubjectAssignments_TeacherUserId] ON [TeacherSubjectAssignments] ([TeacherUserId]);

CREATE INDEX [IX_TeacherSubjectAssignments_TenantId] ON [TeacherSubjectAssignments] ([TenantId]);

CREATE UNIQUE INDEX [IX_Tenants_OfficeCode] ON [Tenants] ([OfficeCode]) WHERE [OfficeCode] IS NOT NULL;

CREATE INDEX [IX_UserClaims_UserId] ON [UserClaims] ([UserId]);

CREATE INDEX [IX_UserLogins_UserId] ON [UserLogins] ([UserId]);

CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [Users] ([NormalizedEmail]);

CREATE INDEX [IX_Users_CollegeId] ON [Users] ([CollegeId]);

CREATE INDEX [IX_Users_DepartmentId] ON [Users] ([DepartmentId]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE [Email] IS NOT NULL;

CREATE INDEX [IX_Users_FacultyId] ON [Users] ([FacultyId]);

CREATE UNIQUE INDEX [UserNameIndex] ON [Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260707132733_Initial', N'10.0.7');

COMMIT;
GO

