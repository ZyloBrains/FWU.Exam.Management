using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGradeGroupFromGradingScheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GradingSchemes_GradeGroups_GradeGroupId",
                table: "GradingSchemes");

            migrationBuilder.DropIndex(
                name: "IX_GradingSchemes_GradeGroupId",
                table: "GradingSchemes");

            migrationBuilder.DropColumn(
                name: "GradeGroupId",
                table: "GradingSchemes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GradeGroupId",
                table: "GradingSchemes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradingSchemes_GradeGroupId",
                table: "GradingSchemes",
                column: "GradeGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_GradingSchemes_GradeGroups_GradeGroupId",
                table: "GradingSchemes",
                column: "GradeGroupId",
                principalTable: "GradeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
