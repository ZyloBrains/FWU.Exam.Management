using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFacultyAndProgramToStudentRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FacultyId",
                table: "StudentRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramId",
                table: "StudentRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_FacultyId",
                table: "StudentRegistrations",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_ProgramId",
                table: "StudentRegistrations",
                column: "ProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_Faculties_FacultyId",
                table: "StudentRegistrations",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_Programs_ProgramId",
                table: "StudentRegistrations",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_Faculties_FacultyId",
                table: "StudentRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_Programs_ProgramId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_FacultyId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_ProgramId",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "FacultyId",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "StudentRegistrations");
        }
    }
}
