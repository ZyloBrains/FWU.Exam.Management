using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class CollegeFacultyManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_Faculties_FacultyId",
                table: "Colleges");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_FacultyId",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "FacultyId",
                table: "Colleges");

            migrationBuilder.CreateTable(
                name: "CollegeFaculty",
                columns: table => new
                {
                    CollegesId = table.Column<int>(type: "int", nullable: false),
                    FacultiesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollegeFaculty", x => new { x.CollegesId, x.FacultiesId });
                    table.ForeignKey(
                        name: "FK_CollegeFaculty_Colleges_CollegesId",
                        column: x => x.CollegesId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollegeFaculty_Faculties_FacultiesId",
                        column: x => x.FacultiesId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollegeFaculty_FacultiesId",
                table: "CollegeFaculty",
                column: "FacultiesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollegeFaculty");

            migrationBuilder.AddColumn<int>(
                name: "FacultyId",
                table: "Colleges",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_FacultyId",
                table: "Colleges",
                column: "FacultyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_Faculties_FacultyId",
                table: "Colleges",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
