using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramSemesterJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Semesters_FacultyId_Code",
                table: "Semesters");

            migrationBuilder.CreateTable(
                name: "ProgramSemesters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramSemesters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramSemesters_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramSemesters_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_FacultyId_AcademicYearId_Number",
                table: "Semesters",
                columns: new[] { "FacultyId", "AcademicYearId", "Number" },
                unique: true,
                filter: "[AcademicYearId] IS NOT NULL");

            migrationBuilder.Sql(@"
                INSERT INTO ProgramSemesters (ProgramId, SemesterId, IsActive, DisplayOrder)
                SELECT DISTINCT so.ProgramId, so.SemesterId, 1, 0
                FROM SubjectOfferings so
                WHERE NOT EXISTS (
                    SELECT 1 FROM ProgramSemesters ps
                    WHERE ps.ProgramId = so.ProgramId AND ps.SemesterId = so.SemesterId
                );
            ");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSemesters_ProgramId_SemesterId",
                table: "ProgramSemesters",
                columns: new[] { "ProgramId", "SemesterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSemesters_SemesterId",
                table: "ProgramSemesters",
                column: "SemesterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgramSemesters");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_FacultyId_AcademicYearId_Number",
                table: "Semesters");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_FacultyId_Code",
                table: "Semesters",
                columns: new[] { "FacultyId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");
        }
    }
}
