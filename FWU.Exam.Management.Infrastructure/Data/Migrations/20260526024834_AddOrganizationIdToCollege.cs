using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationIdToCollege : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Colleges",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_OrganizationId",
                table: "Colleges",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropIndex(
                name: "IX_Colleges_OrganizationId",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Colleges");
        }
    }
}
