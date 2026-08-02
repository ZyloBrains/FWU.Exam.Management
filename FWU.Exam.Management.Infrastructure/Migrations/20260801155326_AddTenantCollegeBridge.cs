using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantCollegeBridge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql(@"
                INSERT INTO TenantColleges (TenantId, CollegeId)
                SELECT TenantId, Id
                FROM Colleges;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_Tenants_TenantId",
                table: "Colleges");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_TenantId_Code",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Colleges");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_Code",
                table: "Colleges",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantColleges_CollegeId",
                table: "TenantColleges",
                column: "CollegeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Colleges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE c
                SET c.TenantId = tc.TenantId
                FROM Colleges c
                INNER JOIN (
                    SELECT CollegeId, MIN(TenantId) AS TenantId
                    FROM TenantColleges
                    GROUP BY CollegeId
                ) tc ON tc.CollegeId = c.Id;
            ");

            migrationBuilder.DropTable(
                name: "TenantColleges");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_Code",
                table: "Colleges");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_TenantId_Code",
                table: "Colleges",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_Tenants_TenantId",
                table: "Colleges",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
