using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWU.Exam.Management.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResultRecordsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIEW vResultRecords AS
SELECT
    0 AS Id,
    0 AS AcademicYearId,
    0 AS ProgramsId,
    0 AS ExamTypeId,
    0 AS CollegeId,
    CAST('' AS nvarchar(3)) AS [Year],
    CAST('' AS nvarchar(2)) AS [Part],
    CAST('' AS nvarchar(50)) AS RegistrationNumber,
    CAST('' AS nvarchar(50)) AS SymbolNumber,
    CAST('' AS nvarchar(1)) AS Alphabet,
    CAST('' AS nvarchar(10)) AS DateOfBirthBs,
    CAST('' AS nvarchar(10)) AS Sex,
    CAST('' AS nvarchar(5)) AS TheoryObtainedMarks,
    CAST('' AS nvarchar(5)) AS InternalObtainedMarks,
    CAST('' AS nvarchar(5)) AS PracticalObtainedMarks,
    CAST('' AS nvarchar(5)) AS TheoryObtainedGrade,
    CAST('' AS nvarchar(5)) AS InternalObtainedGrade,
    CAST('' AS nvarchar(5)) AS PracticalObtainedGrade,
    CAST('' AS nvarchar(5)) AS TotalObtainedMarks,
    CAST('' AS nvarchar(5)) AS TotalObtainedGrade,
    CAST('' AS nvarchar(5)) AS TotalGradePoints,
    CAST('' AS nvarchar(4)) AS Gpa,
    CAST('' AS nvarchar(50)) AS Result,
    CAST('' AS nvarchar(255)) AS StudentName,
    0 AS ResultRecordMasterId,
    NULL AS ExamScheduleId,
    NULL AS CreatedDate
WHERE 1 = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vResultRecords");
        }
    }
}
