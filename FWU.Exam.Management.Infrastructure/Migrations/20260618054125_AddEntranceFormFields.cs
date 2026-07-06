using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntranceFormFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationVoucher_ExamSchedules_ExamScheduleId",
                table: "ApplicationVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationVoucher_StudentRegistrations_StudentRegistrationId",
                table: "ApplicationVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationVoucher_Tenants_TenantId",
                table: "ApplicationVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRegistrations_ApplicationVoucher_ApplicationVoucherId",
                table: "ExamRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentPracticalSubjects_PaymentRequestLog_PaymentRequestLogId",
                table: "PaymentPracticalSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLog_Colleges_CollegeId",
                table: "PaymentRequestLog");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLog_ExamSchedules_ExamScheduleId",
                table: "PaymentRequestLog");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLog_PaymentType_PaymentTypeId",
                table: "PaymentRequestLog");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLog_StudentRegistrations_StudentRegistrationId",
                table: "PaymentRequestLog");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLog_Tenants_TenantId",
                table: "PaymentRequestLog");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentResponseLog_PaymentRequestLog_PaymentRequestLogId",
                table: "PaymentResponseLog");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentResponseLog_Tenants_TenantId",
                table: "PaymentResponseLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentResponseLog",
                table: "PaymentResponseLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentRequestLog",
                table: "PaymentRequestLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationVoucher",
                table: "ApplicationVoucher");

            migrationBuilder.RenameTable(
                name: "PaymentResponseLog",
                newName: "PaymentResponseLogs");

            migrationBuilder.RenameTable(
                name: "PaymentRequestLog",
                newName: "PaymentRequestLogs");

            migrationBuilder.RenameTable(
                name: "ApplicationVoucher",
                newName: "ApplicationVouchers");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentResponseLog_TenantId",
                table: "PaymentResponseLogs",
                newName: "IX_PaymentResponseLogs_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentResponseLog_PaymentRequestLogId",
                table: "PaymentResponseLogs",
                newName: "IX_PaymentResponseLogs_PaymentRequestLogId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestLog_TenantId",
                table: "PaymentRequestLogs",
                newName: "IX_PaymentRequestLogs_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestLog_StudentRegistrationId",
                table: "PaymentRequestLogs",
                newName: "IX_PaymentRequestLogs_StudentRegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestLog_PaymentTypeId",
                table: "PaymentRequestLogs",
                newName: "IX_PaymentRequestLogs_PaymentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestLog_ExamScheduleId",
                table: "PaymentRequestLogs",
                newName: "IX_PaymentRequestLogs_ExamScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestLog_CollegeId",
                table: "PaymentRequestLogs",
                newName: "IX_PaymentRequestLogs_CollegeId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationVoucher_TenantId",
                table: "ApplicationVouchers",
                newName: "IX_ApplicationVouchers_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationVoucher_StudentRegistrationId",
                table: "ApplicationVouchers",
                newName: "IX_ApplicationVouchers_StudentRegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationVoucher_ExamScheduleId",
                table: "ApplicationVouchers",
                newName: "IX_ApplicationVouchers_ExamScheduleId");

            migrationBuilder.AddColumn<int>(
                name: "ApplicationVoucherId",
                table: "EntranceExamApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BirthPlace",
                table: "EntranceExamApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodGroup",
                table: "EntranceExamApplications",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CitizenshipDistrictId",
                table: "EntranceExamApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenshipIssueDateAd",
                table: "EntranceExamApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenshipIssueDateBs",
                table: "EntranceExamApplications",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenshipNo",
                table: "EntranceExamApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "EntranceExamApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentsPath",
                table: "EntranceExamApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherProfession",
                table: "EntranceExamApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuardianEmail",
                table: "EntranceExamApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherProfession",
                table: "EntranceExamApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentVerified",
                table: "EntranceExamApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "EntranceExamApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "EntranceExamApplications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousBoard2",
                table: "EntranceExamApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousBoard3",
                table: "EntranceExamApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousDivision",
                table: "EntranceExamApplications",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousDivision2",
                table: "EntranceExamApplications",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousDivision3",
                table: "EntranceExamApplications",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousGPA2",
                table: "EntranceExamApplications",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousGPA3",
                table: "EntranceExamApplications",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousLevel2Id",
                table: "EntranceExamApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousLevel3Id",
                table: "EntranceExamApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousPassedYear2",
                table: "EntranceExamApplications",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousPassedYear3",
                table: "EntranceExamApplications",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousSchoolCollege2",
                table: "EntranceExamApplications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousSchoolCollege3",
                table: "EntranceExamApplications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousSymbolNumber2",
                table: "EntranceExamApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousSymbolNumber3",
                table: "EntranceExamApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherPath",
                table: "EntranceExamApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentResponseLogs",
                table: "PaymentResponseLogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentRequestLogs",
                table: "PaymentRequestLogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationVouchers",
                table: "ApplicationVouchers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EntranceExamApplications_ApplicationVoucherId",
                table: "EntranceExamApplications",
                column: "ApplicationVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_EntranceExamApplications_CitizenshipDistrictId",
                table: "EntranceExamApplications",
                column: "CitizenshipDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_EntranceExamApplications_PreviousLevel2Id",
                table: "EntranceExamApplications",
                column: "PreviousLevel2Id");

            migrationBuilder.CreateIndex(
                name: "IX_EntranceExamApplications_PreviousLevel3Id",
                table: "EntranceExamApplications",
                column: "PreviousLevel3Id");

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
                name: "FK_ApplicationVouchers_Tenants_TenantId",
                table: "ApplicationVouchers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntranceExamApplications_ApplicationVouchers_ApplicationVoucherId",
                table: "EntranceExamApplications",
                column: "ApplicationVoucherId",
                principalTable: "ApplicationVouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntranceExamApplications_Districts_CitizenshipDistrictId",
                table: "EntranceExamApplications",
                column: "CitizenshipDistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntranceExamApplications_PreviousLevels_PreviousLevel2Id",
                table: "EntranceExamApplications",
                column: "PreviousLevel2Id",
                principalTable: "PreviousLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntranceExamApplications_PreviousLevels_PreviousLevel3Id",
                table: "EntranceExamApplications",
                column: "PreviousLevel3Id",
                principalTable: "PreviousLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRegistrations_ApplicationVouchers_ApplicationVoucherId",
                table: "ExamRegistrations",
                column: "ApplicationVoucherId",
                principalTable: "ApplicationVouchers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentPracticalSubjects_PaymentRequestLogs_PaymentRequestLogId",
                table: "PaymentPracticalSubjects",
                column: "PaymentRequestLogId",
                principalTable: "PaymentRequestLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLogs_Colleges_CollegeId",
                table: "PaymentRequestLogs",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLogs_ExamSchedules_ExamScheduleId",
                table: "PaymentRequestLogs",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLogs_PaymentType_PaymentTypeId",
                table: "PaymentRequestLogs",
                column: "PaymentTypeId",
                principalTable: "PaymentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLogs_StudentRegistrations_StudentRegistrationId",
                table: "PaymentRequestLogs",
                column: "StudentRegistrationId",
                principalTable: "StudentRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLogs_Tenants_TenantId",
                table: "PaymentRequestLogs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentResponseLogs_PaymentRequestLogs_PaymentRequestLogId",
                table: "PaymentResponseLogs",
                column: "PaymentRequestLogId",
                principalTable: "PaymentRequestLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentResponseLogs_Tenants_TenantId",
                table: "PaymentResponseLogs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationVouchers_ExamSchedules_ExamScheduleId",
                table: "ApplicationVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationVouchers_StudentRegistrations_StudentRegistrationId",
                table: "ApplicationVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationVouchers_Tenants_TenantId",
                table: "ApplicationVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_EntranceExamApplications_ApplicationVouchers_ApplicationVoucherId",
                table: "EntranceExamApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_EntranceExamApplications_Districts_CitizenshipDistrictId",
                table: "EntranceExamApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_EntranceExamApplications_PreviousLevels_PreviousLevel2Id",
                table: "EntranceExamApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_EntranceExamApplications_PreviousLevels_PreviousLevel3Id",
                table: "EntranceExamApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRegistrations_ApplicationVouchers_ApplicationVoucherId",
                table: "ExamRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentPracticalSubjects_PaymentRequestLogs_PaymentRequestLogId",
                table: "PaymentPracticalSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLogs_Colleges_CollegeId",
                table: "PaymentRequestLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLogs_ExamSchedules_ExamScheduleId",
                table: "PaymentRequestLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLogs_PaymentType_PaymentTypeId",
                table: "PaymentRequestLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLogs_StudentRegistrations_StudentRegistrationId",
                table: "PaymentRequestLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLogs_Tenants_TenantId",
                table: "PaymentRequestLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentResponseLogs_PaymentRequestLogs_PaymentRequestLogId",
                table: "PaymentResponseLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentResponseLogs_Tenants_TenantId",
                table: "PaymentResponseLogs");

            migrationBuilder.DropIndex(
                name: "IX_EntranceExamApplications_ApplicationVoucherId",
                table: "EntranceExamApplications");

            migrationBuilder.DropIndex(
                name: "IX_EntranceExamApplications_CitizenshipDistrictId",
                table: "EntranceExamApplications");

            migrationBuilder.DropIndex(
                name: "IX_EntranceExamApplications_PreviousLevel2Id",
                table: "EntranceExamApplications");

            migrationBuilder.DropIndex(
                name: "IX_EntranceExamApplications_PreviousLevel3Id",
                table: "EntranceExamApplications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentResponseLogs",
                table: "PaymentResponseLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentRequestLogs",
                table: "PaymentRequestLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationVouchers",
                table: "ApplicationVouchers");

            migrationBuilder.DropColumn(
                name: "ApplicationVoucherId",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "BirthPlace",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "BloodGroup",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "CitizenshipDistrictId",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "CitizenshipIssueDateAd",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "CitizenshipIssueDateBs",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "CitizenshipNo",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "DocumentsPath",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "FatherProfession",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "GuardianEmail",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "MotherProfession",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PaymentVerified",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousBoard2",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousBoard3",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousDivision",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousDivision2",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousDivision3",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousGPA2",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousGPA3",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousLevel2Id",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousLevel3Id",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousPassedYear2",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousPassedYear3",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousSchoolCollege2",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousSchoolCollege3",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousSymbolNumber2",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "PreviousSymbolNumber3",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "VoucherPath",
                table: "EntranceExamApplications");

            migrationBuilder.RenameTable(
                name: "PaymentResponseLogs",
                newName: "PaymentResponseLog");

            migrationBuilder.RenameTable(
                name: "PaymentRequestLogs",
                newName: "PaymentRequestLog");

            migrationBuilder.RenameTable(
                name: "ApplicationVouchers",
                newName: "ApplicationVoucher");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentResponseLogs_TenantId",
                table: "PaymentResponseLog",
                newName: "IX_PaymentResponseLog_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentResponseLogs_PaymentRequestLogId",
                table: "PaymentResponseLog",
                newName: "IX_PaymentResponseLog_PaymentRequestLogId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestLogs_TenantId",
                table: "PaymentRequestLog",
                newName: "IX_PaymentRequestLog_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestLogs_StudentRegistrationId",
                table: "PaymentRequestLog",
                newName: "IX_PaymentRequestLog_StudentRegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestLogs_PaymentTypeId",
                table: "PaymentRequestLog",
                newName: "IX_PaymentRequestLog_PaymentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestLogs_ExamScheduleId",
                table: "PaymentRequestLog",
                newName: "IX_PaymentRequestLog_ExamScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRequestLogs_CollegeId",
                table: "PaymentRequestLog",
                newName: "IX_PaymentRequestLog_CollegeId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationVouchers_TenantId",
                table: "ApplicationVoucher",
                newName: "IX_ApplicationVoucher_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationVouchers_StudentRegistrationId",
                table: "ApplicationVoucher",
                newName: "IX_ApplicationVoucher_StudentRegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationVouchers_ExamScheduleId",
                table: "ApplicationVoucher",
                newName: "IX_ApplicationVoucher_ExamScheduleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentResponseLog",
                table: "PaymentResponseLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentRequestLog",
                table: "PaymentRequestLog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationVoucher",
                table: "ApplicationVoucher",
                column: "Id");

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
                name: "FK_ApplicationVoucher_Tenants_TenantId",
                table: "ApplicationVoucher",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRegistrations_ApplicationVoucher_ApplicationVoucherId",
                table: "ExamRegistrations",
                column: "ApplicationVoucherId",
                principalTable: "ApplicationVoucher",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentPracticalSubjects_PaymentRequestLog_PaymentRequestLogId",
                table: "PaymentPracticalSubjects",
                column: "PaymentRequestLogId",
                principalTable: "PaymentRequestLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLog_Colleges_CollegeId",
                table: "PaymentRequestLog",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLog_ExamSchedules_ExamScheduleId",
                table: "PaymentRequestLog",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLog_PaymentType_PaymentTypeId",
                table: "PaymentRequestLog",
                column: "PaymentTypeId",
                principalTable: "PaymentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLog_StudentRegistrations_StudentRegistrationId",
                table: "PaymentRequestLog",
                column: "StudentRegistrationId",
                principalTable: "StudentRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestLog_Tenants_TenantId",
                table: "PaymentRequestLog",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentResponseLog_PaymentRequestLog_PaymentRequestLogId",
                table: "PaymentResponseLog",
                column: "PaymentRequestLogId",
                principalTable: "PaymentRequestLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentResponseLog_Tenants_TenantId",
                table: "PaymentResponseLog",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
