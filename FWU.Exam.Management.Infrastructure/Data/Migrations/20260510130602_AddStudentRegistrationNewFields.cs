using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentRegistrationNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "UserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "UserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "UserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "BirthPlace",
                table: "StudentRegistrations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenshipDistrict",
                table: "StudentRegistrations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CitizenshipIssueDate",
                table: "StudentRegistrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenshipNo",
                table: "StudentRegistrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "StudentRegistrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "StudentQualifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthPlace",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "CitizenshipDistrict",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "CitizenshipIssueDate",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "CitizenshipNo",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "StudentQualifications");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "UserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "UserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "UserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
