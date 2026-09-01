using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTenantCollegesWithCollegeFaculties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollegeFaculties",
                columns: table => new
                {
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    FacultyId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollegeFaculties", x => new { x.CollegeId, x.FacultyId });
                    table.ForeignKey(
                        name: "FK_CollegeFaculties_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollegeFaculties_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollegeFaculties_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollegeFaculties_FacultyId",
                table: "CollegeFaculties",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeFaculties_TenantId",
                table: "CollegeFaculties",
                column: "TenantId");

            migrationBuilder.Sql("""
                INSERT INTO [CollegeFaculties] ([TenantId], [CollegeId], [FacultyId])
                SELECT COALESCE(f.[TenantId], 0), cf.[CollegesId], cf.[FacultiesId]
                FROM [CollegeFaculty] cf
                INNER JOIN [Faculties] f ON f.[Id] = cf.[FacultiesId]
                """);

            migrationBuilder.DropTable(
                name: "CollegeFaculty");

            migrationBuilder.DropTable(
                name: "TenantColleges");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollegeFaculties");

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

            migrationBuilder.CreateTable(
                name: "TenantColleges",
                columns: table => new
                {
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantColleges", x => new { x.TenantId, x.CollegeId });
                    table.ForeignKey(
                        name: "FK_TenantColleges_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantColleges_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollegeFaculty_FacultiesId",
                table: "CollegeFaculty",
                column: "FacultiesId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantColleges_CollegeId",
                table: "TenantColleges",
                column: "CollegeId");
        }
    }
}
