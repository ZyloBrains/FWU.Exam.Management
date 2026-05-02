using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fwu_examination_management_system.Migrations
{
    /// <inheritdoc />
    public partial class SubjectModelRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubjectGroups_Programs_ProgramsId",
                table: "SubjectGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectGroups_YearParts_YearPartId",
                table: "SubjectGroups");

            migrationBuilder.DropTable(
                name: "SubjectBatches");

            migrationBuilder.DropIndex(
                name: "IX_SubjectGroups_ProgramsId",
                table: "SubjectGroups");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "SubjectTypes");

            migrationBuilder.DropColumn(
                name: "IsCompulsory",
                table: "SubjectGroups");

            migrationBuilder.DropColumn(
                name: "IsExtraAllowed",
                table: "SubjectGroups");

            migrationBuilder.DropColumn(
                name: "ProgramsId",
                table: "SubjectGroups");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "SubjectGroups");

            migrationBuilder.DropColumn(
                name: "SubjectGroupName",
                table: "SubjectGroups");

            migrationBuilder.RenameColumn(
                name: "SubjectTypeName",
                table: "SubjectTypes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "SubjectGroupShortName",
                table: "SubjectGroups",
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

            migrationBuilder.AlterColumn<int>(
                name: "YearPartId",
                table: "SubjectGroups",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SubjectGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SubjectGroups",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramId",
                table: "SubjectGroups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "SubjectGroups",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

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

            migrationBuilder.AddColumn<int>(
                name: "SubjectOfferingId",
                table: "ResultRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubjectOfferingId",
                table: "ExamSubjectRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubjectOfferingId",
                table: "ExamSubjectRegistrationInternals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubjectOfferingId",
                table: "ExamScheduleDetail",
                type: "int",
                nullable: true);

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
                name: "SemesterSubjects",
                columns: table => new
                {
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    SubjectDetailId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemesterSubjects", x => new { x.SemesterId, x.SubjectDetailId });
                    table.ForeignKey(
                        name: "FK_SemesterSubjects_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SemesterSubjects_SubjectDetails_SubjectDetailId",
                        column: x => x.SubjectDetailId,
                        principalTable: "SubjectDetails",
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
                    SubjectGroupId = table.Column<int>(type: "int", nullable: true),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    IsCompulsory = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    HasTheory = table.Column<bool>(type: "bit", nullable: false),
                    HasPractical = table.Column<bool>(type: "bit", nullable: false),
                    HasInternal = table.Column<bool>(type: "bit", nullable: false),
                    TheoryFullMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TheoryPassMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PracticalFullMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    PracticalPassMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    InternalTheoryFullMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    InternalTheoryPassMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    InternalPracticalFullMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    InternalPracticalPassMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectOfferings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectOfferings_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_SubjectOfferings_SubjectGroups_SubjectGroupId",
                        column: x => x.SubjectGroupId,
                        principalTable: "SubjectGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectGroups_ProgramId",
                table: "SubjectGroups",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultRecords_SubjectOfferingId",
                table: "ResultRecords",
                column: "SubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrations_SubjectOfferingId",
                table: "ExamSubjectRegistrations",
                column: "SubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationInternals_SubjectOfferingId",
                table: "ExamSubjectRegistrationInternals",
                column: "SubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleDetail_SubjectOfferingId",
                table: "ExamScheduleDetail",
                column: "SubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumVersions_EffectiveAcademicYearId",
                table: "CurriculumVersions",
                column: "EffectiveAcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumVersions_ProgramId",
                table: "CurriculumVersions",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterSubjects_SubjectDetailId",
                table: "SemesterSubjects",
                column: "SubjectDetailId");

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
                name: "IX_SubjectOfferings_AcademicYearId",
                table: "SubjectOfferings",
                column: "AcademicYearId");

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
                name: "IX_SubjectOfferings_SubjectGroupId",
                table: "SubjectOfferings",
                column: "SubjectGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleDetail_SubjectOfferings_SubjectOfferingId",
                table: "ExamScheduleDetail",
                column: "SubjectOfferingId",
                principalTable: "SubjectOfferings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_SubjectOfferings_SubjectOfferingId",
                table: "ExamSubjectRegistrationInternals",
                column: "SubjectOfferingId",
                principalTable: "SubjectOfferings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSubjectRegistrations_SubjectOfferings_SubjectOfferingId",
                table: "ExamSubjectRegistrations",
                column: "SubjectOfferingId",
                principalTable: "SubjectOfferings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ResultRecords_SubjectOfferings_SubjectOfferingId",
                table: "ResultRecords",
                column: "SubjectOfferingId",
                principalTable: "SubjectOfferings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectGroups_Programs_ProgramId",
                table: "SubjectGroups",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectGroups_YearParts_YearPartId",
                table: "SubjectGroups",
                column: "YearPartId",
                principalTable: "YearParts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleDetail_SubjectOfferings_SubjectOfferingId",
                table: "ExamScheduleDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_SubjectOfferings_SubjectOfferingId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSubjectRegistrations_SubjectOfferings_SubjectOfferingId",
                table: "ExamSubjectRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultRecords_SubjectOfferings_SubjectOfferingId",
                table: "ResultRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectGroups_Programs_ProgramId",
                table: "SubjectGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectGroups_YearParts_YearPartId",
                table: "SubjectGroups");

            migrationBuilder.DropTable(
                name: "CurriculumVersions");

            migrationBuilder.DropTable(
                name: "SemesterSubjects");

            migrationBuilder.DropTable(
                name: "SubjectOfferings");

            migrationBuilder.DropTable(
                name: "SubjectCatalogs");

            migrationBuilder.DropIndex(
                name: "IX_SubjectGroups_ProgramId",
                table: "SubjectGroups");

            migrationBuilder.DropIndex(
                name: "IX_ResultRecords_SubjectOfferingId",
                table: "ResultRecords");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectRegistrations_SubjectOfferingId",
                table: "ExamSubjectRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectRegistrationInternals_SubjectOfferingId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropIndex(
                name: "IX_ExamScheduleDetail_SubjectOfferingId",
                table: "ExamScheduleDetail");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SubjectTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SubjectGroups");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SubjectGroups");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "SubjectGroups");

            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "SubjectGroups");

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

            migrationBuilder.DropColumn(
                name: "SubjectOfferingId",
                table: "ResultRecords");

            migrationBuilder.DropColumn(
                name: "SubjectOfferingId",
                table: "ExamSubjectRegistrations");

            migrationBuilder.DropColumn(
                name: "SubjectOfferingId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropColumn(
                name: "SubjectOfferingId",
                table: "ExamScheduleDetail");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "SubjectTypes",
                newName: "SubjectTypeName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "SubjectGroups",
                newName: "SubjectGroupShortName");

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

            migrationBuilder.AlterColumn<int>(
                name: "YearPartId",
                table: "SubjectGroups",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompulsory",
                table: "SubjectGroups",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExtraAllowed",
                table: "SubjectGroups",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramsId",
                table: "SubjectGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "SubjectGroups",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubjectGroupName",
                table: "SubjectGroups",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.CreateIndex(
                name: "IX_SubjectGroups_ProgramsId",
                table: "SubjectGroups",
                column: "ProgramsId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectBatches_EffectiveAcademicYearId",
                table: "SubjectBatches",
                column: "EffectiveAcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectBatches_ProgramsId",
                table: "SubjectBatches",
                column: "ProgramsId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectGroups_Programs_ProgramsId",
                table: "SubjectGroups",
                column: "ProgramsId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectGroups_YearParts_YearPartId",
                table: "SubjectGroups",
                column: "YearPartId",
                principalTable: "YearParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
