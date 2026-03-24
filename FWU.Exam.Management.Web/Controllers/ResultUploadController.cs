using fwu_examination_management_system.Data;
using fwu_examination_management_system.Models;
using fwu_examination_management_system.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace fwu_examination_management_system.Controllers
{
    [Authorize]
    public class ResultUploadController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResultUploadController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await BuildModelAsync(new ResultUploadViewModel());
            return View(model);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ResultUploadViewModel model)
        {
            model = await BuildModelAsync(model);
            model.HasSubmitted = true;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!model.ExamScheduleId.HasValue)
            {
                ModelState.AddModelError(nameof(model.ExamScheduleId), "Exam Schedule is required.");
                return View(model);
            }

            if (model.UploadFile == null || model.UploadFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.UploadFile), "Please choose a CSV file.");
                return View(model);
            }

            var examSchedule = await _context.ExamSchedules
                .AsNoTracking()
                .Include(x => x.YearPart)
                .FirstOrDefaultAsync(x => x.ExamScheduleId == model.ExamScheduleId.Value);

            if (examSchedule == null)
            {
                ModelState.AddModelError(nameof(model.ExamScheduleId), "Selected Exam Schedule is invalid.");
                return View(model);
            }

            var subjectByCode = await _context.SubjectDetails
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToDictionaryAsync(x => x.SubjectCode.ToUpper(), x => x);

            var examRegistrations = await _context.ExamRegistrations
                .AsNoTracking()
                .Where(x => x.ExamScheduleId == model.ExamScheduleId.Value)
                .Include(x => x.StudentProgramYearPart)
                    .ThenInclude(x => x.StudentAdmission)
                        .ThenInclude(x => x.StudentRegistration)
                .Include(x => x.StudentProgramYearPart)
                    .ThenInclude(x => x.StudentAdmission)
                        .ThenInclude(x => x.Program)
                .ToListAsync();

            var registrationMap = examRegistrations
                .Where(x => !string.IsNullOrWhiteSpace(x.StudentProgramYearPart.StudentAdmission.StudentRegistration.RegistrationNumber))
                .GroupBy(x => x.StudentProgramYearPart.StudentAdmission.StudentRegistration.RegistrationNumber.Trim().ToUpper())
                .ToDictionary(x => x.Key, x => x.First());

            var resultRecordMasterId = (await _context.ResultRecords
                .AsNoTracking()
                .MaxAsync(x => (int?)x.ResultRecordMasterId) ?? 0) + 1;

            await using var stream = model.UploadFile.OpenReadStream();
            using var reader = new StreamReader(stream);

            var header = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(header))
            {
                model.Errors.Add("Uploaded file is empty.");
                model.FailedCount = 1;
                return View(model);
            }

            var lineNo = 1;
            var recordsToInsert = new List<ResultRecord>();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                lineNo++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var cols = ParseCsvLine(line);

                if (cols.Count < 20)
                {
                    model.FailedCount++;
                    model.Errors.Add($"Line {lineNo}: expected 20 columns, found {cols.Count}.");
                    continue;
                }

                var registrationNumber = cols[0].Trim();
                var symbolNumber = cols[1].Trim();
                var subjectCode = cols[2].Trim().ToUpper();

                if (string.IsNullOrWhiteSpace(registrationNumber) || string.IsNullOrWhiteSpace(symbolNumber) || string.IsNullOrWhiteSpace(subjectCode))
                {
                    model.FailedCount++;
                    model.Errors.Add($"Line {lineNo}: RegistrationNumber, SymbolNumber and SubjectCode are required.");
                    continue;
                }

                if (!registrationMap.TryGetValue(registrationNumber.ToUpper(), out var examRegistration))
                {
                    model.FailedCount++;
                    model.Errors.Add($"Line {lineNo}: registration '{registrationNumber}' not found for selected exam schedule.");
                    continue;
                }

                if (!subjectByCode.TryGetValue(subjectCode, out var subject))
                {
                    model.FailedCount++;
                    model.Errors.Add($"Line {lineNo}: subject code '{subjectCode}' not found.");
                    continue;
                }

                var studentReg = examRegistration.StudentProgramYearPart.StudentAdmission.StudentRegistration;
                var program = examRegistration.StudentProgramYearPart.StudentAdmission.Program;

                var record = new ResultRecord
                {
                    AcademicYearId = examSchedule.AcademicYearId,
                    ProgramsId = program.ProgramsId,
                    ExamTypeId = examSchedule.ExamTypeId,
                    CollegeId = examRegistration.CollegeId,
                    SubjectDetailId = subject.SubjectDetailId,
                    Year = Truncate(!string.IsNullOrWhiteSpace(cols[3]) ? cols[3].Trim() : examSchedule.YearPart?.Year.ToString() ?? string.Empty, 3),
                    Part = Truncate(!string.IsNullOrWhiteSpace(cols[4]) ? cols[4].Trim() : examSchedule.YearPart?.Part.ToString() ?? string.Empty, 2),
                    RegistrationNumber = Truncate(registrationNumber, 50),
                    SymbolNumber = Truncate(symbolNumber, 50),
                    Alphabet = Truncate(cols[5].Trim(), 1),
                    DateOfBirthBs = Truncate(!string.IsNullOrWhiteSpace(cols[6]) ? cols[6].Trim() : studentReg.DateOfBirthBs ?? string.Empty, 10),
                    Sex = Truncate(cols[7].Trim(), 10),
                    TheoryObtainedMarks = Truncate(cols[8].Trim(), 5),
                    InternalObtainedMarks = Truncate(cols[9].Trim(), 5),
                    PracticalObtainedMarks = Truncate(cols[10].Trim(), 5),
                    TheoryObtainedGrade = Truncate(cols[11].Trim(), 5),
                    InternalObtainedGrade = Truncate(cols[12].Trim(), 5),
                    PracticalObtainedGrade = Truncate(cols[13].Trim(), 5),
                    TotalObtainedMarks = Truncate(cols[14].Trim(), 5),
                    TotalObtainedGrade = Truncate(cols[15].Trim(), 5),
                    TotalGradePoints = Truncate(cols[16].Trim(), 5),
                    Gpa = Truncate(cols[17].Trim(), 4),
                    Result = Truncate(cols[18].Trim(), 50),
                    StudentName = Truncate(!string.IsNullOrWhiteSpace(cols[19]) ? cols[19].Trim() : string.Join(" ", new[]
                    {
                        studentReg.FirstName,
                        studentReg.MiddleName,
                        studentReg.LastName
                    }.Where(n => !string.IsNullOrWhiteSpace(n))), 255),
                    ResultRecordMasterId = resultRecordMasterId,
                    ExamScheduleId = examSchedule.ExamScheduleId,
                    CreatedDate = DateTime.UtcNow
                };

                EnsureNonNullStrings(record);
                recordsToInsert.Add(record);
            }

            if (recordsToInsert.Count > 0)
            {
                _context.ResultRecords.AddRange(recordsToInsert);
                await _context.SaveChangesAsync();
            }

            model.UploadedCount = recordsToInsert.Count;
            return View(model);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpGet]
        public IActionResult DownloadSample()
        {
            const string sample = "RegistrationNumber,SymbolNumber,SubjectCode,Year,Part,Alphabet,DateOfBirthBs,Sex,TheoryObtainedMarks,InternalObtainedMarks,PracticalObtainedMarks,TheoryObtainedGrade,InternalObtainedGrade,PracticalObtainedGrade,TotalObtainedMarks,TotalObtainedGrade,TotalGradePoints,Gpa,Result,StudentName\n" +
                                  "REG-1001,SY-1001,CSC101,1,1,,2060-01-01,M,75,18,20,A,A+,A,113,A+,3.7,3.70,Pass,Demo Student";

            var bytes = Encoding.UTF8.GetBytes(sample);
            return File(bytes, "text/csv", "result_upload_sample.csv");
        }

        private async Task<ResultUploadViewModel> BuildModelAsync(ResultUploadViewModel model)
        {
            model.ExamSchedules = [new SelectListItem("Select Exam Schedule", "")];
            model.ExamSchedules.AddRange(await _context.ExamSchedules
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new SelectListItem(x.ExamScheduleName, x.ExamScheduleId.ToString()))
                .ToListAsync());

            return model;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];

                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(ch);
                }
            }

            result.Add(current.ToString());
            return result;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Length <= maxLength ? value : value[..maxLength];
        }

        private static void EnsureNonNullStrings(ResultRecord record)
        {
            record.Year ??= string.Empty;
            record.Part ??= string.Empty;
            record.RegistrationNumber ??= string.Empty;
            record.SymbolNumber ??= string.Empty;
            record.Alphabet ??= string.Empty;
            record.DateOfBirthBs ??= string.Empty;
            record.Sex ??= string.Empty;
            record.TheoryObtainedMarks ??= string.Empty;
            record.InternalObtainedMarks ??= string.Empty;
            record.PracticalObtainedMarks ??= string.Empty;
            record.TheoryObtainedGrade ??= string.Empty;
            record.InternalObtainedGrade ??= string.Empty;
            record.PracticalObtainedGrade ??= string.Empty;
            record.TotalObtainedMarks ??= string.Empty;
            record.TotalObtainedGrade ??= string.Empty;
            record.TotalGradePoints ??= string.Empty;
            record.Gpa ??= string.Empty;
            record.Result ??= string.Empty;
            record.StudentName ??= string.Empty;
        }
    }
}
