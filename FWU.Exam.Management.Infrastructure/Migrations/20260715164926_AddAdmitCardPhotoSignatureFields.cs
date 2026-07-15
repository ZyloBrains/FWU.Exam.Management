using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdmitCardPhotoSignatureFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ControllerSignaturePath",
                table: "HallTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "HallTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignaturePath",
                table: "HallTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ControllerSignaturePath",
                table: "Faculties",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ControllerSignaturePath",
                table: "HallTickets");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "HallTickets");

            migrationBuilder.DropColumn(
                name: "SignaturePath",
                table: "HallTickets");

            migrationBuilder.DropColumn(
                name: "ControllerSignaturePath",
                table: "Faculties");
        }
    }
}
