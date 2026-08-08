using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkExamSchedulesToCurriculumVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurriculumVersionId",
                table: "ExamSchedules",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE es
                SET es.CurriculumVersionId = cv.Id
                FROM ExamSchedules es
                INNER JOIN CurriculumVersions cv
                    ON cv.TenantId = es.TenantId
                   AND cv.ProgramId = es.ProgramId
                   AND cv.EffectiveAcademicYearId = es.AcademicYearId
                WHERE cv.Name LIKE 'Default - %'
                  AND es.CurriculumVersionId IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_CurriculumVersionId",
                table: "ExamSchedules",
                column: "CurriculumVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_CurriculumVersions_CurriculumVersionId",
                table: "ExamSchedules",
                column: "CurriculumVersionId",
                principalTable: "CurriculumVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_CurriculumVersions_CurriculumVersionId",
                table: "ExamSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ExamSchedules_CurriculumVersionId",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "CurriculumVersionId",
                table: "ExamSchedules");
        }
    }
}
