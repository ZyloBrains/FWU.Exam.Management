using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentAdmissionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentAdmissions_StudentRegistrations_StudentRegistrationId",
                table: "StudentAdmissions");

            migrationBuilder.DropIndex(
                name: "IX_StudentAdmissions_StudentRegistrationId",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "StudentRegistrationId",
                table: "StudentAdmissions");

            migrationBuilder.AddColumn<int>(
                name: "StudentAdmissionId",
                table: "StudentRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "StudentAdmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_StudentAdmissionId",
                table: "StudentRegistrations",
                column: "StudentAdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_AcademicYearId",
                table: "StudentAdmissions",
                column: "AcademicYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAdmissions_AcademicYears_AcademicYearId",
                table: "StudentAdmissions",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_StudentAdmissions_StudentAdmissionId",
                table: "StudentRegistrations",
                column: "StudentAdmissionId",
                principalTable: "StudentAdmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentAdmissions_AcademicYears_AcademicYearId",
                table: "StudentAdmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_StudentAdmissions_StudentAdmissionId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_StudentAdmissionId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_StudentAdmissions_AcademicYearId",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "StudentAdmissionId",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "StudentAdmissions");

            migrationBuilder.AddColumn<int>(
                name: "StudentRegistrationId",
                table: "StudentAdmissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_StudentRegistrationId",
                table: "StudentAdmissions",
                column: "StudentRegistrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAdmissions_StudentRegistrations_StudentRegistrationId",
                table: "StudentAdmissions",
                column: "StudentRegistrationId",
                principalTable: "StudentRegistrations",
                principalColumn: "Id");
        }
    }
}
