using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdmissionIdentityAndExplicitEnrollmentFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "StudentAdmissions",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DateOfBirthAD",
                table: "StudentAdmissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DateOfBirthBS",
                table: "StudentAdmissions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StudentAdmissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "StudentAdmissions",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GenderId",
                table: "StudentAdmissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "StudentAdmissions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "StudentAdmissions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NepaliName",
                table: "StudentAdmissions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "StudentAdmissions",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE sa
                SET sa.FirstName = sr.FirstName,
                    sa.MiddleName = sr.MiddleName,
                    sa.LastName = sr.LastName,
                    sa.NepaliName = sr.NepaliName,
                    sa.DateOfBirthBS = sr.DateOfBirthBS,
                    sa.DateOfBirthAD = sr.DateOfBirthAD,
                    sa.GenderId = sr.GenderId,
                    sa.ContactNumber = sr.ContactNumber,
                    sa.Phone = sr.Phone,
                    sa.Email = sr.Email
                FROM dbo.StudentAdmissions sa
                INNER JOIN dbo.StudentRegistrations sr ON sr.StudentAdmissionId = sa.Id
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "DateOfBirthAD",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "DateOfBirthBS",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "NepaliName",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "StudentAdmissions");
        }
    }
}
