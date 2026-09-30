using System.IO.Compression;
using FWU.Exam.Management.Domain.Entities.Exams;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace FWU.Exam.Management.Web.Helpers;

public static class AdmitCardPdfExporter
{
    private static readonly string[] Rules =
    [
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
    ];

    public static byte[] BuildPdf(AdmitCard card, string webRootPath)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginVertical(15);
                page.MarginHorizontal(20);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.RelativeItem(1).Column(left =>
                        {
                            var logoBytes = GetLogoBytes(webRootPath);
                            if (logoBytes.Length > 0)
                                left.Item().Width(80).Height(55).Image(logoBytes).FitArea();
                        });

                        row.RelativeItem(2).Column(center =>
                        {
                            center.Item().AlignCenter().Text("Far Western University")
                                .FontSize(18).Bold().FontColor("#1a2a4a");
                            center.Item().AlignCenter().Text("Office of the Controller of Examinations")
                                .FontSize(11).SemiBold().FontColor("#2c3e6b");
                            center.Item().AlignCenter().Text("Mahendranagar, Kanchanpur")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                            center.Item().PaddingTop(4).AlignCenter().Text("ADMIT CARD")
                                .FontSize(16).Bold().FontColor("#1a2a4a").Underline();
                        });

                        row.RelativeItem(1).Column(right =>
                        {
                            var photoBytes = GetPhotoBytes(card, webRootPath);
                            if (photoBytes.Length > 0)
                                right.Item().AlignRight().Width(60).Height(70)
                                    .Border(1).BorderColor("#1a2a4a").Image(photoBytes).FitArea();
                        });
                    });

                    header.Item().PaddingTop(6).LineHorizontal(2).LineColor("#1a2a4a");
                });

                page.Content().PaddingTop(8).Column(column =>
                {
                    // Student Info Grid
                    var leftItems = new (string Label, string Value)[]
                    {
                        ("Symbol No", card.ExamRegistration?.SymbolNumber ?? "-"),
                        ("Full Name", GetStudentName(card)),
                        ("Campus", card.Campus ?? card.ExamRegistration?.College?.Name ?? "-"),
                        ("Level", card.ExamSchedule?.Level?.LevelName ?? card.ExamSchedule?.Program?.Level?.LevelName ?? card.Level ?? "-"),
                        ("Program", card.ExamSchedule?.Program?.ProgramName ?? card.Program ?? card.ExamRegistration?.Program?.ProgramName ?? "-")
                    };

                    var rightItems = new (string Label, string Value)[]
                    {
                        ("Regd. No", card.RegistrationNumber ?? card.StudentRegistration?.RegistrationNumber ?? "-"),
                        ("Semester", card.ExamSchedule?.SemesterInstance?.Semester?.Name ?? card.Semester ?? "-"),
                        ("Exam Type", card.ExamSchedule?.ExamType?.Name ?? card.ExamType ?? "-"),
                        ("Year", card.ExamSchedule?.SemesterInstance?.AcademicYear?.AcademicYearCode ?? card.Year ?? "-")
                    };

                    column.Item().Background(Colors.Grey.Lighten5).Border(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(8).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                foreach (var item in leftItems)
                                    AddInfoRow(col, item.Label, item.Value);
                            });

                            row.RelativeItem().Column(col =>
                            {
                                foreach (var item in rightItems)
                                    AddInfoRow(col, item.Label, item.Value);
                            });
                        });

                    column.Item().PaddingTop(6);

                    // Subject Table
                    var subjects = card.Subjects ?? [];
                    if (subjects.Count > 0)
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2.5f);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(50);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#1a2a4a").Padding(4).Text("S.N.").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background("#1a2a4a").Padding(4).Text("Subject Code").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background("#1a2a4a").Padding(4).Text("Subject Name").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background("#1a2a4a").Padding(4).AlignCenter().Text("Theory").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background("#1a2a4a").Padding(4).AlignCenter().Text("Practical").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background("#1a2a4a").Padding(4).Text("Remarks").Bold().FontColor(Colors.White).FontSize(9);
                            });

                            int sn = 1;
                            foreach (var sub in subjects)
                            {
                                table.Cell().Padding(4).Text(sn.ToString()).FontSize(9);
                                table.Cell().Padding(4).Text(sub.Code ?? "-").FontSize(9);
                                table.Cell().Padding(4).Text(sub.Name ?? "-").FontSize(9);
                                table.Cell().Padding(4).AlignCenter().Text(sub.Theory ? "Y" : "-").FontSize(9);
                                table.Cell().Padding(4).AlignCenter().Text(sub.Practical ? "Y" : "-").FontSize(9);
                                table.Cell().Padding(4).Text(sub.Remarks ?? "").FontSize(9);
                                sn++;
                            }
                        });
                    }

                    column.Item().PaddingTop(6);

                    // Rules
                    column.Item().BorderTop(2).BorderColor("#1a2a4a").PaddingTop(4).Column(rulesCol =>
                    {
                        rulesCol.Item().AlignCenter().Text("Important Instructions")
                            .FontSize(11).Bold().FontColor("#1a2a4a");

                        rulesCol.Item().PaddingTop(4).Column(list =>
                        {
                            for (int i = 0; i < Rules.Length; i++)
                            {
                                list.Item().PaddingBottom(2).Row(row =>
                                {
                                    row.ConstantItem(18).Text($"{i + 1}.").FontSize(8.5f).Bold().FontColor("#1a2a4a");
                                    row.RelativeItem().Text(Rules[i]).FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                                });
                            }
                        });
                    });

                    column.Item().PaddingTop(6).BorderTop(2).BorderColor("#1a2a4a").PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            var sigBytes = GetSignatureBytes(card.SignaturePath, webRootPath);
                            if (sigBytes.Length > 0)
                                col.Item().AlignCenter().Width(110).Height(40).Image(sigBytes).FitArea();
                            col.Item().PaddingTop(8).AlignCenter().Text("Full Signature of Applicant").FontSize(9).FontColor("#1a2a4a");
                        });
                        row.RelativeItem().Column(col =>
                        {
                            var ctrlSigBytes = GetSignatureBytes(card.ControllerSignaturePath, webRootPath);
                            if (ctrlSigBytes.Length > 0)
                                col.Item().AlignCenter().Width(110).Height(40).Image(ctrlSigBytes).FitArea();
                            col.Item().PaddingTop(8).AlignCenter().Text("Signature of Controller").FontSize(9).FontColor("#1a2a4a");
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public static byte[] BuildZip(List<AdmitCard> admitCards, string webRootPath)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var card in admitCards)
            {
                var pdfBytes = BuildPdf(card, webRootPath);
                var entryName = SanitizeFileName($"{card.AdmitCardNumber ?? $"Card_{card.Id}"}.pdf");
                var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                using var entryStream = entry.Open();
                entryStream.Write(pdfBytes, 0, pdfBytes.Length);
            }
        }
        return memoryStream.ToArray();
    }

    private static void AddInfoRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().PaddingBottom(2).Row(row =>
        {
            row.ConstantItem(80).Text(label).FontSize(9.5f).Bold().FontColor("#1a2a4a");
            row.ConstantItem(5).Text(":").FontSize(9.5f).FontColor("#1a2a4a");
            row.RelativeItem().Text(value).FontSize(9.5f).FontColor(Colors.Grey.Darken3);
        });
    }

    private static string GetStudentName(AdmitCard card)
    {
        if (card.StudentRegistration == null) return "-";
        return $"{card.StudentRegistration.FirstName} {card.StudentRegistration.MiddleName} {card.StudentRegistration.LastName}".Trim();
    }

    private static byte[] GetLogoBytes(string webRootPath)
    {
        var logoPath = Path.Combine(webRootPath, "images", "download.png");
        if (File.Exists(logoPath))
            return File.ReadAllBytes(logoPath);
        return [];
    }

    private static byte[] GetPhotoBytes(AdmitCard card, string webRootPath)
    {
        if (!string.IsNullOrEmpty(card.PhotoPath))
        {
            var relativePath = card.PhotoPath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
            var photoPath = Path.Combine(webRootPath, relativePath);
            if (File.Exists(photoPath))
                return File.ReadAllBytes(photoPath);
        }
        return [];
    }

    private static byte[] GetSignatureBytes(string? signaturePath, string webRootPath)
    {
        if (!string.IsNullOrEmpty(signaturePath))
        {
            var relativePath = signaturePath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webRootPath, relativePath);
            if (File.Exists(fullPath))
                return File.ReadAllBytes(fullPath);
        }
        return [];
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(c, '_');
        return fileName;
    }
}
