using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHallTicketsAndRetotalRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HallTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ExamRegistrationId = table.Column<int>(type: "int", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    StudentRegistrationId = table.Column<int>(type: "int", nullable: true),
                    HallTicketNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDownloaded = table.Column<bool>(type: "bit", nullable: false),
                    DownloadedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HallTickets_ExamRegistrations_ExamRegistrationId",
                        column: x => x.ExamRegistrationId,
                        principalTable: "ExamRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HallTickets_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HallTickets_StudentRegistrations_StudentRegistrationId",
                        column: x => x.StudentRegistrationId,
                        principalTable: "StudentRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HallTickets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RetotalRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ExamSubjectResultId = table.Column<int>(type: "int", nullable: false),
                    StudentRegistrationId = table.Column<int>(type: "int", nullable: false),
                    ExamRegistrationId = table.Column<int>(type: "int", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OriginalGradeLetter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalObtainedMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RetotalledGradeLetter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetotalledObtainedMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReviewedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FeePaid = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetotalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetotalRequests_ExamRegistrations_ExamRegistrationId",
                        column: x => x.ExamRegistrationId,
                        principalTable: "ExamRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetotalRequests_ExamSubjectResults_ExamSubjectResultId",
                        column: x => x.ExamSubjectResultId,
                        principalTable: "ExamSubjectResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetotalRequests_StudentRegistrations_StudentRegistrationId",
                        column: x => x.StudentRegistrationId,
                        principalTable: "StudentRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetotalRequests_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HallTickets_ExamRegistrationId",
                table: "HallTickets",
                column: "ExamRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_HallTickets_ExamScheduleId",
                table: "HallTickets",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_HallTickets_StudentRegistrationId",
                table: "HallTickets",
                column: "StudentRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_HallTickets_TenantId",
                table: "HallTickets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RetotalRequests_ExamRegistrationId",
                table: "RetotalRequests",
                column: "ExamRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_RetotalRequests_ExamSubjectResultId",
                table: "RetotalRequests",
                column: "ExamSubjectResultId");

            migrationBuilder.CreateIndex(
                name: "IX_RetotalRequests_StudentRegistrationId",
                table: "RetotalRequests",
                column: "StudentRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_RetotalRequests_TenantId",
                table: "RetotalRequests",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HallTickets");

            migrationBuilder.DropTable(
                name: "RetotalRequests");
        }
    }
}
