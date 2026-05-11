using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEntranceExamApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamAttendanceStatuses");

            migrationBuilder.CreateTable(
                name: "EntranceExamApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NepaliName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfBirthBS = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DateOfBirthAD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GenderId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    PermanentAddressId = table.Column<int>(type: "int", nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FatherContact = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    MotherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MotherContact = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    PreviousSchoolCollege = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PreviousLevelId = table.Column<int>(type: "int", nullable: true),
                    PreviousPassedYear = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PreviousSymbolNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreviousGPA = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntranceExamApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntranceExamApplications_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntranceExamApplications_Addresses_PermanentAddressId",
                        column: x => x.PermanentAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntranceExamApplications_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntranceExamApplications_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntranceExamApplications_PreviousLevels_PreviousLevelId",
                        column: x => x.PreviousLevelId,
                        principalTable: "PreviousLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntranceExamApplications_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntranceExamApplications_AcademicYearId",
                table: "EntranceExamApplications",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_EntranceExamApplications_CollegeId",
                table: "EntranceExamApplications",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_EntranceExamApplications_GenderId",
                table: "EntranceExamApplications",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_EntranceExamApplications_PermanentAddressId",
                table: "EntranceExamApplications",
                column: "PermanentAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_EntranceExamApplications_PreviousLevelId",
                table: "EntranceExamApplications",
                column: "PreviousLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_EntranceExamApplications_ProgramId",
                table: "EntranceExamApplications",
                column: "ProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntranceExamApplications");

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
        }
    }
}
