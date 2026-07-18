using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSubjectOfferingUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId_ProgramId",
                table: "SubjectOfferings");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId_ProgramId_SemesterId",
                table: "SubjectOfferings",
                columns: new[] { "SubjectCatalogId", "ProgramId", "SemesterId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId_ProgramId_SemesterId",
                table: "SubjectOfferings");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId_ProgramId",
                table: "SubjectOfferings",
                columns: new[] { "SubjectCatalogId", "ProgramId" },
                unique: true);
        }
    }
}
