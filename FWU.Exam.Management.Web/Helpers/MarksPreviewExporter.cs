using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FWU.Exam.Management.Web.Helpers;

public class MarksPreviewRow
{
    public string StudentName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string SymbolNumber { get; set; } = string.Empty;
    public float? Marks { get; set; }

    public string MarksDisplay =>
        Marks.HasValue ? Marks.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "Not entered";

    public static MarksPreviewRow Create(string studentName, string registrationNumber, string symbolNumber, float? marks)
    {
        return new MarksPreviewRow
        {
            StudentName = studentName ?? string.Empty,
            RegistrationNumber = registrationNumber ?? string.Empty,
            SymbolNumber = symbolNumber ?? string.Empty,
            Marks = marks
        };
    }
}

public static class MarksPreviewExporter
{
    private const string SpreadsheetContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string PdfContentType = "application/pdf";

    public static byte[] BuildExcel(string title, string marksColumn, bool showSymbol, IEnumerable<MarksPreviewRow> rows)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Marks Preview");

        ws.Cell(1, 1).Value = title;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        var headers = showSymbol
            ? new[] { "S.N", "Student Name", "Registration Number", "Symbol No.", marksColumn }
            : new[] { "S.N", "Student Name", "Registration Number", marksColumn };
        for (var c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(2, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.Gray;
        }

        var row = 3;
        foreach (var r in rows)
        {
            ws.Cell(row, 1).Value = row - 2;
            ws.Cell(row, 2).Value = r.StudentName;
            ws.Cell(row, 3).Value = r.RegistrationNumber;
            var col = 4;
            if (showSymbol)
            {
                ws.Cell(row, col).Value = r.SymbolNumber;
                col++;
            }
            ws.Cell(row, col).Value = r.MarksDisplay;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static byte[] BuildPdf(string title, string subtitle, string marksColumn, bool showSymbol, IEnumerable<MarksPreviewRow> rows)
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
                    if (!string.IsNullOrWhiteSpace(subtitle))
                        header.Item().PaddingTop(1).AlignCenter().Text(subtitle)
                            .FontSize(9).FontColor(Colors.Grey.Darken2);
                    header.Item().PaddingTop(8).LineHorizontal(1).LineColor("#1a5276");
                });

                page.Content().PaddingTop(14).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(28);
                        columns.RelativeColumn(2.4f);
                        columns.RelativeColumn(2);
                        if (showSymbol) columns.RelativeColumn(1.6f);
                        columns.RelativeColumn(showSymbol ? 1 : 1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("S.N").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Student Name").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Reg. No.").Bold().FontSize(9);
                        if (showSymbol)
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Symbol No.").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text(marksColumn).Bold().FontSize(9);
                    });

                    var serial = 0;
                    foreach (var r in rows)
                    {
                        serial++;
                        table.Cell().Padding(6).Text(serial.ToString()).FontSize(9);
                        table.Cell().Padding(6).Text(r.StudentName.Length > 0 ? r.StudentName : "-").FontSize(9);
                        table.Cell().Padding(6).Text(r.RegistrationNumber.Length > 0 ? r.RegistrationNumber : "-").FontSize(9);
                        if (showSymbol)
                            table.Cell().Padding(6).Text(r.SymbolNumber.Length > 0 ? r.SymbolNumber : "-").FontSize(9);
                        table.Cell().Padding(6).Text(r.MarksDisplay).FontSize(9);
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

    public static string SanitizeFileName(string name)
    {
        var cleaned = (name ?? string.Empty)
            .Replace("'", string.Empty)
            .Replace(":", string.Empty)
            .Replace("/", string.Empty)
            .Replace("\\", string.Empty);
        var result = new string(cleaned.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_' or ' ').ToArray());
        return string.IsNullOrWhiteSpace(result) ? "MarksPreview" : result.Trim().Replace(' ', '_');
    }

    public static (string ContentType, string Extension) ResolveFormat(string? format)
    {
        return string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase)
            ? (PdfContentType, "pdf")
            : (SpreadsheetContentType, "xlsx");
    }
}