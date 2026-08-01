using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamScheduleFacultyAndMultiSelect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExamTypeIds",
                table: "ExamSchedules",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacultyId",
                table: "ExamSchedules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubjectOfferingIds",
                table: "ExamSchedules",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_FacultyId",
                table: "ExamSchedules",
                column: "FacultyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_Faculties_FacultyId",
                table: "ExamSchedules",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_Faculties_FacultyId",
                table: "ExamSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ExamSchedules_FacultyId",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "ExamTypeIds",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "FacultyId",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "SubjectOfferingIds",
                table: "ExamSchedules");
        }
    }
}
