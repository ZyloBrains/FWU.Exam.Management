using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToSubjectOfferings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId",
                table: "SubjectOfferings");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId_ProgramId",
                table: "SubjectOfferings",
                columns: new[] { "SubjectCatalogId", "ProgramId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId_ProgramId",
                table: "SubjectOfferings");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId",
                table: "SubjectOfferings",
                column: "SubjectCatalogId");
        }
    }
}
