using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveControllerSignatureToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ControllerSignaturePath",
                table: "Faculties");

            migrationBuilder.AddColumn<string>(
                name: "ControllerSignaturePath",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ControllerSignaturePath",
                table: "Tenants");

            migrationBuilder.AddColumn<string>(
                name: "ControllerSignaturePath",
                table: "Faculties",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
