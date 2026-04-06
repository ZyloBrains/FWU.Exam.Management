using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fwu_examination_management_system.Migrations
{
    /// <inheritdoc />
    public partial class AddCollegeAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Colleges",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Colleges");
        }
    }
}
