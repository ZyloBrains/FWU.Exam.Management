using FWU.Exam.Management.Domain.Entities.Exams;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FWU.Exam.Management.Web.Helpers;

public static class AdmitCardPdfExporter
{
    private static readonly string[] Instructions =
    {
        "Students must keep the admit card with them during the entire period of the examination.",
        "Students should take the examination of the subjects mentioned in the admit card.",
        "Students are not allowed to leave the examination hall without the permission of invigilator or superintendent.",
        "Student will not be allowed to leave the examination hall before an hour after the distribution of question paper.",
        "Students will not be allowed to enter the examination hall after half an hour of the distribution of the question paper.",
        "Student must take their seats according to the seat plan.",
        "Students should not take any prohibited materials in examination hall, invigilator/superintendent may take-action against such students who violate the examination rules.",
        "Students should follow the instructions given in the answer sheet.",
        "Students, who are unable to write themselves, must inform the superintendent an hour before the examination so that proper arrangement can be made to facilitate them.",
        "Cell Phones, Laptops, i-pod, iPhone or other electronic devices (excepts calculator without memory) are not allowed to use in the examination hall."
    };

    public static byte[] Build(AdmitCard card, IWebHostEnvironment environment)
    {
        var logoBytes = TryReadImage(environment, "~/images/download.png");
        var photoBytes = TryReadImage(environment, card.PhotoPath);
        var signatureBytes = TryReadImage(environment, card.SignaturePath);
        var controllerSignatureBytes = TryReadImage(environment, card.ControllerSignaturePath);

        var fullName = string.Join(" ",
            new[] { card.StudentRegistration?.FirstName, card.StudentRegistration?.MiddleName, card.StudentRegistration?.LastName }
                .Where(part => !string.IsNullOrWhiteSpace(part))).Trim();
        if (string.IsNullOrWhiteSpace(fullName)) fullName = "-";

        var leftItems = new[]
        {
            ("Symbol No", card.ExamRegistration?.SymbolNumber ?? "-"),
            ("Full Name", fullName),
            ("Campus", card.Campus ?? card.ExamRegistration?.College?.Name ?? "-"),
            ("Level", card.ExamSchedule?.Level?.LevelName ?? card.ExamSchedule?.Program?.Level?.LevelName ?? card.Level ?? "-"),
            ("Program", card.ExamSchedule?.Program?.ProgramName ?? card.Program ?? card.ExamRegistration?.Program?.ProgramName ?? "-")
        };

        var rightItems = new[]
        {
            ("Regd. No", card.RegistrationNumber ?? card.StudentRegistration?.RegistrationNumber ?? "-"),
            ("Semester", card.ExamSchedule?.SemesterInstance?.Semester?.Name ?? card.Semester ?? "-"),
            ("Exam Type", card.ExamSchedule?.ExamType?.Name ?? card.ExamType ?? "-"),
            ("Year", card.ExamSchedule?.SemesterInstance?.AcademicYear?.AcademicYearCode ?? card.Year ?? "-")
        };

        var subjects = card.Subjects ?? [];

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Content().Column(col =>
                {
                    col.Item().Row(headerRow =>
                    {
                        headerRow.ConstantItem(150).Height(95).Element(header =>
                        {
                            header.AlignCenter().AlignMiddle()
                                .Element(box =>
                                {
                                    if (logoBytes != null)
                                        box.Image(logoBytes).FitArea();
                                });
                        });

                        headerRow.RelativeItem().AlignMiddle().Column(center =>
                        {
                            center.Item().AlignCenter().Text("Far Western University")
                                .FontSize(18).Bold().FontColor("#1a2a4a");
                            center.Item().PaddingTop(3).AlignCenter().Text("Office of the Controller of Examinations")
                                .FontSize(11).SemiBold().FontColor("#2c3e6b");
                            center.Item().AlignCenter().Text("Mahendranagar, Kanchanpur")
                                .FontSize(10).FontColor("#444444");
                            center.Item().PaddingTop(5).AlignCenter().Text("Admit Card")
                                .FontSize(16).Bold().FontColor("#1a2a4a");
                        });

                        headerRow.ConstantItem(90).Height(105).Element(photo =>
                        {
                            photo.Border(2, "#1a2a4a").Background("#f8fafc")
                                .AlignCenter().AlignMiddle()
                                .Element(box =>
                                {
                                    if (photoBytes != null)
                                        box.Image(photoBytes).FitArea();
                                    else
                                        box.Column(inner =>
                                        {
                                            inner.Item().AlignCenter().Text("Photo").FontSize(11).Bold().FontColor("#1a2a4a");
                                        });
                                });
                        });
                    });

                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#1a2a4a");

                    col.Item().PaddingTop(6).Border(1, "#d0d7e2").Background("#f8fafc")
                        .Padding(6)
                        .Table(infoTable =>
                        {
                            infoTable.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            infoTable.Cell().Column(left => FillInfoItems(left, leftItems));
                            infoTable.Cell().Column(right => FillInfoItems(right, rightItems));
                        });

                    col.Item().PaddingTop(8).Table(subjectsTable =>
                    {
                        subjectsTable.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(28);
                            columns.ConstantColumn(58);
                            columns.RelativeColumn(2.2f);
                            columns.ConstantColumn(34);
                            columns.ConstantColumn(42);
                            columns.RelativeColumn(1);
                        });

                        subjectsTable.Header(header =>
                        {
                            header.Cell().Background("#1a2a4a").Border(1, "#1a2a4a").Padding(4).Text("S.N.").FontColor(Colors.White).Bold().FontSize(9).AlignCenter();
                            header.Cell().Background("#1a2a4a").Border(1, "#1a2a4a").Padding(4).Text("Subject Code").FontColor(Colors.White).Bold().FontSize(9).AlignCenter();
                            header.Cell().Background("#1a2a4a").Border(1, "#1a2a4a").Padding(4).Text("Subject Name").FontColor(Colors.White).Bold().FontSize(9).AlignCenter();
                            header.Cell().Background("#1a2a4a").Border(1, "#1a2a4a").Padding(4).Text("Theory").FontColor(Colors.White).Bold().FontSize(9).AlignCenter();
                            header.Cell().Background("#1a2a4a").Border(1, "#1a2a4a").Padding(4).Text("Practical").FontColor(Colors.White).Bold().FontSize(9).AlignCenter();
                            header.Cell().Background("#1a2a4a").Border(1, "#1a2a4a").Padding(4).Text("Remarks").FontColor(Colors.White).Bold().FontSize(9).AlignCenter();
                        });

                        if (subjects.Count == 0)
                        {
                            subjectsTable.Cell().ColumnSpan(6).Border(1, "#b0b8c5").Padding(6).Text("No subjects").FontSize(9).AlignCenter();
                        }
                        else
                        {
                            var sn = 1;
                            foreach (var sub in subjects)
                            {
                                var serial = sn;
                                subjectsTable.Cell().Border(1, "#b0b8c5").Padding(4).Text(serial.ToString()).FontSize(9).AlignCenter();
                                subjectsTable.Cell().Border(1, "#b0b8c5").Padding(4).Text(sub.Code ?? "-").FontSize(9).AlignCenter();
                                subjectsTable.Cell().Border(1, "#b0b8c5").Padding(4).Text(sub.Name ?? "-").FontSize(9).AlignLeft();
                                subjectsTable.Cell().Border(1, "#b0b8c5").Padding(4).Text(sub.Theory ? "Y" : "\u2014").FontSize(9).AlignCenter();
                                subjectsTable.Cell().Border(1, "#b0b8c5").Padding(4).Text(sub.Practical ? "Y" : "\u2014").FontSize(9).AlignCenter();
                                subjectsTable.Cell().Border(1, "#b0b8c5").Padding(4).Text(sub.Remarks ?? "").FontSize(9).AlignCenter();
                                sn++;
                            }
                        }
                    });

                    col.Item().PaddingTop(8).Column(rules =>
                    {
                        rules.Item().LineHorizontal(2).LineColor("#1a2a4a");
                        rules.Item().PaddingTop(5).AlignCenter().Text("Important Instructions")
                            .FontSize(13).Bold().FontColor("#1a2a4a");
                        for (var i = 0; i < Instructions.Length; i++)
                        {
                            var number = i + 1;
                            rules.Item().PaddingTop(2).Text(text =>
                            {
                                text.Span($"{number}. ").Style(TextStyle.Default.SemiBold().FontColor("#1a2a4a"));
                                text.Span(Instructions[i]);
                            });
                        }
                    });

                    col.Item().PaddingTop(8).LineHorizontal(2).LineColor("#1a2a4a");

                    col.Item().PaddingTop(6).Row(sigRow =>
                    {
                        sigRow.RelativeItem().AlignCenter().Column(left =>
                        {
                            if (signatureBytes != null)
                                left.Item().Height(28).AlignCenter().Image(signatureBytes).FitHeight();
                            else
                                left.Item().Height(28);
                            left.Item().Width(150).AlignCenter().LineHorizontal(1).LineColor("#333333");
                            left.Item().Width(150).AlignCenter().PaddingTop(2)
                                .Text("Full Signature of Applicant").FontSize(10).SemiBold().FontColor("#1a2a4a").AlignCenter();
                        });
                        sigRow.RelativeItem().AlignCenter().Column(right =>
                        {
                            if (controllerSignatureBytes != null)
                                right.Item().Height(28).AlignCenter().Image(controllerSignatureBytes).FitHeight();
                            else
                                right.Item().Height(28);
                            right.Item().Width(150).AlignCenter().LineHorizontal(1).LineColor("#333333");
                            right.Item().Width(150).AlignCenter().PaddingTop(2)
                                .Text("Signature of Controller").FontSize(10).SemiBold().FontColor("#1a2a4a").AlignCenter();
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public static string SuggestFileName(AdmitCard card)
    {
        var baseName = string.IsNullOrWhiteSpace(card.AdmitCardNumber) ? $"AdmitCard_{card.Id}" : card.AdmitCardNumber;
        var cleaned = new string((baseName ?? string.Empty).Where(c => char.IsLetterOrDigit(c) || c is '-' or '_' or ' ').ToArray());
        cleaned = string.IsNullOrWhiteSpace(cleaned) ? $"AdmitCard_{card.Id}" : cleaned.Trim().Replace(' ', '_');
        return $"{cleaned}.pdf";
    }

    private static void FillInfoItems(ColumnDescriptor column, IEnumerable<(string Label, string Value)> items)
    {
        foreach (var item in items)
        {
            column.Item().PaddingVertical(1.5f).Row(row =>
            {
                row.ConstantItem(92).Text(item.Label).FontSize(9.5f).Bold().FontColor("#1a2a4a");
                row.RelativeItem().Text(item.Value).FontSize(9.5f).FontColor("#111111");
            });
        }
    }

    private static byte[]? TryReadImage(IWebHostEnvironment environment, string? webPath)
    {
        if (string.IsNullOrWhiteSpace(webPath)) return null;

        var normalized = webPath.Trim().TrimStart('~', '/');
        var fullPath = Path.Combine(environment.WebRootPath, normalized);

        if (!File.Exists(fullPath)) return null;

        try
        {
            return File.ReadAllBytes(fullPath);
        }
        catch (Exception)
        {
            return null;
        }
    }
}