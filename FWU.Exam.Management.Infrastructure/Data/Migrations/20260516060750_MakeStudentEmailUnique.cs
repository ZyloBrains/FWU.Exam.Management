using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeStudentEmailUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentRegistrations_Email' AND object_id = OBJECT_ID('StudentRegistrations'))
                BEGIN
                    CREATE UNIQUE NONCLUSTERED INDEX [IX_StudentRegistrations_Email]
                        ON [StudentRegistrations] ([Email])
                        WHERE [Email] IS NOT NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentRegistrations_Email' AND object_id = OBJECT_ID('StudentRegistrations'))
                BEGIN
                    DROP INDEX [IX_StudentRegistrations_Email] ON [StudentRegistrations];
                END
                """);
        }
    }
}
