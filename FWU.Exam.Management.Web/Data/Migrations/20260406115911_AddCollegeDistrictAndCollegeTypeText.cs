using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fwu_examination_management_system.Migrations
{
    /// <inheritdoc />
    public partial class AddCollegeDistrictAndCollegeTypeText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DistrictName",
                table: "Colleges",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollegeTypeName",
                table: "Colleges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistrictName",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "CollegeTypeName",
                table: "Colleges");
        }
    }
}
