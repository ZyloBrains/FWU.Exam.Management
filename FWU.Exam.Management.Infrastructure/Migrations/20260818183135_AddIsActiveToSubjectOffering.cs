using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToSubjectOffering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId",
                table: "SubjectOfferings");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SubjectOfferings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId",
                table: "SubjectOfferings",
                columns: new[] { "CurriculumVersionId", "SubjectCatalogId", "ProgramId", "SemesterId" },
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId",
                table: "SubjectOfferings");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SubjectOfferings");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId",
                table: "SubjectOfferings",
                columns: new[] { "CurriculumVersionId", "SubjectCatalogId", "ProgramId", "SemesterId" },
                unique: true,
                filter: "[CurriculumVersionId] IS NOT NULL");
        }
    }
}
