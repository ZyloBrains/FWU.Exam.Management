using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExamScheduleCollegeApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamScheduleCollegeApprovals");

            migrationBuilder.DropColumn(
                name: "CollegeApprovalDate",
                table: "ExamSchedules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CollegeApprovalDate",
                table: "ExamSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExamScheduleCollegeApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProposedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamScheduleCollegeApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamScheduleCollegeApprovals_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamScheduleCollegeApprovals_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamScheduleCollegeApprovals_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamScheduleCollegeApprovals_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleCollegeApprovals_ApprovedByUserId",
                table: "ExamScheduleCollegeApprovals",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleCollegeApprovals_CollegeId",
                table: "ExamScheduleCollegeApprovals",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleCollegeApprovals_ExamScheduleId_CollegeId",
                table: "ExamScheduleCollegeApprovals",
                columns: new[] { "ExamScheduleId", "CollegeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleCollegeApprovals_TenantId",
                table: "ExamScheduleCollegeApprovals",
                column: "TenantId");
        }
    }
}
