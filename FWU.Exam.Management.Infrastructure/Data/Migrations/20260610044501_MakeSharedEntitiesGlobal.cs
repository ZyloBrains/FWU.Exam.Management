using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeSharedEntitiesGlobal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Tenants_TenantId",
                table: "AcademicYears");

            migrationBuilder.DropForeignKey(
                name: "FK_Batches_Tenants_TenantId",
                table: "Batches");

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
                name: "FK_Semesters_Tenants_TenantId",
                table: "Semesters");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCategories_Tenants_TenantId",
                table: "StudentCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectCatalogs_Tenants_TenantId",
                table: "SubjectCatalogs");

            migrationBuilder.DropIndex(
                name: "IX_SubjectCatalogs_TenantId",
                table: "SubjectCatalogs");

            migrationBuilder.DropIndex(
                name: "IX_StudentCategories_TenantId",
                table: "StudentCategories");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_TenantId_Code",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_GradingSchemes_TenantId",
                table: "GradingSchemes");

            migrationBuilder.DropIndex(
                name: "IX_GradeDefinitions_TenantId",
                table: "GradeDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_FiscalYears_TenantId_FiscalYearCode",
                table: "FiscalYears");

            migrationBuilder.DropIndex(
                name: "IX_Batches_TenantId",
                table: "Batches");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_TenantId_AcademicYearCode",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SubjectCatalogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "StudentCategories");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Semesters");

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
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AcademicYears");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_Code",
                table: "Semesters",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYears_FiscalYearCode",
                table: "FiscalYears",
                column: "FiscalYearCode",
                unique: true,
                filter: "[FiscalYearCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_AcademicYearCode",
                table: "AcademicYears",
                column: "AcademicYearCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Semesters_Code",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_FiscalYears_FiscalYearCode",
                table: "FiscalYears");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_AcademicYearCode",
                table: "AcademicYears");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "SubjectCatalogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "StudentCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Semesters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "GradingSchemes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "GradeDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "FiscalYears",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Batches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AcademicYears",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCatalogs_TenantId",
                table: "SubjectCatalogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCategories_TenantId",
                table: "StudentCategories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_TenantId_Code",
                table: "Semesters",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GradingSchemes_TenantId",
                table: "GradingSchemes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeDefinitions_TenantId",
                table: "GradeDefinitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYears_TenantId_FiscalYearCode",
                table: "FiscalYears",
                columns: new[] { "TenantId", "FiscalYearCode" },
                unique: true,
                filter: "[FiscalYearCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_TenantId",
                table: "Batches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_TenantId_AcademicYearCode",
                table: "AcademicYears",
                columns: new[] { "TenantId", "AcademicYearCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Tenants_TenantId",
                table: "AcademicYears",
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
                name: "FK_Semesters_Tenants_TenantId",
                table: "Semesters",
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
                name: "FK_SubjectCatalogs_Tenants_TenantId",
                table: "SubjectCatalogs",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
