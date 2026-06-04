using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_TenantId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_TenantId",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_FiscalYears_TenantId",
                table: "FiscalYears");

            migrationBuilder.DropIndex(
                name: "IX_ExamSchedules_TenantId",
                table: "ExamSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_TenantId",
                table: "Colleges");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_TenantId",
                table: "AcademicYears");

            migrationBuilder.AlterColumn<string>(
                name: "OfficeCode",
                table: "Tenants",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OfficeCode",
                table: "Faculties",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_OfficeCode",
                table: "Tenants",
                column: "OfficeCode",
                unique: true,
                filter: "[OfficeCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectTypes_Code",
                table: "SubjectTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_TenantId_RegistrationNumber",
                table: "StudentRegistrations",
                columns: new[] { "TenantId", "RegistrationNumber" },
                unique: true,
                filter: "[RegistrationNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_TenantId_Code",
                table: "Semesters",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolTypes_SchoolTypeName",
                table: "SchoolTypes",
                column: "SchoolTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_ProvinceCode",
                table: "Provinces",
                column: "ProvinceCode",
                unique: true,
                filter: "[ProvinceCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_ProgramCode",
                table: "Programs",
                column: "ProgramCode",
                unique: true,
                filter: "[ProgramCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodTypes_PeriodTypeName",
                table: "PeriodTypes",
                column: "PeriodTypeName",
                unique: true,
                filter: "[PeriodTypeName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentType_PaymentTypeName",
                table: "PaymentType",
                column: "PaymentTypeName",
                unique: true,
                filter: "[PaymentTypeName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Levels_LevelCode",
                table: "Levels",
                column: "LevelCode",
                unique: true,
                filter: "[LevelCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IndexGroups_IndexGroupName",
                table: "IndexGroups",
                column: "IndexGroupName",
                unique: true,
                filter: "[IndexGroupName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Genders_GenderName",
                table: "Genders",
                column: "GenderName",
                unique: true,
                filter: "[GenderName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYears_TenantId_FiscalYearCode",
                table: "FiscalYears",
                columns: new[] { "TenantId", "FiscalYearCode" },
                unique: true,
                filter: "[FiscalYearCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_OfficeCode",
                table: "Faculties",
                column: "OfficeCode",
                unique: true,
                filter: "[OfficeCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExamTypes_Name",
                table: "ExamTypes",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_TenantId_ExamScheduleCode",
                table: "ExamSchedules",
                columns: new[] { "TenantId", "ExamScheduleCode" },
                unique: true,
                filter: "[ExamScheduleCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Ethnicities_EthnicityName",
                table: "Ethnicities",
                column: "EthnicityName",
                unique: true,
                filter: "[EthnicityName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EntryFormats_EntryFormatName",
                table: "EntryFormats",
                column: "EntryFormatName",
                unique: true,
                filter: "[EntryFormatName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_DistrictCode",
                table: "Districts",
                column: "DistrictCode",
                unique: true,
                filter: "[DistrictCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentCode",
                table: "Departments",
                column: "DepartmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollegeTypes_Code",
                table: "CollegeTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_TenantId_Code",
                table: "Colleges",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boards_BoardName",
                table: "Boards",
                column: "BoardName",
                unique: true,
                filter: "[BoardName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_BankCode",
                table: "Banks",
                column: "BankCode",
                unique: true,
                filter: "[BankCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_TenantId_AcademicYearCode",
                table: "AcademicYears",
                columns: new[] { "TenantId", "AcademicYearCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_OfficeCode",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_SubjectTypes_Code",
                table: "SubjectTypes");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_TenantId_RegistrationNumber",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_TenantId_Code",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_SchoolTypes_SchoolTypeName",
                table: "SchoolTypes");

            migrationBuilder.DropIndex(
                name: "IX_Provinces_ProvinceCode",
                table: "Provinces");

            migrationBuilder.DropIndex(
                name: "IX_Programs_ProgramCode",
                table: "Programs");

            migrationBuilder.DropIndex(
                name: "IX_PeriodTypes_PeriodTypeName",
                table: "PeriodTypes");

            migrationBuilder.DropIndex(
                name: "IX_PaymentType_PaymentTypeName",
                table: "PaymentType");

            migrationBuilder.DropIndex(
                name: "IX_Levels_LevelCode",
                table: "Levels");

            migrationBuilder.DropIndex(
                name: "IX_IndexGroups_IndexGroupName",
                table: "IndexGroups");

            migrationBuilder.DropIndex(
                name: "IX_Genders_GenderName",
                table: "Genders");

            migrationBuilder.DropIndex(
                name: "IX_FiscalYears_TenantId_FiscalYearCode",
                table: "FiscalYears");

            migrationBuilder.DropIndex(
                name: "IX_Faculties_OfficeCode",
                table: "Faculties");

            migrationBuilder.DropIndex(
                name: "IX_ExamTypes_Name",
                table: "ExamTypes");

            migrationBuilder.DropIndex(
                name: "IX_ExamSchedules_TenantId_ExamScheduleCode",
                table: "ExamSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Ethnicities_EthnicityName",
                table: "Ethnicities");

            migrationBuilder.DropIndex(
                name: "IX_EntryFormats_EntryFormatName",
                table: "EntryFormats");

            migrationBuilder.DropIndex(
                name: "IX_Districts_DistrictCode",
                table: "Districts");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DepartmentCode",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_CollegeTypes_Code",
                table: "CollegeTypes");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_TenantId_Code",
                table: "Colleges");

            migrationBuilder.DropIndex(
                name: "IX_Boards_BoardName",
                table: "Boards");

            migrationBuilder.DropIndex(
                name: "IX_Banks_BankCode",
                table: "Banks");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_TenantId_AcademicYearCode",
                table: "AcademicYears");

            migrationBuilder.AlterColumn<string>(
                name: "OfficeCode",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "OfficeCode",
                table: "Faculties",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_TenantId",
                table: "StudentRegistrations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_TenantId",
                table: "Semesters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYears_TenantId",
                table: "FiscalYears",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_TenantId",
                table: "ExamSchedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_TenantId",
                table: "Colleges",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_TenantId",
                table: "AcademicYears",
                column: "TenantId");
        }
    }
}
