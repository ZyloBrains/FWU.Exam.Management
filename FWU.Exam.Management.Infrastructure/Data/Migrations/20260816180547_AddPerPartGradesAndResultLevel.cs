using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerPartGradesAndResultLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LevelId",
                table: "ResultRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeLetterPractical",
                table: "ExamSubjectResults",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeLetterTheory",
                table: "ExamSubjectResults",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultRecords_LevelId",
                table: "ResultRecords",
                column: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResultRecords_Levels_LevelId",
                table: "ResultRecords",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResultRecords_Levels_LevelId",
                table: "ResultRecords");

            migrationBuilder.DropIndex(
                name: "IX_ResultRecords_LevelId",
                table: "ResultRecords");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "ResultRecords");

            migrationBuilder.DropColumn(
                name: "GradeLetterPractical",
                table: "ExamSubjectResults");

            migrationBuilder.DropColumn(
                name: "GradeLetterTheory",
                table: "ExamSubjectResults");
        }
    }
}
