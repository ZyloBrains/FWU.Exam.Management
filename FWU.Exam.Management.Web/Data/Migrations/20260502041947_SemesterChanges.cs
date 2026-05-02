using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fwu_examination_management_system.Migrations
{
    /// <inheritdoc />
    public partial class SemesterChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationVouchers_ExamSchedules_ExamScheduleId",
                table: "ApplicationVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationVouchers_StudentRegistrations_StudentRegistrationId",
                table: "ApplicationVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVouchers_AcademicYears_AcademicYearId",
                table: "BankVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVouchers_Banks_BankId",
                table: "BankVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVouchers_BillTitles_BillTitleId",
                table: "BankVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVouchers_Colleges_CollegeId",
                table: "BankVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVouchers_ExamScheduleParents_ExamScheduleParentId",
                table: "BankVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVouchers_UserAttachments_BankVoucherUserAttachmentId",
                table: "BankVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_BillTitles_ExamSchedules_ExamScheduleId",
                table: "BillTitles");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamFormFeeRates_CollegeTypes_CollegeTypeId",
                table: "ExamFormFeeRates");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRegistrations_ApplicationVouchers_ApplicationVoucherId",
                table: "ExamRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRollNumberSetups_ExamScheduleParents_ExamScheduleParentId",
                table: "ExamRollNumberSetups");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleBatches_Batches_BatchId",
                table: "ExamScheduleBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleBatches_ExamSchedules_ExamScheduleId",
                table: "ExamScheduleBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleBatches_ExamTypes_ExamTypeId",
                table: "ExamScheduleBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleDetails_ExamSchedules_ExamScheduleId",
                table: "ExamScheduleDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleDetails_ExamTypes_ExamTypeId",
                table: "ExamScheduleDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleDetails_SubjectDetails_SubjectDetailId",
                table: "ExamScheduleDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_ExamScheduleParents_ExamScheduleParentId",
                table: "ExamSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_YearParts_YearPartId",
                table: "ExamSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_ExamSchedules_ExamScheduleId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLogs_ExamSchedules_ExamScheduleId",
                table: "PaymentRequestLogs");

            migrationBuilder.DropTable(
                name: "ActiveExamSchedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamScheduleParents",
                table: "ExamScheduleParents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamScheduleDetails",
                table: "ExamScheduleDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamScheduleBatches",
                table: "ExamScheduleBatches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BillTitles",
                table: "BillTitles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankVouchers",
                table: "BankVouchers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationVouchers",
                table: "ApplicationVouchers");

            migrationBuilder.DropColumn(
                name: "EndDateAd",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "PartialBatchIds",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "ProgramIds",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "RegularBatchIds",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "StartDateAd",
                table: "ExamSchedules");

            migrationBuilder.RenameTable(
                name: "ExamScheduleParents",
                newName: "ExamScheduleParent");

            migrationBuilder.RenameTable(
                name: "ExamScheduleDetails",
                newName: "ExamScheduleDetail");

            migrationBuilder.RenameTable(
                name: "ExamScheduleBatches",
                newName: "ExamScheduleBatch");

            migrationBuilder.RenameTable(
                name: "BillTitles",
                newName: "BillTitle");

            migrationBuilder.RenameTable(
                name: "BankVouchers",
                newName: "BankVoucher");

            migrationBuilder.RenameTable(
                name: "ApplicationVouchers",
                newName: "ApplicationVoucher");

            migrationBuilder.RenameColumn(
                name: "NegativeMarks",
                table: "ExamSchedules",
                newName: "SemesterId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleDetails_SubjectDetailId",
                table: "ExamScheduleDetail",
                newName: "IX_ExamScheduleDetail_SubjectDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleDetails_ExamTypeId",
                table: "ExamScheduleDetail",
                newName: "IX_ExamScheduleDetail_ExamTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleDetails_ExamScheduleId",
                table: "ExamScheduleDetail",
                newName: "IX_ExamScheduleDetail_ExamScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleBatches_ExamTypeId",
                table: "ExamScheduleBatch",
                newName: "IX_ExamScheduleBatch_ExamTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleBatches_ExamScheduleId",
                table: "ExamScheduleBatch",
                newName: "IX_ExamScheduleBatch_ExamScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleBatches_BatchId",
                table: "ExamScheduleBatch",
                newName: "IX_ExamScheduleBatch_BatchId");

            migrationBuilder.RenameIndex(
                name: "IX_BillTitles_ExamScheduleId",
                table: "BillTitle",
                newName: "IX_BillTitle_ExamScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVouchers_ExamScheduleParentId",
                table: "BankVoucher",
                newName: "IX_BankVoucher_ExamScheduleParentId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVouchers_CollegeId",
                table: "BankVoucher",
                newName: "IX_BankVoucher_CollegeId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVouchers_BillTitleId",
                table: "BankVoucher",
                newName: "IX_BankVoucher_BillTitleId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVouchers_BankVoucherUserAttachmentId",
                table: "BankVoucher",
                newName: "IX_BankVoucher_BankVoucherUserAttachmentId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVouchers_BankId",
                table: "BankVoucher",
                newName: "IX_BankVoucher_BankId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVouchers_AcademicYearId",
                table: "BankVoucher",
                newName: "IX_BankVoucher_AcademicYearId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationVouchers_StudentRegistrationId",
                table: "ApplicationVoucher",
                newName: "IX_ApplicationVoucher_StudentRegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationVouchers_ExamScheduleId",
                table: "ApplicationVoucher",
                newName: "IX_ApplicationVoucher_ExamScheduleId");

            migrationBuilder.AddColumn<int>(
                name: "SemesterEnrollmentId",
                table: "ExamSubjectRegistrationInternals",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "YearPartId",
                table: "ExamSchedules",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SemesterEnrollmentId",
                table: "ExamRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamScheduleParent",
                table: "ExamScheduleParent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamScheduleDetail",
                table: "ExamScheduleDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamScheduleBatch",
                table: "ExamScheduleBatch",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BillTitle",
                table: "BillTitle",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankVoucher",
                table: "BankVoucher",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationVoucher",
                table: "ApplicationVoucher",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcademicYearId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Semesters_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SemesterEnrollment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentAdmissionId = table.Column<int>(type: "int", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentStatus = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemesterEnrollment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SemesterEnrollment_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SemesterEnrollment_StudentAdmissions_StudentAdmissionId",
                        column: x => x.StudentAdmissionId,
                        principalTable: "StudentAdmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationInternals_SemesterEnrollmentId",
                table: "ExamSubjectRegistrationInternals",
                column: "SemesterEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_SemesterId",
                table: "ExamSchedules",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_SemesterEnrollmentId",
                table: "ExamRegistrations",
                column: "SemesterEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterEnrollment_SemesterId",
                table: "SemesterEnrollment",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterEnrollment_StudentAdmissionId",
                table: "SemesterEnrollment",
                column: "StudentAdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_AcademicYearId",
                table: "Semesters",
                column: "AcademicYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationVoucher_ExamSchedules_ExamScheduleId",
                table: "ApplicationVoucher",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationVoucher_StudentRegistrations_StudentRegistrationId",
                table: "ApplicationVoucher",
                column: "StudentRegistrationId",
                principalTable: "StudentRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVoucher_AcademicYears_AcademicYearId",
                table: "BankVoucher",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVoucher_Banks_BankId",
                table: "BankVoucher",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVoucher_BillTitle_BillTitleId",
                table: "BankVoucher",
                column: "BillTitleId",
                principalTable: "BillTitle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVoucher_Colleges_CollegeId",
                table: "BankVoucher",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVoucher_ExamScheduleParent_ExamScheduleParentId",
                table: "BankVoucher",
                column: "ExamScheduleParentId",
                principalTable: "ExamScheduleParent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVoucher_UserAttachments_BankVoucherUserAttachmentId",
                table: "BankVoucher",
                column: "BankVoucherUserAttachmentId",
                principalTable: "UserAttachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillTitle_ExamSchedules_ExamScheduleId",
                table: "BillTitle",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamFormFeeRates_CollegeTypes_CollegeTypeId",
                table: "ExamFormFeeRates",
                column: "CollegeTypeId",
                principalTable: "CollegeTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRegistrations_ApplicationVoucher_ApplicationVoucherId",
                table: "ExamRegistrations",
                column: "ApplicationVoucherId",
                principalTable: "ApplicationVoucher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRegistrations_SemesterEnrollment_SemesterEnrollmentId",
                table: "ExamRegistrations",
                column: "SemesterEnrollmentId",
                principalTable: "SemesterEnrollment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRollNumberSetups_ExamScheduleParent_ExamScheduleParentId",
                table: "ExamRollNumberSetups",
                column: "ExamScheduleParentId",
                principalTable: "ExamScheduleParent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleBatch_Batches_BatchId",
                table: "ExamScheduleBatch",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleBatch_ExamSchedules_ExamScheduleId",
                table: "ExamScheduleBatch",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleBatch_ExamTypes_ExamTypeId",
                table: "ExamScheduleBatch",
                column: "ExamTypeId",
                principalTable: "ExamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleDetail_ExamSchedules_ExamScheduleId",
                table: "ExamScheduleDetail",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleDetail_ExamTypes_ExamTypeId",
                table: "ExamScheduleDetail",
                column: "ExamTypeId",
                principalTable: "ExamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleDetail_SubjectDetails_SubjectDetailId",
                table: "ExamScheduleDetail",
                column: "SubjectDetailId",
                principalTable: "SubjectDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_ExamScheduleParent_ExamScheduleParentId",
                table: "ExamSchedules",
                column: "ExamScheduleParentId",
                principalTable: "ExamScheduleParent",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_Semesters_SemesterId",
                table: "ExamSchedules",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_YearParts_YearPartId",
                table: "ExamSchedules",
                column: "YearPartId",
                principalTable: "YearParts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_ExamSchedules_ExamScheduleId",
                table: "ExamSubjectRegistrationInternals",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_SemesterEnrollment_SemesterEnrollmentId",
                table: "ExamSubjectRegistrationInternals",
                column: "SemesterEnrollmentId",
                principalTable: "SemesterEnrollment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLogs_ExamSchedules_ExamScheduleId",
                table: "PaymentRequestLogs",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationVoucher_ExamSchedules_ExamScheduleId",
                table: "ApplicationVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationVoucher_StudentRegistrations_StudentRegistrationId",
                table: "ApplicationVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVoucher_AcademicYears_AcademicYearId",
                table: "BankVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVoucher_Banks_BankId",
                table: "BankVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVoucher_BillTitle_BillTitleId",
                table: "BankVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVoucher_Colleges_CollegeId",
                table: "BankVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVoucher_ExamScheduleParent_ExamScheduleParentId",
                table: "BankVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVoucher_UserAttachments_BankVoucherUserAttachmentId",
                table: "BankVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_BillTitle_ExamSchedules_ExamScheduleId",
                table: "BillTitle");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamFormFeeRates_CollegeTypes_CollegeTypeId",
                table: "ExamFormFeeRates");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRegistrations_ApplicationVoucher_ApplicationVoucherId",
                table: "ExamRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRegistrations_SemesterEnrollment_SemesterEnrollmentId",
                table: "ExamRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRollNumberSetups_ExamScheduleParent_ExamScheduleParentId",
                table: "ExamRollNumberSetups");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleBatch_Batches_BatchId",
                table: "ExamScheduleBatch");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleBatch_ExamSchedules_ExamScheduleId",
                table: "ExamScheduleBatch");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleBatch_ExamTypes_ExamTypeId",
                table: "ExamScheduleBatch");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleDetail_ExamSchedules_ExamScheduleId",
                table: "ExamScheduleDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleDetail_ExamTypes_ExamTypeId",
                table: "ExamScheduleDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamScheduleDetail_SubjectDetails_SubjectDetailId",
                table: "ExamScheduleDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_ExamScheduleParent_ExamScheduleParentId",
                table: "ExamSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_Semesters_SemesterId",
                table: "ExamSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_YearParts_YearPartId",
                table: "ExamSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_ExamSchedules_ExamScheduleId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_SemesterEnrollment_SemesterEnrollmentId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLogs_ExamSchedules_ExamScheduleId",
                table: "PaymentRequestLogs");

            migrationBuilder.DropTable(
                name: "SemesterEnrollment");

            migrationBuilder.DropTable(
                name: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectRegistrationInternals_SemesterEnrollmentId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropIndex(
                name: "IX_ExamSchedules_SemesterId",
                table: "ExamSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ExamRegistrations_SemesterEnrollmentId",
                table: "ExamRegistrations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamScheduleParent",
                table: "ExamScheduleParent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamScheduleDetail",
                table: "ExamScheduleDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamScheduleBatch",
                table: "ExamScheduleBatch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BillTitle",
                table: "BillTitle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankVoucher",
                table: "BankVoucher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationVoucher",
                table: "ApplicationVoucher");

            migrationBuilder.DropColumn(
                name: "SemesterEnrollmentId",
                table: "ExamSubjectRegistrationInternals");

            migrationBuilder.DropColumn(
                name: "SemesterEnrollmentId",
                table: "ExamRegistrations");

            migrationBuilder.RenameTable(
                name: "ExamScheduleParent",
                newName: "ExamScheduleParents");

            migrationBuilder.RenameTable(
                name: "ExamScheduleDetail",
                newName: "ExamScheduleDetails");

            migrationBuilder.RenameTable(
                name: "ExamScheduleBatch",
                newName: "ExamScheduleBatches");

            migrationBuilder.RenameTable(
                name: "BillTitle",
                newName: "BillTitles");

            migrationBuilder.RenameTable(
                name: "BankVoucher",
                newName: "BankVouchers");

            migrationBuilder.RenameTable(
                name: "ApplicationVoucher",
                newName: "ApplicationVouchers");

            migrationBuilder.RenameColumn(
                name: "SemesterId",
                table: "ExamSchedules",
                newName: "NegativeMarks");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleDetail_SubjectDetailId",
                table: "ExamScheduleDetails",
                newName: "IX_ExamScheduleDetails_SubjectDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleDetail_ExamTypeId",
                table: "ExamScheduleDetails",
                newName: "IX_ExamScheduleDetails_ExamTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleDetail_ExamScheduleId",
                table: "ExamScheduleDetails",
                newName: "IX_ExamScheduleDetails_ExamScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleBatch_ExamTypeId",
                table: "ExamScheduleBatches",
                newName: "IX_ExamScheduleBatches_ExamTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleBatch_ExamScheduleId",
                table: "ExamScheduleBatches",
                newName: "IX_ExamScheduleBatches_ExamScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamScheduleBatch_BatchId",
                table: "ExamScheduleBatches",
                newName: "IX_ExamScheduleBatches_BatchId");

            migrationBuilder.RenameIndex(
                name: "IX_BillTitle_ExamScheduleId",
                table: "BillTitles",
                newName: "IX_BillTitles_ExamScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVoucher_ExamScheduleParentId",
                table: "BankVouchers",
                newName: "IX_BankVouchers_ExamScheduleParentId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVoucher_CollegeId",
                table: "BankVouchers",
                newName: "IX_BankVouchers_CollegeId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVoucher_BillTitleId",
                table: "BankVouchers",
                newName: "IX_BankVouchers_BillTitleId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVoucher_BankVoucherUserAttachmentId",
                table: "BankVouchers",
                newName: "IX_BankVouchers_BankVoucherUserAttachmentId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVoucher_BankId",
                table: "BankVouchers",
                newName: "IX_BankVouchers_BankId");

            migrationBuilder.RenameIndex(
                name: "IX_BankVoucher_AcademicYearId",
                table: "BankVouchers",
                newName: "IX_BankVouchers_AcademicYearId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationVoucher_StudentRegistrationId",
                table: "ApplicationVouchers",
                newName: "IX_ApplicationVouchers_StudentRegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationVoucher_ExamScheduleId",
                table: "ApplicationVouchers",
                newName: "IX_ApplicationVouchers_ExamScheduleId");

            migrationBuilder.AlterColumn<int>(
                name: "YearPartId",
                table: "ExamSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateAd",
                table: "ExamSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartialBatchIds",
                table: "ExamSchedules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgramIds",
                table: "ExamSchedules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegularBatchIds",
                table: "ExamSchedules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateAd",
                table: "ExamSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamScheduleParents",
                table: "ExamScheduleParents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamScheduleDetails",
                table: "ExamScheduleDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamScheduleBatches",
                table: "ExamScheduleBatches",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BillTitles",
                table: "BillTitles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankVouchers",
                table: "BankVouchers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationVouchers",
                table: "ApplicationVouchers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ActiveExamSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveExamSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveExamSchedules_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveExamSchedules_ExamScheduleId",
                table: "ActiveExamSchedules",
                column: "ExamScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationVouchers_ExamSchedules_ExamScheduleId",
                table: "ApplicationVouchers",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationVouchers_StudentRegistrations_StudentRegistrationId",
                table: "ApplicationVouchers",
                column: "StudentRegistrationId",
                principalTable: "StudentRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVouchers_AcademicYears_AcademicYearId",
                table: "BankVouchers",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVouchers_Banks_BankId",
                table: "BankVouchers",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVouchers_BillTitles_BillTitleId",
                table: "BankVouchers",
                column: "BillTitleId",
                principalTable: "BillTitles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVouchers_Colleges_CollegeId",
                table: "BankVouchers",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVouchers_ExamScheduleParents_ExamScheduleParentId",
                table: "BankVouchers",
                column: "ExamScheduleParentId",
                principalTable: "ExamScheduleParents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankVouchers_UserAttachments_BankVoucherUserAttachmentId",
                table: "BankVouchers",
                column: "BankVoucherUserAttachmentId",
                principalTable: "UserAttachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillTitles_ExamSchedules_ExamScheduleId",
                table: "BillTitles",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamFormFeeRates_CollegeTypes_CollegeTypeId",
                table: "ExamFormFeeRates",
                column: "CollegeTypeId",
                principalTable: "CollegeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRegistrations_ApplicationVouchers_ApplicationVoucherId",
                table: "ExamRegistrations",
                column: "ApplicationVoucherId",
                principalTable: "ApplicationVouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRollNumberSetups_ExamScheduleParents_ExamScheduleParentId",
                table: "ExamRollNumberSetups",
                column: "ExamScheduleParentId",
                principalTable: "ExamScheduleParents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleBatches_Batches_BatchId",
                table: "ExamScheduleBatches",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleBatches_ExamSchedules_ExamScheduleId",
                table: "ExamScheduleBatches",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleBatches_ExamTypes_ExamTypeId",
                table: "ExamScheduleBatches",
                column: "ExamTypeId",
                principalTable: "ExamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleDetails_ExamSchedules_ExamScheduleId",
                table: "ExamScheduleDetails",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleDetails_ExamTypes_ExamTypeId",
                table: "ExamScheduleDetails",
                column: "ExamTypeId",
                principalTable: "ExamTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamScheduleDetails_SubjectDetails_SubjectDetailId",
                table: "ExamScheduleDetails",
                column: "SubjectDetailId",
                principalTable: "SubjectDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_ExamScheduleParents_ExamScheduleParentId",
                table: "ExamSchedules",
                column: "ExamScheduleParentId",
                principalTable: "ExamScheduleParents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_YearParts_YearPartId",
                table: "ExamSchedules",
                column: "YearPartId",
                principalTable: "YearParts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSubjectRegistrationInternals_ExamSchedules_ExamScheduleId",
                table: "ExamSubjectRegistrationInternals",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLogs_ExamSchedules_ExamScheduleId",
                table: "PaymentRequestLogs",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
