using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCollegeIdToExamSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollegeId",
                table: "ExamSchedules",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_CollegeId",
                table: "ExamSchedules",
                column: "CollegeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_Colleges_CollegeId",
                table: "ExamSchedules",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_Colleges_CollegeId",
                table: "ExamSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ExamSchedules_CollegeId",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "CollegeId",
                table: "ExamSchedules");
        }
    }
}
