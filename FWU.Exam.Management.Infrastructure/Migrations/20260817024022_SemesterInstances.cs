using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SemesterInstances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SemesterInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemesterInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SemesterInstances_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SemesterInstances_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SemesterInstances_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SemesterInstances_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SemesterInstances_AcademicYearId",
                table: "SemesterInstances",
                column: "AcademicYearId");

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
                name: "IX_SemesterInstances_TenantId",
                table: "SemesterInstances",
                column: "TenantId");

            migrationBuilder.DropForeignKey(
                name: "FK_SemesterEnrollments_Semesters_SemesterId",
                table: "SemesterEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_Semesters_SemesterId",
                table: "ExamSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_AcademicYears_AcademicYearId",
                table: "ExamSchedules");

            migrationBuilder.Sql(@"
                DECLARE @TenantId INT = 1;

                INSERT INTO SemesterInstances (TenantId, SemesterId, AcademicYearId, ProgramId, StartDate, EndDate)
                SELECT @TenantId, s.Id, s.AcademicYearId, ps.ProgramId, s.StartDate, s.EndDate
                FROM Semesters s
                INNER JOIN ProgramSemesters ps ON ps.SemesterId = s.Id
                WHERE s.AcademicYearId > 0
                  AND ps.ProgramId > 0
                  AND ps.IsActive = 1;

                INSERT INTO SemesterInstances (TenantId, SemesterId, AcademicYearId, ProgramId, StartDate, EndDate)
                SELECT @TenantId, es.SemesterId, es.AcademicYearId, es.ProgramId, s.StartDate, s.EndDate
                FROM ExamSchedules es
                INNER JOIN Semesters s ON es.SemesterId = s.Id
                WHERE s.AcademicYearId > 0
                  AND NOT EXISTS (
                      SELECT 1 FROM SemesterInstances si
                      WHERE si.SemesterId = es.SemesterId
                        AND si.AcademicYearId = es.AcademicYearId
                        AND si.ProgramId = es.ProgramId);

                UPDATE se
                SET se.SemesterId = si.Id
                FROM SemesterEnrollments se
                INNER JOIN Semesters s ON se.SemesterId = s.Id
                INNER JOIN StudentAdmissions sa ON se.StudentAdmissionId = sa.Id
                INNER JOIN SemesterInstances si
                    ON si.SemesterId = s.Id
                    AND si.AcademicYearId = s.AcademicYearId
                    AND si.ProgramId = sa.ProgramsId
                WHERE sa.ProgramsId IS NOT NULL AND sa.ProgramsId > 0;

                UPDATE es
                SET es.SemesterId = si.Id
                FROM ExamSchedules es
                INNER JOIN SemesterInstances si
                    ON si.SemesterId = es.SemesterId
                    AND si.AcademicYearId = es.AcademicYearId
                    AND si.ProgramId = es.ProgramId;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_AcademicYears_AcademicYearId",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_AcademicYearId",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Semesters");

            migrationBuilder.RenameColumn(
                name: "SemesterId",
                table: "SemesterEnrollments",
                newName: "SemesterInstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_SemesterEnrollments_SemesterId",
                table: "SemesterEnrollments",
                newName: "IX_SemesterEnrollments_SemesterInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_SemesterEnrollments_SemesterInstances_SemesterInstanceId",
                table: "SemesterEnrollments",
                column: "SemesterInstanceId",
                principalTable: "SemesterInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SemesterEnrollments_SemesterInstances_SemesterInstanceId",
                table: "SemesterEnrollments");

            migrationBuilder.DropTable(
                name: "SemesterInstances");

            migrationBuilder.RenameColumn(
                name: "SemesterInstanceId",
                table: "SemesterEnrollments",
                newName: "SemesterId");

            migrationBuilder.RenameIndex(
                name: "IX_SemesterEnrollments_SemesterInstanceId",
                table: "SemesterEnrollments",
                newName: "IX_SemesterEnrollments_SemesterId");

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "Semesters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Semesters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Semesters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Semesters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_AcademicYearId",
                table: "Semesters",
                column: "AcademicYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_SemesterEnrollments_Semesters_SemesterId",
                table: "SemesterEnrollments",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_Semesters_SemesterId",
                table: "ExamSchedules",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_AcademicYears_AcademicYearId",
                table: "ExamSchedules",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_AcademicYears_AcademicYearId",
                table: "Semesters",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
