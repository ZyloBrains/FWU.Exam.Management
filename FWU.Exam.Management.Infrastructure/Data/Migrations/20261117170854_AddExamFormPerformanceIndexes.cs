using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExamFormPerformanceIndexes : Migration
    {
        // The production schema has drifted from the model snapshot: the single-column
        // indexes that EF expected to replace are not present there. Guard both
        // operations so this migration applies cleanly whether or not those indexes
        // exist, and whether the indexes were already created manually.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE object_id = OBJECT_ID(N'dbo.ExamSchedules')
                 AND name = N'IX_ExamSchedules_ProgramId_IsActive_SemesterInstanceId')
BEGIN
    CREATE INDEX [IX_ExamSchedules_ProgramId_IsActive_SemesterInstanceId]
        ON [dbo].[ExamSchedules] ([ProgramId], [IsActive], [SemesterInstanceId]);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE object_id = OBJECT_ID(N'dbo.ExamRegistrations')
                 AND name = N'IX_ExamRegistrations_ApplicationVoucherId_IsActive')
BEGIN
    CREATE INDEX [IX_ExamRegistrations_ApplicationVoucherId_IsActive]
        ON [dbo].[ExamRegistrations] ([ApplicationVoucherId], [IsActive])
        WHERE [ApplicationVoucherId] IS NOT NULL;
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE object_id = OBJECT_ID(N'dbo.ExamSchedules')
             AND name = N'IX_ExamSchedules_ProgramId_IsActive_SemesterInstanceId')
BEGIN
    DROP INDEX [IX_ExamSchedules_ProgramId_IsActive_SemesterInstanceId] ON [dbo].[ExamSchedules];
END;

IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE object_id = OBJECT_ID(N'dbo.ExamRegistrations')
             AND name = N'IX_ExamRegistrations_ApplicationVoucherId_IsActive')
BEGIN
    DROP INDEX [IX_ExamRegistrations_ApplicationVoucherId_IsActive] ON [dbo].[ExamRegistrations];
END;
");
        }
    }
}
