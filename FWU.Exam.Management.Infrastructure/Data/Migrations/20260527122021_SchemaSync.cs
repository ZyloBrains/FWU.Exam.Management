using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    public partial class SchemaSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing FKs (some may not exist)
            migrationBuilder.Sql("IF OBJECT_ID('FK_Programs_Faculties_FacultyId', 'F') IS NOT NULL ALTER TABLE [Programs] DROP CONSTRAINT [FK_Programs_Faculties_FacultyId]");
            migrationBuilder.Sql("IF OBJECT_ID('FK_StudentRegistrations_Faculties_FacultyId', 'F') IS NOT NULL ALTER TABLE [StudentRegistrations] DROP CONSTRAINT [FK_StudentRegistrations_Faculties_FacultyId]");
            migrationBuilder.Sql("IF OBJECT_ID('FK_Users_Organizations_OrganizationId', 'F') IS NOT NULL ALTER TABLE [Users] DROP CONSTRAINT [FK_Users_Organizations_OrganizationId]");
            migrationBuilder.Sql("IF OBJECT_ID('FK_Colleges_Organizations_OrganizationId', 'F') IS NOT NULL ALTER TABLE [Colleges] DROP CONSTRAINT [FK_Colleges_Organizations_OrganizationId]");

            // Rename old Faculties (department data) columns for Department entity
            migrationBuilder.RenameColumn(name: "FacultyCode", table: "Faculties", newName: "DepartmentCode");
            migrationBuilder.RenameColumn(name: "FacultyName", table: "Faculties", newName: "DepartmentName");

            // Rename Faculties -> Departments (preserves data)
            // Also rename the PK constraint to avoid name conflict with new Faculties table
            migrationBuilder.Sql("EXEC sp_rename N'[dbo].[Faculties]', N'Departments', 'OBJECT'");
            migrationBuilder.Sql("EXEC sp_rename N'[dbo].[PK_Faculties]', N'PK_Departments', 'OBJECT'");

            // Create new Faculties (top-level entity)
            migrationBuilder.CreateTable(
                name: "Faculties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Faculties", x => x.Id));

            // Seed the 9 faculties
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Faculties])
BEGIN
    SET IDENTITY_INSERT [Faculties] ON;
    INSERT INTO [Faculties] ([Id], [Name], [OfficeCode], [ContactNumber], [Address], [Email]) VALUES
    (1, 'Faculty of Humanities and Social Sciences', 'FO-HSS', '', '', ''),
    (2, 'Faculty of Management', 'FO-MGT', '', '', ''),
    (3, 'Faculty of Science and Technology', 'FST', '', '', ''),
    (4, 'Faculty of Education', 'EDU', '', '', ''),
    (5, 'Faculty of Law', 'FOL', '', '', ''),
    (6, 'Faculty of Engineering', 'ENG', '', '', ''),
    (7, 'Faculty of Agriculture', 'AGR', '', '', ''),
    (8, 'Faculty of Medicine', 'FOM', '', '', ''),
    (9, 'Faculty of Ayurveda', 'AYU', '', '', '');
    SET IDENTITY_INSERT [Faculties] OFF;
END");

            // Add FacultyId to Users (column does not exist yet)
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.AddColumn<int>(
                    name: "FacultyId",
                    table: "Users",
                    type: "int",
                    nullable: true);

                migrationBuilder.CreateIndex(
                    name: "IX_Users_FacultyId",
                    table: "Users",
                    column: "FacultyId");
            }

            // Rename Colleges.OrganizationId -> FacultyId
            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Colleges",
                newName: "FacultyId");

            migrationBuilder.RenameIndex(
                name: "IX_Colleges_OrganizationId",
                table: "Colleges",
                newName: "IX_Colleges_FacultyId");

            // Rename StudentRegistrations.FacultyId -> DepartmentId
            migrationBuilder.RenameColumn(
                name: "FacultyId",
                table: "StudentRegistrations",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentRegistrations_FacultyId",
                table: "StudentRegistrations",
                newName: "IX_StudentRegistrations_DepartmentId");

            // Rename Programs.FacultyId -> DepartmentId
            migrationBuilder.RenameColumn(
                name: "FacultyId",
                table: "Programs",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Programs_FacultyId",
                table: "Programs",
                newName: "IX_Programs_DepartmentId");

            // Create new FKs
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Colleges_Faculties_FacultyId", table: "Colleges");
            migrationBuilder.DropForeignKey(name: "FK_Programs_Departments_DepartmentId", table: "Programs");
            migrationBuilder.DropForeignKey(name: "FK_StudentRegistrations_Departments_DepartmentId", table: "StudentRegistrations");
            migrationBuilder.DropForeignKey(name: "FK_Users_Faculties_FacultyId", table: "Users");

            // Drop FacultyId from Users
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.DropIndex(name: "IX_Users_FacultyId", table: "Users");
                migrationBuilder.DropColumn(name: "FacultyId", table: "Users");
            }

            // Drop Faculties table and re-create Organizations instead
            migrationBuilder.DropTable(name: "Faculties");

            // Rename Departments -> Faculties
            migrationBuilder.Sql("EXEC sp_rename N'[dbo].[Departments]', N'Faculties', 'OBJECT'");
            migrationBuilder.Sql("EXEC sp_rename N'[dbo].[PK_Departments]', N'PK_Faculties', 'OBJECT'");

            // Rename Department columns back
            migrationBuilder.RenameColumn(name: "DepartmentCode", table: "Faculties", newName: "FacultyCode");
            migrationBuilder.RenameColumn(name: "DepartmentName", table: "Faculties", newName: "FacultyName");

            // Rename columns back
            migrationBuilder.RenameColumn(name: "FacultyId", table: "Users", newName: "OrganizationId");
            migrationBuilder.RenameIndex(name: "IX_Users_FacultyId", table: "Users", newName: "IX_Users_OrganizationId");

            migrationBuilder.RenameColumn(name: "FacultyId", table: "Colleges", newName: "OrganizationId");
            migrationBuilder.RenameIndex(name: "IX_Colleges_FacultyId", table: "Colleges", newName: "IX_Colleges_OrganizationId");

            migrationBuilder.RenameColumn(name: "DepartmentId", table: "StudentRegistrations", newName: "FacultyId");
            migrationBuilder.RenameIndex(name: "IX_StudentRegistrations_DepartmentId", table: "StudentRegistrations", newName: "IX_StudentRegistrations_FacultyId");

            migrationBuilder.RenameColumn(name: "DepartmentId", table: "Programs", newName: "FacultyId");
            migrationBuilder.RenameIndex(name: "IX_Programs_DepartmentId", table: "Programs", newName: "IX_Programs_FacultyId");

            // Recreate Organizations table
            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Organizations", x => x.Id));

            // Restore old FKs
            migrationBuilder.AddForeignKey(name: "FK_Colleges_Organizations_OrganizationId", table: "Colleges", column: "OrganizationId", principalTable: "Organizations", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_Programs_Faculties_FacultyId", table: "Programs", column: "FacultyId", principalTable: "Faculties", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_StudentRegistrations_Faculties_FacultyId", table: "StudentRegistrations", column: "FacultyId", principalTable: "Faculties", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_Users_Organizations_OrganizationId", table: "Users", column: "OrganizationId", principalTable: "Organizations", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }
    }
}
