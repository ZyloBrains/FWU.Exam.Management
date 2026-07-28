using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdmitCardDisplayProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Campus",
                table: "HallTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamRollNo",
                table: "HallTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamType",
                table: "HallTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "HallTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Program",
                table: "HallTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "HallTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Semester",
                table: "HallTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "HallTickets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Campus",
                table: "HallTickets");

            migrationBuilder.DropColumn(
                name: "ExamRollNo",
                table: "HallTickets");

            migrationBuilder.DropColumn(
                name: "ExamType",
                table: "HallTickets");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "HallTickets");

            migrationBuilder.DropColumn(
                name: "Program",
                table: "HallTickets");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "HallTickets");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "HallTickets");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "HallTickets");
        }
    }
}
