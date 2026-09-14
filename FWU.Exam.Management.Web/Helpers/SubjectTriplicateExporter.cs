using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FWU.Exam.Management.Web.Helpers;

public static class SubjectTriplicateExporter
{
    private const string SpreadsheetContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string PdfContentType = "application/pdf";

    public static byte[] BuildPdf(SubjectTriplicateReportDto report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken3));

                page.Header().Column(header =>
                {
                    header.Item().AlignCenter().Text("Far Western University")
                        .FontSize(16).Bold().FontColor("#1a5276");
                    header.Item().PaddingTop(2).AlignCenter().Text("Subject Triplicate Report")
                        .FontSize(12).SemiBold().FontColor(Colors.Grey.Darken3);
                    header.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text(BuildMetaLine1(report)).FontSize(8.5f);
                        row.RelativeItem().AlignRight().Text(BuildMetaLine2(report)).FontSize(8.5f);
                    });
                    header.Item().PaddingTop(6).LineHorizontal(1).LineColor("#1a5276");
                });

                page.Content().PaddingTop(12).Column(column =>
                {
                    foreach (var group in report.Groups)
                    {
                        column.Item().PaddingTop(8).Background(Colors.Grey.Lighten3).PaddingHorizontal(8).PaddingVertical(5)
                            .Row(row =>
                            {
                                row.RelativeItem().Text($"College: {group.CollegeName ?? "-"}")
                                    .Bold().FontSize(10).FontColor("#1a5276");
                                row.RelativeItem().AlignRight().Text($"{group.TotalStudents} student(s)")
                                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                            });

                        column.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.6f);
                                columns.RelativeColumn(1.6f);
                                columns.RelativeColumn(3);
                                columns.ConstantColumn(48);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#1a5276").Padding(5).Text("S.N").Bold().FontColor(Colors.White).FontSize(8.5f);
                                header.Cell().Background("#1a5276").Padding(5).Text("Symbol No").Bold().FontColor(Colors.White).FontSize(8.5f);
                                header.Cell().Background("#1a5276").Padding(5).Text("Reg No").Bold().FontColor(Colors.White).FontSize(8.5f);
                                header.Cell().Background("#1a5276").Padding(5).Text("Student Name").Bold().FontColor(Colors.White).FontSize(8.5f);
                                header.Cell().Background("#1a5276").Padding(5).Text("Program").Bold().FontColor(Colors.White).FontSize(8.5f);
                                header.Cell().Background("#1a5276").Padding(5).Text("Selected Subjects").Bold().FontColor(Colors.White).FontSize(8.5f);
                                header.Cell().Background("#1a5276").Padding(5).Text("No. of Subjects").Bold().FontColor(Colors.White).FontSize(8.5f);
                            });

                            var serial = 0;
                            foreach (var student in group.Students)
                            {
                                serial++;
                                table.Cell().Padding(5).Text(serial.ToString()).FontSize(8.5f);
                                table.Cell().Padding(5).Text(student.SymbolNumber ?? "-").FontSize(8.5f);
                                table.Cell().Padding(5).Text(student.RegistrationNumber ?? "-").FontSize(8.5f);
                                table.Cell().Padding(5).Text(student.StudentName ?? "-").FontSize(8.5f);
                                table.Cell().Padding(5).Text(student.ProgramName ?? "-").FontSize(8.5f);
                                table.Cell().Padding(5).Text(string.IsNullOrWhiteSpace(student.SubjectsDisplay) ? "-" : student.SubjectsDisplay).FontSize(8.5f);
                                table.Cell().Padding(5).Text(student.SubjectCount.ToString()).FontSize(8.5f);
                            }
                        });
                    }
                });

                page.Footer().Column(footer =>
                {
                    footer.Item().PaddingTop(16).Row(row =>
                    {
                        row.RelativeItem().Text("Prepared By\n\n______________________").FontSize(8.5f);
                        row.RelativeItem().AlignCenter().Text("Checked By\n\n______________________").FontSize(8.5f);
                        row.RelativeItem().AlignRight().Text("Controller of Examinations\n\n______________________").FontSize(8.5f);
                    });
                    footer.Item().PaddingTop(8).AlignCenter().Text("Far Western University - System Generated Report")
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                    footer.Item().PaddingTop(2).AlignCenter().Text(x =>
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

    public static byte[] BuildExcel(SubjectTriplicateReportDto report)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Subject Triplicate");

        var maxSubjects = report.Groups.SelectMany(g => g.Students).Select(s => s.SubjectCount).DefaultIfEmpty(0).Max();
        var totalCols = 7 + maxSubjects;

        ws.Range(1, 1, 1, totalCols).Merge();
        ws.Cell(1, 1).Value = "Far Western University - Subject Triplicate Report";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Row(1).Height = 24;

        ws.Range(2, 1, 2, totalCols).Merge();
        ws.Cell(2, 1).Value = $"College: {report.CollegeName ?? "All"}    |    Exam Schedule: {report.ExamScheduleName ?? "-"}";
        ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        if (!string.IsNullOrWhiteSpace(report.ExamScheduleCode))
        {
            ws.Range(3, 1, 3, totalCols).Merge();
            ws.Cell(3, 1).Value = $"Schedule Code: {report.ExamScheduleCode}";
            ws.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        ws.Range(5, 1, 5, totalCols).Merge();
        ws.Cell(5, 1).Value = $"Academic Year: {report.AcademicYearName ?? "-"} | Level: {report.LevelName ?? "-"} | Semester: {report.SemesterName ?? "-"} | Exam Type: {report.ExamTypeName ?? "-"}";
        ws.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Range(6, 1, 6, totalCols).Merge();
        ws.Cell(6, 1).Value = $"Total Students: {report.TotalStudents}    |    Total Subjects: {report.TotalSubjects}";
        ws.Cell(6, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Range(7, 1, 7, totalCols).Merge();
        ws.Cell(7, 1).Value = $"Generated: {report.GeneratedDate:yyyy-MM-dd}";
        ws.Cell(7, 1).Style.Font.Italic = true;
        ws.Cell(7, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var headers = new List<string> { "S.N", "College", "Symbol No", "Reg No", "Student Name", "Program" };
        for (var i = 1; i <= maxSubjects; i++)
            headers.Add($"Subject {i}");
        headers.Add("No. of Subjects");

        var headerRow = 9;
        for (var c = 0; c < headers.Count; c++)
        {
            var cell = ws.Cell(headerRow, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a5276");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.WrapText = true;
        }

        var row = headerRow + 1;
        var serial = 0;
        foreach (var group in report.Groups)
        {
            foreach (var student in group.Students)
            {
                serial++;
                ws.Cell(row, 1).Value = serial;
                ws.Cell(row, 2).Value = group.CollegeName ?? "-";
                ws.Cell(row, 3).Value = student.SymbolNumber ?? "-";
                ws.Cell(row, 4).Value = student.RegistrationNumber ?? "-";
                ws.Cell(row, 5).Value = student.StudentName ?? "-";
                ws.Cell(row, 6).Value = student.ProgramName ?? "-";
                for (var i = 0; i < student.Subjects.Count; i++)
                    ws.Cell(row, 7 + i).Value = student.Subjects[i].Display;
                ws.Cell(row, 7 + maxSubjects).Value = student.SubjectCount;
                row++;
            }
        }

        ws.Range(headerRow, 1, row - 1, totalCols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range(headerRow, 1, row - 1, totalCols).SetAutoFilter();
        ws.Columns().AdjustToContents();

        var summary = workbook.Worksheets.Add("Subject Summary");

        summary.Range(1, 1, 1, 4).Merge();
        summary.Cell(1, 1).Value = "Far Western University - Subject Summary Report";
        summary.Cell(1, 1).Style.Font.Bold = true;
        summary.Cell(1, 1).Style.Font.FontSize = 14;
        summary.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        summary.Row(1).Height = 24;

        summary.Range(2, 1, 2, 4).Merge();
        summary.Cell(2, 1).Value = $"College: {report.CollegeName ?? "All"}    |    Exam Schedule: {report.ExamScheduleName ?? "-"}";
        summary.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        summary.Range(4, 1, 4, 4).Merge();
        summary.Cell(4, 1).Value = $"Total Students: {report.TotalStudents}";
        summary.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        summary.Range(5, 1, 5, 4).Merge();
        summary.Cell(5, 1).Value = $"Total Subjects: {report.TotalSubjects}";
        summary.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        summary.Range(6, 1, 6, 4).Merge();
        summary.Cell(6, 1).Value = $"Generated: {report.GeneratedDate:yyyy-MM-dd}";
        summary.Cell(6, 1).Style.Font.Italic = true;
        summary.Cell(6, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var summaryHeaders = new[] { "S.N", "Subject Code", "Subject Name", "No. of Students" };
        var summaryHeaderRow = 8;
        for (var c = 0; c < summaryHeaders.Length; c++)
        {
            var cell = summary.Cell(summaryHeaderRow, c + 1);
            cell.Value = summaryHeaders[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a5276");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        var s = summaryHeaderRow + 1;
        var summarySerial = 0;
        foreach (var entry in report.SubjectSummary)
        {
            summarySerial++;
            summary.Cell(s, 1).Value = summarySerial;
            summary.Cell(s, 2).Value = entry.SubjectCode ?? "-";
            summary.Cell(s, 3).Value = entry.SubjectName ?? "-";
            summary.Cell(s, 4).Value = entry.StudentCount;
            s++;
        }

        summary.Range(summaryHeaderRow, 1, s - 1, summaryHeaders.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        summary.Range(summaryHeaderRow, 1, s - 1, summaryHeaders.Length).SetAutoFilter();
        summary.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static (string ContentType, string Extension) ResolveFormat(string? format)
    {
        return string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase)
            ? (PdfContentType, "pdf")
            : (SpreadsheetContentType, "xlsx");
    }

    private static string BuildMetaLine1(SubjectTriplicateReportDto report)
    {
        var college = string.IsNullOrWhiteSpace(report.CollegeName)
            ? "All Colleges"
            : $"College: {report.CollegeName}";
        return $"{college}"
             + $"    |    Academic Year: {report.AcademicYearName ?? "-"}"
             + $"    |    Semester: {report.SemesterName ?? "-"}"
             + $"    |    Level: {report.LevelName ?? "-"}";
    }

    private static string BuildMetaLine2(SubjectTriplicateReportDto report)
    {
        return $"Exam Schedule: {report.ExamScheduleName ?? "-"}"
             + $"    |    Exam Type: {report.ExamTypeName ?? "-"}";
    }
}
