using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Perf_AddCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SemesterEnrollments_StudentAdmissionId",
                table: "SemesterEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequestLogs_ExamScheduleId",
                table: "PaymentRequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectResults_ExamRegistrationId",
                table: "ExamSubjectResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectResults_ExamScheduleId",
                table: "ExamSubjectResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamRegistrations_ExamScheduleId",
                table: "ExamRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_ExamRegistrations_SemesterEnrollmentId",
                table: "ExamRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_CollegeAdminSubjectAssignments_SubjectOfferingId",
                table: "CollegeAdminSubjectAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterEnrollments_StudentAdmissionId_EnrollmentStatus",
                table: "SemesterEnrollments",
                columns: new[] { "StudentAdmissionId", "EnrollmentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_SemesterEnrollments_StudentAdmissionId_SemesterInstanceId",
                table: "SemesterEnrollments",
                columns: new[] { "StudentAdmissionId", "SemesterInstanceId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestLogs_ExamScheduleId_StudentRegistrationId_PaymentRequestLogStatus",
                table: "PaymentRequestLogs",
                columns: new[] { "ExamScheduleId", "StudentRegistrationId", "PaymentRequestLogStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectResults_ExamRegistrationId_ExamScheduleId_IsActive",
                table: "ExamSubjectResults",
                columns: new[] { "ExamRegistrationId", "ExamScheduleId", "IsActive" },
                filter: "[ExamScheduleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectResults_ExamScheduleId_SubjectOfferingId_IsActive",
                table: "ExamSubjectResults",
                columns: new[] { "ExamScheduleId", "SubjectOfferingId", "IsActive" },
                filter: "[ExamScheduleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_ExamScheduleId_CollegeId_IsActive_Status",
                table: "ExamRegistrations",
                columns: new[] { "ExamScheduleId", "CollegeId", "IsActive", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_ExamScheduleId_ProgramsId_IsActive",
                table: "ExamRegistrations",
                columns: new[] { "ExamScheduleId", "ProgramsId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_SemesterEnrollmentId",
                table: "ExamRegistrations",
                column: "SemesterEnrollmentId",
                filter: "[SemesterEnrollmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeAdminSubjectAssignments_SubjectOfferingId_ExamScheduleId",
                table: "CollegeAdminSubjectAssignments",
                columns: new[] { "SubjectOfferingId", "ExamScheduleId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SemesterEnrollments_StudentAdmissionId_EnrollmentStatus",
                table: "SemesterEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_SemesterEnrollments_StudentAdmissionId_SemesterInstanceId",
                table: "SemesterEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequestLogs_ExamScheduleId_StudentRegistrationId_PaymentRequestLogStatus",
                table: "PaymentRequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectResults_ExamRegistrationId_ExamScheduleId_IsActive",
                table: "ExamSubjectResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectResults_ExamScheduleId_SubjectOfferingId_IsActive",
                table: "ExamSubjectResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamRegistrations_ExamScheduleId_CollegeId_IsActive_Status",
                table: "ExamRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_ExamRegistrations_ExamScheduleId_ProgramsId_IsActive",
                table: "ExamRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_ExamRegistrations_SemesterEnrollmentId",
                table: "ExamRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_CollegeAdminSubjectAssignments_SubjectOfferingId_ExamScheduleId",
                table: "CollegeAdminSubjectAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterEnrollments_StudentAdmissionId",
                table: "SemesterEnrollments",
                column: "StudentAdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestLogs_ExamScheduleId",
                table: "PaymentRequestLogs",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectResults_ExamRegistrationId",
                table: "ExamSubjectResults",
                column: "ExamRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectResults_ExamScheduleId",
                table: "ExamSubjectResults",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_ExamScheduleId",
                table: "ExamRegistrations",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_SemesterEnrollmentId",
                table: "ExamRegistrations",
                column: "SemesterEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeAdminSubjectAssignments_SubjectOfferingId",
                table: "CollegeAdminSubjectAssignments",
                column: "SubjectOfferingId");
        }
    }
}
