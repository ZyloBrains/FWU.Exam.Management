using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_Roles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_Users_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "UserRoles");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "UserRoles",
                newName: "IX_UserRoles_RoleId");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "SubjectOfferings",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "SubjectCatalogs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "StudentRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "StudentQualifications",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "StudentGuardians",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "StudentCategories",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "StudentAdmissions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Semesters",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "SemesterEnrollments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "QuestionSets",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ProgramSubjectPracticalCharge",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PaymentResponseLog",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PaymentRequestLog",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PaymentPracticalSubjects",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Notices",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "GradingSchemes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "GradeDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "FiscalYears",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExamSubjectResults",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExamSlots",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExamSchedules",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExamRollNumberSetup",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExamRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExamFees",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ExamCenters",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "EntranceExamApplications",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CurriculumVersions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Colleges",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CollegePrograms",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "CollegeProfiles",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "BillTitle",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Batches",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "BankVoucher",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ApplicationVoucher",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AcademicYears",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Name", "OfficeCode", "ContactNumber", "Address", "Email", "TenantType", "IsActive" },
                values: new object[] { 1, "Default Tenant", "DEFAULT", "000-0000000", "N/A", "default@tenant.com", 1, true });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectOfferings_TenantId",
                table: "SubjectOfferings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCatalogs_TenantId",
                table: "SubjectCatalogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_TenantId",
                table: "StudentRegistrations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentQualifications_TenantId",
                table: "StudentQualifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGuardians_TenantId",
                table: "StudentGuardians",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCategories_TenantId",
                table: "StudentCategories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAdmissions_TenantId",
                table: "StudentAdmissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_TenantId",
                table: "Semesters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterEnrollments_TenantId",
                table: "SemesterEnrollments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSets_TenantId",
                table: "QuestionSets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSubjectPracticalCharge_TenantId",
                table: "ProgramSubjectPracticalCharge",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentResponseLog_TenantId",
                table: "PaymentResponseLog",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestLog_TenantId",
                table: "PaymentRequestLog",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPracticalSubjects_TenantId",
                table: "PaymentPracticalSubjects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Notices_TenantId",
                table: "Notices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingSchemes_TenantId",
                table: "GradingSchemes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeDefinitions_TenantId",
                table: "GradeDefinitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYears_TenantId",
                table: "FiscalYears",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectResults_TenantId",
                table: "ExamSubjectResults",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSlots_TenantId",
                table: "ExamSlots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_TenantId",
                table: "ExamSchedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetup_TenantId",
                table: "ExamRollNumberSetup",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrations_TenantId",
                table: "ExamRegistrations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFees_TenantId",
                table: "ExamFees",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenters_TenantId",
                table: "ExamCenters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EntranceExamApplications_TenantId",
                table: "EntranceExamApplications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumVersions_TenantId",
                table: "CurriculumVersions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_TenantId",
                table: "Colleges",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegePrograms_TenantId",
                table: "CollegePrograms",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeProfiles_TenantId",
                table: "CollegeProfiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BillTitle_TenantId",
                table: "BillTitle",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_TenantId",
                table: "Batches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BankVoucher_TenantId",
                table: "BankVoucher",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVoucher_TenantId",
                table: "ApplicationVoucher",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_TenantId",
                table: "AcademicYears",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Tenants_TenantId",
                table: "AcademicYears",
                column: "TenantId",
                principalTable: "Tenants",
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
                name: "FK_BankVoucher_Tenants_TenantId",
                table: "BankVoucher",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_Tenants_TenantId",
                table: "Batches",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillTitle_Tenants_TenantId",
                table: "BillTitle",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CollegeProfiles_Tenants_TenantId",
                table: "CollegeProfiles",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CollegePrograms_Tenants_TenantId",
                table: "CollegePrograms",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_Tenants_TenantId",
                table: "Colleges",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CurriculumVersions_Tenants_TenantId",
                table: "CurriculumVersions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntranceExamApplications_Tenants_TenantId",
                table: "EntranceExamApplications",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamCenters_Tenants_TenantId",
                table: "ExamCenters",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamFees_Tenants_TenantId",
                table: "ExamFees",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRegistrations_Tenants_TenantId",
                table: "ExamRegistrations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRollNumberSetup_Tenants_TenantId",
                table: "ExamRollNumberSetup",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_Tenants_TenantId",
                table: "ExamSchedules",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSlots_Tenants_TenantId",
                table: "ExamSlots",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSubjectResults_Tenants_TenantId",
                table: "ExamSubjectResults",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FiscalYears_Tenants_TenantId",
                table: "FiscalYears",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GradeDefinitions_Tenants_TenantId",
                table: "GradeDefinitions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GradingSchemes_Tenants_TenantId",
                table: "GradingSchemes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_Tenants_TenantId",
                table: "Notices",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentPracticalSubjects_Tenants_TenantId",
                table: "PaymentPracticalSubjects",
                column: "TenantId",
                principalTable: "Tenants",
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
                name: "FK_PaymentResponseLog_Tenants_TenantId",
                table: "PaymentResponseLog",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramSubjectPracticalCharge_Tenants_TenantId",
                table: "ProgramSubjectPracticalCharge",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionSets_Tenants_TenantId",
                table: "QuestionSets",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SemesterEnrollments_Tenants_TenantId",
                table: "SemesterEnrollments",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_Tenants_TenantId",
                table: "Semesters",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAdmissions_Tenants_TenantId",
                table: "StudentAdmissions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCategories_Tenants_TenantId",
                table: "StudentCategories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGuardians_Tenants_TenantId",
                table: "StudentGuardians",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQualifications_Tenants_TenantId",
                table: "StudentQualifications",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_Tenants_TenantId",
                table: "StudentRegistrations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectCatalogs_Tenants_TenantId",
                table: "SubjectCatalogs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectOfferings_Tenants_TenantId",
                table: "SubjectOfferings",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Tenants_TenantId",
                table: "AcademicYears");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationVoucher_Tenants_TenantId",
                table: "ApplicationVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_BankVoucher_Tenants_TenantId",
                table: "BankVoucher");

            migrationBuilder.DropForeignKey(
                name: "FK_Batches_Tenants_TenantId",
                table: "Batches");

            migrationBuilder.DropForeignKey(
                name: "FK_BillTitle_Tenants_TenantId",
                table: "BillTitle");

            migrationBuilder.DropForeignKey(
                name: "FK_CollegeProfiles_Tenants_TenantId",
                table: "CollegeProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_CollegePrograms_Tenants_TenantId",
                table: "CollegePrograms");

            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_Tenants_TenantId",
                table: "Colleges");

            migrationBuilder.DropForeignKey(
                name: "FK_CurriculumVersions_Tenants_TenantId",
                table: "CurriculumVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_EntranceExamApplications_Tenants_TenantId",
                table: "EntranceExamApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamCenters_Tenants_TenantId",
                table: "ExamCenters");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamFees_Tenants_TenantId",
                table: "ExamFees");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRegistrations_Tenants_TenantId",
                table: "ExamRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamRollNumberSetup_Tenants_TenantId",
                table: "ExamRollNumberSetup");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSchedules_Tenants_TenantId",
                table: "ExamSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSlots_Tenants_TenantId",
                table: "ExamSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSubjectResults_Tenants_TenantId",
                table: "ExamSubjectResults");

            migrationBuilder.DropForeignKey(
                name: "FK_FiscalYears_Tenants_TenantId",
                table: "FiscalYears");

            migrationBuilder.DropForeignKey(
                name: "FK_GradeDefinitions_Tenants_TenantId",
                table: "GradeDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_GradingSchemes_Tenants_TenantId",
                table: "GradingSchemes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notices_Tenants_TenantId",
                table: "Notices");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentPracticalSubjects_Tenants_TenantId",
                table: "PaymentPracticalSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestLog_Tenants_TenantId",
                table: "PaymentRequestLog");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentResponseLog_Tenants_TenantId",
                table: "PaymentResponseLog");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramSubjectPracticalCharge_Tenants_TenantId",
                table: "ProgramSubjectPracticalCharge");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionSets_Tenants_TenantId",
                table: "QuestionSets");

            migrationBuilder.DropForeignKey(
                name: "FK_SemesterEnrollments_Tenants_TenantId",
                table: "SemesterEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_Tenants_TenantId",
                table: "Semesters");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAdmissions_Tenants_TenantId",
                table: "StudentAdmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCategories_Tenants_TenantId",
                table: "StudentCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGuardians_Tenants_TenantId",
                table: "StudentGuardians");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentQualifications_Tenants_TenantId",
                table: "StudentQualifications");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_Tenants_TenantId",
                table: "StudentRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectCatalogs_Tenants_TenantId",
                table: "SubjectCatalogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectOfferings_Tenants_TenantId",
                table: "SubjectOfferings");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_SubjectOfferings_TenantId",
                table: "SubjectOfferings");

            migrationBuilder.DropIndex(
                name: "IX_SubjectCatalogs_TenantId",
                table: "SubjectCatalogs");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_TenantId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_StudentQualifications_TenantId",
                table: "StudentQualifications");

            migrationBuilder.DropIndex(
                name: "IX_StudentGuardians_TenantId",
                table: "StudentGuardians");

            migrationBuilder.DropIndex(
                name: "IX_StudentCategories_TenantId",
                table: "StudentCategories");

            migrationBuilder.DropIndex(
                name: "IX_StudentAdmissions_TenantId",
                table: "StudentAdmissions");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_TenantId",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_SemesterEnrollments_TenantId",
                table: "SemesterEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_QuestionSets_TenantId",
                table: "QuestionSets");

            migrationBuilder.DropIndex(
                name: "IX_ProgramSubjectPracticalCharge_TenantId",
                table: "ProgramSubjectPracticalCharge");

            migrationBuilder.DropIndex(
                name: "IX_PaymentResponseLog_TenantId",
                table: "PaymentResponseLog");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequestLog_TenantId",
                table: "PaymentRequestLog");

            migrationBuilder.DropIndex(
                name: "IX_PaymentPracticalSubjects_TenantId",
                table: "PaymentPracticalSubjects");

            migrationBuilder.DropIndex(
                name: "IX_Notices_TenantId",
                table: "Notices");

            migrationBuilder.DropIndex(
                name: "IX_GradingSchemes_TenantId",
                table: "GradingSchemes");

            migrationBuilder.DropIndex(
                name: "IX_GradeDefinitions_TenantId",
                table: "GradeDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_FiscalYears_TenantId",
                table: "FiscalYears");

            migrationBuilder.DropIndex(
                name: "IX_ExamSubjectResults_TenantId",
                table: "ExamSubjectResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamSlots_TenantId",
                table: "ExamSlots");

            migrationBuilder.DropIndex(
                name: "IX_ExamSchedules_TenantId",
                table: "ExamSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ExamRollNumberSetup_TenantId",
                table: "ExamRollNumberSetup");

            migrationBuilder.DropIndex(
                name: "IX_ExamRegistrations_TenantId",
                table: "ExamRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_ExamFees_TenantId",
                table: "ExamFees");

            migrationBuilder.DropIndex(
                name: "IX_ExamCenters_TenantId",
                table: "ExamCenters");

            migrationBuilder.DropIndex(
                name: "IX_EntranceExamApplications_TenantId",
                table: "EntranceExamApplications");

            migrationBuilder.DropIndex(
                name: "IX_CurriculumVersions_TenantId",
                table: "CurriculumVersions");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_TenantId",
                table: "Colleges");

            migrationBuilder.DropIndex(
                name: "IX_CollegePrograms_TenantId",
                table: "CollegePrograms");

            migrationBuilder.DropIndex(
                name: "IX_CollegeProfiles_TenantId",
                table: "CollegeProfiles");

            migrationBuilder.DropIndex(
                name: "IX_BillTitle_TenantId",
                table: "BillTitle");

            migrationBuilder.DropIndex(
                name: "IX_Batches_TenantId",
                table: "Batches");

            migrationBuilder.DropIndex(
                name: "IX_BankVoucher_TenantId",
                table: "BankVoucher");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationVoucher_TenantId",
                table: "ApplicationVoucher");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_TenantId",
                table: "AcademicYears");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SubjectOfferings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SubjectCatalogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StudentQualifications");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StudentGuardians");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StudentCategories");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StudentAdmissions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SemesterEnrollments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "QuestionSets");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProgramSubjectPracticalCharge");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PaymentResponseLog");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PaymentRequestLog");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PaymentPracticalSubjects");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Notices");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "GradingSchemes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "GradeDefinitions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FiscalYears");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExamSubjectResults");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExamSlots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExamRollNumberSetup");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExamRegistrations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExamFees");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ExamCenters");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EntranceExamApplications");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CurriculumVersions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CollegePrograms");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CollegeProfiles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BillTitle");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BankVoucher");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApplicationVoucher");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AcademicYears");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_RoleId",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_Roles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_Users_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
