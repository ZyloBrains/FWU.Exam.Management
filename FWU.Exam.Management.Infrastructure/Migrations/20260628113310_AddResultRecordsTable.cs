using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResultRecordsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResultRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    ProgramsId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Part = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SymbolNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Alphabet = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    DateOfBirthBs = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TheoryObtainedMarks = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    InternalObtainedMarks = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    PracticalObtainedMarks = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    TheoryObtainedGrade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    InternalObtainedGrade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    PracticalObtainedGrade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    TotalObtainedMarks = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    TotalObtainedGrade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    TotalGradePoints = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Gpa = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StudentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ResultRecordMasterId = table.Column<int>(type: "int", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultRecords_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultRecords_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultRecords_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultRecords_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultRecords_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultRecords_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_ResultRecords_TenantId",
                table: "ResultRecords",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResultRecords");
        }
    }
}
