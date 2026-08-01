using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeSubjectCatalogCodeIndexNonUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubjectCatalogs_SubjectCode",
                table: "SubjectCatalogs");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCatalogs_SubjectCode",
                table: "SubjectCatalogs",
                column: "SubjectCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubjectCatalogs_SubjectCode",
                table: "SubjectCatalogs");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCatalogs_SubjectCode",
                table: "SubjectCatalogs",
                column: "SubjectCode",
                unique: true);
        }
    }
}
