using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopingToSubjectCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "SubjectCatalogs",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SubjectCatalogs')
                BEGIN
                    UPDATE SubjectCatalogs
                    SET TenantId = (SELECT MIN(Id) FROM Tenants)
                    WHERE TenantId IS NULL
                      AND EXISTS (SELECT 1 FROM Tenants);
                END
                """);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCatalogs_TenantId",
                table: "SubjectCatalogs",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectCatalogs_Tenants_TenantId",
                table: "SubjectCatalogs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubjectCatalogs_Tenants_TenantId",
                table: "SubjectCatalogs");

            migrationBuilder.DropIndex(
                name: "IX_SubjectCatalogs_TenantId",
                table: "SubjectCatalogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SubjectCatalogs");
        }
    }
}
