using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RevampExamEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID(N'FK_BankVoucher_ExamScheduleParent_ExamScheduleParentId', N'F') IS NOT NULL ALTER TABLE [BankVoucher] DROP CONSTRAINT [FK_BankVoucher_ExamScheduleParent_ExamScheduleParentId]");

            migrationBuilder.Sql("IF OBJECT_ID(N'FK_ExamRollNumberSetup_ExamScheduleParent_ExamScheduleParentId', N'F') IS NOT NULL ALTER TABLE [ExamRollNumberSetup] DROP CONSTRAINT [FK_ExamRollNumberSetup_ExamScheduleParent_ExamScheduleParentId]");

            migrationBuilder.Sql("IF OBJECT_ID(N'FK_ExamSchedules_ExamScheduleParent_ExamScheduleParentId', N'F') IS NOT NULL ALTER TABLE [ExamSchedules] DROP CONSTRAINT [FK_ExamSchedules_ExamScheduleParent_ExamScheduleParentId]");

            migrationBuilder.Sql("IF OBJECT_ID(N'[ExamCenterDetail]', N'U') IS NOT NULL DROP TABLE [ExamCenterDetail]");
            migrationBuilder.Sql("IF OBJECT_ID(N'[ExamFormFeeRates]', N'U') IS NOT NULL DROP TABLE [ExamFormFeeRates]");
            migrationBuilder.Sql("IF OBJECT_ID(N'[ExamRegistrationActionLog]', N'U') IS NOT NULL DROP TABLE [ExamRegistrationActionLog]");
            migrationBuilder.Sql("IF OBJECT_ID(N'[ExamRegistrationCenterChange]', N'U') IS NOT NULL DROP TABLE [ExamRegistrationCenterChange]");
            migrationBuilder.Sql("IF OBJECT_ID(N'[ExamRollNumberSetupDetail]', N'U') IS NOT NULL DROP TABLE [ExamRollNumberSetupDetail]");
            migrationBuilder.Sql("IF OBJECT_ID(N'[ExamScheduleBatch]', N'U') IS NOT NULL DROP TABLE [ExamScheduleBatch]");
            migrationBuilder.Sql("IF OBJECT_ID(N'[ExamScheduleParent]', N'U') IS NOT NULL DROP TABLE [ExamScheduleParent]");
            migrationBuilder.Sql("IF OBJECT_ID(N'[ExamSubjectRegistrationExamSession]', N'U') IS NOT NULL DROP TABLE [ExamSubjectRegistrationExamSession]");
            migrationBuilder.Sql("IF OBJECT_ID(N'[ExamSubjectRegistrationInternal]', N'U') IS NOT NULL DROP TABLE [ExamSubjectRegistrationInternal]");
            migrationBuilder.Sql("IF OBJECT_ID(N'[ExamFormFeeNames]', N'U') IS NOT NULL DROP TABLE [ExamFormFeeNames]");
            migrationBuilder.Sql("IF OBJECT_ID(N'[PreferredExamCenters]', N'U') IS NOT NULL DROP TABLE [PreferredExamCenters]");
            migrationBuilder.Sql("IF OBJECT_ID(N'[ExamSubjectRegistrations]', N'U') IS NOT NULL DROP TABLE [ExamSubjectRegistrations]");

            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExamSchedules_ExamScheduleParentId' AND object_id = OBJECT_ID(N'[ExamSchedules]')) DROP INDEX [IX_ExamSchedules_ExamScheduleParentId] ON [ExamSchedules]");

            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankVoucher_ExamScheduleParentId' AND object_id = OBJECT_ID(N'[BankVoucher]')) DROP INDEX [IX_BankVoucher_ExamScheduleParentId] ON [BankVoucher]");

            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[ExamSchedules]') AND name = N'ExamScheduleParentId') BEGIN DECLARE @dfname nvarchar(max); SELECT @dfname = QUOTENAME([d].[name]) FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id] WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ExamSchedules]') AND [c].[name] = N'ExamScheduleParentId'); IF @dfname IS NOT NULL EXEC(N'ALTER TABLE [ExamSchedules] DROP CONSTRAINT ' + @dfname); ALTER TABLE [ExamSchedules] DROP COLUMN [ExamScheduleParentId]; END");

            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[BankVoucher]') AND name = N'ExamScheduleParentId') BEGIN DECLARE @dfname2 nvarchar(max); SELECT @dfname2 = QUOTENAME([d].[name]) FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id] WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BankVoucher]') AND [c].[name] = N'ExamScheduleParentId'); IF @dfname2 IS NOT NULL EXEC(N'ALTER TABLE [BankVoucher] DROP CONSTRAINT ' + @dfname2); ALTER TABLE [BankVoucher] DROP COLUMN [ExamScheduleParentId]; END");

            migrationBuilder.RenameColumn(
                name: "ExamScheduleParentId",
                table: "ExamRollNumberSetup",
                newName: "ExamScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamRollNumberSetup_ExamScheduleParentId",
                table: "ExamRollNumberSetup",
                newName: "IX_ExamRollNumberSetup_ExamScheduleId");

            migrationBuilder.AddColumn<string>(
                name: "BatchesJson",
                table: "ExamSchedules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Suffix",
                table: "ExamRollNumberSetup",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Prefix",
                table: "ExamRollNumberSetup",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailsJson",
                table: "ExamRollNumberSetup",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "ExamCenters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollegeAssignmentsJson",
                table: "ExamCenters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExamFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CollegeTypeId = table.Column<int>(type: "int", nullable: true),
                    ExamTypeId = table.Column<int>(type: "int", nullable: true),
                    ThroughDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApplicableDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCollegeFee = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamFees_CollegeTypes_CollegeTypeId",
                        column: x => x.CollegeTypeId,
                        principalTable: "CollegeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamFees_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamFees_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamSubjectResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamRegistrationId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false),
                    SubjectOfferingId = table.Column<int>(type: "int", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: true),
                    ObtainedMarksTheory = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ObtainedMarksTheoryConfirm = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ObtainedMarksPractical = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ObtainedMarksPracticalConfirm = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ObtainedMarksTheoryInternal = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ObtainedMarksPracticalInternal = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    GradeLetter = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsLooseEntry = table.Column<bool>(type: "bit", nullable: true),
                    IsTheoryRegistered = table.Column<bool>(type: "bit", nullable: true),
                    IsPracticalRegistered = table.Column<bool>(type: "bit", nullable: true),
                    IsExtra = table.Column<bool>(type: "bit", nullable: true),
                    ExamStartedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    ObtainedMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ExamSubmittedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAutoSubmitted = table.Column<bool>(type: "bit", nullable: true),
                    LastStatusSyncDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubjectResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubjectResults_ExamRegistrations_ExamRegistrationId",
                        column: x => x.ExamRegistrationId,
                        principalTable: "ExamRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubjectResults_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubjectResults_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubjectResults_SubjectOfferings_SubjectOfferingId",
                        column: x => x.SubjectOfferingId,
                        principalTable: "SubjectOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamFees_CollegeTypeId",
                table: "ExamFees",
                column: "CollegeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFees_ExamScheduleId",
                table: "ExamFees",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFees_ExamTypeId",
                table: "ExamFees",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectResults_ExamRegistrationId",
                table: "ExamSubjectResults",
                column: "ExamRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectResults_ExamScheduleId",
                table: "ExamSubjectResults",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectResults_ExamTypeId",
                table: "ExamSubjectResults",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectResults_SubjectOfferingId",
                table: "ExamSubjectResults",
                column: "SubjectOfferingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRollNumberSetup_ExamSchedules_ExamScheduleId",
                table: "ExamRollNumberSetup",
                column: "ExamScheduleId",
                principalTable: "ExamSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamRollNumberSetup_ExamSchedules_ExamScheduleId",
                table: "ExamRollNumberSetup");

            migrationBuilder.DropTable(
                name: "ExamFees");

            migrationBuilder.DropTable(
                name: "ExamSubjectResults");

            migrationBuilder.DropColumn(
                name: "BatchesJson",
                table: "ExamSchedules");

            migrationBuilder.DropColumn(
                name: "DetailsJson",
                table: "ExamRollNumberSetup");

            migrationBuilder.DropColumn(
                name: "CollegeAssignmentsJson",
                table: "ExamCenters");

            migrationBuilder.RenameColumn(
                name: "ExamScheduleId",
                table: "ExamRollNumberSetup",
                newName: "ExamScheduleParentId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamRollNumberSetup_ExamScheduleId",
                table: "ExamRollNumberSetup",
                newName: "IX_ExamRollNumberSetup_ExamScheduleParentId");

            migrationBuilder.AddColumn<int>(
                name: "ExamScheduleParentId",
                table: "ExamSchedules",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Suffix",
                table: "ExamRollNumberSetup",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Prefix",
                table: "ExamRollNumberSetup",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "ExamCenters",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExamScheduleParentId",
                table: "BankVoucher",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ExamCenterDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    ExamCenterId = table.Column<int>(type: "int", nullable: false),
                    ProgramsId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RollNumberFrom = table.Column<long>(type: "bigint", nullable: false),
                    RollNumberTo = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamCenterDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamCenterDetail_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamCenterDetail_ExamCenters_ExamCenterId",
                        column: x => x.ExamCenterId,
                        principalTable: "ExamCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamCenterDetail_Programs_ProgramsId",
                        column: x => x.ProgramsId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamFormFeeNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsCollegeFee = table.Column<bool>(type: "bit", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamFormFeeNames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamRegistrationActionLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamRegistrationId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRegistrationActionLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRegistrationActionLog_ExamRegistrations_ExamRegistrationId",
                        column: x => x.ExamRegistrationId,
                        principalTable: "ExamRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamRollNumberSetupDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollegeId = table.Column<int>(type: "int", nullable: false),
                    ExamRollNumberSetupId = table.Column<int>(type: "int", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    EndRollNumber = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartRollNumber = table.Column<int>(type: "int", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRollNumberSetupDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRollNumberSetupDetail_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRollNumberSetupDetail_ExamRollNumberSetup_ExamRollNumberSetupId",
                        column: x => x.ExamRollNumberSetupId,
                        principalTable: "ExamRollNumberSetup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRollNumberSetupDetail_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRollNumberSetupDetail_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRollNumberSetupDetail_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamScheduleBatch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamScheduleBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamScheduleBatch_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamScheduleBatch_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamScheduleBatch_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamScheduleParent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamScheduleParentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamScheduleParent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamSubjectRegistrationInternal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryAcademicYearId = table.Column<int>(type: "int", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ObtainedMarksPracticalInternal = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ObtainedMarksTheoryInternal = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SemesterEnrollmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubjectRegistrationInternal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrationInternal_AcademicYears_EntryAcademicYearId",
                        column: x => x.EntryAcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrationInternal_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrationInternal_SemesterEnrollments_SemesterEnrollmentId",
                        column: x => x.SemesterEnrollmentId,
                        principalTable: "SemesterEnrollments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExamSubjectRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamRegistrationId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false),
                    SubjectOfferingId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GradeLetter = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsExtra = table.Column<bool>(type: "bit", nullable: true),
                    IsLooseEntry = table.Column<bool>(type: "bit", nullable: true),
                    IsPracticalRegistered = table.Column<bool>(type: "bit", nullable: true),
                    IsTheoryRegistered = table.Column<bool>(type: "bit", nullable: true),
                    ObtainedMarksPractical = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ObtainedMarksPracticalConfirm = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ObtainedMarksTheory = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ObtainedMarksTheoryConfirm = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubjectRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrations_ExamRegistrations_ExamRegistrationId",
                        column: x => x.ExamRegistrationId,
                        principalTable: "ExamRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrations_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrations_SubjectOfferings_SubjectOfferingId",
                        column: x => x.SubjectOfferingId,
                        principalTable: "SubjectOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreferredExamCenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollegeId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferredExamCenters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreferredExamCenters_Colleges_CollegeId",
                        column: x => x.CollegeId,
                        principalTable: "Colleges",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExamFormFeeRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CollegeTypeId = table.Column<int>(type: "int", nullable: true),
                    ExamFormFeeNameId = table.Column<int>(type: "int", nullable: false),
                    ExamScheduleId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApplicableDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCollegeFee = table.Column<bool>(type: "bit", nullable: false),
                    ThroughDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamFormFeeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamFormFeeRates_CollegeTypes_CollegeTypeId",
                        column: x => x.CollegeTypeId,
                        principalTable: "CollegeTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamFormFeeRates_ExamFormFeeNames_ExamFormFeeNameId",
                        column: x => x.ExamFormFeeNameId,
                        principalTable: "ExamFormFeeNames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamFormFeeRates_ExamSchedules_ExamScheduleId",
                        column: x => x.ExamScheduleId,
                        principalTable: "ExamSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamFormFeeRates_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamSubjectRegistrationExamSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamSubjectRegistrationId = table.Column<int>(type: "int", nullable: false),
                    ExamStartedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExamSubmittedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAutoSubmitted = table.Column<bool>(type: "bit", nullable: true),
                    IsSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    LastStatusSyncDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ObtainedMarks = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubjectRegistrationExamSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubjectRegistrationExamSession_ExamSubjectRegistrations_ExamSubjectRegistrationId",
                        column: x => x.ExamSubjectRegistrationId,
                        principalTable: "ExamSubjectRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamRegistrationCenterChange",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamRegistrationId = table.Column<int>(type: "int", nullable: false),
                    PreferredExamCenterId = table.Column<int>(type: "int", nullable: false),
                    CurrentExamCenterId = table.Column<int>(type: "int", nullable: true),
                    RequestedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRegistrationCenterChange", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRegistrationCenterChange_ExamRegistrations_ExamRegistrationId",
                        column: x => x.ExamRegistrationId,
                        principalTable: "ExamRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamRegistrationCenterChange_PreferredExamCenters_PreferredExamCenterId",
                        column: x => x.PreferredExamCenterId,
                        principalTable: "PreferredExamCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamSchedules_ExamScheduleParentId",
                table: "ExamSchedules",
                column: "ExamScheduleParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BankVoucher_ExamScheduleParentId",
                table: "BankVoucher",
                column: "ExamScheduleParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterDetail_CollegeId",
                table: "ExamCenterDetail",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterDetail_ExamCenterId",
                table: "ExamCenterDetail",
                column: "ExamCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamCenterDetail_ProgramsId",
                table: "ExamCenterDetail",
                column: "ProgramsId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFormFeeRates_CollegeTypeId",
                table: "ExamFormFeeRates",
                column: "CollegeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFormFeeRates_ExamFormFeeNameId",
                table: "ExamFormFeeRates",
                column: "ExamFormFeeNameId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFormFeeRates_ExamScheduleId",
                table: "ExamFormFeeRates",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamFormFeeRates_ExamTypeId",
                table: "ExamFormFeeRates",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrationActionLog_ExamRegistrationId",
                table: "ExamRegistrationActionLog",
                column: "ExamRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrationCenterChange_ExamRegistrationId",
                table: "ExamRegistrationCenterChange",
                column: "ExamRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRegistrationCenterChange_PreferredExamCenterId",
                table: "ExamRegistrationCenterChange",
                column: "PreferredExamCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetupDetail_CollegeId",
                table: "ExamRollNumberSetupDetail",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetupDetail_ExamRollNumberSetupId",
                table: "ExamRollNumberSetupDetail",
                column: "ExamRollNumberSetupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetupDetail_ExamScheduleId",
                table: "ExamRollNumberSetupDetail",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetupDetail_ExamTypeId",
                table: "ExamRollNumberSetupDetail",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRollNumberSetupDetail_ProgramId",
                table: "ExamRollNumberSetupDetail",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleBatch_BatchId",
                table: "ExamScheduleBatch",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleBatch_ExamScheduleId",
                table: "ExamScheduleBatch",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScheduleBatch_ExamTypeId",
                table: "ExamScheduleBatch",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationExamSession_ExamSubjectRegistrationId",
                table: "ExamSubjectRegistrationExamSession",
                column: "ExamSubjectRegistrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationInternal_EntryAcademicYearId",
                table: "ExamSubjectRegistrationInternal",
                column: "EntryAcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationInternal_ExamScheduleId",
                table: "ExamSubjectRegistrationInternal",
                column: "ExamScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrationInternal_SemesterEnrollmentId",
                table: "ExamSubjectRegistrationInternal",
                column: "SemesterEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrations_ExamRegistrationId",
                table: "ExamSubjectRegistrations",
                column: "ExamRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrations_ExamTypeId",
                table: "ExamSubjectRegistrations",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubjectRegistrations_SubjectOfferingId",
                table: "ExamSubjectRegistrations",
                column: "SubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_PreferredExamCenters_CollegeId",
                table: "PreferredExamCenters",
                column: "CollegeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankVoucher_ExamScheduleParent_ExamScheduleParentId",
                table: "BankVoucher",
                column: "ExamScheduleParentId",
                principalTable: "ExamScheduleParent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamRollNumberSetup_ExamScheduleParent_ExamScheduleParentId",
                table: "ExamRollNumberSetup",
                column: "ExamScheduleParentId",
                principalTable: "ExamScheduleParent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSchedules_ExamScheduleParent_ExamScheduleParentId",
                table: "ExamSchedules",
                column: "ExamScheduleParentId",
                principalTable: "ExamScheduleParent",
                principalColumn: "Id");
        }
    }
}
