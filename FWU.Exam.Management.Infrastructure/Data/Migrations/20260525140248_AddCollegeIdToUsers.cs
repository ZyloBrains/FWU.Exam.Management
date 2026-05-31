using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCollegeIdToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollegeId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CollegeId",
                table: "Users",
                column: "CollegeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Colleges_CollegeId",
                table: "Users",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Colleges_CollegeId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CollegeId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CollegeId",
                table: "Users");
        }
    }
}
