using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ResultRecordService(AppDbContext context) : IResultRecordService
{
    private IQueryable<ResultRecord> ApplyScope(IQueryable<ResultRecord> query, int? collegeId, int? facultyId)
    {
        if (collegeId.HasValue)
            return query.Where(r => r.CollegeId == collegeId.Value);

        if (facultyId.HasValue)
        {
            var collegeIds = context.Colleges
                .Where(c => c.Faculties.Any(f => f.Id == facultyId.Value))
                .Select(c => c.Id)
                .ToList();

            return query.Where(r => collegeIds.Contains(r.CollegeId));
        }

        return query;
    }

    public async Task<(List<ResultRecord> Items, int TotalCount)> GetResultRecordsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null)
    {
        var query = ApplyScope(BuildQuery(search, sort, sortDir), collegeId, facultyId);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ResultRecord
            {
                Id = r.Id,
                AcademicYearId = r.AcademicYearId,
                ProgramsId = r.ProgramsId,
                ExamTypeId = r.ExamTypeId,
                CollegeId = r.CollegeId,
                Year = r.Year,
                Part = r.Part,
                RegistrationNumber = r.RegistrationNumber,
                SymbolNumber = r.SymbolNumber,
                StudentName = r.StudentName,
                DateOfBirthBs = r.DateOfBirthBs,
                TheoryObtainedMarks = r.TheoryObtainedMarks,
                InternalObtainedMarks = r.InternalObtainedMarks,
                PracticalObtainedMarks = r.PracticalObtainedMarks,
                TheoryObtainedGrade = r.TheoryObtainedGrade,
                InternalObtainedGrade = r.InternalObtainedGrade,
                PracticalObtainedGrade = r.PracticalObtainedGrade,
                TotalObtainedMarks = r.TotalObtainedMarks,
                TotalObtainedGrade = r.TotalObtainedGrade,
                TotalGradePoints = r.TotalGradePoints,
                Gpa = r.Gpa,
                Result = r.Result,
                AcademicYear = r.AcademicYear,
                Program = r.Program,
                ExamType = r.ExamType,
                College = r.College
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<ResultRecord>> GetFilteredItemsAsync(string? search, int? collegeId = null, int? facultyId = null)
    {
        var query = ApplyScope(BuildQuery(search, "Id", "asc"), collegeId, facultyId);
        return await query
            .Select(r => new ResultRecord
            {
                Id = r.Id,
                AcademicYearId = r.AcademicYearId,
                ProgramsId = r.ProgramsId,
                ExamTypeId = r.ExamTypeId,
                CollegeId = r.CollegeId,
                Year = r.Year,
                Part = r.Part,
                RegistrationNumber = r.RegistrationNumber,
                SymbolNumber = r.SymbolNumber,
                StudentName = r.StudentName,
                TotalObtainedMarks = r.TotalObtainedMarks,
                TotalObtainedGrade = r.TotalObtainedGrade,
                Gpa = r.Gpa,
                Result = r.Result,
                AcademicYear = r.AcademicYear,
                Program = r.Program,
                ExamType = r.ExamType,
                College = r.College
            })
            .ToListAsync();
    }

    public async Task<ResultRecord?> GetResultRecordByIdAsync(int id)
    {
        return await context.ResultRecords
            .AsNoTracking()
            .Include(r => r.AcademicYear)
            .Include(r => r.Program)
            .Include(r => r.ExamType)
            .Include(r => r.College)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    private IQueryable<ResultRecord> BuildQuery(string? search, string sort, string sortDir)
    {
        var query = context.ResultRecords.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(r =>
                (r.SymbolNumber != null && r.SymbolNumber.Contains(search)) ||
                (r.RegistrationNumber != null && r.RegistrationNumber.Contains(search)) ||
                (r.StudentName != null && r.StudentName.Contains(search)) ||
                (r.Result != null && r.Result.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "studentname" => descending ? query.OrderByDescending(r => r.StudentName) : query.OrderBy(r => r.StudentName),
            "symbolnumber" => descending ? query.OrderByDescending(r => r.SymbolNumber) : query.OrderBy(r => r.SymbolNumber),
            "registrationnumber" => descending ? query.OrderByDescending(r => r.RegistrationNumber) : query.OrderBy(r => r.RegistrationNumber),
            "gpa" => descending ? query.OrderByDescending(r => r.Gpa) : query.OrderBy(r => r.Gpa),
            "result" => descending ? query.OrderByDescending(r => r.Result) : query.OrderBy(r => r.Result),
            _ => descending ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id)
        };
    }
}
