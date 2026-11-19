using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMinElectiveSubjectsToExamSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinElectiveSubjects",
                table: "ExamSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinElectiveSubjects",
                table: "ExamSchedules");
        }
    }
}
