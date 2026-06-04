using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToFaculty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Faculties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_TenantId",
                table: "Faculties",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Tenants_TenantId",
                table: "Faculties",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_Tenants_TenantId",
                table: "Faculties");

            migrationBuilder.DropIndex(
                name: "IX_Faculties_TenantId",
                table: "Faculties");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Faculties");
        }
    }
}
