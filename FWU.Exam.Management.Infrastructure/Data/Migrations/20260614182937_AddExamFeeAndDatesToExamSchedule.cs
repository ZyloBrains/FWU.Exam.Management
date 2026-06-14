using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    public partial class AddExamFeeAndDatesToExamSchedule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "ExamSchedules",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExamFee",
                table: "ExamSchedules",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PracticalSubjectFee",
                table: "ExamSchedules",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "ExamSchedules",
                type: "date",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "ExamFee",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "PracticalSubjectFee",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "ExamSchedules");
        }
    }
}
