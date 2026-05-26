using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameOrganizationToFacultyAndFacultyToDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_Organizations_OrganizationId",
                table: "Colleges");

            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Faculties_FacultyId",
                table: "Programs");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_Faculties_FacultyId",
                table: "StudentRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Users",
                newName: "FacultyId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                newName: "IX_Users_FacultyId");

            migrationBuilder.RenameColumn(
                name: "FacultyId",
                table: "StudentRegistrations",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentRegistrations_FacultyId",
                table: "StudentRegistrations",
                newName: "IX_StudentRegistrations_DepartmentId");

            migrationBuilder.RenameColumn(
                name: "FacultyId",
                table: "Programs",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Programs_FacultyId",
                table: "Programs",
                newName: "IX_Programs_DepartmentId");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Colleges",
                newName: "FacultyId");

            migrationBuilder.RenameIndex(
                name: "IX_Colleges_OrganizationId",
                table: "Colleges",
                newName: "IX_Colleges_FacultyId");

            migrationBuilder.RenameColumn(
                name: "FacultyCode",
                table: "Faculties",
                newName: "DepartmentCode");

            migrationBuilder.RenameColumn(
                name: "FacultyName",
                table: "Faculties",
                newName: "DepartmentName");

            migrationBuilder.Sql("EXEC sp_rename 'dbo.Faculties', 'Departments'");

            migrationBuilder.Sql("EXEC sp_rename 'dbo.Organizations', 'Faculties'");

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_Faculties_FacultyId",
                table: "Colleges",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_Departments_DepartmentId",
                table: "Programs",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_Departments_DepartmentId",
                table: "StudentRegistrations",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Faculties_FacultyId",
                table: "Users",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_Faculties_FacultyId",
                table: "Colleges");

            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Departments_DepartmentId",
                table: "Programs");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_Departments_DepartmentId",
                table: "StudentRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Faculties_FacultyId",
                table: "Users");

            migrationBuilder.Sql("EXEC sp_rename 'dbo.Faculties', 'Organizations'");

            migrationBuilder.Sql("EXEC sp_rename 'dbo.Departments', 'Faculties'");

            migrationBuilder.RenameColumn(
                name: "DepartmentCode",
                table: "Faculties",
                newName: "FacultyCode");

            migrationBuilder.RenameColumn(
                name: "DepartmentName",
                table: "Faculties",
                newName: "FacultyName");

            migrationBuilder.RenameColumn(
                name: "FacultyId",
                table: "Users",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_FacultyId",
                table: "Users",
                newName: "IX_Users_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "StudentRegistrations",
                newName: "FacultyId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentRegistrations_DepartmentId",
                table: "StudentRegistrations",
                newName: "IX_StudentRegistrations_FacultyId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Programs",
                newName: "FacultyId");

            migrationBuilder.RenameIndex(
                name: "IX_Programs_DepartmentId",
                table: "Programs",
                newName: "IX_Programs_FacultyId");

            migrationBuilder.RenameColumn(
                name: "FacultyId",
                table: "Colleges",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Colleges_FacultyId",
                table: "Colleges",
                newName: "IX_Colleges_OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_Organizations_OrganizationId",
                table: "Colleges",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_Faculties_FacultyId",
                table: "Programs",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_Faculties_FacultyId",
                table: "StudentRegistrations",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
