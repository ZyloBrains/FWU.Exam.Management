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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [Banks] (
        [Id] int NOT NULL IDENTITY,
        [BankName] nvarchar(100) NOT NULL,
        [BankCode] nvarchar(30) NULL,
        [Remarks] nvarchar(255) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Banks] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [Boards] (
        [Id] int NOT NULL IDENTITY,
        [CountryId] int NOT NULL,
        [BoardName] nvarchar(50) NOT NULL,
        [Remarks] nvarchar(255) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Boards] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [CollegeTypes] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [Remarks] nvarchar(1024) NULL,
        [IsDefault] bit NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_CollegeTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [EntryFormats] (
        [Id] int NOT NULL IDENTITY,
        [EntryFormatName] nvarchar(100) NOT NULL,
        [Remarks] nvarchar(255) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_EntryFormats] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [Ethnicities] (
        [Id] int NOT NULL IDENTITY,
        [EthnicityName] nvarchar(50) NOT NULL,
        [IsDefault] bit NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Ethnicities] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [ExamTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Remarks] nvarchar(255) NULL,
        [IsActive] bit NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_ExamTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [Genders] (
        [Id] int NOT NULL IDENTITY,
        [GenderName] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Genders] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [IndexGroups] (
        [Id] int NOT NULL IDENTITY,
        [IndexGroupName] nvarchar(100) NOT NULL,
        [Remarks] nvarchar(255) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_IndexGroups] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [NepaliDates] (
        [Id] int NOT NULL IDENTITY,
        [GregorianDate] datetime2 NULL,
        [NepaliDateShort] nvarchar(10) NULL,
        [NepaliDateFull] nvarchar(50) NULL,
        [NepaliDateString] nvarchar(50) NULL,
        CONSTRAINT [PK_NepaliDates] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [PaymentType] (
        [Id] int NOT NULL IDENTITY,
        [PaymentTypeName] nvarchar(255) NOT NULL,
        [LogoUrl] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PaymentType] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [PeriodTypes] (
        [Id] int NOT NULL IDENTITY,
        [PeriodTypeName] nvarchar(50) NOT NULL,
        [NumberOfMonths] decimal(5,2) NULL,
        [IsActive] bit NULL,
        [Remarks] nvarchar(255) NULL,
        CONSTRAINT [PK_PeriodTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(256) NOT NULL,
        [DisplayName] nvarchar(256) NULL,
        [Description] nvarchar(500) NULL,
        [Group] nvarchar(128) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [Provinces] (
        [Id] int NOT NULL IDENTITY,
        [ProvinceName] nvarchar(50) NOT NULL,
        [ProvinceCode] nvarchar(10) NULL,
        [IsActive] bit NOT NULL,
        [Remarks] nvarchar(255) NULL,
        CONSTRAINT [PK_Provinces] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [SmsConfigurations] (
        [Id] int NOT NULL IDENTITY,
        [ApiUrl] nvarchar(1024) NOT NULL,
        [ApiKey] nvarchar(2048) NOT NULL,
        [Mode] nvarchar(50) NULL,
        [Tags] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_SmsConfigurations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [StudentCategories] (
        [Id] int NOT NULL IDENTITY,
        [StudentCategoryName] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL,
        [Remarks] nvarchar(255) NULL,
        CONSTRAINT [PK_StudentCategories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [SubjectTypes] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [MaxAllowedSubjects] int NULL,
        [IsDefault] bit NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_SubjectTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [Batches] (
        [Id] int NOT NULL IDENTITY,
        [AcademicYearId] int NOT NULL,
        [BatchName] nvarchar(50) NOT NULL,
        [Remarks] nvarchar(50) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Batches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Batches_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [RoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_RoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoleClaims_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [RoleId] nvarchar(450) NOT NULL,
        [PermissionId] int NOT NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
        CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [QuestionSets] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [QuestionSetName] nvarchar(255) NOT NULL,
        [Description] nvarchar(1024) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_QuestionSets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuestionSets_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [SchoolTypes] (
        [Id] int NOT NULL IDENTITY,
        [PreviousLevelId] int NOT NULL,
        [SchoolTypeName] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_SchoolTypes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SchoolTypes_PreviousLevels_PreviousLevelId] FOREIGN KEY ([PreviousLevelId]) REFERENCES [PreviousLevels] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [LocalLevels] (
        [Id] int NOT NULL IDENTITY,
        [DistrictId] int NOT NULL,
        [LocalLevelName] nvarchar(100) NOT NULL,
        [LocalLevelType] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_LocalLevels] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LocalLevels_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [ProgramSubjectPracticalCharge] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [ProgramsId] int NOT NULL,
        [PracticalSubjectCharge] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_ProgramSubjectPracticalCharge] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProgramSubjectPracticalCharge_Programs_ProgramsId] FOREIGN KEY ([ProgramsId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProgramSubjectPracticalCharge_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [CollegeFaculty] (
        [CollegesId] int NOT NULL,
        [FacultiesId] int NOT NULL,
        CONSTRAINT [PK_CollegeFaculty] PRIMARY KEY ([CollegesId], [FacultiesId]),
        CONSTRAINT [FK_CollegeFaculty_Colleges_CollegesId] FOREIGN KEY ([CollegesId]) REFERENCES [Colleges] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CollegeFaculty_Faculties_FacultiesId] FOREIGN KEY ([FacultiesId]) REFERENCES [Faculties] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
        [CollegeApprovalDate] datetime2 NULL,
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [UserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_UserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [UserLogins] (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_UserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_UserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE TABLE [UserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_UserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AcademicYears_AcademicYearCode] ON [AcademicYears] ([AcademicYearCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Addresses_LocalLevelId] ON [Addresses] ([LocalLevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ApplicationVouchers_ExamScheduleId] ON [ApplicationVouchers] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ApplicationVouchers_StudentRegistrationId] ON [ApplicationVouchers] ([StudentRegistrationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ApplicationVouchers_TenantId] ON [ApplicationVouchers] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Banks_BankCode] ON [Banks] ([BankCode]) WHERE [BankCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_BankVoucher_AcademicYearId] ON [BankVoucher] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_BankVoucher_BankId] ON [BankVoucher] ([BankId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_BankVoucher_BankVoucherUserAttachmentId] ON [BankVoucher] ([BankVoucherUserAttachmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_BankVoucher_BillTitleId] ON [BankVoucher] ([BillTitleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_BankVoucher_CollegeId] ON [BankVoucher] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_BankVoucher_TenantId] ON [BankVoucher] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Batches_AcademicYearId] ON [Batches] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_BillTitle_ExamScheduleId] ON [BillTitle] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_BillTitle_ProgramsId] ON [BillTitle] ([ProgramsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_BillTitle_TenantId] ON [BillTitle] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Boards_BoardName] ON [Boards] ([BoardName]) WHERE [BoardName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_CollegeFaculty_FacultiesId] ON [CollegeFaculty] ([FacultiesId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_CollegeProfiles_AuditReportUserAttachmentId] ON [CollegeProfiles] ([AuditReportUserAttachmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_CollegeProfiles_BlankChequeUserAttachmentId] ON [CollegeProfiles] ([BlankChequeUserAttachmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CollegeProfiles_CollegeId] ON [CollegeProfiles] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_CollegeProfiles_TenantId] ON [CollegeProfiles] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_CollegePrograms_CollegeId] ON [CollegePrograms] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_CollegePrograms_ProgramId] ON [CollegePrograms] ([ProgramId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_CollegePrograms_TenantId] ON [CollegePrograms] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Colleges_AddressId] ON [Colleges] ([AddressId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Colleges_CollegeTypeId] ON [Colleges] ([CollegeTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Colleges_DistrictId] ON [Colleges] ([DistrictId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Colleges_QuestionSetId] ON [Colleges] ([QuestionSetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Colleges_TenantId_Code] ON [Colleges] ([TenantId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CollegeTypes_Code] ON [CollegeTypes] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_CurriculumVersions_EffectiveAcademicYearId] ON [CurriculumVersions] ([EffectiveAcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_CurriculumVersions_ProgramId] ON [CurriculumVersions] ([ProgramId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_CurriculumVersions_TenantId] ON [CurriculumVersions] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Departments_DepartmentCode] ON [Departments] ([DepartmentCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Departments_FacultyId] ON [Departments] ([FacultyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Districts_DistrictCode] ON [Districts] ([DistrictCode]) WHERE [DistrictCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Districts_ProvinceId] ON [Districts] ([ProvinceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_EntranceExamApplications_AcademicYearId] ON [EntranceExamApplications] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_EntranceExamApplications_ApplicationVoucherId] ON [EntranceExamApplications] ([ApplicationVoucherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_EntranceExamApplications_CitizenshipDistrictId] ON [EntranceExamApplications] ([CitizenshipDistrictId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_EntranceExamApplications_CollegeId] ON [EntranceExamApplications] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_EntranceExamApplications_GenderId] ON [EntranceExamApplications] ([GenderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_EntranceExamApplications_PermanentAddressId] ON [EntranceExamApplications] ([PermanentAddressId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_EntranceExamApplications_PreviousLevel2Id] ON [EntranceExamApplications] ([PreviousLevel2Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_EntranceExamApplications_PreviousLevel3Id] ON [EntranceExamApplications] ([PreviousLevel3Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_EntranceExamApplications_PreviousLevelId] ON [EntranceExamApplications] ([PreviousLevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_EntranceExamApplications_ProgramId] ON [EntranceExamApplications] ([ProgramId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_EntranceExamApplications_TenantId] ON [EntranceExamApplications] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_EntryFormats_EntryFormatName] ON [EntryFormats] ([EntryFormatName]) WHERE [EntryFormatName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Ethnicities_EthnicityName] ON [Ethnicities] ([EthnicityName]) WHERE [EthnicityName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenterColleges_CollegeId] ON [ExamCenterColleges] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenterColleges_ExamCenterId] ON [ExamCenterColleges] ([ExamCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenterColleges_TenantId] ON [ExamCenterColleges] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenters_CollegeId] ON [ExamCenters] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenters_ExamScheduleId] ON [ExamCenters] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenters_TenantId] ON [ExamCenters] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenterSymbolRanges_ExamCenterId] ON [ExamCenterSymbolRanges] ([ExamCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenterSymbolRanges_ExamScheduleId] ON [ExamCenterSymbolRanges] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenterSymbolRanges_TenantId] ON [ExamCenterSymbolRanges] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenterVenues_CollegeId] ON [ExamCenterVenues] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenterVenues_ExamCenterId] ON [ExamCenterVenues] ([ExamCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamCenterVenues_TenantId] ON [ExamCenterVenues] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamFees_CollegeTypeId] ON [ExamFees] ([CollegeTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamFees_ExamScheduleId] ON [ExamFees] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamFees_ExamTypeId] ON [ExamFees] ([ExamTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamFees_TenantId] ON [ExamFees] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamRegistrations_AcademicYearId] ON [ExamRegistrations] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamRegistrations_ApplicationVoucherId] ON [ExamRegistrations] ([ApplicationVoucherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamRegistrations_CollegeId] ON [ExamRegistrations] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamRegistrations_ExamCenterId] ON [ExamRegistrations] ([ExamCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamRegistrations_ExamScheduleId] ON [ExamRegistrations] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamRegistrations_ProgramsId] ON [ExamRegistrations] ([ProgramsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamRegistrations_SemesterEnrollmentId] ON [ExamRegistrations] ([SemesterEnrollmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamRegistrations_TenantId] ON [ExamRegistrations] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamRollNumberSetup_ExamScheduleId] ON [ExamRollNumberSetup] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamRollNumberSetup_TenantId] ON [ExamRollNumberSetup] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSchedules_AcademicYearId] ON [ExamSchedules] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSchedules_CollegeId] ON [ExamSchedules] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSchedules_ExamTypeId] ON [ExamSchedules] ([ExamTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSchedules_LevelId] ON [ExamSchedules] ([LevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSchedules_ProgramId] ON [ExamSchedules] ([ProgramId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSchedules_SemesterId] ON [ExamSchedules] ([SemesterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ExamSchedules_TenantId_ExamScheduleCode] ON [ExamSchedules] ([TenantId], [ExamScheduleCode]) WHERE [ExamScheduleCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSlots_BatchId] ON [ExamSlots] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSlots_ExamCenterId] ON [ExamSlots] ([ExamCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSlots_ExamScheduleId] ON [ExamSlots] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSlots_SubjectOfferingId] ON [ExamSlots] ([SubjectOfferingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSlots_TenantId] ON [ExamSlots] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSubjectResults_ExamRegistrationId] ON [ExamSubjectResults] ([ExamRegistrationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSubjectResults_ExamScheduleId] ON [ExamSubjectResults] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSubjectResults_ExamTypeId] ON [ExamSubjectResults] ([ExamTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSubjectResults_SubjectOfferingId] ON [ExamSubjectResults] ([SubjectOfferingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ExamSubjectResults_TenantId] ON [ExamSubjectResults] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ExamTypes_Name] ON [ExamTypes] ([Name]) WHERE [Name] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Faculties_OfficeCode] ON [Faculties] ([OfficeCode]) WHERE [OfficeCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Faculties_TenantId] ON [Faculties] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_FiscalYears_FiscalYearCode] ON [FiscalYears] ([FiscalYearCode]) WHERE [FiscalYearCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Genders_GenderName] ON [Genders] ([GenderName]) WHERE [GenderName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_GradeDefinitions_GradingSchemeId] ON [GradeDefinitions] ([GradingSchemeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_GradingSchemes_AcademicYearId] ON [GradingSchemes] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_GradingSchemes_ProgramId] ON [GradingSchemes] ([ProgramId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_HallTickets_ExamRegistrationId] ON [HallTickets] ([ExamRegistrationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_HallTickets_ExamScheduleId] ON [HallTickets] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_HallTickets_StudentRegistrationId] ON [HallTickets] ([StudentRegistrationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_HallTickets_TenantId] ON [HallTickets] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_IndexGroups_IndexGroupName] ON [IndexGroups] ([IndexGroupName]) WHERE [IndexGroupName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Levels_LevelCode] ON [Levels] ([LevelCode]) WHERE [LevelCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_LocalLevels_DistrictId] ON [LocalLevels] ([DistrictId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Notices_TenantId] ON [Notices] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_PaymentPracticalSubjects_PaymentRequestLogId] ON [PaymentPracticalSubjects] ([PaymentRequestLogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_PaymentPracticalSubjects_TenantId] ON [PaymentPracticalSubjects] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_PaymentRequestLogs_CollegeId] ON [PaymentRequestLogs] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_PaymentRequestLogs_ExamScheduleId] ON [PaymentRequestLogs] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_PaymentRequestLogs_PaymentTypeId] ON [PaymentRequestLogs] ([PaymentTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_PaymentRequestLogs_StudentRegistrationId] ON [PaymentRequestLogs] ([StudentRegistrationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_PaymentRequestLogs_TenantId] ON [PaymentRequestLogs] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_PaymentResponseLogs_PaymentRequestLogId] ON [PaymentResponseLogs] ([PaymentRequestLogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_PaymentResponseLogs_TenantId] ON [PaymentResponseLogs] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PaymentType_PaymentTypeName] ON [PaymentType] ([PaymentTypeName]) WHERE [PaymentTypeName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PeriodTypes_PeriodTypeName] ON [PeriodTypes] ([PeriodTypeName]) WHERE [PeriodTypeName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_Name] ON [Permissions] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_PreviousLevels_LevelId] ON [PreviousLevels] ([LevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Programs_BoardId] ON [Programs] ([BoardId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Programs_DepartmentId] ON [Programs] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Programs_LevelId] ON [Programs] ([LevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Programs_ProgramCode] ON [Programs] ([ProgramCode]) WHERE [ProgramCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ProgramSubjectPracticalCharge_ProgramsId] ON [ProgramSubjectPracticalCharge] ([ProgramsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ProgramSubjectPracticalCharge_TenantId] ON [ProgramSubjectPracticalCharge] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Provinces_ProvinceCode] ON [Provinces] ([ProvinceCode]) WHERE [ProvinceCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_QuestionSets_TenantId] ON [QuestionSets] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ResultRecords_AcademicYearId] ON [ResultRecords] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ResultRecords_CollegeId] ON [ResultRecords] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ResultRecords_ExamScheduleId] ON [ResultRecords] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ResultRecords_ExamTypeId] ON [ResultRecords] ([ExamTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ResultRecords_ProgramsId] ON [ResultRecords] ([ProgramsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_ResultRecords_TenantId] ON [ResultRecords] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_RetotalRequests_ExamRegistrationId] ON [RetotalRequests] ([ExamRegistrationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_RetotalRequests_ExamSubjectResultId] ON [RetotalRequests] ([ExamSubjectResultId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_RetotalRequests_StudentRegistrationId] ON [RetotalRequests] ([StudentRegistrationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_RetotalRequests_TenantId] ON [RetotalRequests] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_RoleClaims_RoleId] ON [RoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_SchoolTypes_PreviousLevelId] ON [SchoolTypes] ([PreviousLevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SchoolTypes_SchoolTypeName] ON [SchoolTypes] ([SchoolTypeName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_SemesterEnrollments_SemesterId] ON [SemesterEnrollments] ([SemesterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_SemesterEnrollments_StudentAdmissionId] ON [SemesterEnrollments] ([StudentAdmissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_SemesterEnrollments_TenantId] ON [SemesterEnrollments] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Semesters_AcademicYearId] ON [Semesters] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Semesters_Code] ON [Semesters] ([Code]) WHERE [Code] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentAdmissions_AppUserId] ON [StudentAdmissions] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentAdmissions_BatchId] ON [StudentAdmissions] ([BatchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentAdmissions_CollegeId] ON [StudentAdmissions] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentAdmissions_ProgramsId] ON [StudentAdmissions] ([ProgramsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentAdmissions_StudentRegistrationId] ON [StudentAdmissions] ([StudentRegistrationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentAdmissions_TenantId] ON [StudentAdmissions] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentGuardians_StudentRegistrationId] ON [StudentGuardians] ([StudentRegistrationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentGuardians_TenantId] ON [StudentGuardians] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentQualifications_BoardId] ON [StudentQualifications] ([BoardId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentQualifications_PreviousLevelId] ON [StudentQualifications] ([PreviousLevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentQualifications_StudentRegistrationId] ON [StudentQualifications] ([StudentRegistrationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentQualifications_TenantId] ON [StudentQualifications] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_AcademicYearId] ON [StudentRegistrations] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_CollegeId] ON [StudentRegistrations] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_CurrentAddressId] ON [StudentRegistrations] ([CurrentAddressId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_DepartmentId] ON [StudentRegistrations] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_DistrictId] ON [StudentRegistrations] ([DistrictId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_StudentRegistrations_Email] ON [StudentRegistrations] ([Email]) WHERE [Email] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_EntryFormatId] ON [StudentRegistrations] ([EntryFormatId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_EthnicityId] ON [StudentRegistrations] ([EthnicityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_FacultyId] ON [StudentRegistrations] ([FacultyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_GenderId] ON [StudentRegistrations] ([GenderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_IndexGroupId] ON [StudentRegistrations] ([IndexGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_LevelId] ON [StudentRegistrations] ([LevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_LocalLevelId] ON [StudentRegistrations] ([LocalLevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_PermanentAddressId] ON [StudentRegistrations] ([PermanentAddressId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_ProgramId] ON [StudentRegistrations] ([ProgramId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_StudentCategoryId] ON [StudentRegistrations] ([StudentCategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_StudentRegistrations_TenantId_RegistrationNumber] ON [StudentRegistrations] ([TenantId], [RegistrationNumber]) WHERE [RegistrationNumber] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SubjectCatalogs_SubjectCode] ON [SubjectCatalogs] ([SubjectCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_SubjectCatalogs_SubjectTypeId] ON [SubjectCatalogs] ([SubjectTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_SubjectOfferings_ProgramId] ON [SubjectOfferings] ([ProgramId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_SubjectOfferings_SemesterId] ON [SubjectOfferings] ([SemesterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SubjectOfferings_SubjectCatalogId_ProgramId] ON [SubjectOfferings] ([SubjectCatalogId], [ProgramId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_SubjectOfferings_TenantId] ON [SubjectOfferings] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SubjectTypes_Code] ON [SubjectTypes] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_TeacherSubjectAssignments_ExamScheduleId] ON [TeacherSubjectAssignments] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_TeacherSubjectAssignments_SubjectOfferingId] ON [TeacherSubjectAssignments] ([SubjectOfferingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_TeacherSubjectAssignments_TeacherUserId] ON [TeacherSubjectAssignments] ([TeacherUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_TeacherSubjectAssignments_TenantId] ON [TeacherSubjectAssignments] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Tenants_OfficeCode] ON [Tenants] ([OfficeCode]) WHERE [OfficeCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_UserClaims_UserId] ON [UserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_UserLogins_UserId] ON [UserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [Users] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Users_CollegeId] ON [Users] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Users_DepartmentId] ON [Users] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE [Email] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    CREATE INDEX [IX_Users_FacultyId] ON [Users] ([FacultyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707132733_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707132733_Initial', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707151511_AddIsActiveToSmtpConfiguration'
)
BEGIN
    ALTER TABLE [SmtpConfigurations] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707151511_AddIsActiveToSmtpConfiguration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707151511_AddIsActiveToSmtpConfiguration', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707163014_RemoveDepartmentEntity'
)
BEGIN
    ALTER TABLE [Programs] DROP CONSTRAINT [FK_Programs_Departments_DepartmentId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707163014_RemoveDepartmentEntity'
)
BEGIN
    ALTER TABLE [StudentRegistrations] DROP CONSTRAINT [FK_StudentRegistrations_Departments_DepartmentId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707163014_RemoveDepartmentEntity'
)
BEGIN
    ALTER TABLE [Users] DROP CONSTRAINT [FK_Users_Departments_DepartmentId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707163014_RemoveDepartmentEntity'
)
BEGIN
    DROP TABLE [Departments];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707163014_RemoveDepartmentEntity'
)
BEGIN
    DROP INDEX [IX_Users_DepartmentId] ON [Users];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707163014_RemoveDepartmentEntity'
)
BEGIN
    DROP INDEX [IX_StudentRegistrations_DepartmentId] ON [StudentRegistrations];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707163014_RemoveDepartmentEntity'
)
BEGIN
    DROP INDEX [IX_Programs_DepartmentId] ON [Programs];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707163014_RemoveDepartmentEntity'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'DepartmentId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Users] DROP COLUMN [DepartmentId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707163014_RemoveDepartmentEntity'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StudentRegistrations]') AND [c].[name] = N'DepartmentId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [StudentRegistrations] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [StudentRegistrations] DROP COLUMN [DepartmentId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707163014_RemoveDepartmentEntity'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Programs]') AND [c].[name] = N'DepartmentId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Programs] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [Programs] DROP COLUMN [DepartmentId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707163014_RemoveDepartmentEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707163014_RemoveDepartmentEntity', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708071042_AddStudentRegistrationIndex'
)
BEGIN
    ALTER TABLE [StudentRegistrations] ADD [StudentRegistrationIndex] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260708071042_AddStudentRegistrationIndex'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260708071042_AddStudentRegistrationIndex', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712101721_MakeEstablishedDateNullable'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Colleges]') AND [c].[name] = N'EstablishedDate');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Colleges] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [Colleges] ALTER COLUMN [EstablishedDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260712101721_MakeEstablishedDateNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260712101721_MakeEstablishedDateNullable', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713180659_AddFacultyIdToSemester'
)
BEGIN
    DROP INDEX [IX_Semesters_Code] ON [Semesters];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713180659_AddFacultyIdToSemester'
)
BEGIN
    ALTER TABLE [Semesters] ADD [FacultyId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713180659_AddFacultyIdToSemester'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Semesters_FacultyId_Code] ON [Semesters] ([FacultyId], [Code]) WHERE [Code] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713180659_AddFacultyIdToSemester'
)
BEGIN
    ALTER TABLE [Semesters] ADD CONSTRAINT [FK_Semesters_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713180659_AddFacultyIdToSemester'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713180659_AddFacultyIdToSemester', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714044339_AddFacultyIdToProgram'
)
BEGIN
    ALTER TABLE [Programs] ADD [FacultyId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714044339_AddFacultyIdToProgram'
)
BEGIN
    CREATE INDEX [IX_Programs_FacultyId] ON [Programs] ([FacultyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714044339_AddFacultyIdToProgram'
)
BEGIN
    ALTER TABLE [Programs] ADD CONSTRAINT [FK_Programs_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714044339_AddFacultyIdToProgram'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260714044339_AddFacultyIdToProgram', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714081401_PendingModelChanges'
)
BEGIN
    DROP TABLE [TeacherSubjectAssignments];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714081401_PendingModelChanges'
)
BEGIN
    CREATE TABLE [CollegeAdminSubjectAssignments] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [CollegeAdminUserId] nvarchar(450) NOT NULL,
        [SubjectOfferingId] int NOT NULL,
        [ExamScheduleId] int NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_CollegeAdminSubjectAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CollegeAdminSubjectAssignments_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CollegeAdminSubjectAssignments_SubjectOfferings_SubjectOfferingId] FOREIGN KEY ([SubjectOfferingId]) REFERENCES [SubjectOfferings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CollegeAdminSubjectAssignments_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CollegeAdminSubjectAssignments_Users_CollegeAdminUserId] FOREIGN KEY ([CollegeAdminUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714081401_PendingModelChanges'
)
BEGIN
    CREATE INDEX [IX_CollegeAdminSubjectAssignments_CollegeAdminUserId] ON [CollegeAdminSubjectAssignments] ([CollegeAdminUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714081401_PendingModelChanges'
)
BEGIN
    CREATE INDEX [IX_CollegeAdminSubjectAssignments_ExamScheduleId] ON [CollegeAdminSubjectAssignments] ([ExamScheduleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714081401_PendingModelChanges'
)
BEGIN
    CREATE INDEX [IX_CollegeAdminSubjectAssignments_SubjectOfferingId] ON [CollegeAdminSubjectAssignments] ([SubjectOfferingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714081401_PendingModelChanges'
)
BEGIN
    CREATE INDEX [IX_CollegeAdminSubjectAssignments_TenantId] ON [CollegeAdminSubjectAssignments] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714081401_PendingModelChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260714081401_PendingModelChanges', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715155629_AddAdmitCardDisplayProperties'
)
BEGIN
    ALTER TABLE [HallTickets] ADD [Campus] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715155629_AddAdmitCardDisplayProperties'
)
BEGIN
    ALTER TABLE [HallTickets] ADD [ExamRollNo] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715155629_AddAdmitCardDisplayProperties'
)
BEGIN
    ALTER TABLE [HallTickets] ADD [ExamType] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715155629_AddAdmitCardDisplayProperties'
)
BEGIN
    ALTER TABLE [HallTickets] ADD [Level] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715155629_AddAdmitCardDisplayProperties'
)
BEGIN
    ALTER TABLE [HallTickets] ADD [Program] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715155629_AddAdmitCardDisplayProperties'
)
BEGIN
    ALTER TABLE [HallTickets] ADD [RegistrationNumber] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715155629_AddAdmitCardDisplayProperties'
)
BEGIN
    ALTER TABLE [HallTickets] ADD [Semester] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715155629_AddAdmitCardDisplayProperties'
)
BEGIN
    ALTER TABLE [HallTickets] ADD [Year] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715155629_AddAdmitCardDisplayProperties'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260715155629_AddAdmitCardDisplayProperties', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715164926_AddAdmitCardPhotoSignatureFields'
)
BEGIN
    ALTER TABLE [HallTickets] ADD [ControllerSignaturePath] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715164926_AddAdmitCardPhotoSignatureFields'
)
BEGIN
    ALTER TABLE [HallTickets] ADD [PhotoPath] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715164926_AddAdmitCardPhotoSignatureFields'
)
BEGIN
    ALTER TABLE [HallTickets] ADD [SignaturePath] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715164926_AddAdmitCardPhotoSignatureFields'
)
BEGIN
    ALTER TABLE [Faculties] ADD [ControllerSignaturePath] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715164926_AddAdmitCardPhotoSignatureFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260715164926_AddAdmitCardPhotoSignatureFields', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715171056_MoveControllerSignatureToTenant'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faculties]') AND [c].[name] = N'ControllerSignaturePath');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Faculties] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [Faculties] DROP COLUMN [ControllerSignaturePath];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715171056_MoveControllerSignatureToTenant'
)
BEGIN
    ALTER TABLE [Tenants] ADD [ControllerSignaturePath] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715171056_MoveControllerSignatureToTenant'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260715171056_MoveControllerSignatureToTenant', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717063019_AddAuditLogTable'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [EntityName] nvarchar(128) NULL,
        [EntityId] nvarchar(128) NULL,
        [Action] nvarchar(32) NULL,
        [UserName] nvarchar(256) NULL,
        [UserId] nvarchar(128) NULL,
        [Timestamp] datetime2 NOT NULL,
        [ChangesJson] nvarchar(4000) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuditLogs_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717063019_AddAuditLogTable'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityName_EntityId] ON [AuditLogs] ([EntityName], [EntityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717063019_AddAuditLogTable'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_TenantId] ON [AuditLogs] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717063019_AddAuditLogTable'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_Timestamp] ON [AuditLogs] ([Timestamp]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717063019_AddAuditLogTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717063019_AddAuditLogTable', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718173521_FixSubjectOfferingUniqueIndex'
)
BEGIN
    DROP INDEX [IX_SubjectOfferings_SubjectCatalogId_ProgramId] ON [SubjectOfferings];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718173521_FixSubjectOfferingUniqueIndex'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SubjectOfferings_SubjectCatalogId_ProgramId_SemesterId] ON [SubjectOfferings] ([SubjectCatalogId], [ProgramId], [SemesterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260718173521_FixSubjectOfferingUniqueIndex'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260718173521_FixSubjectOfferingUniqueIndex', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719181056_AddCountryEntity'
)
BEGIN
    CREATE TABLE [Countries] (
        [Id] int NOT NULL IDENTITY,
        [CountryName] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Countries] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719181056_AddCountryEntity'
)
BEGIN
    CREATE INDEX [IX_Boards_CountryId] ON [Boards] ([CountryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719181056_AddCountryEntity'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Countries_CountryName] ON [Countries] ([CountryName]) WHERE [CountryName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719181056_AddCountryEntity'
)
BEGIN
    ALTER TABLE [Boards] ADD CONSTRAINT [FK_Boards_Countries_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260719181056_AddCountryEntity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260719181056_AddCountryEntity', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722161051_AddShortNameToFaculty'
)
BEGIN
    ALTER TABLE [Faculties] ADD [ShortName] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722161051_AddShortNameToFaculty'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260722161051_AddShortNameToFaculty', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726003934_MakeAuditLogTenantIdNullable'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'TenantId');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT ' + @var5 + ';');
    ALTER TABLE [AuditLogs] ALTER COLUMN [TenantId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726003934_MakeAuditLogTenantIdNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260726003934_MakeAuditLogTenantIdNullable', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726170804_AddGumpNowEmailConfiguration'
)
BEGIN
    CREATE TABLE [GumpNowEmailConfigurations] (
        [Id] int NOT NULL IDENTITY,
        [ApiUrl] nvarchar(1024) NOT NULL,
        [ApiKey] nvarchar(2048) NOT NULL,
        [FromAddr] nvarchar(500) NOT NULL,
        [Mode] nvarchar(50) NULL,
        [OverrideUnsubscription] bit NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_GumpNowEmailConfigurations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726170804_AddGumpNowEmailConfiguration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260726170804_AddGumpNowEmailConfiguration', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727030624_AddBulkUserCreationJobTable'
)
BEGIN
    CREATE TABLE [BulkUserCreationJobs] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NULL,
        [UserId] nvarchar(128) NOT NULL,
        [TotalStudents] int NOT NULL,
        [ProcessedCount] int NOT NULL,
        [SuccessCount] int NOT NULL,
        [FailedCount] int NOT NULL,
        [Status] nvarchar(32) NOT NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CompletedAt] datetime2 NULL,
        CONSTRAINT [PK_BulkUserCreationJobs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BulkUserCreationJobs_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727030624_AddBulkUserCreationJobTable'
)
BEGIN
    CREATE INDEX [IX_BulkUserCreationJobs_CreatedAt] ON [BulkUserCreationJobs] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727030624_AddBulkUserCreationJobTable'
)
BEGIN
    CREATE INDEX [IX_BulkUserCreationJobs_Status] ON [BulkUserCreationJobs] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727030624_AddBulkUserCreationJobTable'
)
BEGIN
    CREATE INDEX [IX_BulkUserCreationJobs_TenantId] ON [BulkUserCreationJobs] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727030624_AddBulkUserCreationJobTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727030624_AddBulkUserCreationJobTable', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727102323_AddStudentAdmissionFields'
)
BEGIN
    ALTER TABLE [StudentAdmissions] DROP CONSTRAINT [FK_StudentAdmissions_StudentRegistrations_StudentRegistrationId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727102323_AddStudentAdmissionFields'
)
BEGIN
    DROP INDEX [IX_StudentAdmissions_StudentRegistrationId] ON [StudentAdmissions];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727102323_AddStudentAdmissionFields'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StudentAdmissions]') AND [c].[name] = N'StudentRegistrationId');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [StudentAdmissions] DROP CONSTRAINT ' + @var6 + ';');
    ALTER TABLE [StudentAdmissions] DROP COLUMN [StudentRegistrationId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727102323_AddStudentAdmissionFields'
)
BEGIN
    ALTER TABLE [StudentRegistrations] ADD [StudentAdmissionId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727102323_AddStudentAdmissionFields'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD [AcademicYearId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727102323_AddStudentAdmissionFields'
)
BEGIN
    CREATE INDEX [IX_StudentRegistrations_StudentAdmissionId] ON [StudentRegistrations] ([StudentAdmissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727102323_AddStudentAdmissionFields'
)
BEGIN
    CREATE INDEX [IX_StudentAdmissions_AcademicYearId] ON [StudentAdmissions] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727102323_AddStudentAdmissionFields'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD CONSTRAINT [FK_StudentAdmissions_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727102323_AddStudentAdmissionFields'
)
BEGIN
    ALTER TABLE [StudentRegistrations] ADD CONSTRAINT [FK_StudentRegistrations_StudentAdmissions_StudentAdmissionId] FOREIGN KEY ([StudentAdmissionId]) REFERENCES [StudentAdmissions] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727102323_AddStudentAdmissionFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727102323_AddStudentAdmissionFields', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728084104_AddITenantScopedToFaculty'
)
BEGIN
    ALTER TABLE [Faculties] DROP CONSTRAINT [FK_Faculties_Tenants_TenantId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728084104_AddITenantScopedToFaculty'
)
BEGIN
    ALTER TABLE [Faculties] ADD CONSTRAINT [FK_Faculties_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728084104_AddITenantScopedToFaculty'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728084104_AddITenantScopedToFaculty', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801065623_AddProgramSemesterJoinTable'
)
BEGIN
    DROP INDEX [IX_Semesters_FacultyId_Code] ON [Semesters];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801065623_AddProgramSemesterJoinTable'
)
BEGIN
    CREATE TABLE [ProgramSemesters] (
        [Id] int NOT NULL IDENTITY,
        [ProgramId] int NOT NULL,
        [SemesterId] int NOT NULL,
        [IsActive] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        CONSTRAINT [PK_ProgramSemesters] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProgramSemesters_Programs_ProgramId] FOREIGN KEY ([ProgramId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProgramSemesters_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801065623_AddProgramSemesterJoinTable'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Semesters_FacultyId_AcademicYearId_Number] ON [Semesters] ([FacultyId], [AcademicYearId], [Number]) WHERE [AcademicYearId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801065623_AddProgramSemesterJoinTable'
)
BEGIN

                    INSERT INTO ProgramSemesters (ProgramId, SemesterId, IsActive, DisplayOrder)
                    SELECT DISTINCT so.ProgramId, so.SemesterId, 1, 0
                    FROM SubjectOfferings so
                    WHERE NOT EXISTS (
                        SELECT 1 FROM ProgramSemesters ps
                        WHERE ps.ProgramId = so.ProgramId AND ps.SemesterId = so.SemesterId
                    );
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801065623_AddProgramSemesterJoinTable'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProgramSemesters_ProgramId_SemesterId] ON [ProgramSemesters] ([ProgramId], [SemesterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801065623_AddProgramSemesterJoinTable'
)
BEGIN
    CREATE INDEX [IX_ProgramSemesters_SemesterId] ON [ProgramSemesters] ([SemesterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801065623_AddProgramSemesterJoinTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801065623_AddProgramSemesterJoinTable', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    DROP INDEX [IX_PaymentType_PaymentTypeName] ON [PaymentType];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    DROP INDEX [IX_Banks_BankCode] ON [Banks];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    ALTER TABLE [PaymentType] ADD [TenantId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    ALTER TABLE [KhaltiConfigurations] ADD [TenantId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    ALTER TABLE [ESewaConfiguration] ADD [TenantId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    ALTER TABLE [ConnectIpsPaymentConfiguration] ADD [TenantId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    ALTER TABLE [Banks] ADD [TenantId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    UPDATE [PaymentType] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]) WHERE [TenantId] IS NULL;
    UPDATE [KhaltiConfigurations] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]) WHERE [TenantId] IS NULL;
    UPDATE [ESewaConfiguration] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]) WHERE [TenantId] IS NULL;
    UPDATE [ConnectIpsPaymentConfiguration] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]) WHERE [TenantId] IS NULL;
    UPDATE [Banks] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]) WHERE [TenantId] IS NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PaymentType_TenantId_PaymentTypeName] ON [PaymentType] ([TenantId], [PaymentTypeName]) WHERE [PaymentTypeName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    CREATE INDEX [IX_KhaltiConfigurations_TenantId] ON [KhaltiConfigurations] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    CREATE INDEX [IX_ESewaConfiguration_TenantId] ON [ESewaConfiguration] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    CREATE INDEX [IX_ConnectIpsPaymentConfiguration_TenantId] ON [ConnectIpsPaymentConfiguration] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Banks_TenantId_BankCode] ON [Banks] ([TenantId], [BankCode]) WHERE [BankCode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    ALTER TABLE [Banks] ADD CONSTRAINT [FK_Banks_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    ALTER TABLE [ConnectIpsPaymentConfiguration] ADD CONSTRAINT [FK_ConnectIpsPaymentConfiguration_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    ALTER TABLE [ESewaConfiguration] ADD CONSTRAINT [FK_ESewaConfiguration_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    ALTER TABLE [KhaltiConfigurations] ADD CONSTRAINT [FK_KhaltiConfigurations_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    ALTER TABLE [PaymentType] ADD CONSTRAINT [FK_PaymentType_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801114506_AddTenantScopingToPaymentConfigs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801114506_AddTenantScopingToPaymentConfigs', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801155326_AddTenantCollegeBridge'
)
BEGIN
    CREATE TABLE [TenantColleges] (
        [TenantId] int NOT NULL,
        [CollegeId] int NOT NULL,
        CONSTRAINT [PK_TenantColleges] PRIMARY KEY ([TenantId], [CollegeId]),
        CONSTRAINT [FK_TenantColleges_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TenantColleges_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801155326_AddTenantCollegeBridge'
)
BEGIN

                    INSERT INTO TenantColleges (TenantId, CollegeId)
                    SELECT TenantId, Id
                    FROM Colleges;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801155326_AddTenantCollegeBridge'
)
BEGIN
    ALTER TABLE [Colleges] DROP CONSTRAINT [FK_Colleges_Tenants_TenantId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801155326_AddTenantCollegeBridge'
)
BEGIN
    DROP INDEX [IX_Colleges_TenantId_Code] ON [Colleges];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801155326_AddTenantCollegeBridge'
)
BEGIN
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Colleges]') AND [c].[name] = N'TenantId');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Colleges] DROP CONSTRAINT ' + @var7 + ';');
    ALTER TABLE [Colleges] DROP COLUMN [TenantId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801155326_AddTenantCollegeBridge'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Colleges_Code] ON [Colleges] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801155326_AddTenantCollegeBridge'
)
BEGIN
    CREATE INDEX [IX_TenantColleges_CollegeId] ON [TenantColleges] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801155326_AddTenantCollegeBridge'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801155326_AddTenantCollegeBridge', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801171428_MakeSemesterGlobal'
)
BEGIN
    ALTER TABLE [Semesters] DROP CONSTRAINT [FK_Semesters_Faculties_FacultyId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801171428_MakeSemesterGlobal'
)
BEGIN
    DROP INDEX [IX_Semesters_FacultyId_AcademicYearId_Number] ON [Semesters];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801171428_MakeSemesterGlobal'
)
BEGIN
    DECLARE @var8 nvarchar(max);
    SELECT @var8 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Semesters]') AND [c].[name] = N'FacultyId');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Semesters] DROP CONSTRAINT ' + @var8 + ';');
    ALTER TABLE [Semesters] DROP COLUMN [FacultyId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801171428_MakeSemesterGlobal'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Semesters_Code] ON [Semesters] ([Code]) WHERE [Code] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801171428_MakeSemesterGlobal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801171428_MakeSemesterGlobal', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801184658_MakeSubjectCatalogCodeIndexNonUnique'
)
BEGIN
    DROP INDEX [IX_SubjectCatalogs_SubjectCode] ON [SubjectCatalogs];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801184658_MakeSubjectCatalogCodeIndexNonUnique'
)
BEGIN
    CREATE INDEX [IX_SubjectCatalogs_SubjectCode] ON [SubjectCatalogs] ([SubjectCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801184658_MakeSubjectCatalogCodeIndexNonUnique'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801184658_MakeSubjectCatalogCodeIndexNonUnique', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805150605_AddBannerImageToTenant'
)
BEGIN
    ALTER TABLE [Tenants] ADD [BannerImagePath] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805150605_AddBannerImageToTenant'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260805150605_AddBannerImageToTenant', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806170703_AddGumpNowEmailLog'
)
BEGIN
    CREATE TABLE [GumpNowEmailLogs] (
        [Id] int NOT NULL IDENTITY,
        [ToAddr] nvarchar(500) NOT NULL,
        [FromAddr] nvarchar(500) NULL,
        [Subject] nvarchar(500) NULL,
        [TemplateId] nvarchar(100) NULL,
        [ContextJson] nvarchar(max) NULL,
        [Mode] nvarchar(50) NULL,
        [Status] nvarchar(50) NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [SentAt] datetime2 NOT NULL,
        CONSTRAINT [PK_GumpNowEmailLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806170703_AddGumpNowEmailLog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806170703_AddGumpNowEmailLog', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806184230_AddSmsLog'
)
BEGIN
    CREATE TABLE [SmsLogs] (
        [Id] int NOT NULL IDENTITY,
        [ToAddr] nvarchar(50) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [Mode] nvarchar(50) NULL,
        [TagsJson] nvarchar(max) NULL,
        [Status] nvarchar(50) NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [SentAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SmsLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806184230_AddSmsLog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806184230_AddSmsLog', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807090550_AddExamScheduleCollegeApproval'
)
BEGIN
    CREATE TABLE [ExamScheduleCollegeApprovals] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [ExamScheduleId] int NOT NULL,
        [CollegeId] int NOT NULL,
        [Status] int NOT NULL,
        [RequestedApprovalDate] datetime2 NULL,
        [ApprovedDate] datetime2 NULL,
        [RejectedDate] datetime2 NULL,
        [ProposedDate] datetime2 NULL,
        [Remarks] nvarchar(500) NULL,
        [ApprovedByUserId] nvarchar(450) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ExamScheduleCollegeApprovals] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExamScheduleCollegeApprovals_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ExamScheduleCollegeApprovals_ExamSchedules_ExamScheduleId] FOREIGN KEY ([ExamScheduleId]) REFERENCES [ExamSchedules] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ExamScheduleCollegeApprovals_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ExamScheduleCollegeApprovals_Users_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807090550_AddExamScheduleCollegeApproval'
)
BEGIN
    CREATE INDEX [IX_ExamScheduleCollegeApprovals_ApprovedByUserId] ON [ExamScheduleCollegeApprovals] ([ApprovedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807090550_AddExamScheduleCollegeApproval'
)
BEGIN
    CREATE INDEX [IX_ExamScheduleCollegeApprovals_CollegeId] ON [ExamScheduleCollegeApprovals] ([CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807090550_AddExamScheduleCollegeApproval'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExamScheduleCollegeApprovals_ExamScheduleId_CollegeId] ON [ExamScheduleCollegeApprovals] ([ExamScheduleId], [CollegeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807090550_AddExamScheduleCollegeApproval'
)
BEGIN
    CREATE INDEX [IX_ExamScheduleCollegeApprovals_TenantId] ON [ExamScheduleCollegeApprovals] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807090550_AddExamScheduleCollegeApproval'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807090550_AddExamScheduleCollegeApproval', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807162810_EnforceStudentAdmissionRegistrationOneToOne'
)
BEGIN
    DROP INDEX [IX_StudentRegistrations_StudentAdmissionId] ON [StudentRegistrations];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807162810_EnforceStudentAdmissionRegistrationOneToOne'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_StudentRegistrations_StudentAdmissionId] ON [StudentRegistrations] ([StudentAdmissionId]) WHERE [StudentAdmissionId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807162810_EnforceStudentAdmissionRegistrationOneToOne'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807162810_EnforceStudentAdmissionRegistrationOneToOne', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD [ContactNumber] nvarchar(15) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD [DateOfBirthAD] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD [DateOfBirthBS] nvarchar(10) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD [Email] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD [FirstName] nvarchar(80) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD [GenderId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD [LastName] nvarchar(30) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD [MiddleName] nvarchar(30) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD [NepaliName] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    ALTER TABLE [StudentAdmissions] ADD [Phone] nvarchar(15) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    UPDATE sa
    SET sa.FirstName = sr.FirstName,
        sa.MiddleName = sr.MiddleName,
        sa.LastName = sr.LastName,
        sa.NepaliName = sr.NepaliName,
        sa.DateOfBirthBS = sr.DateOfBirthBS,
        sa.DateOfBirthAD = sr.DateOfBirthAD,
        sa.GenderId = sr.GenderId,
        sa.ContactNumber = sr.ContactNumber,
        sa.Phone = sr.Phone,
        sa.Email = sr.Email
    FROM dbo.StudentAdmissions sa
    INNER JOIN dbo.StudentRegistrations sr ON sr.StudentAdmissionId = sa.Id
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807171602_AddAdmissionIdentityAndExplicitEnrollmentFk', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
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
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807202753_LinkExamSchedulesToCurriculumVersion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807202753_LinkExamSchedulesToCurriculumVersion', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809073414_ReplaceTenantCollegesWithCollegeFaculties'
)
BEGIN
    CREATE TABLE [CollegeFaculties] (
        [CollegeId] int NOT NULL,
        [FacultyId] int NOT NULL,
        [TenantId] int NOT NULL,
        CONSTRAINT [PK_CollegeFaculties] PRIMARY KEY ([CollegeId], [FacultyId]),
        CONSTRAINT [FK_CollegeFaculties_Colleges_CollegeId] FOREIGN KEY ([CollegeId]) REFERENCES [Colleges] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CollegeFaculties_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CollegeFaculties_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809073414_ReplaceTenantCollegesWithCollegeFaculties'
)
BEGIN
    CREATE INDEX [IX_CollegeFaculties_FacultyId] ON [CollegeFaculties] ([FacultyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809073414_ReplaceTenantCollegesWithCollegeFaculties'
)
BEGIN
    CREATE INDEX [IX_CollegeFaculties_TenantId] ON [CollegeFaculties] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809073414_ReplaceTenantCollegesWithCollegeFaculties'
)
BEGIN
    INSERT INTO [CollegeFaculties] ([TenantId], [CollegeId], [FacultyId])
    SELECT COALESCE(f.[TenantId], 0), cf.[CollegesId], cf.[FacultiesId]
    FROM [CollegeFaculty] cf
    INNER JOIN [Faculties] f ON f.[Id] = cf.[FacultiesId]
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809073414_ReplaceTenantCollegesWithCollegeFaculties'
)
BEGIN
    DROP TABLE [CollegeFaculty];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809073414_ReplaceTenantCollegesWithCollegeFaculties'
)
BEGIN
    DROP TABLE [TenantColleges];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809073414_ReplaceTenantCollegesWithCollegeFaculties'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260809073414_ReplaceTenantCollegesWithCollegeFaculties', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    ALTER TABLE [Colleges] DROP CONSTRAINT [FK_Colleges_Districts_DistrictId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    ALTER TABLE [Colleges] DROP CONSTRAINT [FK_Colleges_QuestionSets_QuestionSetId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    ALTER TABLE [StudentRegistrations] DROP CONSTRAINT [FK_StudentRegistrations_EntryFormats_EntryFormatId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    ALTER TABLE [StudentRegistrations] DROP CONSTRAINT [FK_StudentRegistrations_IndexGroups_IndexGroupId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP TABLE [CollegeProfiles];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP TABLE [EntryFormats];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP TABLE [FiscalYears];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP TABLE [IndexGroups];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP TABLE [PeriodTypes];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP TABLE [ProgramSubjectPracticalCharge];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP TABLE [QuestionSets];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP TABLE [SchoolTypes];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP INDEX [IX_StudentRegistrations_EntryFormatId] ON [StudentRegistrations];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP INDEX [IX_StudentRegistrations_IndexGroupId] ON [StudentRegistrations];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP INDEX [IX_Colleges_DistrictId] ON [Colleges];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DROP INDEX [IX_Colleges_QuestionSetId] ON [Colleges];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DECLARE @var9 nvarchar(max);
    SELECT @var9 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StudentRegistrations]') AND [c].[name] = N'EntryFormatId');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [StudentRegistrations] DROP CONSTRAINT ' + @var9 + ';');
    ALTER TABLE [StudentRegistrations] DROP COLUMN [EntryFormatId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DECLARE @var10 nvarchar(max);
    SELECT @var10 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StudentRegistrations]') AND [c].[name] = N'IndexGroupId');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [StudentRegistrations] DROP CONSTRAINT ' + @var10 + ';');
    ALTER TABLE [StudentRegistrations] DROP COLUMN [IndexGroupId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DECLARE @var11 nvarchar(max);
    SELECT @var11 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Colleges]') AND [c].[name] = N'CollegeProfileId');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Colleges] DROP CONSTRAINT ' + @var11 + ';');
    ALTER TABLE [Colleges] DROP COLUMN [CollegeProfileId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DECLARE @var12 nvarchar(max);
    SELECT @var12 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Colleges]') AND [c].[name] = N'DistrictId');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Colleges] DROP CONSTRAINT ' + @var12 + ';');
    ALTER TABLE [Colleges] DROP COLUMN [DistrictId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    DECLARE @var13 nvarchar(max);
    SELECT @var13 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Colleges]') AND [c].[name] = N'QuestionSetId');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Colleges] DROP CONSTRAINT ' + @var13 + ';');
    ALTER TABLE [Colleges] DROP COLUMN [QuestionSetId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809092453_RemoveUnusedTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260809092453_RemoveUnusedTables', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810043840_AddActivityLogColumns'
)
BEGIN
    DECLARE @var14 nvarchar(max);
    SELECT @var14 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'ChangesJson');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT ' + @var14 + ';');
    ALTER TABLE [AuditLogs] ALTER COLUMN [ChangesJson] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810043840_AddActivityLogColumns'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [ActivityType] nvarchar(128) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810043840_AddActivityLogColumns'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [Description] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810043840_AddActivityLogColumns'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [DetailsJson] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810043840_AddActivityLogColumns'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [Kind] nvarchar(32) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810043840_AddActivityLogColumns'
)
BEGIN
    UPDATE [AuditLogs] SET [Kind] = 'DataChange' WHERE [Kind] = '' OR [Kind] IS NULL
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810043840_AddActivityLogColumns'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [RowCount] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810043840_AddActivityLogColumns'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD [Severity] nvarchar(32) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810043840_AddActivityLogColumns'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_Kind_ActivityType_Timestamp] ON [AuditLogs] ([Kind], [ActivityType], [Timestamp]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260810043840_AddActivityLogColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260810043840_AddActivityLogColumns', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811104849_AddNotificationTemplates'
)
BEGIN
    CREATE TABLE [NotificationTemplates] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(100) NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Channel] int NOT NULL,
        [Subject] nvarchar(250) NULL,
        [Body] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [PlaceholdersHelp] nvarchar(500) NULL,
        CONSTRAINT [PK_NotificationTemplates] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811104849_AddNotificationTemplates'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NotificationTemplates_Code_Channel] ON [NotificationTemplates] ([Code], [Channel]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260811104849_AddNotificationTemplates'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260811104849_AddNotificationTemplates', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813170454_RemoveExamScheduleCollegeApproval'
)
BEGIN
    DROP TABLE [ExamScheduleCollegeApprovals];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813170454_RemoveExamScheduleCollegeApproval'
)
BEGIN
    DECLARE @var15 nvarchar(max);
    SELECT @var15 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ExamSchedules]') AND [c].[name] = N'CollegeApprovalDate');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [ExamSchedules] DROP CONSTRAINT ' + @var15 + ';');
    ALTER TABLE [ExamSchedules] DROP COLUMN [CollegeApprovalDate];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813170454_RemoveExamScheduleCollegeApproval'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813170454_RemoveExamScheduleCollegeApproval', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815024615_DropInternalPracticalMarksFromSubjectOffering'
)
BEGIN
    DECLARE @var16 nvarchar(max);
    SELECT @var16 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SubjectOfferings]') AND [c].[name] = N'InternalPracticalFullMarks');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [SubjectOfferings] DROP CONSTRAINT ' + @var16 + ';');
    ALTER TABLE [SubjectOfferings] DROP COLUMN [InternalPracticalFullMarks];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815024615_DropInternalPracticalMarksFromSubjectOffering'
)
BEGIN
    DECLARE @var17 nvarchar(max);
    SELECT @var17 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SubjectOfferings]') AND [c].[name] = N'InternalPracticalPassMarks');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [SubjectOfferings] DROP CONSTRAINT ' + @var17 + ';');
    ALTER TABLE [SubjectOfferings] DROP COLUMN [InternalPracticalPassMarks];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815024615_DropInternalPracticalMarksFromSubjectOffering'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260815024615_DropInternalPracticalMarksFromSubjectOffering', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816152655_AddTenantScopingToSubjectCatalog'
)
BEGIN
    ALTER TABLE [SubjectCatalogs] ADD [TenantId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816152655_AddTenantScopingToSubjectCatalog'
)
BEGIN
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SubjectCatalogs')
    BEGIN
        UPDATE SubjectCatalogs
        SET TenantId = (SELECT MIN(Id) FROM Tenants)
        WHERE TenantId IS NULL
          AND EXISTS (SELECT 1 FROM Tenants);
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816152655_AddTenantScopingToSubjectCatalog'
)
BEGIN
    CREATE INDEX [IX_SubjectCatalogs_TenantId] ON [SubjectCatalogs] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816152655_AddTenantScopingToSubjectCatalog'
)
BEGIN
    ALTER TABLE [SubjectCatalogs] ADD CONSTRAINT [FK_SubjectCatalogs_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816152655_AddTenantScopingToSubjectCatalog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260816152655_AddTenantScopingToSubjectCatalog', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816160536_AddGradeGroupAndGradePoint'
)
BEGIN
    ALTER TABLE [GradingSchemes] ADD [GradeGroupId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816160536_AddGradeGroupAndGradePoint'
)
BEGIN
    CREATE TABLE [GradeGroups] (
        [Id] int NOT NULL,
        [GradeGroupName] nvarchar(100) NOT NULL,
        [Remarks] nvarchar(500) NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedDate] datetime2 NULL,
        CONSTRAINT [PK_GradeGroups] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816160536_AddGradeGroupAndGradePoint'
)
BEGIN
    CREATE TABLE [GradePoints] (
        [Id] int NOT NULL,
        [Grade] nvarchar(5) NOT NULL,
        [ObtainedMark] int NOT NULL,
        [GradePointValue] decimal(5,2) NOT NULL,
        [GradeGroupId] int NOT NULL,
        CONSTRAINT [PK_GradePoints] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GradePoints_GradeGroups_GradeGroupId] FOREIGN KEY ([GradeGroupId]) REFERENCES [GradeGroups] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816160536_AddGradeGroupAndGradePoint'
)
BEGIN
    CREATE INDEX [IX_GradingSchemes_GradeGroupId] ON [GradingSchemes] ([GradeGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816160536_AddGradeGroupAndGradePoint'
)
BEGIN
    CREATE INDEX [IX_GradeGroups_GradeGroupName] ON [GradeGroups] ([GradeGroupName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816160536_AddGradeGroupAndGradePoint'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GradePoints_GradeGroupId_ObtainedMark] ON [GradePoints] ([GradeGroupId], [ObtainedMark]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816160536_AddGradeGroupAndGradePoint'
)
BEGIN
    ALTER TABLE [GradingSchemes] ADD CONSTRAINT [FK_GradingSchemes_GradeGroups_GradeGroupId] FOREIGN KEY ([GradeGroupId]) REFERENCES [GradeGroups] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816160536_AddGradeGroupAndGradePoint'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260816160536_AddGradeGroupAndGradePoint', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816180547_AddPerPartGradesAndResultLevel'
)
BEGIN
    ALTER TABLE [ResultRecords] ADD [LevelId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816180547_AddPerPartGradesAndResultLevel'
)
BEGIN
    ALTER TABLE [ExamSubjectResults] ADD [GradeLetterPractical] nvarchar(5) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816180547_AddPerPartGradesAndResultLevel'
)
BEGIN
    ALTER TABLE [ExamSubjectResults] ADD [GradeLetterTheory] nvarchar(5) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816180547_AddPerPartGradesAndResultLevel'
)
BEGIN
    CREATE INDEX [IX_ResultRecords_LevelId] ON [ResultRecords] ([LevelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816180547_AddPerPartGradesAndResultLevel'
)
BEGIN
    ALTER TABLE [ResultRecords] ADD CONSTRAINT [FK_ResultRecords_Levels_LevelId] FOREIGN KEY ([LevelId]) REFERENCES [Levels] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260816180547_AddPerPartGradesAndResultLevel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260816180547_AddPerPartGradesAndResultLevel', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    CREATE TABLE [SemesterInstances] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [SemesterId] int NOT NULL,
        [AcademicYearId] int NOT NULL,
        [ProgramId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Remark] nvarchar(50) NULL,
        CONSTRAINT [PK_SemesterInstances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SemesterInstances_AcademicYears_AcademicYearId] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYears] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SemesterInstances_Programs_ProgramId] FOREIGN KEY ([ProgramId]) REFERENCES [Programs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SemesterInstances_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SemesterInstances_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    CREATE INDEX [IX_SemesterInstances_AcademicYearId] ON [SemesterInstances] ([AcademicYearId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    CREATE INDEX [IX_SemesterInstances_ProgramId] ON [SemesterInstances] ([ProgramId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SemesterInstances_SemesterId_AcademicYearId_ProgramId] ON [SemesterInstances] ([SemesterId], [AcademicYearId], [ProgramId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    CREATE INDEX [IX_SemesterInstances_TenantId] ON [SemesterInstances] ([TenantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    ALTER TABLE [SemesterEnrollments] DROP CONSTRAINT [FK_SemesterEnrollments_Semesters_SemesterId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    ALTER TABLE [ExamSchedules] DROP CONSTRAINT [FK_ExamSchedules_Semesters_SemesterId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    ALTER TABLE [ExamSchedules] DROP CONSTRAINT [FK_ExamSchedules_AcademicYears_AcademicYearId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN

                    DECLARE @TenantId INT = 1;

                    INSERT INTO SemesterInstances (TenantId, SemesterId, AcademicYearId, ProgramId, StartDate, EndDate)
                    SELECT @TenantId, s.Id, s.AcademicYearId, ps.ProgramId, s.StartDate, s.EndDate
                    FROM Semesters s
                    INNER JOIN ProgramSemesters ps ON ps.SemesterId = s.Id
                    WHERE s.AcademicYearId > 0
                      AND ps.ProgramId > 0
                      AND ps.IsActive = 1;

                    INSERT INTO SemesterInstances (TenantId, SemesterId, AcademicYearId, ProgramId, StartDate, EndDate)
                    SELECT @TenantId, es.SemesterId, es.AcademicYearId, es.ProgramId, s.StartDate, s.EndDate
                    FROM ExamSchedules es
                    INNER JOIN Semesters s ON es.SemesterId = s.Id
                    WHERE s.AcademicYearId > 0
                      AND NOT EXISTS (
                          SELECT 1 FROM SemesterInstances si
                          WHERE si.SemesterId = es.SemesterId
                            AND si.AcademicYearId = es.AcademicYearId
                            AND si.ProgramId = es.ProgramId);

                    UPDATE se
                    SET se.SemesterId = si.Id
                    FROM SemesterEnrollments se
                    INNER JOIN Semesters s ON se.SemesterId = s.Id
                    INNER JOIN StudentAdmissions sa ON se.StudentAdmissionId = sa.Id
                    INNER JOIN SemesterInstances si
                        ON si.SemesterId = s.Id
                        AND si.AcademicYearId = s.AcademicYearId
                        AND si.ProgramId = sa.ProgramsId
                    WHERE sa.ProgramsId IS NOT NULL AND sa.ProgramsId > 0;

                    UPDATE es
                    SET es.SemesterId = si.Id
                    FROM ExamSchedules es
                    INNER JOIN SemesterInstances si
                        ON si.SemesterId = es.SemesterId
                        AND si.AcademicYearId = es.AcademicYearId
                        AND si.ProgramId = es.ProgramId;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    ALTER TABLE [Semesters] DROP CONSTRAINT [FK_Semesters_AcademicYears_AcademicYearId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    DROP INDEX [IX_Semesters_AcademicYearId] ON [Semesters];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    DECLARE @var18 nvarchar(max);
    SELECT @var18 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Semesters]') AND [c].[name] = N'AcademicYearId');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Semesters] DROP CONSTRAINT ' + @var18 + ';');
    ALTER TABLE [Semesters] DROP COLUMN [AcademicYearId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    DECLARE @var19 nvarchar(max);
    SELECT @var19 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Semesters]') AND [c].[name] = N'EndDate');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Semesters] DROP CONSTRAINT ' + @var19 + ';');
    ALTER TABLE [Semesters] DROP COLUMN [EndDate];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    DECLARE @var20 nvarchar(max);
    SELECT @var20 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Semesters]') AND [c].[name] = N'StartDate');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Semesters] DROP CONSTRAINT ' + @var20 + ';');
    ALTER TABLE [Semesters] DROP COLUMN [StartDate];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    DECLARE @var21 nvarchar(max);
    SELECT @var21 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Semesters]') AND [c].[name] = N'Year');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Semesters] DROP CONSTRAINT ' + @var21 + ';');
    ALTER TABLE [Semesters] DROP COLUMN [Year];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    EXEC sp_rename N'[SemesterEnrollments].[SemesterId]', N'SemesterInstanceId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    EXEC sp_rename N'[SemesterEnrollments].[IX_SemesterEnrollments_SemesterId]', N'IX_SemesterEnrollments_SemesterInstanceId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    ALTER TABLE [SemesterEnrollments] ADD CONSTRAINT [FK_SemesterEnrollments_SemesterInstances_SemesterInstanceId] FOREIGN KEY ([SemesterInstanceId]) REFERENCES [SemesterInstances] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817024022_SemesterInstances'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260817024022_SemesterInstances', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    DROP INDEX [IX_ExamSchedules_AcademicYearId] ON [ExamSchedules];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    DROP INDEX [IX_AcademicYears_AcademicYearCode] ON [AcademicYears];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    DECLARE @var22 nvarchar(max);
    SELECT @var22 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ExamSchedules]') AND [c].[name] = N'AcademicYearId');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [ExamSchedules] DROP CONSTRAINT ' + @var22 + ';');
    ALTER TABLE [ExamSchedules] DROP COLUMN [AcademicYearId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    EXEC sp_rename N'[ExamSchedules].[SemesterId]', N'SemesterInstanceId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    EXEC sp_rename N'[ExamSchedules].[IX_ExamSchedules_SemesterId]', N'IX_ExamSchedules_SemesterInstanceId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    ALTER TABLE [AcademicYears] ADD [EndDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    ALTER TABLE [AcademicYears] ADD [StartDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    ALTER TABLE [AcademicYears] ADD [TenantId] int NOT NULL DEFAULT 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AcademicYears_TenantId_AcademicYearCode] ON [AcademicYears] ([TenantId], [AcademicYearCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    ALTER TABLE [AcademicYears] ADD CONSTRAINT [FK_AcademicYears_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    ALTER TABLE [ExamSchedules] ADD CONSTRAINT [FK_ExamSchedules_SemesterInstances_SemesterInstanceId] FOREIGN KEY ([SemesterInstanceId]) REFERENCES [SemesterInstances] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260817054948_TenantScopedAcademicYearAndSemesterInstanceProgramId', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817175434_PendingChanges'
)
BEGIN
    DECLARE @var23 nvarchar(max);
    SELECT @var23 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ResultRecords]') AND [c].[name] = N'ResultRecordMasterId');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [ResultRecords] DROP CONSTRAINT ' + @var23 + ';');
    ALTER TABLE [ResultRecords] ALTER COLUMN [ResultRecordMasterId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260817175434_PendingChanges'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260817175434_PendingChanges', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260818183135_AddIsActiveToSubjectOffering'
)
BEGIN
    DROP INDEX [IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId] ON [SubjectOfferings];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260818183135_AddIsActiveToSubjectOffering'
)
BEGIN
    ALTER TABLE [SubjectOfferings] ADD [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260818183135_AddIsActiveToSubjectOffering'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId] ON [SubjectOfferings] ([CurriculumVersionId], [SubjectCatalogId], [ProgramId], [SemesterId]) WHERE [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260818183135_AddIsActiveToSubjectOffering'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260818183135_AddIsActiveToSubjectOffering', N'10.0.7');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820200430_AddIsSupplementaryToExamRegistrationAndSubjectResult'
)
BEGIN
    ALTER TABLE [ExamSubjectResults] ADD [IsSupplementary] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820200430_AddIsSupplementaryToExamRegistrationAndSubjectResult'
)
BEGIN
    ALTER TABLE [ExamRegistrations] ADD [IsSupplementary] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820200430_AddIsSupplementaryToExamRegistrationAndSubjectResult'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260820200430_AddIsSupplementaryToExamRegistrationAndSubjectResult', N'10.0.7');
END;

COMMIT;
GO

