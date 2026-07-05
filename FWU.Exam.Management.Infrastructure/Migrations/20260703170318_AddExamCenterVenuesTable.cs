using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamCenterVenuesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamCenters_Colleges_CollegeId",
                table: "ExamCenters");

            migrationBuilder.AlterColumn<int>(
                name: "CollegeId",
                table: "ExamCenters",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "ExamCenterVenues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ExamCenterId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamCenterVenues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamCenterVenues_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamCenterVenues_ExamCenters_ExamCenterId",
                        column: x => x.ExamCenterId,
                        principalTable: "ExamCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamCenterVenues_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterVenues_CollegeId",
                table: "ExamCenterVenues",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterVenues_ExamCenterId",
                table: "ExamCenterVenues",
                column: "ExamCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterVenues_TenantId",
                table: "ExamCenterVenues",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamCenters_Colleges_CollegeId",
                table: "ExamCenters",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamCenters_Colleges_CollegeId",
                table: "ExamCenters");

            migrationBuilder.DropTable(
                name: "ExamCenterVenues");

            migrationBuilder.AlterColumn<int>(
                name: "CollegeId",
                table: "ExamCenters",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamCenters_Colleges_CollegeId",
                table: "ExamCenters",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
