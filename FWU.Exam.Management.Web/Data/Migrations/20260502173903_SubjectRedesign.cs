using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fwu_examination_management_system.Migrations
{
    /// <inheritdoc />
    public partial class SubjectRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamRegistrations_StudentProgramYearParts_StudentProgramYearPartId",
                table: "ExamRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_YearParts_YearPartId",
                table: "ExamSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_StudentProgramYearParts_StudentProgramYearPartId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_SubjectDetails_SubjectDetailId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSubjectRegistrations_SubjectDetails_SubjectDetailId",
                table: "ExamSubjectRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Programs_ProgramPeriodTypes_ProgramPeriodTypeId",
                table: "Programs");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultRecords_SubjectDetails_SubjectDetailId",
                table: "ResultRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAdmissions_SubjectGroups_SubjectGroupId",
                table: "StudentAdmissions");

            migrationBuilder.DropTable(
                name: "ExamScheduleDetail");

            migrationBuilder.DropTable(
                name: "ProgramYearParts");

            migrationBuilder.DropTable(
                name: "StudentProgramYearParts");

            migrationBuilder.DropTable(
                name: "SubjectBatches");

            migrationBuilder.DropTable(
                name: "SubjectGroupDetailMaps");

            migrationBuilder.DropTable(
                name: "SubjectDetails");

            migrationBuilder.DropTable(
                name: "SubjectGroups");

            migrationBuilder.DropTable(
                name: "YearParts");

            migrationBuilder.DropTable(
                name: "ProgramPeriodTypes");

            migrationBuilder.DropIndex(
                name: "IX_StudentAdmissions_SubjectGroupId",
                table: "StudentAdmissions");

            migrationBuilder.DropIndex(
                name: "IX_ResultRecords_SubjectDetailId",
                table: "ResultRecords");

            migrationBuilder.DropIndex(
                name: "IX_Programs_ProgramPeriodTypeId",
                table: "Programs");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectRegistrations_SubjectDetailId",
                table: "ExamSubjectRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectRegistrationInternals_StudentProgramYearPartId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectRegistrationInternals_SubjectDetailId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropIndex(
                name: "IX_ExamSchedules_YearPartId",
                table: "ExamSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ExamRegistrations_StudentProgramYearPartId",
                table: "ExamRegistrations");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "SubjectTypes");

            migrationBuilder.DropColumn(
                name: "SubjectGroupId",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "SubjectDetailId",
                table: "ResultRecords");

            migrationBuilder.DropColumn(
                name: "ProgramPeriodTypeId",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "SubjectDetailId",
                table: "ExamSubjectRegistrations");

            migrationBuilder.DropColumn(
                name: "StudentProgramYearPartId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropColumn(
                name: "SubjectDetailId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropColumn(
                name: "YearPartId",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "StudentProgramYearPartId",
                table: "ExamRegistrations");

            migrationBuilder.RenameColumn(
                name: "SubjectTypeName",
                table: "SubjectTypes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "SemesterEnrollment",
                newName: "Deficiency");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SubjectTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Semesters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DropDate",
                table: "SemesterEnrollment",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DropReason",
                table: "SemesterEnrollment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EnrolledDate",
                table: "SemesterEnrollment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "EnrollmentType",
                table: "SemesterEnrollment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "GradePoints",
                table: "SemesterEnrollment",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PaidAmount",
                table: "SemesterEnrollment",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "SemesterEnrollment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ResultStatus",
                table: "SemesterEnrollment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SemesterResultDate",
                table: "SemesterEnrollment",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalCredits",
                table: "SemesterEnrollment",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalFee",
                table: "SemesterEnrollment",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

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

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumVersions_EffectiveAcademicYearId",
                table: "CurriculumVersions",
                column: "EffectiveAcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumVersions_ProgramId",
                table: "CurriculumVersions",
                column: "ProgramId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurriculumVersions");

            migrationBuilder.DropTable(
                name: "SubjectOfferings");

            migrationBuilder.DropTable(
                name: "SubjectCatalogs");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SubjectTypes");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "DropDate",
                table: "SemesterEnrollment");

            migrationBuilder.DropColumn(
                name: "DropReason",
                table: "SemesterEnrollment");

            migrationBuilder.DropColumn(
                name: "EnrolledDate",
                table: "SemesterEnrollment");

            migrationBuilder.DropColumn(
                name: "EnrollmentType",
                table: "SemesterEnrollment");

            migrationBuilder.DropColumn(
                name: "GradePoints",
                table: "SemesterEnrollment");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "SemesterEnrollment");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "SemesterEnrollment");

            migrationBuilder.DropColumn(
                name: "ResultStatus",
                table: "SemesterEnrollment");

            migrationBuilder.DropColumn(
                name: "SemesterResultDate",
                table: "SemesterEnrollment");

            migrationBuilder.DropColumn(
                name: "TotalCredits",
                table: "SemesterEnrollment");

            migrationBuilder.DropColumn(
                name: "TotalFee",
                table: "SemesterEnrollment");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "SubjectTypes",
                newName: "SubjectTypeName");

            migrationBuilder.RenameColumn(
                name: "Deficiency",
                table: "SemesterEnrollment",
                newName: "IsActive");

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "SubjectTypes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubjectGroupId",
                table: "StudentAdmissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubjectDetailId",
                table: "ResultRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProgramPeriodTypeId",
                table: "Programs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubjectDetailId",
                table: "ExamSubjectRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StudentProgramYearPartId",
                table: "ExamSubjectRegistrationInternals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubjectDetailId",
                table: "ExamSubjectRegistrationInternals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YearPartId",
                table: "ExamSchedules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentProgramYearPartId",
                table: "ExamRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProgramPeriodTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumberOfMonths = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ProgramPeriodTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramPeriodTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubjectBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EffectiveAcademicYearId = table.Column<int>(type: "int", nullable: false),
                    ProgramsId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    SubjectBatchName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectBatches_AcademicYears_EffectiveAcademicYearId",
                        column: x => x.EffectiveAcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectBatches_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YearParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramPeriodTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    Part = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    YearPartName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YearParts_ProgramPeriodTypes_ProgramPeriodTypeId",
                        column: x => x.ProgramPeriodTypeId,
                        principalTable: "ProgramPeriodTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramYearParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramsId = table.Column<int>(type: "int", nullable: false),
                    YearPartId = table.Column<int>(type: "int", nullable: false),
                    TotalMarks = table.Column<int>(type: "int", nullable: false),
                    TotalPassMarks = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramYearParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramYearParts_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramYearParts_YearParts_YearPartId",
                        column: x => x.YearPartId,
                        principalTable: "YearParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentProgramYearParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    StudentAdmissionId = table.Column<int>(type: "int", nullable: false),
                    YearPartId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProgramYearParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProgramYearParts_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProgramYearParts_StudentAdmissions_StudentAdmissionId",
                        column: x => x.StudentAdmissionId,
                        principalTable: "StudentAdmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProgramYearParts_YearParts_YearPartId",
                        column: x => x.YearPartId,
                        principalTable: "YearParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubjectGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramsId = table.Column<int>(type: "int", nullable: false),
                    YearPartId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCompulsory = table.Column<bool>(type: "bit", nullable: true),
                    IsExtraAllowed = table.Column<bool>(type: "bit", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SubjectGroupName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    SubjectGroupShortName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectGroups_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectGroups_YearParts_YearPartId",
                        column: x => x.YearPartId,
                        principalTable: "YearParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubjectDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramsId = table.Column<int>(type: "int", nullable: false),
                    SubjectGroupId = table.Column<int>(type: "int", nullable: true),
                    SubjectTypeId = table.Column<int>(type: "int", nullable: false),
                    YearPartId = table.Column<int>(type: "int", nullable: false),
                    ConcurrentSubjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreditHours = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true),
                    HasInternal = table.Column<bool>(type: "bit", nullable: false),
                    HasPractical = table.Column<bool>(type: "bit", nullable: false),
                    HasTheory = table.Column<bool>(type: "bit", nullable: false),
                    InternalPracticalFullMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    InternalPracticalPassMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    InternalTheoryFullMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    InternalTheoryPassMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCompulsory = table.Column<bool>(type: "bit", nullable: false),
                    Part = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PracticalFullMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    PracticalPassMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SubjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TheoryFullMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TheoryPassMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Year = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectDetails_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectDetails_SubjectGroups_SubjectGroupId",
                        column: x => x.SubjectGroupId,
                        principalTable: "SubjectGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectDetails_SubjectTypes_SubjectTypeId",
                        column: x => x.SubjectTypeId,
                        principalTable: "SubjectTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectDetails_YearParts_YearPartId",
                        column: x => x.YearPartId,
                        principalTable: "YearParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamScheduleDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false),
                    SubjectDetailId = table.Column<int>(type: "int", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExamDateBs = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamScheduleDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamScheduleDetail_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamScheduleDetail_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamScheduleDetail_SubjectDetails_SubjectDetailId",
                        column: x => x.SubjectDetailId,
                        principalTable: "SubjectDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectGroupDetailMaps",
                columns: table => new
                {
                    SubjectGroupId = table.Column<int>(type: "int", nullable: false),
                    SubjectDetailId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectGroupDetailMaps", x => new { x.SubjectGroupId, x.SubjectDetailId });
                    table.ForeignKey(
                        name: "FK_SubjectGroupDetailMaps_SubjectDetails_SubjectDetailId",
                        column: x => x.SubjectDetailId,
                        principalTable: "SubjectDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectGroupDetailMaps_SubjectGroups_SubjectGroupId",
                        column: x => x.SubjectGroupId,
                        principalTable: "SubjectGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_SubjectGroupId",
                table: "StudentAdmissions",
                column: "SubjectGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultRecords_SubjectDetailId",
                table: "ResultRecords",
                column: "SubjectDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_ProgramPeriodTypeId",
                table: "Programs",
                column: "ProgramPeriodTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrations_SubjectDetailId",
                table: "ExamSubjectRegistrations",
                column: "SubjectDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationInternals_StudentProgramYearPartId",
                table: "ExamSubjectRegistrationInternals",
                column: "StudentProgramYearPartId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationInternals_SubjectDetailId",
                table: "ExamSubjectRegistrationInternals",
                column: "SubjectDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_YearPartId",
                table: "ExamSchedules",
                column: "YearPartId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_StudentProgramYearPartId",
                table: "ExamRegistrations",
                column: "StudentProgramYearPartId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleDetail_ExamScheduleId",
                table: "ExamScheduleDetail",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleDetail_ExamTypeId",
                table: "ExamScheduleDetail",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleDetail_SubjectDetailId",
                table: "ExamScheduleDetail",
                column: "SubjectDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramYearParts_ProgramsId",
                table: "ProgramYearParts",
                column: "ProgramsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramYearParts_YearPartId",
                table: "ProgramYearParts",
                column: "YearPartId");

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
                name: "IX_YearParts_ProgramPeriodTypeId",
                table: "YearParts",
                column: "ProgramPeriodTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRegistrations_StudentProgramYearParts_StudentProgramYearPartId",
                table: "ExamRegistrations",
                column: "StudentProgramYearPartId",
                principalTable: "StudentProgramYearParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_YearParts_YearPartId",
                table: "ExamSchedules",
                column: "YearPartId",
                principalTable: "YearParts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_StudentProgramYearParts_StudentProgramYearPartId",
                table: "ExamSubjectRegistrationInternals",
                column: "StudentProgramYearPartId",
                principalTable: "StudentProgramYearParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_SubjectDetails_SubjectDetailId",
                table: "ExamSubjectRegistrationInternals",
                column: "SubjectDetailId",
                principalTable: "SubjectDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSubjectRegistrations_SubjectDetails_SubjectDetailId",
                table: "ExamSubjectRegistrations",
                column: "SubjectDetailId",
                principalTable: "SubjectDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_ProgramPeriodTypes_ProgramPeriodTypeId",
                table: "Programs",
                column: "ProgramPeriodTypeId",
                principalTable: "ProgramPeriodTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultRecords_SubjectDetails_SubjectDetailId",
                table: "ResultRecords",
                column: "SubjectDetailId",
                principalTable: "SubjectDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAdmissions_SubjectGroups_SubjectGroupId",
                table: "StudentAdmissions",
                column: "SubjectGroupId",
                principalTable: "SubjectGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
