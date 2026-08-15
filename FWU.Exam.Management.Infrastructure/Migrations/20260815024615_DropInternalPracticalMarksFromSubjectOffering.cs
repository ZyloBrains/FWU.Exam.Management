using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropInternalPracticalMarksFromSubjectOffering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternalPracticalFullMarks",
                table: "SubjectOfferings");

            migrationBuilder.DropColumn(
                name: "InternalPracticalPassMarks",
                table: "SubjectOfferings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "InternalPracticalFullMarks",
                table: "SubjectOfferings",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "InternalPracticalPassMarks",
                table: "SubjectOfferings",
                type: "real",
                nullable: true);
        }
    }
}
