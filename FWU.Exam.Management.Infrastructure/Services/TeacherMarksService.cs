using System.Globalization;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class TeacherMarksService(
    AppDbContext context,
    ITeacherSubjectAssignmentService assignmentService,
    IGradeCalculationService gradeCalculationService) : ITeacherMarksService
{
    public async Task<TeacherDashboardDto> GetTeacherDashboardAsync(string teacherUserId)
    {
        var assignments = await assignmentService.GetAssignmentsAsync(teacherUserId);
        var subjectOfferingIds = assignments.Select(a => a.SubjectOfferingId).Distinct().ToList();

        var subjectOfferings = await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .Include(so => so.Program)
            .Include(so => so.Semester)
            .Where(so => subjectOfferingIds.Contains(so.Id))
            .ToListAsync();

        var result = new TeacherDashboardDto();

        foreach (var so in subjectOfferings)
        {
            var examSchedules = assignments
                .Where(a => a.SubjectOfferingId == so.Id && a.ExamScheduleId != null)
                .Select(a => a.ExamScheduleId!.Value)
                .Distinct()
                .ToList();

            var examScheduleIds = examSchedules.Any()
                ? examSchedules
                : await context.ExamSchedules
                    .Where(es => es.ProgramId == so.ProgramId && es.SemesterId == so.SemesterId && es.IsActive)
                    .Select(es => es.Id)
                    .ToListAsync();

            var registeredCount = await context.ExamRegistrations
                .CountAsync(er => examScheduleIds.Contains(er.ExamScheduleId)
                               && er.ProgramsId == so.ProgramId
                               && er.IsActive);

            var marksEnteredCount = await context.ExamSubjectResults
                .CountAsync(esr => examScheduleIds.Contains(esr.ExamScheduleId ?? 0)
                                && esr.SubjectOfferingId == so.Id
                                && esr.IsSubmitted);

            result.AssignedSubjects.Add(new TeacherSubjectInfo
            {
                SubjectOfferingId = so.Id,
                SubjectName = so.SubjectCatalog?.SubjectName ?? "Unknown",
                SubjectCode = so.SubjectCatalog?.SubjectCode ?? "",
                ProgramName = so.Program?.ProgramName ?? "",
                SemesterName = so.Semester?.Name ?? "",
                RegisteredStudentCount = registeredCount,
                MarksEnteredCount = marksEnteredCount
            });
        }

        return result;
    }

    public async Task<MarksEntryViewModel> GetMarksEntryViewAsync(int subjectOfferingId, int examScheduleId, string teacherUserId)
    {
        if (!await assignmentService.IsTeacherAssignedToSubjectAsync(teacherUserId, subjectOfferingId))
            throw new UnauthorizedAccessException("You are not assigned to this subject.");

        var subjectOffering = await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .FirstOrDefaultAsync(so => so.Id == subjectOfferingId)
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var examRegistrations = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.ExamScheduleId == examScheduleId
                      && er.ProgramsId == subjectOffering.ProgramId
                      && er.IsActive
                      && er.Status == Domain.Enums.RegistrationStatus.Registered)
            .ToListAsync();

        var erIds = examRegistrations.Select(er => er.Id).ToList();

        var studentNames = await GetStudentNamesForExamRegistrationsAsync(erIds);
        var registrationNumbers = await GetRegistrationNumbersForExamRegistrationsAsync(erIds);

        var existingResults = await context.ExamSubjectResults
            .AsNoTracking()
            .Where(esr => esr.SubjectOfferingId == subjectOfferingId
                       && esr.ExamScheduleId == examScheduleId)
            .ToListAsync();

        var studentRows = examRegistrations.Select(er =>
        {
            var existing = existingResults.FirstOrDefault(esr => esr.ExamRegistrationId == er.Id);
            studentNames.TryGetValue(er.Id, out var name);

            registrationNumbers.TryGetValue(er.Id, out var regNum);

            return new StudentMarksRowDto
            {
                ExamRegistrationId = er.Id,
                ExamSubjectResultId = existing?.Id,
                StudentName = name ?? $"Student #{er.Id}",
                SymbolNumber = er.ExamRollNumber ?? "",
                RegistrationNumber = regNum ?? "",
                TheoryMarks = existing?.ObtainedMarksTheory,
                TheoryConfirm = existing?.ObtainedMarksTheoryConfirm,
                PracticalMarks = existing?.ObtainedMarksPractical,
                PracticalConfirm = existing?.ObtainedMarksPracticalConfirm,
                TheoryInternal = existing?.ObtainedMarksTheoryInternal,
                PracticalInternal = existing?.ObtainedMarksPracticalInternal,
                TotalMarks = existing?.ObtainedMarks,
                GradeLetter = existing?.GradeLetter,
                IsSubmitted = existing?.IsSubmitted ?? false
            };
        }).ToList();

        return new MarksEntryViewModel
        {
            SubjectOfferingId = subjectOfferingId,
            ExamScheduleId = examScheduleId,
            SubjectName = subjectOffering.SubjectCatalog?.SubjectName ?? "Unknown",
            TheoryFullMarks = subjectOffering.TheoryFullMarks,
            TheoryPassMarks = subjectOffering.TheoryPassMarks,
            PracticalFullMarks = subjectOffering.PracticalFullMarks,
            PracticalPassMarks = subjectOffering.PracticalPassMarks,
            HasTheory = subjectOffering.HasTheory,
            HasPractical = subjectOffering.HasPractical,
            HasInternal = subjectOffering.HasInternal,
            Students = studentRows
        };
    }

    private async Task<Dictionary<int, string>> GetStudentNamesForExamRegistrationsAsync(List<int> examRegistrationIds)
    {
        var semEnrollments = await context.Set<SemesterEnrollment>()
            .AsNoTracking()
            .Include(se => se.StudentAdmission)
            .Include(se => se.ExamRegistrations)
            .Where(se => se.ExamRegistrations!.Any(er => examRegistrationIds.Contains(er.Id)))
            .ToListAsync();

        var userIds = semEnrollments
            .Select(se => se.StudentAdmission?.AppUserId)
            .Where(id => id != null)
            .Distinct()
            .Cast<string>()
            .ToList();

        var userNames = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FullName ?? u.Email ?? "" })
            .ToDictionaryAsync(u => u.Id, u => u.Name);

        var names = new Dictionary<int, string>();
        foreach (var se in semEnrollments)
        {
            if (se.ExamRegistrations == null) continue;
            var appUserId = se.StudentAdmission?.AppUserId;
            var name = appUserId != null && userNames.TryGetValue(appUserId, out var n) ? n : "";
            foreach (var er in se.ExamRegistrations.Where(er => examRegistrationIds.Contains(er.Id)))
            {
                names[er.Id] = name;
            }
        }

        return names;
    }

    private async Task<Dictionary<int, string>> GetRegistrationNumbersForExamRegistrationsAsync(List<int> examRegistrationIds)
    {
        var semEnrollments = await context.Set<SemesterEnrollment>()
            .AsNoTracking()
            .Include(se => se.StudentAdmission)
            .Include(se => se.ExamRegistrations)
            .Where(se => se.ExamRegistrations!.Any(er => examRegistrationIds.Contains(er.Id)))
            .ToListAsync();

        var admissionIds = semEnrollments
            .Where(se => se.StudentAdmission != null)
            .Select(se => se.StudentAdmission!.Id)
            .Distinct()
            .ToList();

        if (admissionIds.Count == 0) return new Dictionary<int, string>();

        var regByAdmission = await context.StudentRegistrations!
            .AsNoTracking()
            .Where(sr => sr.StudentAdmissions!.Any(sa => admissionIds.Contains(sa.Id)))
            .SelectMany(sr => sr.StudentAdmissions!, (sr, sa) => new { AdmissionId = sa.Id, sr.RegistrationNumber })
            .Where(x => x.RegistrationNumber != null)
            .Distinct()
            .ToDictionaryAsync(x => x.AdmissionId, x => x.RegistrationNumber!);

        var result = new Dictionary<int, string>();
        foreach (var se in semEnrollments)
        {
            if (se.ExamRegistrations == null) continue;
            var regNum = se.StudentAdmission != null && regByAdmission.TryGetValue(se.StudentAdmission.Id, out var rn) ? rn : "";
            foreach (var er in se.ExamRegistrations.Where(er => examRegistrationIds.Contains(er.Id)))
            {
                result[er.Id] = regNum;
            }
        }

        return result;
    }

    public async Task<BulkSaveResult> SaveMarksBulkAsync(BulkMarksSaveDto dto, string teacherUserId)
    {
        if (!await assignmentService.IsTeacherAssignedToSubjectAsync(teacherUserId, dto.SubjectOfferingId))
            throw new UnauthorizedAccessException("You are not assigned to this subject.");

        var subjectOffering = await context.SubjectOfferings
            .FirstOrDefaultAsync(so => so.Id == dto.SubjectOfferingId)
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var result = new BulkSaveResult { Success = true };

        foreach (var student in dto.Students)
        {
            try
            {
                ExamSubjectResult? entity;

                if (student.ExamSubjectResultId.HasValue)
                {
                    entity = await context.ExamSubjectResults
                        .FirstOrDefaultAsync(esr => esr.Id == student.ExamSubjectResultId.Value);
                    if (entity == null) continue;
                }
                else
                {
                    entity = new ExamSubjectResult
                    {
                        TenantId = 1,
                        ExamRegistrationId = student.ExamRegistrationId,
                        SubjectOfferingId = dto.SubjectOfferingId,
                        ExamScheduleId = dto.ExamScheduleId,
                        IsActive = true
                    };

                    var examTypeId = await context.ExamSchedules
                        .Where(es => es.Id == dto.ExamScheduleId)
                        .Select(es => es.ExamTypeId)
                        .FirstOrDefaultAsync();
                    entity.ExamTypeId = examTypeId > 0 ? examTypeId : 1;
                }

                entity.ObtainedMarksTheory = student.TheoryMarks;
                entity.ObtainedMarksTheoryConfirm = student.TheoryConfirm;
                entity.ObtainedMarksPractical = student.PracticalMarks;
                entity.ObtainedMarksPracticalConfirm = student.PracticalConfirm;
                entity.ObtainedMarksTheoryInternal = student.TheoryInternal;
                entity.ObtainedMarksPracticalInternal = student.PracticalInternal;

                var totalMarks = gradeCalculationService.CalculateTotalMarks(
                    student.TheoryMarks, student.PracticalMarks,
                    student.TheoryInternal, student.PracticalInternal);
                entity.ObtainedMarks = totalMarks;

                var grade = gradeCalculationService.CalculateGrade(totalMarks, subjectOffering);
                entity.GradeLetter = grade.GradeLetter;

                if (dto.SubmitAll)
                {
                    entity.IsSubmitted = true;
                    entity.ExamSubmittedDateTime = DateTime.UtcNow;
                }

                if (student.ExamSubjectResultId.HasValue)
                {
                    context.ExamSubjectResults.Update(entity);
                }
                else
                {
                    context.ExamSubjectResults.Add(entity);
                }

                result.SavedCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Student '{student.StudentName}': {ex.Message}");
            }
        }

        await context.SaveChangesAsync();
        result.Success = result.Errors.Count == 0;

        return result;
    }

    public async Task<ExcelImportResultDto> ImportMarksFromExcelAsync(Stream excelStream, int subjectOfferingId, int examScheduleId, string teacherUserId)
    {
        if (!await assignmentService.IsTeacherAssignedToSubjectAsync(teacherUserId, subjectOfferingId))
            throw new UnauthorizedAccessException("You are not assigned to this subject.");

        var subjectOffering = await context.SubjectOfferings
            .FirstOrDefaultAsync(so => so.Id == subjectOfferingId)
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var result = new ExcelImportResultDto();

        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheet(1);
        var range = worksheet.RangeUsed();
        if (range == null) return result;

        var allRows = range.RowsUsed().ToList();
        if (allRows.Count < 2) return result;

        var headerCells = allRows[0].CellsUsed().Select(c => c.GetString().Trim().ToLowerInvariant()).ToList();

        int colSymbolNo = -1, colRegNo = -1, colStudentName = -1;
        int colTheory = -1, colTheoryConfirm = -1;
        int colPractical = -1, colPracticalConfirm = -1;
        int colTheoryInternal = -1, colPracticalInternal = -1;

        for (int i = 0; i < headerCells.Count; i++)
        {
            var h = headerCells[i];
            if (h.Contains("symbol") || h.Contains("roll no") || h.Contains("exam roll")) colSymbolNo = i;
            else if (h.Contains("reg no") || h == "regno" || h.Contains("registration")) colRegNo = i;
            else if (h.Contains("student")) colStudentName = i;
            else if ((h.Contains("theory") || h.Contains("th.")) && h.Contains("confirm")) colTheoryConfirm = i;
            else if ((h.Contains("theory") || h.Contains("th.")) && !h.Contains("internal") && colTheory == -1) colTheory = i;
            else if ((h.Contains("practical") || h.Contains("pr.")) && h.Contains("confirm")) colPracticalConfirm = i;
            else if ((h.Contains("practical") || h.Contains("pr.")) && !h.Contains("internal") && colPractical == -1) colPractical = i;
            else if (h.Contains("theory") && h.Contains("internal")) colTheoryInternal = i;
            else if (h.Contains("practical") && h.Contains("internal")) colPracticalInternal = i;
        }

        if (colSymbolNo == -1 && colRegNo == -1 && colStudentName == -1)
        {
            var foundHeaders = string.Join(", ", headerCells.Select(c => $"\"{c}\""));
            result.Errors.Add($"Could not identify student identifier columns (Symbol No., Reg No., or Student Name). Found headers: {foundHeaders}");
            result.ErrorCount++;
            return result;
        }

        foreach (var row in allRows.Skip(1))
        {
            try
            {
                var examRollNumber = colSymbolNo >= 0 ? row.Cell(colSymbolNo + 1).GetString().Trim() : "";
                var regNo = colRegNo >= 0 ? row.Cell(colRegNo + 1).GetString().Trim() : "";
                var studentName = colStudentName >= 0 ? row.Cell(colStudentName + 1).GetString().Trim() : "";

                var theoryStr = colTheory >= 0 ? row.Cell(colTheory + 1).GetString().Trim() : "";
                var theoryConfirmStr = colTheoryConfirm >= 0 ? row.Cell(colTheoryConfirm + 1).GetString().Trim() : "";
                var practicalStr = colPractical >= 0 ? row.Cell(colPractical + 1).GetString().Trim() : "";
                var practicalConfirmStr = colPracticalConfirm >= 0 ? row.Cell(colPracticalConfirm + 1).GetString().Trim() : "";
                var theoryInternalStr = colTheoryInternal >= 0 ? row.Cell(colTheoryInternal + 1).GetString().Trim() : "";
                var practicalInternalStr = colPracticalInternal >= 0 ? row.Cell(colPracticalInternal + 1).GetString().Trim() : "";

                var examRegs = await context.ExamRegistrations
                    .Where(er => er.ExamScheduleId == examScheduleId && er.IsActive)
                    .ToListAsync();

                var examReg = examRegs.FirstOrDefault(er => er.ExamRollNumber == examRollNumber);

                if (examReg == null && !string.IsNullOrEmpty(studentName))
                {
                    var erIds = examRegs.Select(er => er.Id).ToList();
                    var names = await GetStudentNamesForExamRegistrationsAsync(erIds);
                    examReg = examRegs.FirstOrDefault(er =>
                        names.TryGetValue(er.Id, out var name) &&
                        string.Equals(name, studentName, StringComparison.OrdinalIgnoreCase));
                }

                if (examReg == null && !string.IsNullOrEmpty(regNo))
                {
                    var erIds = examRegs.Select(er => er.Id).ToList();
                    var regNos = await GetRegistrationNumbersForExamRegistrationsAsync(erIds);
                    examReg = examRegs.FirstOrDefault(er =>
                        regNos.TryGetValue(er.Id, out var rn) &&
                        string.Equals(rn, regNo, StringComparison.OrdinalIgnoreCase));
                }

                if (examReg == null)
                {
                    var identifier = !string.IsNullOrEmpty(examRollNumber) ? examRollNumber
                        : !string.IsNullOrEmpty(regNo) ? regNo
                        : studentName;
                    result.Errors.Add($"Row {row.RowNumber()}: No exam registration found for '{identifier}'.");
                    result.ErrorCount++;
                    continue;
                }

                var existing = await context.ExamSubjectResults
                    .FirstOrDefaultAsync(esr => esr.ExamRegistrationId == examReg.Id
                                             && esr.SubjectOfferingId == subjectOfferingId
                                             && esr.ExamScheduleId == examScheduleId);

                decimal? theoryInternal = string.IsNullOrEmpty(theoryInternalStr) ? null : decimal.Parse(theoryInternalStr, CultureInfo.InvariantCulture);
                decimal? practicalInternal = string.IsNullOrEmpty(practicalInternalStr) ? null : decimal.Parse(practicalInternalStr, CultureInfo.InvariantCulture);

                if (existing != null)
                {
                    existing.ObtainedMarksTheory = string.IsNullOrEmpty(theoryStr) ? null : theoryStr;
                    existing.ObtainedMarksTheoryConfirm = string.IsNullOrEmpty(theoryConfirmStr) ? null : theoryConfirmStr;
                    existing.ObtainedMarksPractical = string.IsNullOrEmpty(practicalStr) ? null : practicalStr;
                    existing.ObtainedMarksPracticalConfirm = string.IsNullOrEmpty(practicalConfirmStr) ? null : practicalConfirmStr;
                    existing.ObtainedMarksTheoryInternal = theoryInternal;
                    existing.ObtainedMarksPracticalInternal = practicalInternal;
                    existing.ObtainedMarks = gradeCalculationService.CalculateTotalMarks(theoryStr, practicalStr, theoryInternal, practicalInternal);
                    existing.GradeLetter = gradeCalculationService.CalculateGrade(existing.ObtainedMarks.Value, subjectOffering).GradeLetter;
                }
                else
                {
                    var totalMarks = gradeCalculationService.CalculateTotalMarks(theoryStr, practicalStr, theoryInternal, practicalInternal);
                    var examTypeId = await context.ExamSchedules
                        .Where(es => es.Id == examScheduleId)
                        .Select(es => es.ExamTypeId)
                        .FirstOrDefaultAsync();

                    var newResult = new ExamSubjectResult
                    {
                        TenantId = 1,
                        ExamRegistrationId = examReg.Id,
                        ExamTypeId = examTypeId,
                        SubjectOfferingId = subjectOfferingId,
                        ExamScheduleId = examScheduleId,
                        ObtainedMarksTheory = string.IsNullOrEmpty(theoryStr) ? null : theoryStr,
                        ObtainedMarksTheoryConfirm = string.IsNullOrEmpty(theoryConfirmStr) ? null : theoryConfirmStr,
                        ObtainedMarksPractical = string.IsNullOrEmpty(practicalStr) ? null : practicalStr,
                        ObtainedMarksPracticalConfirm = string.IsNullOrEmpty(practicalConfirmStr) ? null : practicalConfirmStr,
                        ObtainedMarksTheoryInternal = theoryInternal,
                        ObtainedMarksPracticalInternal = practicalInternal,
                        ObtainedMarks = totalMarks,
                        GradeLetter = gradeCalculationService.CalculateGrade(totalMarks, subjectOffering).GradeLetter,
                        IsActive = true,
                        IsSubmitted = false
                    };
                    context.ExamSubjectResults.Add(newResult);
                }

                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                result.ErrorCount++;
            }
        }

        await context.SaveChangesAsync();
        return result;
    }

    public async Task<byte[]> ExportMarksTemplateAsync(int subjectOfferingId, int examScheduleId)
    {
        var subjectOffering = await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .FirstOrDefaultAsync(so => so.Id == subjectOfferingId)
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var examRegistrations = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.ExamScheduleId == examScheduleId
                      && er.ProgramsId == subjectOffering.ProgramId
                      && er.IsActive
                      && er.Status == Domain.Enums.RegistrationStatus.Registered)
            .ToListAsync();

        var erIds = examRegistrations.Select(er => er.Id).ToList();
        var studentNames = await GetStudentNamesForExamRegistrationsAsync(erIds);
        var registrationNumbers = await GetRegistrationNumbersForExamRegistrationsAsync(erIds);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Marks");

        ws.Cell(1, 1).Value = "S.N.";
        ws.Cell(1, 2).Value = "Student Name";
        ws.Cell(1, 3).Value = "Symbol No.";
        ws.Cell(1, 4).Value = "Reg No.";
        ws.Cell(1, 5).Value = $"Theory Marks (Full: {subjectOffering.TheoryFullMarks})";
        ws.Cell(1, 6).Value = "Theory Marks (Confirm)";
        ws.Cell(1, 7).Value = subjectOffering.HasPractical
            ? $"Practical Marks (Full: {subjectOffering.PracticalFullMarks})"
            : "Practical Marks";
        ws.Cell(1, 8).Value = "Practical Marks (Confirm)";
        ws.Cell(1, 9).Value = subjectOffering.HasInternal ? "Theory Internal" : "";
        ws.Cell(1, 10).Value = subjectOffering.HasInternal ? "Practical Internal" : "";

        var headerRange = ws.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        var sn = 1;
        foreach (var er in examRegistrations)
        {
            studentNames.TryGetValue(er.Id, out var name);
            registrationNumbers.TryGetValue(er.Id, out var regNo);

            ws.Cell(row, 1).Value = sn++;
            ws.Cell(row, 2).Value = name ?? "";
            ws.Cell(row, 3).Value = er.ExamRollNumber ?? "";
            ws.Cell(row, 4).Value = regNo ?? "";
            ws.Cell(row, 5).Value = "";
            ws.Cell(row, 6).Value = "";
            ws.Cell(row, 7).Value = "";
            ws.Cell(row, 8).Value = "";
            ws.Cell(row, 9).Value = "";
            ws.Cell(row, 10).Value = "";

            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportMarksAsync(int subjectOfferingId, int examScheduleId)
    {
        var subjectOffering = await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .FirstOrDefaultAsync(so => so.Id == subjectOfferingId)
            ?? throw new KeyNotFoundException("Subject offering not found.");

        var examRegistrations = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.ExamScheduleId == examScheduleId
                      && er.ProgramsId == subjectOffering.ProgramId
                      && er.IsActive
                      && er.Status == Domain.Enums.RegistrationStatus.Registered)
            .ToListAsync();

        var erIds = examRegistrations.Select(er => er.Id).ToList();
        var existingResults = await context.ExamSubjectResults
            .AsNoTracking()
            .Where(esr => esr.SubjectOfferingId == subjectOfferingId
                       && esr.ExamScheduleId == examScheduleId)
            .ToDictionaryAsync(esr => esr.ExamRegistrationId);

        var erIds2 = examRegistrations.Select(er => er.Id).ToList();
        var studentNames = await GetStudentNamesForExamRegistrationsAsync(erIds2);
        var registrationNumbers = await GetRegistrationNumbersForExamRegistrationsAsync(erIds2);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Marks");

        ws.Cell(1, 1).Value = "S.N.";
        ws.Cell(1, 2).Value = "Student Name";
        ws.Cell(1, 3).Value = "Symbol No.";
        ws.Cell(1, 4).Value = "Reg No.";
        ws.Cell(1, 5).Value = $"Theory (Full: {subjectOffering.TheoryFullMarks})";
        ws.Cell(1, 6).Value = "Theory (Confirm)";
        ws.Cell(1, 7).Value = subjectOffering.HasPractical
            ? $"Practical (Full: {subjectOffering.PracticalFullMarks})"
            : "Practical";
        ws.Cell(1, 8).Value = "Practical (Confirm)";
        ws.Cell(1, 9).Value = subjectOffering.HasInternal ? "Theory Internal" : "";
        ws.Cell(1, 10).Value = subjectOffering.HasInternal ? "Practical Internal" : "";
        ws.Cell(1, 11).Value = "Total";
        ws.Cell(1, 12).Value = "Grade";
        ws.Cell(1, 13).Value = "Status";
        ws.Cell(1, 14).Value = "Submitted";

        var headerRange = ws.Range(1, 1, 1, 14);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 2;
        var sn = 1;
        foreach (var er in examRegistrations)
        {
            studentNames.TryGetValue(er.Id, out var name);
            registrationNumbers.TryGetValue(er.Id, out var regNo);
            existingResults.TryGetValue(er.Id, out var result);

            ws.Cell(row, 1).Value = sn++;
            ws.Cell(row, 2).Value = name ?? "";
            ws.Cell(row, 3).Value = er.ExamRollNumber ?? "";
            ws.Cell(row, 4).Value = regNo ?? "";
            ws.Cell(row, 5).Value = result?.ObtainedMarksTheory ?? "";
            ws.Cell(row, 6).Value = result?.ObtainedMarksTheoryConfirm ?? "";
            ws.Cell(row, 7).Value = result?.ObtainedMarksPractical ?? "";
            ws.Cell(row, 8).Value = result?.ObtainedMarksPracticalConfirm ?? "";
            ws.Cell(row, 9).Value = result?.ObtainedMarksTheoryInternal?.ToString() ?? "";
            ws.Cell(row, 10).Value = result?.ObtainedMarksPracticalInternal?.ToString() ?? "";
            ws.Cell(row, 11).Value = result?.ObtainedMarks?.ToString() ?? "";
            ws.Cell(row, 12).Value = result?.GradeLetter ?? "";
            if (result == null)
                ws.Cell(row, 13).Value = "";
            else
            {
                decimal? theoryMarks = decimal.TryParse(result.ObtainedMarksTheory, out var t) ? t : null;
                decimal? practicalMarks = decimal.TryParse(result.ObtainedMarksPractical, out var p) ? p : null;
                ws.Cell(row, 13).Value = gradeCalculationService.IsStudentPassing(theoryMarks, practicalMarks, subjectOffering) ? "Pass" : "Fail";
            }
            ws.Cell(row, 14).Value = result?.IsSubmitted == true ? "Yes" : "No";

            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
