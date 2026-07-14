using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherSubjectAssignments");

            migrationBuilder.CreateTable(
                name: "CollegeAdminSubjectAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CollegeAdminUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubjectOfferingId = table.Column<int>(type: "int", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollegeAdminSubjectAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollegeAdminSubjectAssignments_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollegeAdminSubjectAssignments_SubjectOfferings_SubjectOfferingId",
                        column: x => x.SubjectOfferingId,
                        principalTable: "SubjectOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollegeAdminSubjectAssignments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollegeAdminSubjectAssignments_Users_CollegeAdminUserId",
                        column: x => x.CollegeAdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollegeAdminSubjectAssignments_CollegeAdminUserId",
                table: "CollegeAdminSubjectAssignments",
                column: "CollegeAdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeAdminSubjectAssignments_ExamScheduleId",
                table: "CollegeAdminSubjectAssignments",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeAdminSubjectAssignments_SubjectOfferingId",
                table: "CollegeAdminSubjectAssignments",
                column: "SubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeAdminSubjectAssignments_TenantId",
                table: "CollegeAdminSubjectAssignments",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollegeAdminSubjectAssignments");

            migrationBuilder.CreateTable(
                name: "TeacherSubjectAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: true),
                    SubjectOfferingId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TeacherUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSubjectAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherSubjectAssignments_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherSubjectAssignments_SubjectOfferings_SubjectOfferingId",
                        column: x => x.SubjectOfferingId,
                        principalTable: "SubjectOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherSubjectAssignments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherSubjectAssignments_Users_TeacherUserId",
                        column: x => x.TeacherUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjectAssignments_ExamScheduleId",
                table: "TeacherSubjectAssignments",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjectAssignments_SubjectOfferingId",
                table: "TeacherSubjectAssignments",
                column: "SubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjectAssignments_TeacherUserId",
                table: "TeacherSubjectAssignments",
                column: "TeacherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjectAssignments_TenantId",
                table: "TeacherSubjectAssignments",
                column: "TenantId");
        }
    }
}
