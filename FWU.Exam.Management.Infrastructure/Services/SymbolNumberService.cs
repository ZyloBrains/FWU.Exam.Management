using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SymbolNumberService(AppDbContext context) : ISymbolNumberService
{
    public async Task<int> GetNextStartSequenceAsync(int examScheduleId, string? prefix = null)
    {
        var examTypeId = await GetExamTypeIdAsync(examScheduleId);
        var effectivePrefix = ResolvePrefix(examTypeId, prefix);
        var symbols = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.SymbolNumber != null && er.SymbolNumber.StartsWith(effectivePrefix))
            .Select(er => er.SymbolNumber!)
            .ToListAsync();

        var max = 0;
        foreach (var s in symbols)
        {
            if (SymbolNumberDefaults.TryParseSequence(effectivePrefix, s, out var seq) && seq > max)
                max = seq;
        }

        return max + 1;
    }

    public async Task<SymbolNumberGenerationDto> GetOverviewAsync(int examScheduleId, int? startSequence = null, int? sequenceWidth = null, string? prefix = null)
    {
        var examTypeId = await GetExamTypeIdAsync(examScheduleId);
        var effectivePrefix = ResolvePrefix(examTypeId, prefix);
        var width = NormalizeWidth(sequenceWidth);

        var registrations = await LoadEligibleAsync(examScheduleId);
        var nextStart = startSequence ?? await GetNextStartSequenceAsync(examScheduleId, prefix);

        var schedule = await context.ExamSchedules
            .AsNoTracking()
            .Where(es => es.Id == examScheduleId)
            .Select(es => new
            {
                es.ExamScheduleName,
                ExamTypeName = es.ExamType != null ? es.ExamType.Name : null,
            })
            .FirstOrDefaultAsync();

        var dto = new SymbolNumberGenerationDto
        {
            ExamScheduleId = examScheduleId,
            ExamScheduleName = schedule?.ExamScheduleName,
            ExamTypeId = examTypeId,
            ExamTypeName = schedule?.ExamTypeName,
            Prefix = effectivePrefix,
            SequenceWidth = width,
            TotalRegistrations = registrations.Count,
            AssignedCount = registrations.Count(r => !string.IsNullOrEmpty(r.SymbolNumber)),
            UnassignedCount = registrations.Count(r => string.IsNullOrEmpty(r.SymbolNumber)),
            NextStartSequence = nextStart,
        };

        var maxSeq = SymbolNumberDefaults.MaxSequence(width);
        var existingMax = await GetMaxExistingSequenceAsync(effectivePrefix);
        var effectiveLast = Math.Max(Math.Min(nextStart - 1, maxSeq), Math.Min(existingMax, maxSeq));
        dto.RemainingCapacity = Math.Max(0, maxSeq - effectiveLast);
        dto.OverCapacity = nextStart > maxSeq || dto.UnassignedCount > dto.RemainingCapacity;
        dto.NearCapacity = !dto.OverCapacity && dto.RemainingCapacity <= maxSeq / 10;

        SimulateAssignment(registrations, effectivePrefix, width, nextStart, out var blocks);

        foreach (var b in blocks.Values)
        {
            dto.Blocks.Add(new SymbolBlockInfo
            {
                ProgramId = b.ProgramId,
                ProgramName = b.ProgramName,
                CollegeId = b.CollegeId,
                CollegeName = b.CollegeName,
                RegularCount = b.RegularCount,
                SupplementaryCount = b.SupplementaryCount,
                FromSymbol = b.FromSymbol,
                ToSymbol = b.ToSymbol,
            });
        }

        var identities = await StudentIdentityResolver.ResolveAsync(context, registrations.Select(r => r.Id).ToList());

        foreach (var r in registrations)
        {
            var identity = identities[r.Id];
            dto.Students.Add(new StudentSymbolInfo
            {
                RegistrationId = r.Id,
                SymbolNumber = r.SymbolNumber,
                StudentName = identity.StudentName,
                RegistrationNumber = identity.RegistrationNumber,
                ProgramName = r.Program?.ProgramName ?? r.Program?.ShortName,
                CollegeId = r.CollegeId,
                CollegeName = r.College?.Name,
                AcademicYearId = identity.AcademicYearId,
                AcademicYearName = identity.AcademicYearName,
                IsSupplementary = r.IsSupplementary,
            });
        }

        return dto;
    }

    public async Task<SymbolNumberAssignmentResult> GenerateAsync(int examScheduleId, int? startSequence = null, int? sequenceWidth = null, string? prefix = null)
    {
        var examTypeId = await GetExamTypeIdAsync(examScheduleId);
        var effectivePrefix = ResolvePrefix(examTypeId, prefix);
        var width = NormalizeWidth(sequenceWidth);
        var maxSeq = SymbolNumberDefaults.MaxSequence(width);

        var registrations = await LoadEligibleAsync(examScheduleId, asNoTracking: false);

        var result = new SymbolNumberAssignmentResult
        {
            TotalRegistrations = registrations.Count,
            Skipped = registrations.Count(r => !string.IsNullOrEmpty(r.SymbolNumber)),
        };

        var start = startSequence ?? await GetNextStartSequenceAsync(examScheduleId, prefix);
        if (start < 1) start = SymbolNumberDefaults.DefaultStartSequence;
        if (start > maxSeq)
            throw new InvalidOperationException(
                $"Start sequence {start} exceeds the maximum of {effectivePrefix}{new string('9', width)} for a {width}-digit sequence. Use a {width + 1}-digit width or a lower start.");

        var counter = start;
        foreach (var reg in registrations)
        {
            if (!string.IsNullOrEmpty(reg.SymbolNumber))
            {
                if (SymbolNumberDefaults.TryParseSequence(effectivePrefix, reg.SymbolNumber, out var existing) && existing >= counter)
                    counter = existing + 1;
                continue;
            }

            if (counter > maxSeq)
                throw new InvalidOperationException(
                    $"Sequence exhausted for prefix {effectivePrefix}: cannot assign beyond {effectivePrefix}{new string('9', width)}. " +
                    $"Raise the sequence width to {width + 1} digits and regenerate the remaining students.");

            reg.SymbolNumber = SymbolNumberDefaults.Format(effectivePrefix, counter, width);
            counter++;
            result.Assigned++;
        }

        var duplicates = registrations
            .Where(r => !string.IsNullOrEmpty(r.SymbolNumber))
            .GroupBy(r => r.SymbolNumber)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key!)
            .ToList();

        if (duplicates.Count > 0)
            throw new InvalidOperationException(
                "Symbol number collision detected: " + string.Join(", ", duplicates.Take(5)) +
                ". Please adjust the starting sequence.");

        await context.SaveChangesAsync();

        result.Message = $"{result.Assigned} symbol number(s) assigned, {result.Skipped} skipped (already assigned).";
        return result;
    }

    public async Task<string?> UpdateSymbolNumberAsync(int registrationId, string symbolNumber)
    {
        symbolNumber = symbolNumber?.Trim() ?? string.Empty;
        if (symbolNumber.Length == 0)
            throw new InvalidOperationException("Symbol number cannot be empty.");
        if (symbolNumber.Length > 50)
            throw new InvalidOperationException("Symbol number is too long (max 50 characters).");

        var reg = await context.ExamRegistrations
            .FirstOrDefaultAsync(er => er.Id == registrationId)
            ?? throw new InvalidOperationException("Registration not found.");

        if (!SymbolNumberDefaults.IsValidStrict(symbolNumber))
            throw new InvalidOperationException(
                $"Invalid format '{symbolNumber}'. Expected a prefix (BS year + exam type) followed by a 4-5 digit sequence, e.g. 8310585.");

        var duplicate = await context.ExamRegistrations
            .AnyAsync(er => er.Id != registrationId && er.SymbolNumber == symbolNumber);
        if (duplicate)
            throw new InvalidOperationException($"Symbol number '{symbolNumber}' is already assigned to another student.");

        var old = reg.SymbolNumber;
        reg.SymbolNumber = symbolNumber;
        await context.SaveChangesAsync();
        return old;
    }

    private async Task<int> GetExamTypeIdAsync(int examScheduleId)
    {
        return await context.ExamSchedules
            .AsNoTracking()
            .Where(es => es.Id == examScheduleId)
            .Select(es => es.ExamTypeId)
            .FirstOrDefaultAsync();
    }

    private async Task<int> GetMaxExistingSequenceAsync(string prefix)
    {
        var symbols = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.SymbolNumber != null && er.SymbolNumber.StartsWith(prefix))
            .Select(er => er.SymbolNumber!)
            .ToListAsync();

        var max = 0;
        foreach (var s in symbols)
        {
            if (SymbolNumberDefaults.TryParseSequence(prefix, s, out var seq) && seq > max)
                max = seq;
        }

        return max;
    }

    private static int NormalizeWidth(int? sequenceWidth) =>
        sequenceWidth is >= SymbolNumberDefaults.DefaultSequenceDigits and <= SymbolNumberDefaults.MaxSequenceDigits
            ? sequenceWidth.Value
            : SymbolNumberDefaults.DefaultSequenceDigits;

    private static string ResolvePrefix(int examTypeId, string? prefix)
    {
        var normalized = prefix?.Trim();
        if (string.IsNullOrEmpty(normalized)) return SymbolNumberDefaults.BuildPrefix(examTypeId);
        return System.Text.RegularExpressions.Regex.IsMatch(normalized, "^\\d{3,6}$")
            ? normalized
            : SymbolNumberDefaults.BuildPrefix(examTypeId);
    }

    private async Task<List<Domain.Entities.Exams.ExamRegistration>> LoadEligibleAsync(int examScheduleId, bool asNoTracking = true)
    {
        var query = context.ExamRegistrations
            .Include(er => er.College)
            .Include(er => er.Program)
            .Include(er => er.SemesterEnrollment)
                .ThenInclude(se => se!.StudentAdmission)
                    .ThenInclude(sa => sa!.StudentRegistration)
            .Include(er => er.ApplicationVoucher)
                .ThenInclude(v => v!.StudentRegistration)
            .Where(er => er.ExamScheduleId == examScheduleId
                && er.IsActive
                && er.Status >= RegistrationStatus.CollegeVerified)
            .AsQueryable();

        if (asNoTracking) query = query.AsNoTracking();
        var registrations = await query.ToListAsync();

        return registrations
            .OrderBy(er => er.College != null ? er.College.Name : "", StringComparer.OrdinalIgnoreCase)
            .ThenBy(er => er.ProgramsId)
            .ThenBy(er => er.IsSupplementary)
            .ThenBy(er => ComposeSortName(er), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void SimulateAssignment(
        List<Domain.Entities.Exams.ExamRegistration> registrations,
        string prefix,
        int width,
        int startSequence,
        out Dictionary<(int?, int), BlockAccumulator> blocks)
    {
        blocks = [];
        var counter = Math.Max(startSequence, 1);

        foreach (var reg in registrations)
        {
            string symbol;
            if (!string.IsNullOrEmpty(reg.SymbolNumber))
            {
                symbol = reg.SymbolNumber;
                if (SymbolNumberDefaults.TryParseSequence(prefix, symbol, out var existing) && existing >= counter)
                    counter = existing + 1;
            }
            else
            {
                symbol = SymbolNumberDefaults.Format(prefix, counter, width);
                counter++;
            }

            var key = (reg.ProgramsId, reg.CollegeId);
            if (!blocks.TryGetValue(key, out var block))
            {
                block = new BlockAccumulator
                {
                    ProgramId = reg.ProgramsId,
                    ProgramName = reg.Program?.ProgramName ?? reg.Program?.ShortName,
                    CollegeId = reg.CollegeId,
                    CollegeName = reg.College?.Name,
                };
                blocks[key] = block;
            }

            block.Add(symbol, reg.IsSupplementary);
        }
    }

    private static string ComposeSortName(Domain.Entities.Exams.ExamRegistration er)
    {
        var name = ComposeName(er.SemesterEnrollment?.StudentAdmission);
        return name ?? er.ApplicationVoucher?.StudentName ?? string.Empty;
    }

    private static string? ComposeName(Domain.Entities.Students.StudentAdmission? admission)
    {
        if (admission == null) return null;
        var parts = new[] { admission.FirstName, admission.MiddleName, admission.LastName }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        var name = string.Join(" ", parts);
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    private class BlockAccumulator
    {
        public int? ProgramId { get; set; }
        public string? ProgramName { get; set; }
        public int CollegeId { get; set; }
        public string? CollegeName { get; set; }
        public int RegularCount { get; set; }
        public int SupplementaryCount { get; set; }
        public string? FromSymbol { get; set; }
        public string? ToSymbol { get; set; }

        public void Add(string symbol, bool isSupplementary)
        {
            if (isSupplementary) SupplementaryCount++; else RegularCount++;
            FromSymbol ??= symbol;
            ToSymbol = symbol;
        }
    }
}
