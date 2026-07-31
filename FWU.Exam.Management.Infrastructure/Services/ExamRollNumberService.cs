using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamRollNumberService(AppDbContext context) : IExamRollNumberService
{
    public async Task<int> GenerateRollNumbersAsync(int examScheduleId)
    {
        var schedule = await context.ExamSchedules!.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(s => s.AcademicYear)
            .FirstOrDefaultAsync(s => s.Id == examScheduleId);

        if (schedule?.AcademicYear == null)
            throw new InvalidOperationException("Exam schedule or academic year not found.");

        var academicYearBs = schedule.AcademicYear.AcademicYearCodeNepali
            ?? schedule.AcademicYear.AcademicYearCode
            ?? DateTime.UtcNow.Year.ToString();

        var setup = await context.ExamRollNumberSetup!
            .FirstOrDefaultAsync(r => r.ExamScheduleId == examScheduleId && r.IsActive);

        var firstNumber = setup?.FirstExamRollNumber ?? 1;
        var minLength = setup?.MinimumRollNumberLength ?? 4;
        var prefix = setup?.Prefix ?? "";
        var suffix = setup?.Suffix ?? "";

        var registrations = await context.ExamRegistrations!.IgnoreQueryFilters()
            .Where(r => r.ExamScheduleId == examScheduleId)
            .OrderBy(r => r.Id)
            .ToListAsync();

        int index = 0;
        foreach (var reg in registrations)
        {
            var sequence = firstNumber + index;
            var paddedSequence = sequence.ToString().PadLeft(minLength, '0');
            var rollNumber = $"{prefix}{academicYearBs}{paddedSequence}{suffix}";

            reg.ExamRollNumber = rollNumber;
            reg.ExamRollNumberCoding = long.TryParse(rollNumber, out var coding) ? coding : null;
            reg.RollNumberIndex = index;
            index++;
        }

        await context.SaveChangesAsync();
        return index;
    }

    public async Task<int> ClearRollNumbersAsync(int examScheduleId)
    {
        var registrations = await context.ExamRegistrations!.IgnoreQueryFilters()
            .Where(r => r.ExamScheduleId == examScheduleId && r.ExamRollNumber != null)
            .ToListAsync();

        foreach (var reg in registrations)
        {
            reg.ExamRollNumber = null;
            reg.ExamRollNumberCoding = null;
            reg.RollNumberIndex = null;
        }

        await context.SaveChangesAsync();
        return registrations.Count;
    }

    public async Task<bool> HasRollNumbersAsync(int examScheduleId)
    {
        return await context.ExamRegistrations!.IgnoreQueryFilters()
            .AnyAsync(r => r.ExamScheduleId == examScheduleId && r.ExamRollNumber != null);
    }
}
