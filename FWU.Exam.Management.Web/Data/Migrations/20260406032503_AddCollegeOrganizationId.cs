using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fwu_examination_management_system.Migrations
{
    /// <inheritdoc />
    public partial class AddCollegeOrganizationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF NOT EXISTS (SELECT 1 FROM [Organizations] WHERE [Name] = N'Panika' OR [OfficeCode] = N'PANIKA')
BEGIN
    INSERT INTO [Organizations] ([Name], [OfficeCode], [ContactNumber], [Address], [Email], [LogoPath])
    VALUES (N'Panika', N'PANIKA', N'', N'Far Western University, Office of the Controller of Examinations', N'', NULL);
END
""");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Colleges",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
UPDATE [Colleges]
SET [OrganizationId] = (
    SELECT TOP(1) [Id]
    FROM [Organizations]
    WHERE [Name] = N'Panika' OR [OfficeCode] = N'PANIKA'
    ORDER BY [Id]
)
WHERE [OrganizationId] IS NULL;
""");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "Colleges",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_OrganizationId",
                table: "Colleges",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_Organizations_OrganizationId",
                table: "Colleges",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_Organizations_OrganizationId",
                table: "Colleges");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_OrganizationId",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Colleges");
        }
    }
}
