using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeTypeAndProgramsIdToBillTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeeType",
                table: "BillTitle",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramsId",
                table: "BillTitle",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillTitle_ProgramsId",
                table: "BillTitle",
                column: "ProgramsId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillTitle_Programs_ProgramsId",
                table: "BillTitle",
                column: "ProgramsId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillTitle_Programs_ProgramsId",
                table: "BillTitle");

            migrationBuilder.DropIndex(
                name: "IX_BillTitle_ProgramsId",
                table: "BillTitle");

            migrationBuilder.DropColumn(
                name: "FeeType",
                table: "BillTitle");

            migrationBuilder.DropColumn(
                name: "ProgramsId",
                table: "BillTitle");
        }
    }
}
