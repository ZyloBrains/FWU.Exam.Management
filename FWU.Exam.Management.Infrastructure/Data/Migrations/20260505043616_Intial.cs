using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Intial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcademicYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcademicYearCode = table.Column<int>(type: "int", nullable: false),
                    AcademicYearCodeNepali = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AcademicYearName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AcademicYearNameNepali = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankCode = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Boards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    BoardName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CollegeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollegeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntryFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryFormatName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryFormats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ESewaConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostUrl = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecretKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SuccessUrl = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ServiceChargeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VerifyUrl = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESewaConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ethnicities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EthnicityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ethnicities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamAttendanceStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamAttendanceStatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamAttendanceStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamFormFeeNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    IsCollegeFee = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamFormFeeNames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamScheduleParent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamScheduleParentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamScheduleParent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Faculties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacultyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FacultyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faculties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FiscalYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYearName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EndDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FiscalYearCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GenderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndexGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndexGroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KhaltiConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    AuthorizationKey = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ServiceCharge = table.Column<int>(type: "int", nullable: false),
                    PostUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    VerifyUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhaltiConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Levels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    LevelName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LevelDisplayOrder = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsRunning = table.Column<bool>(type: "bit", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NepaliDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GregorianDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NepaliDateShort = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NepaliDateFull = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NepaliDateString = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NepaliDates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoticeTitle = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    NoticePreview = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NoticeContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentTypeName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeriodTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumberOfMonths = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProvinceName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProvinceCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionSetName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmtpConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Host = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    From = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    EnableSsl = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmtpConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentCategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubjectTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxAllowedSubjects = table.Column<int>(type: "int", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    BatchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batches_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Semesters_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamRollNumberSetup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamScheduleParentId = table.Column<int>(type: "int", nullable: false),
                    FirstExamRollNumber = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Suffix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MinimumRollNumberLength = table.Column<int>(type: "int", nullable: false),
                    Round = table.Column<int>(type: "int", nullable: false),
                    MinimumGap = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRollNumberSetup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRollNumberSetup_ExamScheduleParent_ExamScheduleParentId",
                        column: x => x.ExamScheduleParentId,
                        principalTable: "ExamScheduleParent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreviousLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreviousLevelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LevelId = table.Column<int>(type: "int", nullable: true),
                    LevelDisplayOrder = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreviousLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreviousLevels_Levels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelId = table.Column<int>(type: "int", nullable: false),
                    FacultyId = table.Column<int>(type: "int", nullable: false),
                    BoardId = table.Column<int>(type: "int", nullable: true),
                    ProgramCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    GrandTotalMarks = table.Column<int>(type: "int", nullable: true),
                    HasMultipleIntakes = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfSeats = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScholarshipSeats = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RollNumberPrefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Programs_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Programs_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Programs_Levels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProfilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Designation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    DistrictCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DistrictName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectCatalogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreditHours = table.Column<int>(type: "int", nullable: true),
                    SubjectTypeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectCatalogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectCatalogs_SubjectTypes_SubjectTypeId",
                        column: x => x.SubjectTypeId,
                        principalTable: "SubjectTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreviousLevelId = table.Column<int>(type: "int", nullable: false),
                    SchoolTypeName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolTypes_PreviousLevels_PreviousLevelId",
                        column: x => x.PreviousLevelId,
                        principalTable: "PreviousLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    EffectiveAcademicYearId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumVersions_AcademicYears_EffectiveAcademicYearId",
                        column: x => x.EffectiveAcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurriculumVersions_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamScheduleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDateBs = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EndDateBs = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExtendedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtendedDateCharge = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CollegeApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdmissionCardReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExamScheduleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false),
                    ExamScheduleParentId = table.Column<int>(type: "int", nullable: true),
                    LevelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSchedules_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSchedules_ExamScheduleParent_ExamScheduleParentId",
                        column: x => x.ExamScheduleParentId,
                        principalTable: "ExamScheduleParent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamSchedules_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSchedules_Levels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Levels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamSchedules_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSchedules_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GradingSchemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingSchemes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradingSchemes_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradingSchemes_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramSubjectPracticalCharge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramsId = table.Column<int>(type: "int", nullable: false),
                    PracticalSubjectCharge = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramSubjectPracticalCharge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramSubjectPracticalCharge_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProgramsId = table.Column<int>(type: "int", nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sections_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    LocalLevelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LocalLevelType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalLevels_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubjectOfferings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectCatalogId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    IsCompulsory = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    HasTheory = table.Column<bool>(type: "bit", nullable: false),
                    HasPractical = table.Column<bool>(type: "bit", nullable: false),
                    HasInternal = table.Column<bool>(type: "bit", nullable: false),
                    TheoryFullMarks = table.Column<float>(type: "real", nullable: false),
                    TheoryPassMarks = table.Column<float>(type: "real", nullable: false),
                    PracticalFullMarks = table.Column<float>(type: "real", nullable: true),
                    PracticalPassMarks = table.Column<float>(type: "real", nullable: true),
                    InternalTheoryFullMarks = table.Column<float>(type: "real", nullable: true),
                    InternalTheoryPassMarks = table.Column<float>(type: "real", nullable: true),
                    InternalPracticalFullMarks = table.Column<float>(type: "real", nullable: true),
                    InternalPracticalPassMarks = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectOfferings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectOfferings_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectOfferings_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectOfferings_SubjectCatalogs_SubjectCatalogId",
                        column: x => x.SubjectCatalogId,
                        principalTable: "SubjectCatalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillTitle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillTitleName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ThroughDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApplicableDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillTitle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillTitle_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamFormFeeRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    ExamFormFeeNameId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CollegeTypeId = table.Column<int>(type: "int", nullable: true),
                    ExamTypeId = table.Column<int>(type: "int", nullable: true),
                    ThroughDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApplicableDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCollegeFee = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamFormFeeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamFormFeeRates_CollegeTypes_CollegeTypeId",
                        column: x => x.CollegeTypeId,
                        principalTable: "CollegeTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamFormFeeRates_ExamFormFeeNames_ExamFormFeeNameId",
                        column: x => x.ExamFormFeeNameId,
                        principalTable: "ExamFormFeeNames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamFormFeeRates_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamFormFeeRates_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamScheduleBatch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamScheduleBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamScheduleBatch_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamScheduleBatch_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamScheduleBatch_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GradeDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeLetter = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MinPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GradePoint = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsPass = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    GradingSchemeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradeDefinitions_GradingSchemes_GradingSchemeId",
                        column: x => x.GradingSchemeId,
                        principalTable: "GradingSchemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocalLevelId = table.Column<int>(type: "int", nullable: false),
                    WardNumber = table.Column<int>(type: "int", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ToleStreet = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FullAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddressType = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_LocalLevels_LocalLevelId",
                        column: x => x.LocalLevelId,
                        principalTable: "LocalLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Colleges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CollegeNameNepali = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EstablishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone1 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Phone2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PrincipalName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PrincipalContactNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsExamCenterOnly = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true),
                    AddressId = table.Column<int>(type: "int", nullable: true),
                    CollegeTypeId = table.Column<int>(type: "int", nullable: true),
                    CollegeProfileId = table.Column<int>(type: "int", nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    QuestionSetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colleges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Colleges_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Colleges_CollegeTypes_CollegeTypeId",
                        column: x => x.CollegeTypeId,
                        principalTable: "CollegeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Colleges_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Colleges_QuestionSets_QuestionSetId",
                        column: x => x.QuestionSetId,
                        principalTable: "QuestionSets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankVoucher",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    BillTitleId = table.Column<int>(type: "int", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    BankAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VoucherDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VoucherNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VoucherAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BankVoucherUserAttachmentId = table.Column<int>(type: "int", nullable: true),
                    ExamScheduleParentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankVoucher", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankVoucher_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankVoucher_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankVoucher_BillTitle_BillTitleId",
                        column: x => x.BillTitleId,
                        principalTable: "BillTitle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankVoucher_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankVoucher_ExamScheduleParent_ExamScheduleParentId",
                        column: x => x.ExamScheduleParentId,
                        principalTable: "ExamScheduleParent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankVoucher_UserAttachments_BankVoucherUserAttachmentId",
                        column: x => x.BankVoucherUserAttachmentId,
                        principalTable: "UserAttachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollegeProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    BankBranchName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ContactPersonName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ContactPersonMobileNumber = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ContactPersonEmail = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    BlankChequeUserAttachmentId = table.Column<int>(type: "int", nullable: false),
                    AuditReportUserAttachmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollegeProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollegeProfiles_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollegeProfiles_UserAttachments_AuditReportUserAttachmentId",
                        column: x => x.AuditReportUserAttachmentId,
                        principalTable: "UserAttachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollegeProfiles_UserAttachments_BlankChequeUserAttachmentId",
                        column: x => x.BlankChequeUserAttachmentId,
                        principalTable: "UserAttachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollegePrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AffiliationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumberOfStudents = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollegePrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollegePrograms_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollegePrograms_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamCenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamCenters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamCenters_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamCenters_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamRollNumberSetupDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamRollNumberSetupId = table.Column<int>(type: "int", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    StartRollNumber = table.Column<int>(type: "int", nullable: false),
                    EndRollNumber = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Suffix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRollNumberSetupDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRollNumberSetupDetail_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRollNumberSetupDetail_ExamRollNumberSetup_ExamRollNumberSetupId",
                        column: x => x.ExamRollNumberSetupId,
                        principalTable: "ExamRollNumberSetup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRollNumberSetupDetail_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRollNumberSetupDetail_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRollNumberSetupDetail_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreferredExamCenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferredExamCenters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreferredExamCenters_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelId = table.Column<int>(type: "int", nullable: false),
                    FacultyId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NepaliName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfBirthBS = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DateOfBirthAD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GenderId = table.Column<int>(type: "int", nullable: false),
                    BloodGroup = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Religion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PermanentAddressId = table.Column<int>(type: "int", nullable: true),
                    CurrentAddressId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StudentCategoryId = table.Column<int>(type: "int", nullable: false),
                    VerifiedBy = table.Column<int>(type: "int", nullable: true),
                    VerifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EthnicityId = table.Column<int>(type: "int", nullable: true),
                    EntranceRollNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsRegistrationNumberGenerated = table.Column<bool>(type: "bit", nullable: true),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    EntryFormatId = table.Column<int>(type: "int", nullable: true),
                    IndexGroupId = table.Column<int>(type: "int", nullable: true),
                    LocalLevelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_Addresses_CurrentAddressId",
                        column: x => x.CurrentAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_Addresses_PermanentAddressId",
                        column: x => x.PermanentAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_EntryFormats_EntryFormatId",
                        column: x => x.EntryFormatId,
                        principalTable: "EntryFormats",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_Ethnicities_EthnicityId",
                        column: x => x.EthnicityId,
                        principalTable: "Ethnicities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_IndexGroups_IndexGroupId",
                        column: x => x.IndexGroupId,
                        principalTable: "IndexGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_Levels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_LocalLevels_LocalLevelId",
                        column: x => x.LocalLevelId,
                        principalTable: "LocalLevels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentRegistrations_StudentCategories_StudentCategoryId",
                        column: x => x.StudentCategoryId,
                        principalTable: "StudentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamCenterDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamCenterId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    ProgramsId = table.Column<int>(type: "int", nullable: true),
                    RollNumberFrom = table.Column<long>(type: "bigint", nullable: false),
                    RollNumberTo = table.Column<long>(type: "bigint", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamCenterDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamCenterDetail_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamCenterDetail_ExamCenters_ExamCenterId",
                        column: x => x.ExamCenterId,
                        principalTable: "ExamCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamCenterDetail_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationVoucher",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    DateOfBirthAd = table.Column<DateOnly>(type: "date", nullable: true),
                    DateOfBirthBs = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    StudentRegistrationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationVoucher", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationVoucher_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationVoucher_StudentRegistrations_StudentRegistrationId",
                        column: x => x.StudentRegistrationId,
                        principalTable: "StudentRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRequestLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentRequestLogStatus = table.Column<int>(type: "int", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ForwardedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfBirthAd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FullRequestContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    StudentRegistrationId = table.Column<int>(type: "int", nullable: true),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CollegeId = table.Column<int>(type: "int", nullable: true),
                    StudentCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequestLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRequestLog_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequestLog_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentRequestLog_PaymentType_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequestLog_StudentRegistrations_StudentRegistrationId",
                        column: x => x.StudentRegistrationId,
                        principalTable: "StudentRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentAdmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramsId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckedBy = table.Column<int>(type: "int", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CollegeRollNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HasFeeExemption = table.Column<bool>(type: "bit", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SectionId = table.Column<int>(type: "int", nullable: true),
                    StudentRegistrationId = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAdmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAdmissions_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentAdmissions_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAdmissions_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAdmissions_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentAdmissions_StudentRegistrations_StudentRegistrationId",
                        column: x => x.StudentRegistrationId,
                        principalTable: "StudentRegistrations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentAdmissions_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentGuardians",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentRegistrationId = table.Column<int>(type: "int", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FatherContactNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FatherPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FatherEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FatherQualification = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FatherProfession = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FatherAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FatherOrganization = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FatherOrganizationAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MotherName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MotherContactNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MotherPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MotherEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MotherQualification = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MotherProfession = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MotherAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MotherOrganization = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MotherOrganizationAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GuardianName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GuardianContactNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GuardianPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GuardianEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GuardianQualification = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GuardianProfession = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GuardianAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GuardianOrganization = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GuardianOrganizationAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelationWithStudent = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentGuardians", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentGuardians_StudentRegistrations_StudentRegistrationId",
                        column: x => x.StudentRegistrationId,
                        principalTable: "StudentRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentQualifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentRegistrationId = table.Column<int>(type: "int", nullable: false),
                    BoardId = table.Column<int>(type: "int", nullable: false),
                    PreviousLevelId = table.Column<int>(type: "int", nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    InstituteName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PassedYear = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    TotalCredits = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsHigherDegree = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExamRollNumber = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentQualifications_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentQualifications_PreviousLevels_PreviousLevelId",
                        column: x => x.PreviousLevelId,
                        principalTable: "PreviousLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentQualifications_StudentRegistrations_StudentRegistrationId",
                        column: x => x.StudentRegistrationId,
                        principalTable: "StudentRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentPracticalSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentRequestLogId = table.Column<int>(type: "int", nullable: false),
                    PracticalSubjectsCount = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentPracticalSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentPracticalSubjects_PaymentRequestLog_PaymentRequestLogId",
                        column: x => x.PaymentRequestLogId,
                        principalTable: "PaymentRequestLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentResponseLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentRequestLogId = table.Column<int>(type: "int", nullable: false),
                    ResponseTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ResponseMessage = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    FullResponse = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentResponseLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentResponseLog_PaymentRequestLog_PaymentRequestLogId",
                        column: x => x.PaymentRequestLogId,
                        principalTable: "PaymentRequestLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SemesterEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentAdmissionId = table.Column<int>(type: "int", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentStatus = table.Column<int>(type: "int", nullable: false),
                    EnrollmentType = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    EnrolledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DropDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DropReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SemesterResultDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalCredits = table.Column<double>(type: "float", nullable: false),
                    GradePoints = table.Column<double>(type: "float", nullable: false),
                    TotalFee = table.Column<double>(type: "float", nullable: false),
                    PaidAmount = table.Column<double>(type: "float", nullable: false),
                    Deficiency = table.Column<bool>(type: "bit", nullable: false),
                    ResultStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemesterEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SemesterEnrollments_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SemesterEnrollments_StudentAdmissions_StudentAdmissionId",
                        column: x => x.StudentAdmissionId,
                        principalTable: "StudentAdmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    ExamCenterId = table.Column<int>(type: "int", nullable: true),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    ExamRollNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExamRollNumberCoding = table.Column<long>(type: "bigint", nullable: true),
                    FeeEnclosed = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AttendancePercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    VerifiedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sgpa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    RollNumberIndex = table.Column<int>(type: "int", nullable: true),
                    IsAppliedByStudent = table.Column<bool>(type: "bit", nullable: true),
                    ProgramsId = table.Column<int>(type: "int", nullable: true),
                    ApplicationVoucherId = table.Column<int>(type: "int", nullable: true),
                    AdminVerifiedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminVerifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SemesterEnrollmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_ApplicationVoucher_ApplicationVoucherId",
                        column: x => x.ApplicationVoucherId,
                        principalTable: "ApplicationVoucher",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_ExamCenters_ExamCenterId",
                        column: x => x.ExamCenterId,
                        principalTable: "ExamCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrations_SemesterEnrollments_SemesterEnrollmentId",
                        column: x => x.SemesterEnrollmentId,
                        principalTable: "SemesterEnrollments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExamSubjectRegistrationInternal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryAcademicYearId = table.Column<int>(type: "int", nullable: false),
                    ObtainedMarksTheoryInternal = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ObtainedMarksPracticalInternal = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: true),
                    SemesterEnrollmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubjectRegistrationInternal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrationInternal_AcademicYears_EntryAcademicYearId",
                        column: x => x.EntryAcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrationInternal_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrationInternal_SemesterEnrollments_SemesterEnrollmentId",
                        column: x => x.SemesterEnrollmentId,
                        principalTable: "SemesterEnrollments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExamRegistrationActionLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamRegistrationId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRegistrationActionLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRegistrationActionLog_ExamRegistrations_ExamRegistrationId",
                        column: x => x.ExamRegistrationId,
                        principalTable: "ExamRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamRegistrationCenterChange",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamRegistrationId = table.Column<int>(type: "int", nullable: false),
                    PreferredExamCenterId = table.Column<int>(type: "int", nullable: false),
                    RequestedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentExamCenterId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRegistrationCenterChange", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRegistrationCenterChange_ExamRegistrations_ExamRegistrationId",
                        column: x => x.ExamRegistrationId,
                        principalTable: "ExamRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrationCenterChange_PreferredExamCenters_PreferredExamCenterId",
                        column: x => x.PreferredExamCenterId,
                        principalTable: "PreferredExamCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamSubjectRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamRegistrationId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false),
                    SubjectOfferingId = table.Column<int>(type: "int", nullable: false),
                    ObtainedMarksTheory = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ObtainedMarksTheoryConfirm = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ObtainedMarksPractical = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ObtainedMarksPracticalConfirm = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    GradeLetter = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsLooseEntry = table.Column<bool>(type: "bit", nullable: true),
                    IsTheoryRegistered = table.Column<bool>(type: "bit", nullable: true),
                    IsPracticalRegistered = table.Column<bool>(type: "bit", nullable: true),
                    IsExtra = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubjectRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrations_ExamRegistrations_ExamRegistrationId",
                        column: x => x.ExamRegistrationId,
                        principalTable: "ExamRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrations_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrations_SubjectOfferings_SubjectOfferingId",
                        column: x => x.SubjectOfferingId,
                        principalTable: "SubjectOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamSubjectRegistrationExamSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamSubjectRegistrationId = table.Column<int>(type: "int", nullable: false),
                    ExamStartedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    ObtainedMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ExamSubmittedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAutoSubmitted = table.Column<bool>(type: "bit", nullable: true),
                    LastStatusSyncDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubjectRegistrationExamSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrationExamSession_ExamSubjectRegistrations_ExamSubjectRegistrationId",
                        column: x => x.ExamSubjectRegistrationId,
                        principalTable: "ExamSubjectRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_LocalLevelId",
                table: "Addresses",
                column: "LocalLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVoucher_ExamScheduleId",
                table: "ApplicationVoucher",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVoucher_StudentRegistrationId",
                table: "ApplicationVoucher",
                column: "StudentRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BankVoucher_AcademicYearId",
                table: "BankVoucher",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_BankVoucher_BankId",
                table: "BankVoucher",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_BankVoucher_BankVoucherUserAttachmentId",
                table: "BankVoucher",
                column: "BankVoucherUserAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_BankVoucher_BillTitleId",
                table: "BankVoucher",
                column: "BillTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_BankVoucher_CollegeId",
                table: "BankVoucher",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_BankVoucher_ExamScheduleParentId",
                table: "BankVoucher",
                column: "ExamScheduleParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_AcademicYearId",
                table: "Batches",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_BillTitle_ExamScheduleId",
                table: "BillTitle",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeProfiles_AuditReportUserAttachmentId",
                table: "CollegeProfiles",
                column: "AuditReportUserAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeProfiles_BlankChequeUserAttachmentId",
                table: "CollegeProfiles",
                column: "BlankChequeUserAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeProfiles_CollegeId",
                table: "CollegeProfiles",
                column: "CollegeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollegePrograms_CollegeId",
                table: "CollegePrograms",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegePrograms_ProgramId",
                table: "CollegePrograms",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_AddressId",
                table: "Colleges",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_CollegeTypeId",
                table: "Colleges",
                column: "CollegeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_DistrictId",
                table: "Colleges",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_QuestionSetId",
                table: "Colleges",
                column: "QuestionSetId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumVersions_EffectiveAcademicYearId",
                table: "CurriculumVersions",
                column: "EffectiveAcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumVersions_ProgramId",
                table: "CurriculumVersions",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_ProvinceId",
                table: "Districts",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterDetail_CollegeId",
                table: "ExamCenterDetail",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterDetail_ExamCenterId",
                table: "ExamCenterDetail",
                column: "ExamCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterDetail_ProgramsId",
                table: "ExamCenterDetail",
                column: "ProgramsId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenters_CollegeId",
                table: "ExamCenters",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenters_ExamScheduleId",
                table: "ExamCenters",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFormFeeRates_CollegeTypeId",
                table: "ExamFormFeeRates",
                column: "CollegeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFormFeeRates_ExamFormFeeNameId",
                table: "ExamFormFeeRates",
                column: "ExamFormFeeNameId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFormFeeRates_ExamScheduleId",
                table: "ExamFormFeeRates",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFormFeeRates_ExamTypeId",
                table: "ExamFormFeeRates",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrationActionLog_ExamRegistrationId",
                table: "ExamRegistrationActionLog",
                column: "ExamRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrationCenterChange_ExamRegistrationId",
                table: "ExamRegistrationCenterChange",
                column: "ExamRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrationCenterChange_PreferredExamCenterId",
                table: "ExamRegistrationCenterChange",
                column: "PreferredExamCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_AcademicYearId",
                table: "ExamRegistrations",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_ApplicationVoucherId",
                table: "ExamRegistrations",
                column: "ApplicationVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_CollegeId",
                table: "ExamRegistrations",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_ExamCenterId",
                table: "ExamRegistrations",
                column: "ExamCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_ExamScheduleId",
                table: "ExamRegistrations",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_ProgramsId",
                table: "ExamRegistrations",
                column: "ProgramsId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_SemesterEnrollmentId",
                table: "ExamRegistrations",
                column: "SemesterEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetup_ExamScheduleParentId",
                table: "ExamRollNumberSetup",
                column: "ExamScheduleParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetupDetail_CollegeId",
                table: "ExamRollNumberSetupDetail",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetupDetail_ExamRollNumberSetupId",
                table: "ExamRollNumberSetupDetail",
                column: "ExamRollNumberSetupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetupDetail_ExamScheduleId",
                table: "ExamRollNumberSetupDetail",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetupDetail_ExamTypeId",
                table: "ExamRollNumberSetupDetail",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetupDetail_ProgramId",
                table: "ExamRollNumberSetupDetail",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleBatch_BatchId",
                table: "ExamScheduleBatch",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleBatch_ExamScheduleId",
                table: "ExamScheduleBatch",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleBatch_ExamTypeId",
                table: "ExamScheduleBatch",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_AcademicYearId",
                table: "ExamSchedules",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_ExamScheduleParentId",
                table: "ExamSchedules",
                column: "ExamScheduleParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_ExamTypeId",
                table: "ExamSchedules",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_LevelId",
                table: "ExamSchedules",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_ProgramId",
                table: "ExamSchedules",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_SemesterId",
                table: "ExamSchedules",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationExamSession_ExamSubjectRegistrationId",
                table: "ExamSubjectRegistrationExamSession",
                column: "ExamSubjectRegistrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationInternal_EntryAcademicYearId",
                table: "ExamSubjectRegistrationInternal",
                column: "EntryAcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationInternal_ExamScheduleId",
                table: "ExamSubjectRegistrationInternal",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationInternal_SemesterEnrollmentId",
                table: "ExamSubjectRegistrationInternal",
                column: "SemesterEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrations_ExamRegistrationId",
                table: "ExamSubjectRegistrations",
                column: "ExamRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrations_ExamTypeId",
                table: "ExamSubjectRegistrations",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrations_SubjectOfferingId",
                table: "ExamSubjectRegistrations",
                column: "SubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeDefinitions_GradingSchemeId",
                table: "GradeDefinitions",
                column: "GradingSchemeId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingSchemes_AcademicYearId",
                table: "GradingSchemes",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingSchemes_ProgramId",
                table: "GradingSchemes",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalLevels_DistrictId",
                table: "LocalLevels",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPracticalSubjects_PaymentRequestLogId",
                table: "PaymentPracticalSubjects",
                column: "PaymentRequestLogId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestLog_CollegeId",
                table: "PaymentRequestLog",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestLog_ExamScheduleId",
                table: "PaymentRequestLog",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestLog_PaymentTypeId",
                table: "PaymentRequestLog",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestLog_StudentRegistrationId",
                table: "PaymentRequestLog",
                column: "StudentRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentResponseLog_PaymentRequestLogId",
                table: "PaymentResponseLog",
                column: "PaymentRequestLogId");

            migrationBuilder.CreateIndex(
                name: "IX_PreferredExamCenters_CollegeId",
                table: "PreferredExamCenters",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_PreviousLevels_LevelId",
                table: "PreviousLevels",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_BoardId",
                table: "Programs",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_FacultyId",
                table: "Programs",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_LevelId",
                table: "Programs",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSubjectPracticalCharge_ProgramsId",
                table: "ProgramSubjectPracticalCharge",
                column: "ProgramsId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolTypes_PreviousLevelId",
                table: "SchoolTypes",
                column: "PreviousLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_BatchId",
                table: "Sections",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ProgramsId",
                table: "Sections",
                column: "ProgramsId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterEnrollments_SemesterId",
                table: "SemesterEnrollments",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterEnrollments_StudentAdmissionId",
                table: "SemesterEnrollments",
                column: "StudentAdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_AcademicYearId",
                table: "Semesters",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_AppUserId",
                table: "StudentAdmissions",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_BatchId",
                table: "StudentAdmissions",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_CollegeId",
                table: "StudentAdmissions",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_ProgramsId",
                table: "StudentAdmissions",
                column: "ProgramsId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_SectionId",
                table: "StudentAdmissions",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_StudentRegistrationId",
                table: "StudentAdmissions",
                column: "StudentRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGuardians_StudentRegistrationId",
                table: "StudentGuardians",
                column: "StudentRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentQualifications_BoardId",
                table: "StudentQualifications",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentQualifications_PreviousLevelId",
                table: "StudentQualifications",
                column: "PreviousLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentQualifications_StudentRegistrationId",
                table: "StudentQualifications",
                column: "StudentRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_AcademicYearId",
                table: "StudentRegistrations",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_CollegeId",
                table: "StudentRegistrations",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_CurrentAddressId",
                table: "StudentRegistrations",
                column: "CurrentAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_DistrictId",
                table: "StudentRegistrations",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_EntryFormatId",
                table: "StudentRegistrations",
                column: "EntryFormatId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_EthnicityId",
                table: "StudentRegistrations",
                column: "EthnicityId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_FacultyId",
                table: "StudentRegistrations",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_GenderId",
                table: "StudentRegistrations",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_IndexGroupId",
                table: "StudentRegistrations",
                column: "IndexGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_LevelId",
                table: "StudentRegistrations",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_LocalLevelId",
                table: "StudentRegistrations",
                column: "LocalLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_PermanentAddressId",
                table: "StudentRegistrations",
                column: "PermanentAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_StudentCategoryId",
                table: "StudentRegistrations",
                column: "StudentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCatalogs_SubjectCode",
                table: "SubjectCatalogs",
                column: "SubjectCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCatalogs_SubjectTypeId",
                table: "SubjectCatalogs",
                column: "SubjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_ProgramId",
                table: "SubjectOfferings",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_SemesterId",
                table: "SubjectOfferings",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId",
                table: "SubjectOfferings",
                column: "SubjectCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "BankVoucher");

            migrationBuilder.DropTable(
                name: "CollegeProfiles");

            migrationBuilder.DropTable(
                name: "CollegePrograms");

            migrationBuilder.DropTable(
                name: "CurriculumVersions");

            migrationBuilder.DropTable(
                name: "ESewaConfiguration");

            migrationBuilder.DropTable(
                name: "ExamAttendanceStatuses");

            migrationBuilder.DropTable(
                name: "ExamCenterDetail");

            migrationBuilder.DropTable(
                name: "ExamFormFeeRates");

            migrationBuilder.DropTable(
                name: "ExamRegistrationActionLog");

            migrationBuilder.DropTable(
                name: "ExamRegistrationCenterChange");

            migrationBuilder.DropTable(
                name: "ExamRollNumberSetupDetail");

            migrationBuilder.DropTable(
                name: "ExamScheduleBatch");

            migrationBuilder.DropTable(
                name: "ExamSubjectRegistrationExamSession");

            migrationBuilder.DropTable(
                name: "ExamSubjectRegistrationInternal");

            migrationBuilder.DropTable(
                name: "FiscalYears");

            migrationBuilder.DropTable(
                name: "GradeDefinitions");

            migrationBuilder.DropTable(
                name: "KhaltiConfiguration");

            migrationBuilder.DropTable(
                name: "NepaliDates");

            migrationBuilder.DropTable(
                name: "Notices");

            migrationBuilder.DropTable(
                name: "PaymentPracticalSubjects");

            migrationBuilder.DropTable(
                name: "PaymentResponseLog");

            migrationBuilder.DropTable(
                name: "PeriodTypes");

            migrationBuilder.DropTable(
                name: "ProgramSubjectPracticalCharge");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "SchoolTypes");

            migrationBuilder.DropTable(
                name: "SmtpConfigurations");

            migrationBuilder.DropTable(
                name: "StudentGuardians");

            migrationBuilder.DropTable(
                name: "StudentQualifications");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "BillTitle");

            migrationBuilder.DropTable(
                name: "UserAttachments");

            migrationBuilder.DropTable(
                name: "ExamFormFeeNames");

            migrationBuilder.DropTable(
                name: "PreferredExamCenters");

            migrationBuilder.DropTable(
                name: "ExamRollNumberSetup");

            migrationBuilder.DropTable(
                name: "ExamSubjectRegistrations");

            migrationBuilder.DropTable(
                name: "GradingSchemes");

            migrationBuilder.DropTable(
                name: "PaymentRequestLog");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "PreviousLevels");

            migrationBuilder.DropTable(
                name: "ExamRegistrations");

            migrationBuilder.DropTable(
                name: "SubjectOfferings");

            migrationBuilder.DropTable(
                name: "PaymentType");

            migrationBuilder.DropTable(
                name: "ApplicationVoucher");

            migrationBuilder.DropTable(
                name: "ExamCenters");

            migrationBuilder.DropTable(
                name: "SemesterEnrollments");

            migrationBuilder.DropTable(
                name: "SubjectCatalogs");

            migrationBuilder.DropTable(
                name: "ExamSchedules");

            migrationBuilder.DropTable(
                name: "StudentAdmissions");

            migrationBuilder.DropTable(
                name: "SubjectTypes");

            migrationBuilder.DropTable(
                name: "ExamScheduleParent");

            migrationBuilder.DropTable(
                name: "ExamTypes");

            migrationBuilder.DropTable(
                name: "Semesters");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "StudentRegistrations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropTable(
                name: "Programs");

            migrationBuilder.DropTable(
                name: "Colleges");

            migrationBuilder.DropTable(
                name: "EntryFormats");

            migrationBuilder.DropTable(
                name: "Ethnicities");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "IndexGroups");

            migrationBuilder.DropTable(
                name: "StudentCategories");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "AcademicYears");

            migrationBuilder.DropTable(
                name: "Boards");

            migrationBuilder.DropTable(
                name: "Faculties");

            migrationBuilder.DropTable(
                name: "Levels");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "CollegeTypes");

            migrationBuilder.DropTable(
                name: "QuestionSets");

            migrationBuilder.DropTable(
                name: "LocalLevels");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Provinces");
        }
    }
}
