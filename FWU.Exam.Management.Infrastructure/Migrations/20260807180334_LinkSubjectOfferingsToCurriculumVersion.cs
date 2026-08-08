using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkSubjectOfferingsToCurriculumVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId_ProgramId_SemesterId",
                table: "SubjectOfferings");

            migrationBuilder.AddColumn<int>(
                name: "CurriculumVersionId",
                table: "SubjectOfferings",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SubjectOfferings')
                BEGIN
                    INSERT INTO CurriculumVersions (TenantId, ProgramId, EffectiveAcademicYearId, Name, Description, IsActive)
                    SELECT DISTINCT
                        so.TenantId,
                        so.ProgramId,
                        s.AcademicYearId,
                        LEFT(N'Default - ' + ISNULL(p.ProgramName, N'Program') + N' (' + ISNULL(ay.AcademicYearName, N'') + N')', 100),
                        N'Auto-created curriculum version for existing subject offerings.',
                        1
                    FROM SubjectOfferings so
                    INNER JOIN Semesters s ON so.SemesterId = s.Id
                    LEFT JOIN Programs p ON so.ProgramId = p.Id
                    LEFT JOIN AcademicYears ay ON s.AcademicYearId = ay.Id
                    WHERE so.CurriculumVersionId IS NULL
                      AND NOT EXISTS (
                          SELECT 1 FROM CurriculumVersions cv
                          WHERE cv.TenantId = so.TenantId
                            AND cv.ProgramId = so.ProgramId
                            AND cv.EffectiveAcademicYearId = s.AcademicYearId
                      );

                    UPDATE so
                    SET so.CurriculumVersionId = cv.Id
                    FROM SubjectOfferings so
                    INNER JOIN Semesters s ON so.SemesterId = s.Id
                    INNER JOIN (
                        SELECT TenantId, ProgramId, EffectiveAcademicYearId, MAX(Id) AS Id
                        FROM CurriculumVersions
                        WHERE Name LIKE N'Default - %'
                        GROUP BY TenantId, ProgramId, EffectiveAcademicYearId
                    ) cv ON cv.TenantId = so.TenantId
                        AND cv.ProgramId = so.ProgramId
                        AND cv.EffectiveAcademicYearId = s.AcademicYearId
                    WHERE so.CurriculumVersionId IS NULL;
                END
                """);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId",
                table: "SubjectOfferings",
                columns: new[] { "CurriculumVersionId", "SubjectCatalogId", "ProgramId", "SemesterId" },
                unique: true,
                filter: "[CurriculumVersionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId",
                table: "SubjectOfferings",
                column: "SubjectCatalogId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectOfferings_CurriculumVersions_CurriculumVersionId",
                table: "SubjectOfferings",
                column: "CurriculumVersionId",
                principalTable: "CurriculumVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubjectOfferings_CurriculumVersions_CurriculumVersionId",
                table: "SubjectOfferings");

            migrationBuilder.DropIndex(
                name: "IX_SubjectOfferings_CurriculumVersionId_SubjectCatalogId_ProgramId_SemesterId",
                table: "SubjectOfferings");

            migrationBuilder.DropIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId",
                table: "SubjectOfferings");

            migrationBuilder.DropColumn(
                name: "CurriculumVersionId",
                table: "SubjectOfferings");

            migrationBuilder.Sql(
                """
                DELETE FROM CurriculumVersions WHERE Name LIKE N'Default - %';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_SubjectCatalogId_ProgramId_SemesterId",
                table: "SubjectOfferings",
                columns: new[] { "SubjectCatalogId", "ProgramId", "SemesterId" },
                unique: true);
        }
    }
}
