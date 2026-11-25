using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentAdmissionAppUserIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentAdmissions_AppUserId",
                table: "StudentAdmissions");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_AppUserId_IsActive_Id",
                table: "StudentAdmissions",
                columns: new[] { "AppUserId", "IsActive", "Id" })
                .Annotation("SqlServer:Include", new[] { "TenantId", "CollegeId", "ProgramsId", "HasFeeExemption" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentAdmissions_AppUserId_IsActive_Id",
                table: "StudentAdmissions");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_AppUserId",
                table: "StudentAdmissions",
                column: "AppUserId");
        }
    }
}
