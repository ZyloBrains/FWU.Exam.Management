using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ResultRecordService(
    AppDbContext context,
    IUserContext userContext,
    IEmailService emailService,
    ISmsService smsService,
    ILogger<ResultRecordService> logger) : IResultRecordService
{
    public async Task<(List<ResultRecord> Items, int TotalCount)> GetResultRecordsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search, sort, sortDir).ApplyScope(userContext);

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

    public async Task<List<ResultRecord>> GetFilteredItemsAsync(string? search)
    {
        var query = BuildQuery(search, "Id", "asc").ApplyScope(userContext);
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

    public async Task<PublishResultsResult> PublishResultsAsync(int? examScheduleId = null)
    {
        var result = new PublishResultsResult();

        var query = context.ResultRecords
            .AsNoTracking()
            .Where(r => !r.IsPublished)
            .ApplyScope(userContext);

        if (examScheduleId.HasValue)
            query = query.Where(r => r.ExamScheduleId == examScheduleId.Value);

        var records = await query
            .Include(r => r.Program)
            .Include(r => r.College)
            .ToListAsync();

        if (records.Count == 0)
            return result;

        var regNumbers = records
            .Where(r => !string.IsNullOrWhiteSpace(r.RegistrationNumber))
            .Select(r => r.RegistrationNumber!)
            .Distinct()
            .ToList();

        var students = new Dictionary<string, StudentRegistration>();
        if (regNumbers.Count > 0)
        {
            students = await context.StudentRegistrations
                .AsNoTracking()
                .Where(sr => sr.RegistrationNumber != null && regNumbers.Contains(sr.RegistrationNumber) && sr.IsActive)
                .ToDictionaryAsync(sr => sr.RegistrationNumber!);
        }

        foreach (var record in records)
        {
            record.IsPublished = true;
            record.PublishedDate = DateTime.UtcNow;
            result.Published++;

            if (!string.IsNullOrWhiteSpace(record.RegistrationNumber)
                && students.TryGetValue(record.RegistrationNumber, out var student))
            {
                var fullName = record.StudentName ?? $"{student.FirstName} {student.LastName}".Trim();
                var notified = await NotifyStudentResultAsync(fullName, student, record, result);
                if (notified)
                    result.Notified++;
                else
                    result.Failed++;
            }
            else
            {
                result.Failed++;
                result.Errors.Add($"No student registration found for registration number '{record.RegistrationNumber}'.");
            }
        }

        await context.SaveChangesAsync();

        return result;
    }

    private async Task<bool> NotifyStudentResultAsync(string fullName, StudentRegistration student, ResultRecord record, PublishResultsResult result)
    {
        var anySent = false;

        if (!string.IsNullOrWhiteSpace(student.Email))
        {
            try
            {
                var emailBody = EmailTemplateHelper.ResultPublished(
                    fullName,
                    record.RegistrationNumber ?? "",
                    record.SymbolNumber,
                    record.Program?.ProgramName ?? "",
                    record.Gpa ?? "",
                    record.Result ?? "",
                    record.College?.Name ?? "");
                await emailService.SendEmailAsync(student.Email, "Result Published", emailBody);
                anySent = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{fullName} (email): {ex.Message}");
                logger.LogWarning(ex, "Failed to send result email to {Email}", student.Email);
            }
        }

        var phone = student.ContactNumber ?? student.Phone;
        if (!string.IsNullOrWhiteSpace(phone))
        {
            try
            {
                var smsMessage = $"Dear {fullName}, your exam result has been published. Reg No: {record.RegistrationNumber}, Symbol No: {record.SymbolNumber}, GPA: {record.Gpa ?? ""}, Result: {record.Result ?? ""}. - FWU";
                await smsService.SendSmsAsync(phone, smsMessage);
                anySent = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{fullName} (sms): {ex.Message}");
                logger.LogWarning(ex, "Failed to send result SMS to {Phone}", phone);
            }
        }

        return anySent;
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
