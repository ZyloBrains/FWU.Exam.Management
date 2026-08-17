using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TenantScopedAcademicYearAndSemesterInstanceProgramId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_AcademicYears_AcademicYearId",
                table: "ExamSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_Semesters_SemesterId",
                table: "ExamSchedules");

            migrationBuilder.DropIndex(
                name: "IX_SemesterInstances_SemesterId_AcademicYearId",
                table: "SemesterInstances");

            migrationBuilder.DropIndex(
                name: "IX_ExamSchedules_AcademicYearId",
                table: "ExamSchedules");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_AcademicYearCode",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "ExamSchedules");

            migrationBuilder.RenameColumn(
                name: "SemesterId",
                table: "ExamSchedules",
                newName: "SemesterInstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamSchedules_SemesterId",
                table: "ExamSchedules",
                newName: "IX_ExamSchedules_SemesterInstanceId");

            migrationBuilder.AddColumn<int>(
                name: "ProgramId",
                table: "SemesterInstances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "AcademicYears",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "AcademicYears",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AcademicYears",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SemesterInstances_ProgramId",
                table: "SemesterInstances",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterInstances_SemesterId_AcademicYearId_ProgramId",
                table: "SemesterInstances",
                columns: new[] { "SemesterId", "AcademicYearId", "ProgramId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_TenantId_AcademicYearCode",
                table: "AcademicYears",
                columns: new[] { "TenantId", "AcademicYearCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Tenants_TenantId",
                table: "AcademicYears",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_SemesterInstances_SemesterInstanceId",
                table: "ExamSchedules",
                column: "SemesterInstanceId",
                principalTable: "SemesterInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SemesterInstances_Programs_ProgramId",
                table: "SemesterInstances",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Tenants_TenantId",
                table: "AcademicYears");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_SemesterInstances_SemesterInstanceId",
                table: "ExamSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_SemesterInstances_Programs_ProgramId",
                table: "SemesterInstances");

            migrationBuilder.DropIndex(
                name: "IX_SemesterInstances_ProgramId",
                table: "SemesterInstances");

            migrationBuilder.DropIndex(
                name: "IX_SemesterInstances_SemesterId_AcademicYearId_ProgramId",
                table: "SemesterInstances");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_TenantId_AcademicYearCode",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "SemesterInstances");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AcademicYears");

            migrationBuilder.RenameColumn(
                name: "SemesterInstanceId",
                table: "ExamSchedules",
                newName: "SemesterId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamSchedules_SemesterInstanceId",
                table: "ExamSchedules",
                newName: "IX_ExamSchedules_SemesterId");

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "ExamSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SemesterInstances_SemesterId_AcademicYearId",
                table: "SemesterInstances",
                columns: new[] { "SemesterId", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_AcademicYearId",
                table: "ExamSchedules",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_AcademicYearCode",
                table: "AcademicYears",
                column: "AcademicYearCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_AcademicYears_AcademicYearId",
                table: "ExamSchedules",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_Semesters_SemesterId",
                table: "ExamSchedules",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
