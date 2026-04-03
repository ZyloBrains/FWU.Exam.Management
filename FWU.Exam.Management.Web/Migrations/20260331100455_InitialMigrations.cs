using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace fwu_examination_management_system.Migrations;

/// <inheritdoc />
public partial class InitialMigrations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AcademicYears",
            columns: table => new
            {
                AcademicYearId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                AcademicYearCode = table.Column<int>(type: "integer", nullable: false),
                AcademicYearCodeNepali = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                AcademicYearName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                AcademicYearNameNepali = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Remark = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                IsRunning = table.Column<bool>(type: "boolean", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AcademicYears", x => x.AcademicYearId);
            });

        migrationBuilder.CreateTable(
            name: "Areas",
            columns: table => new
            {
                AreaId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                AreaName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Areas", x => x.AreaId);
            });

        migrationBuilder.CreateTable(
            name: "Banks",
            columns: table => new
            {
                BankId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                BankCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Banks", x => x.BankId);
            });

        migrationBuilder.CreateTable(
            name: "Boards",
            columns: table => new
            {
                BoardId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CountryId = table.Column<int>(type: "integer", nullable: false),
                BoardName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Boards", x => x.BoardId);
            });

        migrationBuilder.CreateTable(
            name: "CollegeTypes",
            columns: table => new
            {
                CollegeTypeId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CollegeTypeCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                CollegeTypeName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Remarks = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                IsDefault = table.Column<bool>(type: "boolean", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CollegeTypes", x => x.CollegeTypeId);
            });

        migrationBuilder.CreateTable(
            name: "ConnectIpsPaymentConfigurations",
            columns: table => new
            {
                ConnectIpsPaymentConfigurationId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                GatewayUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                MerchantId = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                AppId = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                AppName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                ValidationApiUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                UsernameForValidationApi = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                PasswordForValidationApi = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                PasswordForCreditorPfx = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                TransactionCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConnectIpsPaymentConfigurations", x => x.ConnectIpsPaymentConfigurationId);
            });

        migrationBuilder.CreateTable(
            name: "EntryFormats",
            columns: table => new
            {
                EntryFormatId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                EntryFormatName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EntryFormats", x => x.EntryFormatId);
            });

        migrationBuilder.CreateTable(
            name: "ESewaConfigurations",
            columns: table => new
            {
                ESewaConfigurationId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                PostUrl = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ProductCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                SecretKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                SuccessUrl = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ServiceChargeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                VerifyUrl = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ESewaConfigurations", x => x.ESewaConfigurationId);
            });

        migrationBuilder.CreateTable(
            name: "Ethnicities",
            columns: table => new
            {
                EthnicityId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                EthnicityName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Ethnicities", x => x.EthnicityId);
            });

        migrationBuilder.CreateTable(
            name: "ExamAttendanceStatuses",
            columns: table => new
            {
                ExamAttendanceStatusId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamAttendanceStatusName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamAttendanceStatuses", x => x.ExamAttendanceStatusId);
            });

        migrationBuilder.CreateTable(
            name: "ExamFormFeeNames",
            columns: table => new
            {
                ExamFormFeeNameId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                IsCollegeFee = table.Column<bool>(type: "boolean", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamFormFeeNames", x => x.ExamFormFeeNameId);
            });

        migrationBuilder.CreateTable(
            name: "ExamScheduleParents",
            columns: table => new
            {
                ExamScheduleParentId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamScheduleParentName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamScheduleParents", x => x.ExamScheduleParentId);
            });

        migrationBuilder.CreateTable(
            name: "ExamTypes",
            columns: table => new
            {
                ExamTypeId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamTypeName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                Code = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamTypes", x => x.ExamTypeId);
            });

        migrationBuilder.CreateTable(
            name: "Faculties",
            columns: table => new
            {
                FacultyId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                FacultyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                FacultyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Remarks = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Faculties", x => x.FacultyId);
            });

        migrationBuilder.CreateTable(
            name: "Genders",
            columns: table => new
            {
                GenderId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                GenderName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Genders", x => x.GenderId);
            });

        migrationBuilder.CreateTable(
            name: "IndexGroups",
            columns: table => new
            {
                IndexGroupId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                IndexGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IndexGroups", x => x.IndexGroupId);
            });

        migrationBuilder.CreateTable(
            name: "KhaltiConfigurations",
            columns: table => new
            {
                KhaltiConfigurationId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ReturnUrl = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                WebsiteUrl = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                Amount = table.Column<decimal>(type: "numeric", nullable: true),
                ProductName = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                AuthorizationKey = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                ServiceCharge = table.Column<int>(type: "integer", nullable: false),
                PostUrl = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                VerifyUrl = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_KhaltiConfigurations", x => x.KhaltiConfigurationId);
            });

        migrationBuilder.CreateTable(
            name: "Levels",
            columns: table => new
            {
                LevelId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                LevelCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                LevelName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                LevelDisplayOrder = table.Column<int>(type: "integer", nullable: true),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsRunning = table.Column<bool>(type: "boolean", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Levels", x => x.LevelId);
            });

        migrationBuilder.CreateTable(
            name: "NepaliDates",
            columns: table => new
            {
                NepaliDateId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                GregorianDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                NepaliDateShort = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                NepaliDateFull = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                NepaliDateString = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NepaliDates", x => x.NepaliDateId);
            });

        migrationBuilder.CreateTable(
            name: "Organizations",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false),
                OfficeCode = table.Column<string>(type: "text", nullable: false),
                ContactNumber = table.Column<string>(type: "text", nullable: false),
                Address = table.Column<string>(type: "text", nullable: false),
                Email = table.Column<string>(type: "text", nullable: false),
                LogoPath = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Organizations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PaymentTypes",
            columns: table => new
            {
                PaymentTypeId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                PaymentTypeName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaymentTypes", x => x.PaymentTypeId);
            });

        migrationBuilder.CreateTable(
            name: "ProgramPeriodTypes",
            columns: table => new
            {
                ProgramPeriodTypeId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProgramPeriodTypeName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                NumberOfMonths = table.Column<decimal>(type: "numeric", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProgramPeriodTypes", x => x.ProgramPeriodTypeId);
            });

        migrationBuilder.CreateTable(
            name: "Provinces",
            columns: table => new
            {
                ProvinceId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProvinceName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Provinces", x => x.ProvinceId);
            });

        migrationBuilder.CreateTable(
            name: "QuestionSets",
            columns: table => new
            {
                QuestionSetId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                QuestionSetName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QuestionSets", x => x.QuestionSetId);
            });

        migrationBuilder.CreateTable(
            name: "Roles",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SmtpConfigurations",
            columns: table => new
            {
                SmtpConfigurationId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Host = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                From = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                Port = table.Column<int>(type: "integer", nullable: false),
                UserName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                Password = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                EnableSsl = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SmtpConfigurations", x => x.SmtpConfigurationId);
            });

        migrationBuilder.CreateTable(
            name: "StudentCategories",
            columns: table => new
            {
                StudentCategoryId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                StudentCategoryName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudentCategories", x => x.StudentCategoryId);
            });

        migrationBuilder.CreateTable(
            name: "SubjectTriplicates",
            columns: table => new
            {
                SubjectTriplicateId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Year = table.Column<int>(type: "integer", nullable: false),
                RegistrationNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                School = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                Center = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                Symbol = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                Alphabet = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Grade = table.Column<int>(type: "integer", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                Sex = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                DateOfBirth = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Subject1 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Theory1 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Practical1 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Subject2 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Theory2 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Practical2 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Subject3 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Theory3 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Practical3 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Subject4 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Theory4 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Practical4 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Subject5 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Theory5 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Practical5 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Subject6 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Theory6 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Practical6 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Subject7 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Theory7 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Practical7 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Subject8 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Theory8 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Practical8 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Subject9 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Theory9 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Practical9 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Subject10 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Theory10 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Practical10 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Subject11 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Theory11 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Practical11 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubjectTriplicates", x => x.SubjectTriplicateId);
            });

        migrationBuilder.CreateTable(
            name: "SubjectTypes",
            columns: table => new
            {
                SubjectTypeId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SubjectTypeName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                MaxAllowedSubjects = table.Column<int>(type: "integer", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubjectTypes", x => x.SubjectTypeId);
            });

        migrationBuilder.CreateTable(
            name: "Batches",
            columns: table => new
            {
                BatchId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                BatchName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Remarks = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Batches", x => x.BatchId);
                table.ForeignKey(
                    name: "FK_Batches_AcademicYears_AcademicYearId",
                    column: x => x.AcademicYearId,
                    principalTable: "AcademicYears",
                    principalColumn: "AcademicYearId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExamRollNumberSetups",
            columns: table => new
            {
                ExamRollNumberSetupId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamScheduleParentId = table.Column<int>(type: "integer", nullable: false),
                FirstExamRollNumber = table.Column<int>(type: "integer", nullable: false),
                Prefix = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Suffix = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                MinimumRollNumberLength = table.Column<int>(type: "integer", nullable: false),
                Round = table.Column<int>(type: "integer", nullable: false),
                MinimumGap = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamRollNumberSetups", x => x.ExamRollNumberSetupId);
                table.ForeignKey(
                    name: "FK_ExamRollNumberSetups_ExamScheduleParents_ExamScheduleParent~",
                    column: x => x.ExamScheduleParentId,
                    principalTable: "ExamScheduleParents",
                    principalColumn: "ExamScheduleParentId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PreviousLevels",
            columns: table => new
            {
                PreviousLevelId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                PreviousLevelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                LevelId = table.Column<int>(type: "integer", nullable: true),
                LevelDisplayOrder = table.Column<int>(type: "integer", nullable: true),
                Remarks = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PreviousLevels", x => x.PreviousLevelId);
                table.ForeignKey(
                    name: "FK_PreviousLevels_Levels_LevelId",
                    column: x => x.LevelId,
                    principalTable: "Levels",
                    principalColumn: "LevelId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Programs",
            columns: table => new
            {
                ProgramsId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                LevelId = table.Column<int>(type: "integer", nullable: false),
                FacultyId = table.Column<int>(type: "integer", nullable: false),
                BoardId = table.Column<int>(type: "integer", nullable: true),
                ProgramPeriodTypeId = table.Column<int>(type: "integer", nullable: false),
                ProgramCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                ProgramName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Duration = table.Column<int>(type: "integer", nullable: false),
                GrandTotalMarks = table.Column<int>(type: "integer", nullable: true),
                HasMultipleIntakes = table.Column<bool>(type: "boolean", nullable: false),
                NumberOfSeats = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                ScholarshipSeats = table.Column<int>(type: "integer", nullable: true),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                RollNumberPrefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Programs", x => x.ProgramsId);
                table.ForeignKey(
                    name: "FK_Programs_Boards_BoardId",
                    column: x => x.BoardId,
                    principalTable: "Boards",
                    principalColumn: "BoardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Programs_Faculties_FacultyId",
                    column: x => x.FacultyId,
                    principalTable: "Faculties",
                    principalColumn: "FacultyId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Programs_Levels_LevelId",
                    column: x => x.LevelId,
                    principalTable: "Levels",
                    principalColumn: "LevelId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Programs_ProgramPeriodTypes_ProgramPeriodTypeId",
                    column: x => x.ProgramPeriodTypeId,
                    principalTable: "ProgramPeriodTypes",
                    principalColumn: "ProgramPeriodTypeId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "YearParts",
            columns: table => new
            {
                YearPartId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProgramPeriodTypeId = table.Column<int>(type: "integer", nullable: false),
                Year = table.Column<int>(type: "integer", nullable: false),
                Part = table.Column<int>(type: "integer", nullable: false),
                YearPartName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Remark = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                IsEditable = table.Column<bool>(type: "boolean", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_YearParts", x => x.YearPartId);
                table.ForeignKey(
                    name: "FK_YearParts_ProgramPeriodTypes_ProgramPeriodTypeId",
                    column: x => x.ProgramPeriodTypeId,
                    principalTable: "ProgramPeriodTypes",
                    principalColumn: "ProgramPeriodTypeId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Districts",
            columns: table => new
            {
                DistrictId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProvinceId = table.Column<int>(type: "integer", nullable: false),
                DistrictCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                DistrictName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Districts", x => x.DistrictId);
                table.ForeignKey(
                    name: "FK_Districts_Provinces_ProvinceId",
                    column: x => x.ProvinceId,
                    principalTable: "Provinces",
                    principalColumn: "ProvinceId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "RoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RoleId = table.Column<string>(type: "text", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
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
            name: "SchoolTypes",
            columns: table => new
            {
                SchoolTypeId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                PreviousLevelId = table.Column<int>(type: "integer", nullable: false),
                SchoolTypeName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SchoolTypes", x => x.SchoolTypeId);
                table.ForeignKey(
                    name: "FK_SchoolTypes_PreviousLevels_PreviousLevelId",
                    column: x => x.PreviousLevelId,
                    principalTable: "PreviousLevels",
                    principalColumn: "PreviousLevelId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProgramSubjectPracticalCharges",
            columns: table => new
            {
                ProgramSubjectPracticalChargeId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProgramsId = table.Column<int>(type: "integer", nullable: false),
                PracticalSubjectCharge = table.Column<decimal>(type: "numeric", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProgramSubjectPracticalCharges", x => x.ProgramSubjectPracticalChargeId);
                table.ForeignKey(
                    name: "FK_ProgramSubjectPracticalCharges_Programs_ProgramsId",
                    column: x => x.ProgramsId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Sections",
            columns: table => new
            {
                SectionId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SectionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                ProgramsId = table.Column<int>(type: "integer", nullable: true),
                BatchId = table.Column<int>(type: "integer", nullable: true),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sections", x => x.SectionId);
                table.ForeignKey(
                    name: "FK_Sections_Batches_BatchId",
                    column: x => x.BatchId,
                    principalTable: "Batches",
                    principalColumn: "BatchId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Sections_Programs_ProgramsId",
                    column: x => x.ProgramsId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "SubjectBatches",
            columns: table => new
            {
                SubjectBatchId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SubjectBatchName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                EffectiveAcademicYearId = table.Column<int>(type: "integer", nullable: false),
                ProgramsId = table.Column<int>(type: "integer", nullable: false),
                Remarks = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubjectBatches", x => x.SubjectBatchId);
                table.ForeignKey(
                    name: "FK_SubjectBatches_AcademicYears_EffectiveAcademicYearId",
                    column: x => x.EffectiveAcademicYearId,
                    principalTable: "AcademicYears",
                    principalColumn: "AcademicYearId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_SubjectBatches_Programs_ProgramsId",
                    column: x => x.ProgramsId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExamSchedules",
            columns: table => new
            {
                ExamScheduleId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                LevelId = table.Column<int>(type: "integer", nullable: false),
                YearPartId = table.Column<int>(type: "integer", nullable: false),
                ExamTypeId = table.Column<int>(type: "integer", nullable: false),
                ExamScheduleName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                StartDateAd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                EndDateAd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                StartDateBs = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                EndDateBs = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                PublishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                ExamScheduleParentId = table.Column<int>(type: "integer", nullable: true),
                NegativeMarks = table.Column<int>(type: "integer", nullable: true),
                ProgramIds = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                RegularBatchIds = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                PartialBatchIds = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                ExtendedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ExtendedDateCharge = table.Column<decimal>(type: "numeric", nullable: true),
                CollegeApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                AdmissionCardReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ExamScheduleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamSchedules", x => x.ExamScheduleId);
                table.ForeignKey(
                    name: "FK_ExamSchedules_AcademicYears_AcademicYearId",
                    column: x => x.AcademicYearId,
                    principalTable: "AcademicYears",
                    principalColumn: "AcademicYearId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamSchedules_ExamScheduleParents_ExamScheduleParentId",
                    column: x => x.ExamScheduleParentId,
                    principalTable: "ExamScheduleParents",
                    principalColumn: "ExamScheduleParentId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamSchedules_ExamTypes_ExamTypeId",
                    column: x => x.ExamTypeId,
                    principalTable: "ExamTypes",
                    principalColumn: "ExamTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamSchedules_Levels_LevelId",
                    column: x => x.LevelId,
                    principalTable: "Levels",
                    principalColumn: "LevelId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamSchedules_YearParts_YearPartId",
                    column: x => x.YearPartId,
                    principalTable: "YearParts",
                    principalColumn: "YearPartId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProgramYearParts",
            columns: table => new
            {
                ProgramYearPartId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProgramsId = table.Column<int>(type: "integer", nullable: false),
                YearPartId = table.Column<int>(type: "integer", nullable: false),
                TotalMarks = table.Column<int>(type: "integer", nullable: false),
                TotalPassMarks = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProgramYearParts", x => x.ProgramYearPartId);
                table.ForeignKey(
                    name: "FK_ProgramYearParts_Programs_ProgramsId",
                    column: x => x.ProgramsId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProgramYearParts_YearParts_YearPartId",
                    column: x => x.YearPartId,
                    principalTable: "YearParts",
                    principalColumn: "YearPartId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "SubjectGroups",
            columns: table => new
            {
                SubjectGroupId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProgramsId = table.Column<int>(type: "integer", nullable: false),
                SubjectGroupName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                SubjectGroupShortName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                YearPartId = table.Column<int>(type: "integer", nullable: false),
                IsExtraAllowed = table.Column<bool>(type: "boolean", nullable: true),
                IsCompulsory = table.Column<bool>(type: "boolean", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubjectGroups", x => x.SubjectGroupId);
                table.ForeignKey(
                    name: "FK_SubjectGroups_Programs_ProgramsId",
                    column: x => x.ProgramsId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_SubjectGroups_YearParts_YearPartId",
                    column: x => x.YearPartId,
                    principalTable: "YearParts",
                    principalColumn: "YearPartId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Colleges",
            columns: table => new
            {
                CollegeId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CollegeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                CollegeName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                CollegeNameNepali = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                ShortName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                EstablishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ClosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                DistrictId = table.Column<int>(type: "integer", nullable: false),
                MunicipalityVdc = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                WardNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                HouseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Website = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Phone1 = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                Phone2 = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                PrincipalName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                PrincipalContactNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Fax = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsExamCenterOnly = table.Column<bool>(type: "boolean", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CollegeTypeId = table.Column<int>(type: "integer", nullable: true),
                AllocatedAmount = table.Column<decimal>(type: "numeric", nullable: true),
                AreaId = table.Column<int>(type: "integer", nullable: false),
                DisplayOrder = table.Column<int>(type: "integer", nullable: true),
                QuestionSetId = table.Column<int>(type: "integer", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Colleges", x => x.CollegeId);
                table.ForeignKey(
                    name: "FK_Colleges_Areas_AreaId",
                    column: x => x.AreaId,
                    principalTable: "Areas",
                    principalColumn: "AreaId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Colleges_CollegeTypes_CollegeTypeId",
                    column: x => x.CollegeTypeId,
                    principalTable: "CollegeTypes",
                    principalColumn: "CollegeTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Colleges_Districts_DistrictId",
                    column: x => x.DistrictId,
                    principalTable: "Districts",
                    principalColumn: "DistrictId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Colleges_QuestionSets_QuestionSetId",
                    column: x => x.QuestionSetId,
                    principalTable: "QuestionSets",
                    principalColumn: "QuestionSetId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "LocalLevels",
            columns: table => new
            {
                LocalLevelId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                DistrictId = table.Column<int>(type: "integer", nullable: false),
                LocalLevelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Remark = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LocalLevels", x => x.LocalLevelId);
                table.ForeignKey(
                    name: "FK_LocalLevels_Districts_DistrictId",
                    column: x => x.DistrictId,
                    principalTable: "Districts",
                    principalColumn: "DistrictId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ActiveExamSchedules",
            columns: table => new
            {
                ActiveExamScheduleId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: false),
                StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                OpenTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                EndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                Remarks = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ActiveExamSchedules", x => x.ActiveExamScheduleId);
                table.ForeignKey(
                    name: "FK_ActiveExamSchedules_ExamSchedules_ExamScheduleId",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "BillTitles",
            columns: table => new
            {
                BillTitleId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                BillTitleName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Category = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                Amount = table.Column<decimal>(type: "numeric", nullable: true),
                ThroughDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ApplicableDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BillTitles", x => x.BillTitleId);
                table.ForeignKey(
                    name: "FK_BillTitles_ExamSchedules_ExamScheduleId",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExamFormFeeRates",
            columns: table => new
            {
                ExamFormFeeRateId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: false),
                ExamFormFeeNameId = table.Column<int>(type: "integer", nullable: false),
                Amount = table.Column<decimal>(type: "numeric", nullable: false),
                CollegeTypeId = table.Column<int>(type: "integer", nullable: true),
                ExamTypeId = table.Column<int>(type: "integer", nullable: true),
                ThroughDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ApplicableDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IsCollegeFee = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamFormFeeRates", x => x.ExamFormFeeRateId);
                table.ForeignKey(
                    name: "FK_ExamFormFeeRates_CollegeTypes_CollegeTypeId",
                    column: x => x.CollegeTypeId,
                    principalTable: "CollegeTypes",
                    principalColumn: "CollegeTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamFormFeeRates_ExamFormFeeNames_ExamFormFeeNameId",
                    column: x => x.ExamFormFeeNameId,
                    principalTable: "ExamFormFeeNames",
                    principalColumn: "ExamFormFeeNameId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamFormFeeRates_ExamSchedules_ExamScheduleId",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamFormFeeRates_ExamTypes_ExamTypeId",
                    column: x => x.ExamTypeId,
                    principalTable: "ExamTypes",
                    principalColumn: "ExamTypeId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExamScheduleBatches",
            columns: table => new
            {
                ExamScheduleBatchId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: false),
                ExamTypeId = table.Column<int>(type: "integer", nullable: false),
                BatchId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamScheduleBatches", x => x.ExamScheduleBatchId);
                table.ForeignKey(
                    name: "FK_ExamScheduleBatches_Batches_BatchId",
                    column: x => x.BatchId,
                    principalTable: "Batches",
                    principalColumn: "BatchId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamScheduleBatches_ExamSchedules_ExamScheduleId",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamScheduleBatches_ExamTypes_ExamTypeId",
                    column: x => x.ExamTypeId,
                    principalTable: "ExamTypes",
                    principalColumn: "ExamTypeId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "SubjectDetails",
            columns: table => new
            {
                SubjectDetailId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SubjectGroupId = table.Column<int>(type: "integer", nullable: true),
                ProgramsId = table.Column<int>(type: "integer", nullable: false),
                YearPartId = table.Column<int>(type: "integer", nullable: false),
                SubjectCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                SubjectName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                TheoryFullMarks = table.Column<decimal>(type: "numeric", nullable: false),
                TheoryPassMarks = table.Column<decimal>(type: "numeric", nullable: false),
                PracticalFullMarks = table.Column<decimal>(type: "numeric", nullable: true),
                PracticalPassMarks = table.Column<decimal>(type: "numeric", nullable: true),
                InternalTheoryFullMarks = table.Column<decimal>(type: "numeric", nullable: true),
                InternalTheoryPassMarks = table.Column<decimal>(type: "numeric", nullable: true),
                InternalPracticalFullMarks = table.Column<decimal>(type: "numeric", nullable: true),
                InternalPracticalPassMarks = table.Column<decimal>(type: "numeric", nullable: true),
                CreditHours = table.Column<int>(type: "integer", nullable: true),
                HasPractical = table.Column<bool>(type: "boolean", nullable: false),
                HasInternal = table.Column<bool>(type: "boolean", nullable: false),
                DisplayOrder = table.Column<int>(type: "integer", nullable: true),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                IsCompulsory = table.Column<bool>(type: "boolean", nullable: false),
                ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                ConcurrentSubjectCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                SubjectTypeId = table.Column<int>(type: "integer", nullable: false),
                HasTheory = table.Column<bool>(type: "boolean", nullable: false),
                Year = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Part = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubjectDetails", x => x.SubjectDetailId);
                table.ForeignKey(
                    name: "FK_SubjectDetails_Programs_ProgramsId",
                    column: x => x.ProgramsId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_SubjectDetails_SubjectGroups_SubjectGroupId",
                    column: x => x.SubjectGroupId,
                    principalTable: "SubjectGroups",
                    principalColumn: "SubjectGroupId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_SubjectDetails_SubjectTypes_SubjectTypeId",
                    column: x => x.SubjectTypeId,
                    principalTable: "SubjectTypes",
                    principalColumn: "SubjectTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_SubjectDetails_YearParts_YearPartId",
                    column: x => x.YearPartId,
                    principalTable: "YearParts",
                    principalColumn: "YearPartId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CollegePrograms",
            columns: table => new
            {
                CollegeProgramId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CollegeId = table.Column<int>(type: "integer", nullable: false),
                ProgramsId = table.Column<int>(type: "integer", nullable: false),
                AffiliationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                NumberOfStudents = table.Column<int>(type: "integer", nullable: false),
                Remarks = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CollegePrograms", x => x.CollegeProgramId);
                table.ForeignKey(
                    name: "FK_CollegePrograms_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CollegePrograms_Programs_ProgramsId",
                    column: x => x.ProgramsId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExamCenters",
            columns: table => new
            {
                ExamCenterId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: false),
                CollegeId = table.Column<int>(type: "integer", nullable: false),
                Remark = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                Code = table.Column<int>(type: "integer", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamCenters", x => x.ExamCenterId);
                table.ForeignKey(
                    name: "FK_ExamCenters_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamCenters_ExamSchedules_ExamScheduleId",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExamRollNumberSetupDetails",
            columns: table => new
            {
                ExamRollNumberSetupDetailId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamRollNumberSetupId = table.Column<int>(type: "integer", nullable: false),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: false),
                ProgramId = table.Column<int>(type: "integer", nullable: false),
                ExamTypeId = table.Column<int>(type: "integer", nullable: false),
                CollegeId = table.Column<int>(type: "integer", nullable: false),
                StartRollNumber = table.Column<int>(type: "integer", nullable: false),
                EndRollNumber = table.Column<int>(type: "integer", nullable: false),
                Count = table.Column<int>(type: "integer", nullable: false),
                Prefix = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Suffix = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamRollNumberSetupDetails", x => x.ExamRollNumberSetupDetailId);
                table.ForeignKey(
                    name: "FK_ExamRollNumberSetupDetails_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamRollNumberSetupDetails_ExamRollNumberSetups_ExamRollNum~",
                    column: x => x.ExamRollNumberSetupId,
                    principalTable: "ExamRollNumberSetups",
                    principalColumn: "ExamRollNumberSetupId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamRollNumberSetupDetails_ExamSchedules_ExamScheduleId",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamRollNumberSetupDetails_ExamTypes_ExamTypeId",
                    column: x => x.ExamTypeId,
                    principalTable: "ExamTypes",
                    principalColumn: "ExamTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamRollNumberSetupDetails_Programs_ProgramId",
                    column: x => x.ProgramId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PreferredExamCenters",
            columns: table => new
            {
                PreferredExamCenterId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                CollegeId = table.Column<int>(type: "integer", nullable: true),
                Remarks = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PreferredExamCenters", x => x.PreferredExamCenterId);
                table.ForeignKey(
                    name: "FK_PreferredExamCenters_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExamScheduleDetails",
            columns: table => new
            {
                ExamScheduleDetailId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: false),
                ExamTypeId = table.Column<int>(type: "integer", nullable: false),
                SubjectDetailId = table.Column<int>(type: "integer", nullable: false),
                ExamDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ExamDateBs = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamScheduleDetails", x => x.ExamScheduleDetailId);
                table.ForeignKey(
                    name: "FK_ExamScheduleDetails_ExamSchedules_ExamScheduleId",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamScheduleDetails_ExamTypes_ExamTypeId",
                    column: x => x.ExamTypeId,
                    principalTable: "ExamTypes",
                    principalColumn: "ExamTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamScheduleDetails_SubjectDetails_SubjectDetailId",
                    column: x => x.SubjectDetailId,
                    principalTable: "SubjectDetails",
                    principalColumn: "SubjectDetailId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ResultRecords",
            columns: table => new
            {
                ResultRecordId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                ProgramsId = table.Column<int>(type: "integer", nullable: false),
                ExamTypeId = table.Column<int>(type: "integer", nullable: false),
                CollegeId = table.Column<int>(type: "integer", nullable: false),
                SubjectDetailId = table.Column<int>(type: "integer", nullable: false),
                Year = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                Part = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                SymbolNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Alphabet = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                DateOfBirthBs = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                Sex = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                TheoryObtainedMarks = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                InternalObtainedMarks = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                PracticalObtainedMarks = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                TheoryObtainedGrade = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                InternalObtainedGrade = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                PracticalObtainedGrade = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                TotalObtainedMarks = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                TotalObtainedGrade = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                TotalGradePoints = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                Gpa = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                StudentName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                ResultRecordMasterId = table.Column<int>(type: "integer", nullable: false),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ResultRecords", x => x.ResultRecordId);
                table.ForeignKey(
                    name: "FK_ResultRecords_AcademicYears_AcademicYearId",
                    column: x => x.AcademicYearId,
                    principalTable: "AcademicYears",
                    principalColumn: "AcademicYearId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ResultRecords_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ResultRecords_ExamSchedules_ExamScheduleId",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ResultRecords_ExamTypes_ExamTypeId",
                    column: x => x.ExamTypeId,
                    principalTable: "ExamTypes",
                    principalColumn: "ExamTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ResultRecords_Programs_ProgramsId",
                    column: x => x.ProgramsId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ResultRecords_SubjectDetails_SubjectDetailId",
                    column: x => x.SubjectDetailId,
                    principalTable: "SubjectDetails",
                    principalColumn: "SubjectDetailId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "SubjectGroupDetailMaps",
            columns: table => new
            {
                SubjectGroupId = table.Column<int>(type: "integer", nullable: false),
                SubjectDetailId = table.Column<int>(type: "integer", nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubjectGroupDetailMaps", x => new { x.SubjectGroupId, x.SubjectDetailId });
                table.ForeignKey(
                    name: "FK_SubjectGroupDetailMaps_SubjectDetails_SubjectDetailId",
                    column: x => x.SubjectDetailId,
                    principalTable: "SubjectDetails",
                    principalColumn: "SubjectDetailId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_SubjectGroupDetailMaps_SubjectGroups_SubjectGroupId",
                    column: x => x.SubjectGroupId,
                    principalTable: "SubjectGroups",
                    principalColumn: "SubjectGroupId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExamCenterDetails",
            columns: table => new
            {
                ExamCenterDetailId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamCenterId = table.Column<int>(type: "integer", nullable: false),
                CollegeId = table.Column<int>(type: "integer", nullable: false),
                ProgramsId = table.Column<int>(type: "integer", nullable: true),
                RollNumberFrom = table.Column<long>(type: "bigint", nullable: false),
                RollNumberTo = table.Column<long>(type: "bigint", nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamCenterDetails", x => x.ExamCenterDetailId);
                table.ForeignKey(
                    name: "FK_ExamCenterDetails_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamCenterDetails_ExamCenters_ExamCenterId",
                    column: x => x.ExamCenterId,
                    principalTable: "ExamCenters",
                    principalColumn: "ExamCenterId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamCenterDetails_Programs_ProgramsId",
                    column: x => x.ProgramsId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ApplicationVouchers",
            columns: table => new
            {
                ApplicationVoucherId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                VoucherNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                StudentName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                DateOfBirthAd = table.Column<DateOnly>(type: "date", nullable: true),
                DateOfBirthBs = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Amount = table.Column<decimal>(type: "numeric", nullable: false),
                VoucherDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ContactNumber = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                Branch = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: false),
                StudentRegistrationId = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApplicationVouchers", x => x.ApplicationVoucherId);
                table.ForeignKey(
                    name: "FK_ApplicationVouchers_ExamSchedules_ExamScheduleId",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "BankVouchers",
            columns: table => new
            {
                BankVoucherId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                CollegeId = table.Column<int>(type: "integer", nullable: false),
                BillTitleId = table.Column<int>(type: "integer", nullable: false),
                BankId = table.Column<int>(type: "integer", nullable: false),
                BankAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                VoucherDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                VoucherNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                VoucherAmount = table.Column<decimal>(type: "numeric", nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                BankVoucherUserAttachmentId = table.Column<int>(type: "integer", nullable: true),
                ExamScheduleParentId = table.Column<int>(type: "integer", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BankVouchers", x => x.BankVoucherId);
                table.ForeignKey(
                    name: "FK_BankVouchers_AcademicYears_AcademicYearId",
                    column: x => x.AcademicYearId,
                    principalTable: "AcademicYears",
                    principalColumn: "AcademicYearId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_BankVouchers_Banks_BankId",
                    column: x => x.BankId,
                    principalTable: "Banks",
                    principalColumn: "BankId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_BankVouchers_BillTitles_BillTitleId",
                    column: x => x.BillTitleId,
                    principalTable: "BillTitles",
                    principalColumn: "BillTitleId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_BankVouchers_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_BankVouchers_ExamScheduleParents_ExamScheduleParentId",
                    column: x => x.ExamScheduleParentId,
                    principalTable: "ExamScheduleParents",
                    principalColumn: "ExamScheduleParentId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CollegeProfiles",
            columns: table => new
            {
                CollegeProfileId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CollegeId = table.Column<int>(type: "integer", nullable: false),
                BankName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                BankBranchName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                BankAccountNumber = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                ContactPersonName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                ContactPersonMobileNumber = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                ContactPersonEmail = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                BlankChequeUserAttachmentId = table.Column<int>(type: "integer", nullable: false),
                AuditReportUserAttachmentId = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CollegeProfiles", x => x.CollegeProfileId);
                table.ForeignKey(
                    name: "FK_CollegeProfiles_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ExamRegistrationActionLogs",
            columns: table => new
            {
                ExamRegistrationActionLogId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamRegistrationId = table.Column<int>(type: "integer", nullable: false),
                Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Remarks = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamRegistrationActionLogs", x => x.ExamRegistrationActionLogId);
            });

        migrationBuilder.CreateTable(
            name: "ExamRegistrationCenterChanges",
            columns: table => new
            {
                ExamRegistrationCenterChangeId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamRegistrationId = table.Column<int>(type: "integer", nullable: false),
                PreferredExamCenterId = table.Column<int>(type: "integer", nullable: false),
                RequestedTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CurrentExamCenterId = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamRegistrationCenterChanges", x => x.ExamRegistrationCenterChangeId);
                table.ForeignKey(
                    name: "FK_ExamRegistrationCenterChanges_PreferredExamCenters_Preferre~",
                    column: x => x.PreferredExamCenterId,
                    principalTable: "PreferredExamCenters",
                    principalColumn: "PreferredExamCenterId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExamRegistrations",
            columns: table => new
            {
                ExamRegistrationId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                StudentProgramYearPartId = table.Column<int>(type: "integer", nullable: false),
                AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                ExamCenterId = table.Column<int>(type: "integer", nullable: true),
                CollegeId = table.Column<int>(type: "integer", nullable: false),
                ExamRollNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                ExamRollNumberCoding = table.Column<long>(type: "bigint", nullable: true),
                FeeEnclosed = table.Column<decimal>(type: "numeric", nullable: true),
                AttendancePercentage = table.Column<decimal>(type: "numeric", nullable: true),
                RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IsVerifiedByCollege = table.Column<bool>(type: "boolean", nullable: true),
                VerifiedBy = table.Column<int>(type: "integer", nullable: true),
                VerifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IsWithheld = table.Column<bool>(type: "boolean", nullable: true),
                Sgpa = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                IsExamRegistered = table.Column<bool>(type: "boolean", nullable: true),
                TypeId = table.Column<int>(type: "integer", nullable: true),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: false),
                RollNumberIndex = table.Column<int>(type: "integer", nullable: true),
                IsAppliedByStudent = table.Column<bool>(type: "boolean", nullable: true),
                ProgramsId = table.Column<int>(type: "integer", nullable: true),
                ApplicationVoucherId = table.Column<int>(type: "integer", nullable: true),
                AdminVerifiedBy = table.Column<int>(type: "integer", nullable: true),
                AdminVerifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamRegistrations", x => x.ExamRegistrationId);
                table.ForeignKey(
                    name: "FK_ExamRegistrations_AcademicYears_AcademicYearId",
                    column: x => x.AcademicYearId,
                    principalTable: "AcademicYears",
                    principalColumn: "AcademicYearId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamRegistrations_ApplicationVouchers_ApplicationVoucherId",
                    column: x => x.ApplicationVoucherId,
                    principalTable: "ApplicationVouchers",
                    principalColumn: "ApplicationVoucherId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamRegistrations_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamRegistrations_ExamCenters_ExamCenterId",
                    column: x => x.ExamCenterId,
                    principalTable: "ExamCenters",
                    principalColumn: "ExamCenterId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamRegistrations_ExamSchedules_ExamScheduleId",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamRegistrations_Programs_ProgramsId",
                    column: x => x.ProgramsId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExamSubjectRegistrations",
            columns: table => new
            {
                ExamSubjectRegistrationId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamRegistrationId = table.Column<int>(type: "integer", nullable: false),
                SubjectDetailId = table.Column<int>(type: "integer", nullable: false),
                ExamTypeId = table.Column<int>(type: "integer", nullable: false),
                ObtainedMarksTheory = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                ObtainedMarksTheoryConfirm = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                ObtainedMarksPractical = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                ObtainedMarksPracticalConfirm = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                GradeLetter = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                IsLooseEntry = table.Column<bool>(type: "boolean", nullable: true),
                CreatedByTab1 = table.Column<int>(type: "integer", nullable: true),
                CreatedDateTab1 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ModifiedByTab1 = table.Column<int>(type: "integer", nullable: true),
                ModifiedDateTab1 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedByTab2 = table.Column<int>(type: "integer", nullable: true),
                CreatedDateTab2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ModifiedByTab2 = table.Column<int>(type: "integer", nullable: true),
                ModifiedDateTab2 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IsTheoryRegistered = table.Column<bool>(type: "boolean", nullable: true),
                IsPracticalRegistered = table.Column<bool>(type: "boolean", nullable: true),
                IsExtra = table.Column<bool>(type: "boolean", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamSubjectRegistrations", x => x.ExamSubjectRegistrationId);
                table.ForeignKey(
                    name: "FK_ExamSubjectRegistrations_ExamRegistrations_ExamRegistration~",
                    column: x => x.ExamRegistrationId,
                    principalTable: "ExamRegistrations",
                    principalColumn: "ExamRegistrationId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamSubjectRegistrations_ExamTypes_ExamTypeId",
                    column: x => x.ExamTypeId,
                    principalTable: "ExamTypes",
                    principalColumn: "ExamTypeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamSubjectRegistrations_SubjectDetails_SubjectDetailId",
                    column: x => x.SubjectDetailId,
                    principalTable: "SubjectDetails",
                    principalColumn: "SubjectDetailId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExamSubjectRegistrationExamSessions",
            columns: table => new
            {
                ExamSubjectRegistrationExamSessionId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ExamSubjectRegistrationId = table.Column<int>(type: "integer", nullable: false),
                ExamStartedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsSubmitted = table.Column<bool>(type: "boolean", nullable: false),
                ObtainedMarks = table.Column<decimal>(type: "numeric", nullable: true),
                ExamSubmittedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IsAutoSubmitted = table.Column<bool>(type: "boolean", nullable: true),
                LastStatusSyncDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamSubjectRegistrationExamSessions", x => x.ExamSubjectRegistrationExamSessionId);
                table.ForeignKey(
                    name: "FK_ExamSubjectRegistrationExamSessions_ExamSubjectRegistration~",
                    column: x => x.ExamSubjectRegistrationId,
                    principalTable: "ExamSubjectRegistrations",
                    principalColumn: "ExamSubjectRegistrationId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ExamSubjectRegistrationInternals",
            columns: table => new
            {
                ExamSubjectRegistrationInternalId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                EntryAcademicYearId = table.Column<int>(type: "integer", nullable: false),
                StudentProgramYearPartId = table.Column<int>(type: "integer", nullable: false),
                SubjectDetailId = table.Column<int>(type: "integer", nullable: false),
                ObtainedMarksTheoryInternal = table.Column<decimal>(type: "numeric", nullable: true),
                ObtainedMarksPracticalInternal = table.Column<decimal>(type: "numeric", nullable: true),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExamSubjectRegistrationInternals", x => x.ExamSubjectRegistrationInternalId);
                table.ForeignKey(
                    name: "FK_ExamSubjectRegistrationInternals_AcademicYears_EntryAcademi~",
                    column: x => x.EntryAcademicYearId,
                    principalTable: "AcademicYears",
                    principalColumn: "AcademicYearId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamSubjectRegistrationInternals_ExamSchedules_ExamSchedule~",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ExamSubjectRegistrationInternals_SubjectDetails_SubjectDeta~",
                    column: x => x.SubjectDetailId,
                    principalTable: "SubjectDetails",
                    principalColumn: "SubjectDetailId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PasswordResetLogs",
            columns: table => new
            {
                PasswordResetLogId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false),
                Browser = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                Device = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                IpAddress = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                PasswordChangedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PasswordResetLogs", x => x.PasswordResetLogId);
            });

        migrationBuilder.CreateTable(
            name: "PaymentPracticalSubjects",
            columns: table => new
            {
                PaymentPracticalSubjectsId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                PaymentRequestLogId = table.Column<int>(type: "integer", nullable: false),
                PracticalSubjectsCount = table.Column<int>(type: "integer", nullable: false),
                TotalAmount = table.Column<decimal>(type: "numeric", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaymentPracticalSubjects", x => x.PaymentPracticalSubjectsId);
            });

        migrationBuilder.CreateTable(
            name: "PaymentRequestLogs",
            columns: table => new
            {
                PaymentRequestLogId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                PaymentRequestLogStatus = table.Column<int>(type: "integer", nullable: true),
                InvoiceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                ForwardedTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                DateOfBirthAd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                MobileNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                FullName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Amount = table.Column<decimal>(type: "numeric", nullable: false),
                FullRequestContent = table.Column<string>(type: "text", nullable: false),
                PaymentTypeId = table.Column<int>(type: "integer", nullable: false),
                StudentRegistrationId = table.Column<int>(type: "integer", nullable: true),
                ExamScheduleId = table.Column<int>(type: "integer", nullable: false),
                TransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                CollegeId = table.Column<int>(type: "integer", nullable: true),
                StudentCount = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaymentRequestLogs", x => x.PaymentRequestLogId);
                table.ForeignKey(
                    name: "FK_PaymentRequestLogs_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId");
                table.ForeignKey(
                    name: "FK_PaymentRequestLogs_ExamSchedules_ExamScheduleId",
                    column: x => x.ExamScheduleId,
                    principalTable: "ExamSchedules",
                    principalColumn: "ExamScheduleId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PaymentRequestLogs_PaymentTypes_PaymentTypeId",
                    column: x => x.PaymentTypeId,
                    principalTable: "PaymentTypes",
                    principalColumn: "PaymentTypeId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PaymentResponseLogs",
            columns: table => new
            {
                PaymentResponseLogId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                PaymentRequestLogId = table.Column<int>(type: "integer", nullable: false),
                ResponseTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                ResponseMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                FullResponse = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaymentResponseLogs", x => x.PaymentResponseLogId);
                table.ForeignKey(
                    name: "FK_PaymentResponseLogs_PaymentRequestLogs_PaymentRequestLogId",
                    column: x => x.PaymentRequestLogId,
                    principalTable: "PaymentRequestLogs",
                    principalColumn: "PaymentRequestLogId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StudentAdmissions",
            columns: table => new
            {
                StudentAdmissionId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                BatchId = table.Column<int>(type: "integer", nullable: false),
                StudentRegistrationId = table.Column<int>(type: "integer", nullable: false),
                ProgramsId = table.Column<int>(type: "integer", nullable: false),
                CollegeId = table.Column<int>(type: "integer", nullable: false),
                SectionId = table.Column<int>(type: "integer", nullable: true),
                AdmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CheckedBy = table.Column<int>(type: "integer", nullable: true),
                IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                Cgpa = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CollegeRollNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                RepeatBatchId = table.Column<int>(type: "integer", nullable: true),
                SubjectGroupId = table.Column<int>(type: "integer", nullable: true),
                HasFeeExemption = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudentAdmissions", x => x.StudentAdmissionId);
                table.ForeignKey(
                    name: "FK_StudentAdmissions_Batches_BatchId",
                    column: x => x.BatchId,
                    principalTable: "Batches",
                    principalColumn: "BatchId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentAdmissions_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentAdmissions_Programs_ProgramsId",
                    column: x => x.ProgramsId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentAdmissions_Sections_SectionId",
                    column: x => x.SectionId,
                    principalTable: "Sections",
                    principalColumn: "SectionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentAdmissions_SubjectGroups_SubjectGroupId",
                    column: x => x.SubjectGroupId,
                    principalTable: "SubjectGroups",
                    principalColumn: "SubjectGroupId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "StudentProgramYearParts",
            columns: table => new
            {
                StudentProgramYearPartId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                StudentAdmissionId = table.Column<int>(type: "integer", nullable: false),
                AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                YearPartId = table.Column<int>(type: "integer", nullable: false),
                IsRunning = table.Column<bool>(type: "boolean", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudentProgramYearParts", x => x.StudentProgramYearPartId);
                table.ForeignKey(
                    name: "FK_StudentProgramYearParts_AcademicYears_AcademicYearId",
                    column: x => x.AcademicYearId,
                    principalTable: "AcademicYears",
                    principalColumn: "AcademicYearId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentProgramYearParts_StudentAdmissions_StudentAdmissionId",
                    column: x => x.StudentAdmissionId,
                    principalTable: "StudentAdmissions",
                    principalColumn: "StudentAdmissionId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentProgramYearParts_YearParts_YearPartId",
                    column: x => x.YearPartId,
                    principalTable: "YearParts",
                    principalColumn: "YearPartId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "StudentGuardians",
            columns: table => new
            {
                StudentGuardianId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                StudentRegistrationId = table.Column<int>(type: "integer", nullable: false),
                FatherName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                FatherContactNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                FatherPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                FatherEmail = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                FatherQualification = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                FatherProfession = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                FatherAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                FatherOrganization = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                FatherOrganizationAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                MotherName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                MotherContactNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                MotherPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                MotherEmail = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                MotherQualification = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                MotherProfession = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                MotherAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                MotherOrganization = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                MotherOrganizationAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                GuardianName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                GuardianContactNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                GuardianPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                GuardianEmail = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                GuardianQualification = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                GuardianProfession = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                GuardianAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                GuardianOrganization = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                GuardianOrganizationAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                RelationWithStudent = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudentGuardians", x => x.StudentGuardianId);
            });

        migrationBuilder.CreateTable(
            name: "StudentQualifications",
            columns: table => new
            {
                StudentQualificationId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                StudentRegistrationId = table.Column<int>(type: "integer", nullable: false),
                BoardId = table.Column<int>(type: "integer", nullable: false),
                PreviousLevelId = table.Column<int>(type: "integer", nullable: false),
                ProgramName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                InstituteName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                PassedYear = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Specialization = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                Percentage = table.Column<decimal>(type: "numeric", nullable: true),
                TotalCredits = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Remarks = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                IsHigherDegree = table.Column<bool>(type: "boolean", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                ExamRollNumber = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudentQualifications", x => x.StudentQualificationId);
                table.ForeignKey(
                    name: "FK_StudentQualifications_Boards_BoardId",
                    column: x => x.BoardId,
                    principalTable: "Boards",
                    principalColumn: "BoardId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentQualifications_PreviousLevels_PreviousLevelId",
                    column: x => x.PreviousLevelId,
                    principalTable: "PreviousLevels",
                    principalColumn: "PreviousLevelId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "StudentRegistrations",
            columns: table => new
            {
                StudentRegistrationId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                LevelId = table.Column<int>(type: "integer", nullable: false),
                FacultyId = table.Column<int>(type: "integer", nullable: false),
                CollegeId = table.Column<int>(type: "integer", nullable: false),
                RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                FirstName = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                MiddleName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                LastName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                NepaliName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                ContactNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                Phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                DateOfBirthBs = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                DateOfBirthAd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                GenderId = table.Column<int>(type: "integer", nullable: false),
                IndexGroupId = table.Column<int>(type: "integer", nullable: true),
                BloodGroup = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                Nationality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Religion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                DistrictId = table.Column<int>(type: "integer", nullable: false),
                MunicipalityVdc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                WardNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                StudentRegistrationIndex = table.Column<int>(type: "integer", nullable: true),
                StudentCategoryId = table.Column<int>(type: "integer", nullable: false),
                VerifiedBy = table.Column<int>(type: "integer", nullable: true),
                VerifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                PhotoAttachmentId = table.Column<int>(type: "integer", nullable: true),
                EthnicityId = table.Column<int>(type: "integer", nullable: true),
                EntranceRollNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                EntryFormatId = table.Column<int>(type: "integer", nullable: true),
                IsRegistrationNumberGenerated = table.Column<bool>(type: "boolean", nullable: true),
                RowIndex = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                PreviousAcademicYear = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                PreviousSymbolNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                StudentRegistrationSearchId = table.Column<int>(type: "integer", nullable: true),
                LocalLevelId = table.Column<int>(type: "integer", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudentRegistrations", x => x.StudentRegistrationId);
                table.ForeignKey(
                    name: "FK_StudentRegistrations_AcademicYears_AcademicYearId",
                    column: x => x.AcademicYearId,
                    principalTable: "AcademicYears",
                    principalColumn: "AcademicYearId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentRegistrations_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentRegistrations_Districts_DistrictId",
                    column: x => x.DistrictId,
                    principalTable: "Districts",
                    principalColumn: "DistrictId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentRegistrations_EntryFormats_EntryFormatId",
                    column: x => x.EntryFormatId,
                    principalTable: "EntryFormats",
                    principalColumn: "EntryFormatId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentRegistrations_Ethnicities_EthnicityId",
                    column: x => x.EthnicityId,
                    principalTable: "Ethnicities",
                    principalColumn: "EthnicityId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentRegistrations_Faculties_FacultyId",
                    column: x => x.FacultyId,
                    principalTable: "Faculties",
                    principalColumn: "FacultyId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentRegistrations_Genders_GenderId",
                    column: x => x.GenderId,
                    principalTable: "Genders",
                    principalColumn: "GenderId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentRegistrations_IndexGroups_IndexGroupId",
                    column: x => x.IndexGroupId,
                    principalTable: "IndexGroups",
                    principalColumn: "IndexGroupId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentRegistrations_Levels_LevelId",
                    column: x => x.LevelId,
                    principalTable: "Levels",
                    principalColumn: "LevelId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentRegistrations_LocalLevels_LocalLevelId",
                    column: x => x.LocalLevelId,
                    principalTable: "LocalLevels",
                    principalColumn: "LocalLevelId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_StudentRegistrations_StudentCategories_StudentCategoryId",
                    column: x => x.StudentCategoryId,
                    principalTable: "StudentCategories",
                    principalColumn: "StudentCategoryId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                ProfilePath = table.Column<string>(type: "text", nullable: true),
                OrganizationId = table.Column<int>(type: "integer", nullable: true),
                FullName = table.Column<string>(type: "text", nullable: true),
                Designation = table.Column<string>(type: "text", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                NtUser = table.Column<string>(type: "text", nullable: true),
                Remarks = table.Column<string>(type: "text", nullable: true),
                Active = table.Column<bool>(type: "boolean", nullable: false),
                PasswordResetLogId = table.Column<int>(type: "integer", nullable: true),
                ContactNumber = table.Column<string>(type: "text", nullable: true),
                StudentRegistrationId = table.Column<int>(type: "integer", nullable: true),
                LastPasswordChanged = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CollegeId = table.Column<int>(type: "integer", nullable: true),
                UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: true),
                SecurityStamp = table.Column<string>(type: "text", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                PhoneNumber = table.Column<string>(type: "text", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
                table.ForeignKey(
                    name: "FK_Users_Colleges_CollegeId",
                    column: x => x.CollegeId,
                    principalTable: "Colleges",
                    principalColumn: "CollegeId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Users_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Users_PasswordResetLogs_PasswordResetLogId",
                    column: x => x.PasswordResetLogId,
                    principalTable: "PasswordResetLogs",
                    principalColumn: "PasswordResetLogId");
                table.ForeignKey(
                    name: "FK_Users_StudentRegistrations_StudentRegistrationId",
                    column: x => x.StudentRegistrationId,
                    principalTable: "StudentRegistrations",
                    principalColumn: "StudentRegistrationId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "StudentRegistrationSearches",
            columns: table => new
            {
                StudentRegistrationSearchId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SearchCriteria = table.Column<string>(type: "text", nullable: false),
                SearchDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: true),
                SearchResults = table.Column<string>(type: "text", nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudentRegistrationSearches", x => x.StudentRegistrationSearchId);
                table.ForeignKey(
                    name: "FK_StudentRegistrationSearches_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "UserAttachments",
            columns: table => new
            {
                UserAttachmentId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                FilePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                FileSize = table.Column<long>(type: "bigint", nullable: true),
                UploadedByUserId = table.Column<string>(type: "text", nullable: false),
                UploadedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserAttachments", x => x.UserAttachmentId);
                table.ForeignKey(
                    name: "FK_UserAttachments_Users_UploadedByUserId",
                    column: x => x.UploadedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "UserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false),
                ClaimType = table.Column<string>(type: "text", nullable: true),
                ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ProviderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                UserId = table.Column<string>(type: "text", nullable: false)
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
            name: "UserProgramMaps",
            columns: table => new
            {
                UserProgramMapId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false),
                ProgramId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserProgramMaps", x => x.UserProgramMapId);
                table.ForeignKey(
                    name: "FK_UserProgramMaps_Programs_ProgramId",
                    column: x => x.ProgramId,
                    principalTable: "Programs",
                    principalColumn: "ProgramsId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_UserProgramMaps_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "UserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "text", nullable: false),
                RoleId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_UserRoles_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserRoles_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "text", nullable: false),
                LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Value = table.Column<string>(type: "text", nullable: true)
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

        migrationBuilder.CreateIndex(
            name: "IX_ActiveExamSchedules_ExamScheduleId",
            table: "ActiveExamSchedules",
            column: "ExamScheduleId");

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationVouchers_ExamScheduleId",
            table: "ApplicationVouchers",
            column: "ExamScheduleId");

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationVouchers_StudentRegistrationId",
            table: "ApplicationVouchers",
            column: "StudentRegistrationId");

        migrationBuilder.CreateIndex(
            name: "IX_BankVouchers_AcademicYearId",
            table: "BankVouchers",
            column: "AcademicYearId");

        migrationBuilder.CreateIndex(
            name: "IX_BankVouchers_BankId",
            table: "BankVouchers",
            column: "BankId");

        migrationBuilder.CreateIndex(
            name: "IX_BankVouchers_BankVoucherUserAttachmentId",
            table: "BankVouchers",
            column: "BankVoucherUserAttachmentId");

        migrationBuilder.CreateIndex(
            name: "IX_BankVouchers_BillTitleId",
            table: "BankVouchers",
            column: "BillTitleId");

        migrationBuilder.CreateIndex(
            name: "IX_BankVouchers_CollegeId",
            table: "BankVouchers",
            column: "CollegeId");

        migrationBuilder.CreateIndex(
            name: "IX_BankVouchers_ExamScheduleParentId",
            table: "BankVouchers",
            column: "ExamScheduleParentId");

        migrationBuilder.CreateIndex(
            name: "IX_Batches_AcademicYearId",
            table: "Batches",
            column: "AcademicYearId");

        migrationBuilder.CreateIndex(
            name: "IX_BillTitles_ExamScheduleId",
            table: "BillTitles",
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
            name: "IX_CollegePrograms_ProgramsId",
            table: "CollegePrograms",
            column: "ProgramsId");

        migrationBuilder.CreateIndex(
            name: "IX_Colleges_AreaId",
            table: "Colleges",
            column: "AreaId");

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
            name: "IX_Districts_ProvinceId",
            table: "Districts",
            column: "ProvinceId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamCenterDetails_CollegeId",
            table: "ExamCenterDetails",
            column: "CollegeId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamCenterDetails_ExamCenterId",
            table: "ExamCenterDetails",
            column: "ExamCenterId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamCenterDetails_ProgramsId",
            table: "ExamCenterDetails",
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
            name: "IX_ExamRegistrationActionLogs_ExamRegistrationId",
            table: "ExamRegistrationActionLogs",
            column: "ExamRegistrationId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamRegistrationCenterChanges_ExamRegistrationId",
            table: "ExamRegistrationCenterChanges",
            column: "ExamRegistrationId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ExamRegistrationCenterChanges_PreferredExamCenterId",
            table: "ExamRegistrationCenterChanges",
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
            name: "IX_ExamRegistrations_StudentProgramYearPartId",
            table: "ExamRegistrations",
            column: "StudentProgramYearPartId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamRollNumberSetupDetails_CollegeId",
            table: "ExamRollNumberSetupDetails",
            column: "CollegeId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamRollNumberSetupDetails_ExamRollNumberSetupId",
            table: "ExamRollNumberSetupDetails",
            column: "ExamRollNumberSetupId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamRollNumberSetupDetails_ExamScheduleId",
            table: "ExamRollNumberSetupDetails",
            column: "ExamScheduleId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamRollNumberSetupDetails_ExamTypeId",
            table: "ExamRollNumberSetupDetails",
            column: "ExamTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamRollNumberSetupDetails_ProgramId",
            table: "ExamRollNumberSetupDetails",
            column: "ProgramId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamRollNumberSetups_ExamScheduleParentId",
            table: "ExamRollNumberSetups",
            column: "ExamScheduleParentId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamScheduleBatches_BatchId",
            table: "ExamScheduleBatches",
            column: "BatchId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamScheduleBatches_ExamScheduleId",
            table: "ExamScheduleBatches",
            column: "ExamScheduleId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamScheduleBatches_ExamTypeId",
            table: "ExamScheduleBatches",
            column: "ExamTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamScheduleDetails_ExamScheduleId",
            table: "ExamScheduleDetails",
            column: "ExamScheduleId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamScheduleDetails_ExamTypeId",
            table: "ExamScheduleDetails",
            column: "ExamTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamScheduleDetails_SubjectDetailId",
            table: "ExamScheduleDetails",
            column: "SubjectDetailId");

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
            name: "IX_ExamSchedules_YearPartId",
            table: "ExamSchedules",
            column: "YearPartId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamSubjectRegistrationExamSessions_ExamSubjectRegistration~",
            table: "ExamSubjectRegistrationExamSessions",
            column: "ExamSubjectRegistrationId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ExamSubjectRegistrationInternals_EntryAcademicYearId",
            table: "ExamSubjectRegistrationInternals",
            column: "EntryAcademicYearId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamSubjectRegistrationInternals_ExamScheduleId",
            table: "ExamSubjectRegistrationInternals",
            column: "ExamScheduleId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamSubjectRegistrationInternals_StudentProgramYearPartId",
            table: "ExamSubjectRegistrationInternals",
            column: "StudentProgramYearPartId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamSubjectRegistrationInternals_SubjectDetailId",
            table: "ExamSubjectRegistrationInternals",
            column: "SubjectDetailId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamSubjectRegistrations_ExamRegistrationId",
            table: "ExamSubjectRegistrations",
            column: "ExamRegistrationId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamSubjectRegistrations_ExamTypeId",
            table: "ExamSubjectRegistrations",
            column: "ExamTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_ExamSubjectRegistrations_SubjectDetailId",
            table: "ExamSubjectRegistrations",
            column: "SubjectDetailId");

        migrationBuilder.CreateIndex(
            name: "IX_LocalLevels_DistrictId",
            table: "LocalLevels",
            column: "DistrictId");

        migrationBuilder.CreateIndex(
            name: "IX_PasswordResetLogs_UserId",
            table: "PasswordResetLogs",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_PaymentPracticalSubjects_PaymentRequestLogId",
            table: "PaymentPracticalSubjects",
            column: "PaymentRequestLogId");

        migrationBuilder.CreateIndex(
            name: "IX_PaymentRequestLogs_CollegeId",
            table: "PaymentRequestLogs",
            column: "CollegeId");

        migrationBuilder.CreateIndex(
            name: "IX_PaymentRequestLogs_ExamScheduleId",
            table: "PaymentRequestLogs",
            column: "ExamScheduleId");

        migrationBuilder.CreateIndex(
            name: "IX_PaymentRequestLogs_PaymentTypeId",
            table: "PaymentRequestLogs",
            column: "PaymentTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_PaymentRequestLogs_StudentRegistrationId",
            table: "PaymentRequestLogs",
            column: "StudentRegistrationId");

        migrationBuilder.CreateIndex(
            name: "IX_PaymentResponseLogs_PaymentRequestLogId",
            table: "PaymentResponseLogs",
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
            name: "IX_Programs_ProgramPeriodTypeId",
            table: "Programs",
            column: "ProgramPeriodTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_ProgramSubjectPracticalCharges_ProgramsId",
            table: "ProgramSubjectPracticalCharges",
            column: "ProgramsId");

        migrationBuilder.CreateIndex(
            name: "IX_ProgramYearParts_ProgramsId",
            table: "ProgramYearParts",
            column: "ProgramsId");

        migrationBuilder.CreateIndex(
            name: "IX_ProgramYearParts_YearPartId",
            table: "ProgramYearParts",
            column: "YearPartId");

        migrationBuilder.CreateIndex(
            name: "IX_ResultRecords_AcademicYearId",
            table: "ResultRecords",
            column: "AcademicYearId");

        migrationBuilder.CreateIndex(
            name: "IX_ResultRecords_CollegeId",
            table: "ResultRecords",
            column: "CollegeId");

        migrationBuilder.CreateIndex(
            name: "IX_ResultRecords_ExamScheduleId",
            table: "ResultRecords",
            column: "ExamScheduleId");

        migrationBuilder.CreateIndex(
            name: "IX_ResultRecords_ExamTypeId",
            table: "ResultRecords",
            column: "ExamTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_ResultRecords_ProgramsId",
            table: "ResultRecords",
            column: "ProgramsId");

        migrationBuilder.CreateIndex(
            name: "IX_ResultRecords_SubjectDetailId",
            table: "ResultRecords",
            column: "SubjectDetailId");

        migrationBuilder.CreateIndex(
            name: "IX_RoleClaims_RoleId",
            table: "RoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "Roles",
            column: "NormalizedName",
            unique: true);

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
            name: "IX_StudentAdmissions_SubjectGroupId",
            table: "StudentAdmissions",
            column: "SubjectGroupId");

        migrationBuilder.CreateIndex(
            name: "IX_StudentGuardians_StudentRegistrationId",
            table: "StudentGuardians",
            column: "StudentRegistrationId");

        migrationBuilder.CreateIndex(
            name: "IX_StudentProgramYearParts_AcademicYearId",
            table: "StudentProgramYearParts",
            column: "AcademicYearId");

        migrationBuilder.CreateIndex(
            name: "IX_StudentProgramYearParts_StudentAdmissionId",
            table: "StudentProgramYearParts",
            column: "StudentAdmissionId");

        migrationBuilder.CreateIndex(
            name: "IX_StudentProgramYearParts_YearPartId",
            table: "StudentProgramYearParts",
            column: "YearPartId");

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
            name: "IX_StudentRegistrations_PhotoAttachmentId",
            table: "StudentRegistrations",
            column: "PhotoAttachmentId");

        migrationBuilder.CreateIndex(
            name: "IX_StudentRegistrations_StudentCategoryId",
            table: "StudentRegistrations",
            column: "StudentCategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_StudentRegistrations_StudentRegistrationSearchId",
            table: "StudentRegistrations",
            column: "StudentRegistrationSearchId");

        migrationBuilder.CreateIndex(
            name: "IX_StudentRegistrationSearches_UserId",
            table: "StudentRegistrationSearches",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_SubjectBatches_EffectiveAcademicYearId",
            table: "SubjectBatches",
            column: "EffectiveAcademicYearId");

        migrationBuilder.CreateIndex(
            name: "IX_SubjectBatches_ProgramsId",
            table: "SubjectBatches",
            column: "ProgramsId");

        migrationBuilder.CreateIndex(
            name: "IX_SubjectDetails_ProgramsId",
            table: "SubjectDetails",
            column: "ProgramsId");

        migrationBuilder.CreateIndex(
            name: "IX_SubjectDetails_SubjectGroupId",
            table: "SubjectDetails",
            column: "SubjectGroupId");

        migrationBuilder.CreateIndex(
            name: "IX_SubjectDetails_SubjectTypeId",
            table: "SubjectDetails",
            column: "SubjectTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_SubjectDetails_YearPartId",
            table: "SubjectDetails",
            column: "YearPartId");

        migrationBuilder.CreateIndex(
            name: "IX_SubjectGroupDetailMaps_SubjectDetailId",
            table: "SubjectGroupDetailMaps",
            column: "SubjectDetailId");

        migrationBuilder.CreateIndex(
            name: "IX_SubjectGroups_ProgramsId",
            table: "SubjectGroups",
            column: "ProgramsId");

        migrationBuilder.CreateIndex(
            name: "IX_SubjectGroups_YearPartId",
            table: "SubjectGroups",
            column: "YearPartId");

        migrationBuilder.CreateIndex(
            name: "IX_UserAttachments_UploadedByUserId",
            table: "UserAttachments",
            column: "UploadedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserClaims_UserId",
            table: "UserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserLogins_UserId",
            table: "UserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserProgramMaps_ProgramId",
            table: "UserProgramMaps",
            column: "ProgramId");

        migrationBuilder.CreateIndex(
            name: "IX_UserProgramMaps_UserId",
            table: "UserProgramMaps",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserRoles_RoleId",
            table: "UserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "Users",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "IX_Users_CollegeId",
            table: "Users",
            column: "CollegeId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_OrganizationId",
            table: "Users",
            column: "OrganizationId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_PasswordResetLogId",
            table: "Users",
            column: "PasswordResetLogId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_StudentRegistrationId",
            table: "Users",
            column: "StudentRegistrationId");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "Users",
            column: "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_YearParts_ProgramPeriodTypeId",
            table: "YearParts",
            column: "ProgramPeriodTypeId");

        migrationBuilder.AddForeignKey(
            name: "FK_ApplicationVouchers_StudentRegistrations_StudentRegistratio~",
            table: "ApplicationVouchers",
            column: "StudentRegistrationId",
            principalTable: "StudentRegistrations",
            principalColumn: "StudentRegistrationId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_BankVouchers_UserAttachments_BankVoucherUserAttachmentId",
            table: "BankVouchers",
            column: "BankVoucherUserAttachmentId",
            principalTable: "UserAttachments",
            principalColumn: "UserAttachmentId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_CollegeProfiles_UserAttachments_AuditReportUserAttachmentId",
            table: "CollegeProfiles",
            column: "AuditReportUserAttachmentId",
            principalTable: "UserAttachments",
            principalColumn: "UserAttachmentId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_CollegeProfiles_UserAttachments_BlankChequeUserAttachmentId",
            table: "CollegeProfiles",
            column: "BlankChequeUserAttachmentId",
            principalTable: "UserAttachments",
            principalColumn: "UserAttachmentId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_ExamRegistrationActionLogs_ExamRegistrations_ExamRegistrati~",
            table: "ExamRegistrationActionLogs",
            column: "ExamRegistrationId",
            principalTable: "ExamRegistrations",
            principalColumn: "ExamRegistrationId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_ExamRegistrationCenterChanges_ExamRegistrations_ExamRegistr~",
            table: "ExamRegistrationCenterChanges",
            column: "ExamRegistrationId",
            principalTable: "ExamRegistrations",
            principalColumn: "ExamRegistrationId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_ExamRegistrations_StudentProgramYearParts_StudentProgramYea~",
            table: "ExamRegistrations",
            column: "StudentProgramYearPartId",
            principalTable: "StudentProgramYearParts",
            principalColumn: "StudentProgramYearPartId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_ExamSubjectRegistrationInternals_StudentProgramYearParts_St~",
            table: "ExamSubjectRegistrationInternals",
            column: "StudentProgramYearPartId",
            principalTable: "StudentProgramYearParts",
            principalColumn: "StudentProgramYearPartId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_PasswordResetLogs_Users_UserId",
            table: "PasswordResetLogs",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_PaymentPracticalSubjects_PaymentRequestLogs_PaymentRequestL~",
            table: "PaymentPracticalSubjects",
            column: "PaymentRequestLogId",
            principalTable: "PaymentRequestLogs",
            principalColumn: "PaymentRequestLogId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_PaymentRequestLogs_StudentRegistrations_StudentRegistration~",
            table: "PaymentRequestLogs",
            column: "StudentRegistrationId",
            principalTable: "StudentRegistrations",
            principalColumn: "StudentRegistrationId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_StudentAdmissions_StudentRegistrations_StudentRegistrationId",
            table: "StudentAdmissions",
            column: "StudentRegistrationId",
            principalTable: "StudentRegistrations",
            principalColumn: "StudentRegistrationId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_StudentGuardians_StudentRegistrations_StudentRegistrationId",
            table: "StudentGuardians",
            column: "StudentRegistrationId",
            principalTable: "StudentRegistrations",
            principalColumn: "StudentRegistrationId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_StudentQualifications_StudentRegistrations_StudentRegistrat~",
            table: "StudentQualifications",
            column: "StudentRegistrationId",
            principalTable: "StudentRegistrations",
            principalColumn: "StudentRegistrationId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_StudentRegistrations_StudentRegistrationSearches_StudentReg~",
            table: "StudentRegistrations",
            column: "StudentRegistrationSearchId",
            principalTable: "StudentRegistrationSearches",
            principalColumn: "StudentRegistrationSearchId",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_StudentRegistrations_UserAttachments_PhotoAttachmentId",
            table: "StudentRegistrations",
            column: "PhotoAttachmentId",
            principalTable: "UserAttachments",
            principalColumn: "UserAttachmentId",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Users_StudentRegistrations_StudentRegistrationId",
            table: "Users");

        migrationBuilder.DropForeignKey(
            name: "FK_Users_Colleges_CollegeId",
            table: "Users");

        migrationBuilder.DropForeignKey(
            name: "FK_PasswordResetLogs_Users_UserId",
            table: "PasswordResetLogs");

        migrationBuilder.DropTable(
            name: "ActiveExamSchedules");

        migrationBuilder.DropTable(
            name: "BankVouchers");

        migrationBuilder.DropTable(
            name: "CollegeProfiles");

        migrationBuilder.DropTable(
            name: "CollegePrograms");

        migrationBuilder.DropTable(
            name: "ConnectIpsPaymentConfigurations");

        migrationBuilder.DropTable(
            name: "ESewaConfigurations");

        migrationBuilder.DropTable(
            name: "ExamAttendanceStatuses");

        migrationBuilder.DropTable(
            name: "ExamCenterDetails");

        migrationBuilder.DropTable(
            name: "ExamFormFeeRates");

        migrationBuilder.DropTable(
            name: "ExamRegistrationActionLogs");

        migrationBuilder.DropTable(
            name: "ExamRegistrationCenterChanges");

        migrationBuilder.DropTable(
            name: "ExamRollNumberSetupDetails");

        migrationBuilder.DropTable(
            name: "ExamScheduleBatches");

        migrationBuilder.DropTable(
            name: "ExamScheduleDetails");

        migrationBuilder.DropTable(
            name: "ExamSubjectRegistrationExamSessions");

        migrationBuilder.DropTable(
            name: "ExamSubjectRegistrationInternals");

        migrationBuilder.DropTable(
            name: "KhaltiConfigurations");

        migrationBuilder.DropTable(
            name: "NepaliDates");

        migrationBuilder.DropTable(
            name: "PaymentPracticalSubjects");

        migrationBuilder.DropTable(
            name: "PaymentResponseLogs");

        migrationBuilder.DropTable(
            name: "ProgramSubjectPracticalCharges");

        migrationBuilder.DropTable(
            name: "ProgramYearParts");

        migrationBuilder.DropTable(
            name: "ResultRecords");

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
            name: "SubjectBatches");

        migrationBuilder.DropTable(
            name: "SubjectGroupDetailMaps");

        migrationBuilder.DropTable(
            name: "SubjectTriplicates");

        migrationBuilder.DropTable(
            name: "UserClaims");

        migrationBuilder.DropTable(
            name: "UserLogins");

        migrationBuilder.DropTable(
            name: "UserProgramMaps");

        migrationBuilder.DropTable(
            name: "UserRoles");

        migrationBuilder.DropTable(
            name: "UserTokens");

        migrationBuilder.DropTable(
            name: "Banks");

        migrationBuilder.DropTable(
            name: "BillTitles");

        migrationBuilder.DropTable(
            name: "ExamFormFeeNames");

        migrationBuilder.DropTable(
            name: "PreferredExamCenters");

        migrationBuilder.DropTable(
            name: "ExamRollNumberSetups");

        migrationBuilder.DropTable(
            name: "ExamSubjectRegistrations");

        migrationBuilder.DropTable(
            name: "PaymentRequestLogs");

        migrationBuilder.DropTable(
            name: "PreviousLevels");

        migrationBuilder.DropTable(
            name: "Roles");

        migrationBuilder.DropTable(
            name: "ExamRegistrations");

        migrationBuilder.DropTable(
            name: "SubjectDetails");

        migrationBuilder.DropTable(
            name: "PaymentTypes");

        migrationBuilder.DropTable(
            name: "ApplicationVouchers");

        migrationBuilder.DropTable(
            name: "ExamCenters");

        migrationBuilder.DropTable(
            name: "StudentProgramYearParts");

        migrationBuilder.DropTable(
            name: "SubjectTypes");

        migrationBuilder.DropTable(
            name: "ExamSchedules");

        migrationBuilder.DropTable(
            name: "StudentAdmissions");

        migrationBuilder.DropTable(
            name: "ExamScheduleParents");

        migrationBuilder.DropTable(
            name: "ExamTypes");

        migrationBuilder.DropTable(
            name: "Sections");

        migrationBuilder.DropTable(
            name: "SubjectGroups");

        migrationBuilder.DropTable(
            name: "Batches");

        migrationBuilder.DropTable(
            name: "Programs");

        migrationBuilder.DropTable(
            name: "YearParts");

        migrationBuilder.DropTable(
            name: "Boards");

        migrationBuilder.DropTable(
            name: "ProgramPeriodTypes");

        migrationBuilder.DropTable(
            name: "StudentRegistrations");

        migrationBuilder.DropTable(
            name: "AcademicYears");

        migrationBuilder.DropTable(
            name: "EntryFormats");

        migrationBuilder.DropTable(
            name: "Ethnicities");

        migrationBuilder.DropTable(
            name: "Faculties");

        migrationBuilder.DropTable(
            name: "Genders");

        migrationBuilder.DropTable(
            name: "IndexGroups");

        migrationBuilder.DropTable(
            name: "Levels");

        migrationBuilder.DropTable(
            name: "LocalLevels");

        migrationBuilder.DropTable(
            name: "StudentCategories");

        migrationBuilder.DropTable(
            name: "StudentRegistrationSearches");

        migrationBuilder.DropTable(
            name: "UserAttachments");

        migrationBuilder.DropTable(
            name: "Colleges");

        migrationBuilder.DropTable(
            name: "Areas");

        migrationBuilder.DropTable(
            name: "CollegeTypes");

        migrationBuilder.DropTable(
            name: "Districts");

        migrationBuilder.DropTable(
            name: "QuestionSets");

        migrationBuilder.DropTable(
            name: "Provinces");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropTable(
            name: "Organizations");

        migrationBuilder.DropTable(
            name: "PasswordResetLogs");
    }
}
