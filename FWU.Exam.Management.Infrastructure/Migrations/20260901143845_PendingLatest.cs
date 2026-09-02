using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PendingLatest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GradingSchemes_AcademicYears_AcademicYearId",
                table: "GradingSchemes");

            migrationBuilder.DropForeignKey(
                name: "FK_GradingSchemes_Programs_ProgramId",
                table: "GradingSchemes");

            migrationBuilder.DropIndex(
                name: "IX_GradingSchemes_AcademicYearId",
                table: "GradingSchemes");

            migrationBuilder.DropIndex(
                name: "IX_GradingSchemes_ProgramId",
                table: "GradingSchemes");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "GradingSchemes");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "GradingSchemes");

            migrationBuilder.AddColumn<int>(
                name: "GradingSchemeId",
                table: "ExamSubjectResults",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GradingSchemePrograms",
                columns: table => new
                {
                    GradingSchemeId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingSchemePrograms", x => new { x.GradingSchemeId, x.ProgramId });
                    table.ForeignKey(
                        name: "FK_GradingSchemePrograms_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradingSchemePrograms_GradingSchemes_GradingSchemeId",
                        column: x => x.GradingSchemeId,
                        principalTable: "GradingSchemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GradingSchemePrograms_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectResults_GradingSchemeId",
                table: "ExamSubjectResults",
                column: "GradingSchemeId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingSchemePrograms_AcademicYearId",
                table: "GradingSchemePrograms",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingSchemePrograms_GradingSchemeId_ProgramId",
                table: "GradingSchemePrograms",
                columns: new[] { "GradingSchemeId", "ProgramId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradingSchemePrograms_ProgramId",
                table: "GradingSchemePrograms",
                column: "ProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSubjectResults_GradingSchemes_GradingSchemeId",
                table: "ExamSubjectResults",
                column: "GradingSchemeId",
                principalTable: "GradingSchemes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSubjectResults_GradingSchemes_GradingSchemeId",
                table: "ExamSubjectResults");

            migrationBuilder.DropTable(
                name: "GradingSchemePrograms");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectResults_GradingSchemeId",
                table: "ExamSubjectResults");

            migrationBuilder.DropColumn(
                name: "GradingSchemeId",
                table: "ExamSubjectResults");

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "GradingSchemes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramId",
                table: "GradingSchemes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GradingSchemes_AcademicYearId",
                table: "GradingSchemes",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingSchemes_ProgramId",
                table: "GradingSchemes",
                column: "ProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_GradingSchemes_AcademicYears_AcademicYearId",
                table: "GradingSchemes",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GradingSchemes_Programs_ProgramId",
                table: "GradingSchemes",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
