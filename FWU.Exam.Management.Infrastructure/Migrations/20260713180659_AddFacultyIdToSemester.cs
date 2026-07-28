using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFacultyIdToSemester : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Semesters_Code",
                table: "Semesters");

            migrationBuilder.AddColumn<int>(
                name: "FacultyId",
                table: "Semesters",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_FacultyId_Code",
                table: "Semesters",
                columns: new[] { "FacultyId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_Faculties_FacultyId",
                table: "Semesters",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_Faculties_FacultyId",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_FacultyId_Code",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "FacultyId",
                table: "Semesters");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_Code",
                table: "Semesters",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }
    }
}
