using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_Districts_DistrictId",
                table: "Colleges");

            migrationBuilder.DropForeignKey(
                name: "FK_Colleges_QuestionSets_QuestionSetId",
                table: "Colleges");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_EntryFormats_EntryFormatId",
                table: "StudentRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRegistrations_IndexGroups_IndexGroupId",
                table: "StudentRegistrations");

            migrationBuilder.DropTable(
                name: "CollegeProfiles");

            migrationBuilder.DropTable(
                name: "EntryFormats");

            migrationBuilder.DropTable(
                name: "FiscalYears");

            migrationBuilder.DropTable(
                name: "IndexGroups");

            migrationBuilder.DropTable(
                name: "PeriodTypes");

            migrationBuilder.DropTable(
                name: "ProgramSubjectPracticalCharge");

            migrationBuilder.DropTable(
                name: "QuestionSets");

            migrationBuilder.DropTable(
                name: "SchoolTypes");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_EntryFormatId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_StudentRegistrations_IndexGroupId",
                table: "StudentRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_DistrictId",
                table: "Colleges");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_QuestionSetId",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "EntryFormatId",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "IndexGroupId",
                table: "StudentRegistrations");

            migrationBuilder.DropColumn(
                name: "CollegeProfileId",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Colleges");

            migrationBuilder.DropColumn(
                name: "QuestionSetId",
                table: "Colleges");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntryFormatId",
                table: "StudentRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IndexGroupId",
                table: "StudentRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CollegeProfileId",
                table: "Colleges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Colleges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionSetId",
                table: "Colleges",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CollegeProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditReportUserAttachmentId = table.Column<int>(type: "int", nullable: false),
                    BlankChequeUserAttachmentId = table.Column<int>(type: "int", nullable: false),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    BankBranchName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ContactPersonEmail = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ContactPersonMobileNumber = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ContactPersonName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollegeProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollegeProfiles_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollegeProfiles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollegeProfiles_UserAttachments_AuditReportUserAttachmentId",
                        column: x => x.AuditReportUserAttachmentId,
                        principalTable: "UserAttachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollegeProfiles_UserAttachments_BlankChequeUserAttachmentId",
                        column: x => x.BlankChequeUserAttachmentId,
                        principalTable: "UserAttachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntryFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryFormatName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryFormats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FiscalYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EndDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FiscalYearCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FiscalYearName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StartDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndexGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndexGroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndexGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeriodTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    NumberOfMonths = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    PeriodTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramSubjectPracticalCharge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramsId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    PracticalSubjectCharge = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramSubjectPracticalCharge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramSubjectPracticalCharge_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramSubjectPracticalCharge_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    QuestionSetName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreviousLevelId = table.Column<int>(type: "int", nullable: false),
                    SchoolTypeName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolTypes_PreviousLevels_PreviousLevelId",
                        column: x => x.PreviousLevelId,
                        principalTable: "PreviousLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_EntryFormatId",
                table: "StudentRegistrations",
                column: "EntryFormatId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRegistrations_IndexGroupId",
                table: "StudentRegistrations",
                column: "IndexGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_DistrictId",
                table: "Colleges",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_QuestionSetId",
                table: "Colleges",
                column: "QuestionSetId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeProfiles_AuditReportUserAttachmentId",
                table: "CollegeProfiles",
                column: "AuditReportUserAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeProfiles_BlankChequeUserAttachmentId",
                table: "CollegeProfiles",
                column: "BlankChequeUserAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeProfiles_CollegeId",
                table: "CollegeProfiles",
                column: "CollegeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollegeProfiles_TenantId",
                table: "CollegeProfiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryFormats_EntryFormatName",
                table: "EntryFormats",
                column: "EntryFormatName",
                unique: true,
                filter: "[EntryFormatName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYears_FiscalYearCode",
                table: "FiscalYears",
                column: "FiscalYearCode",
                unique: true,
                filter: "[FiscalYearCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IndexGroups_IndexGroupName",
                table: "IndexGroups",
                column: "IndexGroupName",
                unique: true,
                filter: "[IndexGroupName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodTypes_PeriodTypeName",
                table: "PeriodTypes",
                column: "PeriodTypeName",
                unique: true,
                filter: "[PeriodTypeName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSubjectPracticalCharge_ProgramsId",
                table: "ProgramSubjectPracticalCharge",
                column: "ProgramsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSubjectPracticalCharge_TenantId",
                table: "ProgramSubjectPracticalCharge",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSets_TenantId",
                table: "QuestionSets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolTypes_PreviousLevelId",
                table: "SchoolTypes",
                column: "PreviousLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolTypes_SchoolTypeName",
                table: "SchoolTypes",
                column: "SchoolTypeName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_Districts_DistrictId",
                table: "Colleges",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Colleges_QuestionSets_QuestionSetId",
                table: "Colleges",
                column: "QuestionSetId",
                principalTable: "QuestionSets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_EntryFormats_EntryFormatId",
                table: "StudentRegistrations",
                column: "EntryFormatId",
                principalTable: "EntryFormats",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRegistrations_IndexGroups_IndexGroupId",
                table: "StudentRegistrations",
                column: "IndexGroupId",
                principalTable: "IndexGroups",
                principalColumn: "Id");
        }
    }
}
