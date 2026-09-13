using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Authorization;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("examcenters.view")]
public class SymbolNumberGenerationController(
    ISymbolNumberService symbolNumberService,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int? examScheduleId, int? startSequence, int? sequenceWidth, string? prefix, int[]? academicYearIds)
    {
        ViewData["ExamScheduleId"] = new SelectList(
            await context.ExamSchedules.AsNoTracking().OrderByDescending(es => es.Id).ToListAsync(),
            "Id", "ExamScheduleName", examScheduleId);

        ViewData["SelectedAcademicYearIds"] = academicYearIds;

        if (!examScheduleId.HasValue) return View(null);

        var dto = await symbolNumberService.GetOverviewAsync(examScheduleId.Value, startSequence, sequenceWidth, prefix, academicYearIds);

        ViewData["CollegesFilter"] = new SelectList(
            dto.Students
                .Where(s => s.CollegeId > 0)
                .GroupBy(s => s.CollegeId)
                .Select(g => new SelectListItem
                {
                    Value = g.Key.ToString(),
                    Text = g.First().CollegeName ?? "Unknown College",
                })
                .OrderBy(i => i.Text, StringComparer.OrdinalIgnoreCase),
            "Value", "Text");

        ViewData["AcademicYearsFilter"] = new SelectList(
            dto.Students
                .Where(s => s.AcademicYearId > 0)
                .GroupBy(s => s.AcademicYearId)
                .Select(g => new SelectListItem
                {
                    Value = g.Key.ToString(),
                    Text = g.First().AcademicYearName ?? "Unknown Academic Year",
                })
                .OrderByDescending(i => i.Text, StringComparer.OrdinalIgnoreCase),
            "Value", "Text");

        SymbolNumberService.FilterForAcademicYears(dto, academicYearIds);

        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Export(int examScheduleId, int? collegeId, int? academicYearId, string? prefix, string? format = "excel")
    {
        var dto = await symbolNumberService.GetOverviewAsync(examScheduleId, prefix: prefix);

        var students = dto.Students
            .Where(s => !string.IsNullOrWhiteSpace(s.SymbolNumber))
            .Where(s => !collegeId.HasValue || s.CollegeId == collegeId.Value)
            .Where(s => !academicYearId.HasValue || s.AcademicYearId == academicYearId.Value)
            .ToList();

        var collegePart = collegeId.HasValue
            ? (students.FirstOrDefault()?.CollegeName ?? $"College{collegeId.Value}")
            : "All";
        var yearPart = academicYearId.HasValue
            ? (students.FirstOrDefault()?.AcademicYearName ?? $"Year{academicYearId.Value}")
            : "All";
        var schedulePart = string.IsNullOrWhiteSpace(dto.ExamScheduleName) ? dto.ExamScheduleId.ToString() : dto.ExamScheduleName;
        var baseName = $"SymbolNumbers_{MarksPreviewExporter.SanitizeFileName(schedulePart)}_{MarksPreviewExporter.SanitizeFileName(collegePart)}_{MarksPreviewExporter.SanitizeFileName(yearPart)}";
        var title = string.IsNullOrWhiteSpace(dto.ExamScheduleName)
            ? $"Symbol Numbers - Schedule {dto.ExamScheduleId}"
            : $"Symbol Numbers - {dto.ExamScheduleName}";
        var examTypeName = string.IsNullOrWhiteSpace(dto.ExamTypeName) ? "Regular" : dto.ExamTypeName;

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdfBytes = BuildPdf(title, examTypeName, students);
            return File(pdfBytes, "application/pdf", baseName + ".pdf");
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Symbol Numbers");

        worksheet.Cell(1, 1).Value = title;
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        var headers = new[] { "S.N", "Symbol Number", "Student Name", "Reg. No.", "Program", "College", "Academic Year", "Type" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(2, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.Gray;
        }

        var row = 3;
        foreach (var s in students)
        {
            var type = examTypeName;
            if (s.IsSupplementary) type += " (Supplementary)";

            worksheet.Cell(row, 1).Value = row - 2;
            worksheet.Cell(row, 2).Value = s.SymbolNumber ?? string.Empty;
            worksheet.Cell(row, 3).Value = s.StudentName ?? string.Empty;
            worksheet.Cell(row, 4).Value = s.RegistrationNumber ?? string.Empty;
            worksheet.Cell(row, 5).Value = s.ProgramName ?? string.Empty;
            worksheet.Cell(row, 6).Value = s.CollegeName ?? string.Empty;
            worksheet.Cell(row, 7).Value = s.AcademicYearName ?? string.Empty;
            worksheet.Cell(row, 8).Value = type;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", baseName + ".xlsx");
    }

    private static byte[] BuildPdf(string title, string examTypeName, IEnumerable<StudentSymbolInfo> students)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Column(header =>
                {
                    header.Item().AlignCenter().Text("Far Western University")
                        .FontSize(17).Bold().FontColor("#1a5276");
                    header.Item().PaddingTop(2).AlignCenter().Text(title)
                        .FontSize(12).SemiBold().FontColor(Colors.Grey.Darken3);
                    header.Item().PaddingTop(8).LineHorizontal(1).LineColor("#1a5276");
                });

                page.Content().PaddingTop(14).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(28);
                        columns.RelativeColumn(1.6f);
                        columns.RelativeColumn(2.4f);
                        columns.RelativeColumn(2.2f);
                        columns.RelativeColumn(2.0f);
                        columns.RelativeColumn(2.0f);
                        columns.RelativeColumn(1.6f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("S.N").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Symbol No.").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Student Name").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Reg. No.").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Program").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("College").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Academic Year").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Type").Bold().FontSize(9);
                    });

                    var serial = 0;
                    foreach (var s in students)
                    {
                        serial++;
                        var type = examTypeName;
                        if (s.IsSupplementary) type += " / Supplementary";

                        table.Cell().Padding(6).Text(serial.ToString()).FontSize(9);
                        table.Cell().Padding(6).Text(s.SymbolNumber ?? "-").FontSize(9);
                        table.Cell().Padding(6).Text(string.IsNullOrEmpty(s.StudentName) ? "-" : s.StudentName).FontSize(9);
                        table.Cell().Padding(6).Text(string.IsNullOrEmpty(s.RegistrationNumber) ? "-" : s.RegistrationNumber).FontSize(9);
                        table.Cell().Padding(6).Text(string.IsNullOrEmpty(s.ProgramName) ? "-" : s.ProgramName).FontSize(9);
                        table.Cell().Padding(6).Text(string.IsNullOrEmpty(s.CollegeName) ? "-" : s.CollegeName).FontSize(9);
                        table.Cell().Padding(6).Text(string.IsNullOrEmpty(s.AcademicYearName) ? "-" : s.AcademicYearName).FontSize(9);
                        table.Cell().Padding(6).Text(type).FontSize(9);
                    }
                });

                page.Footer().Column(footer =>
                {
                    footer.Item().AlignCenter().Text("Far Western University - System Generated Report")
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                    footer.Item().PaddingTop(3).AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    [HttpPost]
    [RequirePermission("examcenters.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int examScheduleId, int? startSequence, int? sequenceWidth, string? prefix, int[]? academicYearIds)
    {
        try
        {
            var result = await symbolNumberService.GenerateAsync(examScheduleId, startSequence, sequenceWidth, prefix, academicYearIds);
            TempData["SuccessMessage"] = result.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { examScheduleId, startSequence, sequenceWidth, prefix, academicYearIds });
    }

    [HttpPost]
    [RequirePermission("examcenters.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSymbolNumber(int registrationId, string symbolNumber, int examScheduleId, int[]? academicYearIds)
    {
        try
        {
            await symbolNumberService.UpdateSymbolNumberAsync(registrationId, symbolNumber);
            TempData["SuccessMessage"] = $"Symbol number updated to '{symbolNumber}'.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { examScheduleId, academicYearIds });
    }

    [HttpPost]
    [RequirePermission("examcenters.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnassignSymbolNumber(int registrationId, int examScheduleId, int[]? academicYearIds)
    {
        try
        {
            var removed = await symbolNumberService.UnassignSymbolNumberAsync(registrationId);
            TempData["SuccessMessage"] = $"Symbol number '{removed}' unassigned.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { examScheduleId, academicYearIds });
    }

    [HttpPost]
    [RequirePermission("examcenters.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnassignAll(int examScheduleId, int[]? academicYearIds, int[]? registrationIds)
    {
        try
        {
            var count = await symbolNumberService.UnassignAllSymbolNumbersAsync(examScheduleId, registrationIds ?? []);
            TempData["SuccessMessage"] = count > 0
                ? $"{count} symbol number(s) unassigned."
                : "No symbol numbers were found to unassign.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { examScheduleId, academicYearIds });
    }
}
