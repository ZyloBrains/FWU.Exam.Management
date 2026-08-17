using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeGroupAndGradePoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GradeGroupId",
                table: "GradingSchemes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GradeGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    GradeGroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GradePoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    ObtainedMark = table.Column<int>(type: "int", nullable: false),
                    GradePointValue = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    GradeGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradePoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradePoints_GradeGroups_GradeGroupId",
                        column: x => x.GradeGroupId,
                        principalTable: "GradeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GradingSchemes_GradeGroupId",
                table: "GradingSchemes",
                column: "GradeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeGroups_GradeGroupName",
                table: "GradeGroups",
                column: "GradeGroupName");

            migrationBuilder.CreateIndex(
                name: "IX_GradePoints_GradeGroupId_ObtainedMark",
                table: "GradePoints",
                columns: new[] { "GradeGroupId", "ObtainedMark" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GradingSchemes_GradeGroups_GradeGroupId",
                table: "GradingSchemes",
                column: "GradeGroupId",
                principalTable: "GradeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GradingSchemes_GradeGroups_GradeGroupId",
                table: "GradingSchemes");

            migrationBuilder.DropTable(
                name: "GradePoints");

            migrationBuilder.DropTable(
                name: "GradeGroups");

            migrationBuilder.DropIndex(
                name: "IX_GradingSchemes_GradeGroupId",
                table: "GradingSchemes");

            migrationBuilder.DropColumn(
                name: "GradeGroupId",
                table: "GradingSchemes");
        }
    }
}
