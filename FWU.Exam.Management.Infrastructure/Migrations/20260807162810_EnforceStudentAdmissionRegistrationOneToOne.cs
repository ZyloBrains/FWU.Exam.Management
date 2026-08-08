using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceStudentAdmissionRegistrationOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_StudentAdmissionId",
                table: "StudentRegistrations");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_StudentAdmissionId",
                table: "StudentRegistrations",
                column: "StudentAdmissionId",
                unique: true,
                filter: "[StudentAdmissionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_StudentAdmissionId",
                table: "StudentRegistrations");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_StudentAdmissionId",
                table: "StudentRegistrations",
                column: "StudentAdmissionId");
        }
    }
}
