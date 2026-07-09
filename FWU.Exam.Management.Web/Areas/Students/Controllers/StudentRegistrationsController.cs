using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.Data.SqlClient;

using Microsoft.AspNetCore.Authorization;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin")]
public class StudentRegistrationsController(IStudentRegistrationService studentRegistrationService, UserManager<AppUser> userManager, AppDbContext context, IFileUploadHelper fileUploadHelper) : Controller
{
    private async Task<List<int>> GetUserCollegeIdsAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return new List<int>();

        if (User.IsInRole(Role.SuperAdmin))
            return new List<int>();

        if (User.IsInRole(Role.FacultyAdmin) && user.FacultyId != null)
        {
            return await context.Colleges
                .Where(c => c.Faculties.Any(f => f.Id == user.FacultyId))
                .Select(c => c.Id)
                .ToListAsync();
        }

        if (User.IsInRole(Role.CollegeAdmin) && user.CollegeId != null)
        {
            return new List<int> { user.CollegeId.Value };
        }

        return new List<int>();
    }

    public async Task<IActionResult> Index()
    {
        var collegeIds = await GetUserCollegeIdsAsync();
        var studentRegistrations = await studentRegistrationService.GetAllStudentRegistrationsAsync(collegeIds.Count > 0 ? collegeIds : null);
        return View(studentRegistrations);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var studentRegistration = await studentRegistrationService.GetStudentRegistrationByIdAsync(id.Value);
        if (studentRegistration == null) return NotFound();

        ViewBag.Qualifications = await studentRegistrationService.GetQualificationsByRegistrationAsync(id.Value);
        ViewBag.Guardian = await studentRegistrationService.GetGuardianByRegistrationAsync(id.Value);
        return View(studentRegistration);
    }

    public async Task<IActionResult> Create()
    {
        var selectLists = await studentRegistrationService.GetSelectListDataAsync();
        PopulateSelectLists(selectLists, null);
        return View(new StudentRegistration());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("LevelId,FacultyId,CollegeId,ProgramId,RegistrationNumber,FirstName,MiddleName,LastName,ContactNumber,Email,DateOfBirthBS,DateOfBirthAD,GenderId,Nationality,Religion,StudentCategoryId,VerifiedBy,VerifiedDate,EthnicityId,AcademicYearId")] StudentRegistration studentRegistration)
    {
        var permanentLocalLevelId = Request.Form["LocalLevelId"].ToString();
        var permanentWardNumber = Request.Form["WardNumber"].ToString();
        var permanentToleStreet = Request.Form["ToleStreet"].ToString();
        var permanentHouseNumber = Request.Form["HouseNumber"].ToString();

        if (ModelState.IsValid)
        {
            var registrationId = await studentRegistrationService.CreateStudentRegistrationAsync(studentRegistration, permanentLocalLevelId, permanentWardNumber, permanentToleStreet, permanentHouseNumber);
            await SaveQualificationsFromFormAsync(registrationId);
            await SaveGuardiansFromFormAsync(registrationId);
            TempData["SuccessMessage"] = "Student registration created successfully!";
            return RedirectToAction(nameof(Index));
        }

        var selectLists = await studentRegistrationService.GetSelectListDataAsync();
        PopulateSelectLists(selectLists, studentRegistration);
        return View(studentRegistration);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var studentRegistration = await studentRegistrationService.GetStudentRegistrationByIdAsync(id.Value);
        if (studentRegistration == null) return NotFound();

        ViewBag.Qualifications = await studentRegistrationService.GetQualificationsByRegistrationAsync(id.Value);
        ViewBag.Guardian = await studentRegistrationService.GetGuardianByRegistrationAsync(id.Value);

        if (studentRegistration.PermanentAddress?.LocalLevelId != null)
        {
            var localLevel = await context.LocalLevels
                .Include(ll => ll.District)
                .FirstOrDefaultAsync(ll => ll.Id == studentRegistration.PermanentAddress.LocalLevelId);
            if (localLevel?.District != null)
            {
                ViewBag.ExistingProvinceId = localLevel.District.ProvinceId;
                ViewBag.ExistingDistrictId = localLevel.District.Id;
                ViewBag.ExistingLocalLevelId = localLevel.Id;
                ViewBag.ExistingWardNumber = studentRegistration.PermanentAddress.WardNumber;
                ViewBag.ExistingToleStreet = studentRegistration.PermanentAddress.ToleStreet;
                ViewBag.ExistingHouseNumber = studentRegistration.PermanentAddress.HouseNumber;
            }
        }

        var selectLists = await studentRegistrationService.GetSelectListDataAsync(studentRegistration);
        PopulateSelectLists(selectLists, studentRegistration);
        return View(studentRegistration);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LevelId,FacultyId,CollegeId,ProgramId,RegistrationNumber,FirstName,MiddleName,LastName,ContactNumber,Email,DateOfBirthBS,DateOfBirthAD,GenderId,Nationality,Religion,IsActive,StudentCategoryId,VerifiedBy,VerifiedDate,EthnicityId,AcademicYearId,PermanentAddressId")] StudentRegistration studentRegistration)
    {
        if (id != studentRegistration.Id) return NotFound();

        var permanentLocalLevelId = Request.Form["LocalLevelId"].ToString();
        var permanentWardNumber = Request.Form["WardNumber"].ToString();
        var permanentToleStreet = Request.Form["ToleStreet"].ToString();
        var permanentHouseNumber = Request.Form["HouseNumber"].ToString();

        if (ModelState.IsValid)
        {
            try
            {
                await studentRegistrationService.UpdateStudentRegistrationAsync(studentRegistration, permanentLocalLevelId, permanentWardNumber, permanentToleStreet, permanentHouseNumber);
                await SaveQualificationsFromFormAsync(id);
                await SaveGuardiansFromFormAsync(id);
                TempData["SuccessMessage"] = "Student registration updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await studentRegistrationService.StudentRegistrationExistsAsync(studentRegistration.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Qualifications = await studentRegistrationService.GetQualificationsByRegistrationAsync(id);
        var selectLists = await studentRegistrationService.GetSelectListDataAsync(studentRegistration);
        PopulateSelectLists(selectLists, studentRegistration);
        return View(studentRegistration);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var studentRegistration = await studentRegistrationService.GetStudentRegistrationByIdAsync(id.Value);
        if (studentRegistration == null) return NotFound();

        return View(studentRegistration);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await studentRegistrationService.DeleteStudentRegistrationAsync(id);
        TempData["SuccessMessage"] = "Student registration deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedData(string searchTerm = "", int page = 1, int pageSize = 10)
    {
        var collegeIds = await GetUserCollegeIdsAsync();
        var (data, totalCount) = await studentRegistrationService.GetPagedDataAsync(searchTerm, page, pageSize, collegeIds.Count > 0 ? collegeIds : null);

        return Json(new { data, totalCount });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        await studentRegistrationService.UpdateStatusAsync(id, status == "Approved");
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ImportExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "No file uploaded";
            return RedirectToAction(nameof(Index));
        }

        var fileExtension = Path.GetExtension(file.FileName);
        if (!string.Equals(fileExtension, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Please upload an Excel file in .xlsx format.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                    if (rowCount < 2)
                    {
                        TempData["ErrorMessage"] = "Excel file is empty";
                        return RedirectToAction(nameof(Index));
                    }

                    // Load lookup dictionaries for name-to-ID resolution
                    var academicYearLookup = await context.AcademicYears!
                        .Where(a => a.IsActive)
                        .Select(a => new { a.Id, a.AcademicYearName, a.AcademicYearCode })
                        .ToListAsync();
                    var ayMap = BuildLookup(academicYearLookup, a => a.AcademicYearName, a => a.AcademicYearCode, a => a.Id);

                    var levelLookup = await context.Levels!
                        .Where(l => l.IsActive)
                        .Select(l => new { l.Id, l.LevelName, l.LevelCode })
                        .ToListAsync();
                    var levelMap = BuildLookup(levelLookup, l => l.LevelName, l => l.LevelCode, l => l.Id);

                    var collegeLookup = await context.Colleges!
                        .Where(c => c.IsActive)
                        .Select(c => new { c.Id, c.Name, c.Code })
                        .ToListAsync();
                    var collegeMap = BuildLookup(collegeLookup, c => c.Name, c => c.Code, c => c.Id);

                    var facultyLookup = await context.Faculties!
                        .Select(f => new { f.Id, f.Name, f.OfficeCode })
                        .ToListAsync();
                    var facultyMap = BuildLookup(facultyLookup, f => f.Name, f => f.OfficeCode, f => f.Id);

                    var genderLookup = await context.Genders!
                        .Select(g => new { g.Id, g.GenderName })
                        .ToListAsync();
                    var genderMap = genderLookup
                        .Where(g => !string.IsNullOrEmpty(g.GenderName))
                        .DistinctBy(g => g.GenderName!.Trim().ToLowerInvariant())
                        .ToDictionary(g => g.GenderName!.Trim(), g => g.Id, StringComparer.OrdinalIgnoreCase);

                    var categoryLookup = await context.StudentCategories!
                        .Where(c => c.IsActive)
                        .Select(c => new { c.Id, c.StudentCategoryName })
                        .ToListAsync();
                    var categoryMap = categoryLookup
                        .Where(c => !string.IsNullOrEmpty(c.StudentCategoryName))
                        .DistinctBy(c => c.StudentCategoryName!.Trim().ToLowerInvariant())
                        .ToDictionary(c => c.StudentCategoryName!.Trim(), c => c.Id, StringComparer.OrdinalIgnoreCase);

                    int successCount = 0;
                    var errors = new List<string>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var rawAy = worksheet.Cell(row, 8).GetString();
                            var rawLevel = worksheet.Cell(row, 9).GetString();
                            var rawCollege = worksheet.Cell(row, 10).GetString();
                            var ayId = ResolveId(rawAy, ayMap);
                            var levelId = ResolveId(rawLevel, levelMap);
                            var collegeId = ResolveId(rawCollege, collegeMap);

                            if (ayId == 0)
                            {
                                errors.Add($"Row {row}: AcademicYear '{rawAy}' not found. Available: {string.Join(", ", ayMap.Keys)}");
                                continue;
                            }
                            if (levelId == 0)
                            {
                                errors.Add($"Row {row}: Level '{rawLevel}' not found. Available: {string.Join(", ", levelMap.Keys)}");
                                continue;
                            }
                            if (collegeId == 0)
                            {
                                errors.Add($"Row {row}: College '{rawCollege}' not found. Available: {string.Join(", ", collegeMap.Keys)}");
                                continue;
                            }

                            var rawEmail = worksheet.Cell(row, 4).GetString();
                            if (string.IsNullOrWhiteSpace(rawEmail))
                            {
                                var firstName = worksheet.Cell(row, 1).GetString();
                                var lastName = worksheet.Cell(row, 2).GetString();
                                var regNum = worksheet.Cell(row, 7).GetString();
                                if (!string.IsNullOrWhiteSpace(firstName))
                                    rawEmail = $"{firstName}.{lastName}@fwu.edu.np".ToLowerInvariant();
                                else if (!string.IsNullOrWhiteSpace(regNum))
                                    rawEmail = $"{regNum}@fwu.edu.np";
                                else
                                    rawEmail = $"student@fwu.edu.np";
                            }

                            // Ensure generated email is unique by appending a suffix if needed
                            var finalEmail = rawEmail;
                            var suffix = 1;
                            while (await context.StudentRegistrations.AnyAsync(s => s.Email == finalEmail))
                            {
                                var atIndex = rawEmail.IndexOf('@');
                                finalEmail = atIndex > 0 ? $"{rawEmail[..atIndex]}{suffix}{rawEmail[atIndex..]}" : $"{rawEmail}{suffix}";
                                suffix++;
                            }

                            var registration = new StudentRegistration
                            {
                                FirstName = worksheet.Cell(row, 1).GetString(),
                                LastName = worksheet.Cell(row, 2).GetString(),
                                MiddleName = worksheet.Cell(row, 3).GetString(),
                                Email = finalEmail,
                                ContactNumber = worksheet.Cell(row, 5).GetString(),
                                DateOfBirthBS = worksheet.Cell(row, 6).GetString(),
                                RegistrationNumber = worksheet.Cell(row, 7).GetString(),
                                AcademicYearId = ayId,
                                LevelId = levelId,
                                CollegeId = collegeId,
                                GenderId = ResolveId(worksheet.Cell(row, 11).GetString(), genderMap),
                                StudentCategoryId = ResolveId(worksheet.Cell(row, 12).GetString(), categoryMap),
                                IsActive = worksheet.Cell(row, 13).GetString() is string activeStr && bool.TryParse(activeStr, out var isActive) ? isActive : true,
                                FacultyId = ResolveNullableId(worksheet.Cell(row, 14).GetString(), facultyMap),
                                ProgramId = int.TryParse(worksheet.Cell(row, 15).GetString(), out var progId) ? progId : null
                            };

                            if (registration.GenderId == 0)
                            {
                                errors.Add($"Row {row}: Gender not found. Use: Male, Female, or Other");
                                continue;
                            }

                            if (registration.StudentCategoryId == 0)
                            {
                                errors.Add($"Row {row}: StudentCategory not found. Check your category name.");
                                continue;
                            }

                            // Address columns 16-19
                            var permanentLocalLevelId = worksheet.Cell(row, 16).GetString();
                            var permanentWardNumber = worksheet.Cell(row, 17).GetString();
                            var permanentToleStreet = worksheet.Cell(row, 18).GetString();
                            var permanentHouseNumber = worksheet.Cell(row, 19).GetString();

                            var registrationId = await studentRegistrationService.CreateStudentRegistrationAsync(
                                registration, permanentLocalLevelId, permanentWardNumber, permanentToleStreet, permanentHouseNumber);

                            // Guardian columns 20-27
                            var fatherFirstName = worksheet.Cell(row, 20).GetString();
                            var fatherLastName = worksheet.Cell(row, 21).GetString();
                            var fatherOccupation = worksheet.Cell(row, 22).GetString();
                            var fatherPhone = worksheet.Cell(row, 23).GetString();
                            var motherFirstName = worksheet.Cell(row, 24).GetString();
                            var motherLastName = worksheet.Cell(row, 25).GetString();
                            var motherOccupation = worksheet.Cell(row, 26).GetString();
                            var motherPhone = worksheet.Cell(row, 27).GetString();

                            if (!string.IsNullOrWhiteSpace(fatherFirstName) || !string.IsNullOrWhiteSpace(motherFirstName))
                            {
                                var guardian = new StudentGuardian
                                {
                                    FatherName = $"{fatherFirstName} {fatherLastName}".Trim(),
                                    FatherProfession = fatherOccupation,
                                    FatherContactNumber = fatherPhone,
                                    MotherName = $"{motherFirstName} {motherLastName}".Trim(),
                                    MotherProfession = motherOccupation,
                                    MotherContactNumber = motherPhone,
                                    GuardianName = $"{fatherFirstName} {fatherLastName}".Trim(),
                                    RelationWithStudent = "Father"
                                };
                                await studentRegistrationService.SaveGuardiansAsync(registrationId, guardian);
                            }

                            // Qualification columns — 3 sets (export starts at col 28)
                            var qualifications = new List<StudentQualification>();
                            AddQualificationFromRow(worksheet, row, 28, qualifications);
                            AddQualificationFromRow(worksheet, row, 34, qualifications);
                            AddQualificationFromRow(worksheet, row, 40, qualifications);

                            if (qualifications.Count > 0)
                                await studentRegistrationService.SaveQualificationsAsync(registrationId, qualifications);

                            successCount++;
                        }
                        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
                        {
                            errors.Add($"Row {row}: Foreign key error — one of the referenced IDs (College, Level, Gender, etc.) does not match existing records.");
                        }
                        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
                        {
                            errors.Add($"Row {row}: Duplicate value (Email or RegistrationNumber already exists).");
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Row {row}: {ex.Message}");
                        }
                    }

                    if (successCount > 0)
                    {
                        TempData["SuccessMessage"] = $"Imported {successCount} records successfully";
                    }

                    if (errors.Count > 0)
                    {
                        TempData["ErrorMessage"] = string.Join(Environment.NewLine, errors);
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
        {
            TempData["ErrorMessage"] = "A foreign key constraint was violated. Check that all referenced values (College, Level, Gender, etc.) exist in the system.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error processing file: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    private static Dictionary<string, int> BuildLookup<T>(IEnumerable<T> items, Func<T, string?> nameSelector, Func<T, string?> codeSelector, Func<T, int> idSelector)
    {
        var lookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            var name = nameSelector(item);
            if (!string.IsNullOrEmpty(name) && !lookup.ContainsKey(name.Trim()))
                lookup[name.Trim()] = idSelector(item);
            var code = codeSelector(item);
            if (!string.IsNullOrEmpty(code) && !lookup.ContainsKey(code.Trim()))
                lookup[code.Trim()] = idSelector(item);
        }
        return lookup;
    }

    private static int ResolveId(string cellValue, Dictionary<string, int> lookup)
    {
        if (!string.IsNullOrWhiteSpace(cellValue) && lookup.TryGetValue(cellValue.Trim(), out var resolvedId))
            return resolvedId;
        if (int.TryParse(cellValue, out var id) && lookup.ContainsValue(id))
            return id;
        return 0;
    }

    private static int? ResolveNullableId(string cellValue, Dictionary<string, int> lookup)
    {
        if (!string.IsNullOrWhiteSpace(cellValue) && lookup.TryGetValue(cellValue.Trim(), out var resolvedId))
            return resolvedId;
        if (int.TryParse(cellValue, out var id) && lookup.ContainsValue(id))
            return id;
        return null;
    }

    private static void AddQualificationFromRow(IXLWorksheet worksheet, int row, int startCol, List<StudentQualification> qualifications)
    {
        // startCol = PreviousLevelId, startCol+1 = BoardId, startCol+2 = InstituteName,
        // startCol+3 = PassedYear, startCol+4 = Percentage, startCol+5 = ExamRollNumber
        var levelIdStr = worksheet.Cell(row, startCol).GetString();
        var boardIdStr = worksheet.Cell(row, startCol + 1).GetString();

        if (string.IsNullOrWhiteSpace(levelIdStr) || string.IsNullOrWhiteSpace(boardIdStr))
            return;

        if (!int.TryParse(levelIdStr, out var previousLevelId) || previousLevelId == 0)
            return;
        if (!int.TryParse(boardIdStr, out var boardId) || boardId == 0)
            return;

        var instituteName = worksheet.Cell(row, startCol + 2).GetString();
        var passedYear = worksheet.Cell(row, startCol + 3).GetString();
        var percentageStr = worksheet.Cell(row, startCol + 4).GetString();
        var examRollNumber = worksheet.Cell(row, startCol + 5).GetString();

        qualifications.Add(new StudentQualification
        {
            PreviousLevelId = previousLevelId,
            BoardId = boardId,
            InstituteName = string.IsNullOrWhiteSpace(instituteName) ? null : instituteName,
            PassedYear = string.IsNullOrWhiteSpace(passedYear) ? null : passedYear,
            Percentage = decimal.TryParse(percentageStr, out var pct) ? pct : null,
            ExamRollNumber = string.IsNullOrWhiteSpace(examRollNumber) ? null : examRollNumber,
            IsHigherDegree = false,
            IsActive = true
        });
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(string searchTerm = "")
    {
        var collegeIds = await GetUserCollegeIdsAsync();
        var data = await studentRegistrationService.GetAllStudentRegistrationsAsync(collegeIds.Count > 0 ? collegeIds : null);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Student Registrations");

            worksheet.Cell(1, 1).Value = "FirstName";
            worksheet.Cell(1, 2).Value = "LastName";
            worksheet.Cell(1, 3).Value = "MiddleName";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "ContactNumber";
            worksheet.Cell(1, 6).Value = "DateOfBirthBS";
            worksheet.Cell(1, 7).Value = "RegistrationNumber";
            worksheet.Cell(1, 8).Value = "AcademicYearId";
            worksheet.Cell(1, 9).Value = "LevelId";
            worksheet.Cell(1, 10).Value = "CollegeId";
            worksheet.Cell(1, 11).Value = "GenderId";
            worksheet.Cell(1, 12).Value = "StudentCategoryId";
            worksheet.Cell(1, 13).Value = "Active";
            worksheet.Cell(1, 14).Value = "FacultyId";
            worksheet.Cell(1, 15).Value = "ProgramId";
            worksheet.Cell(1, 16).Value = "PermanentLocalLevelId";
            worksheet.Cell(1, 17).Value = "PermanentWardNumber";
            worksheet.Cell(1, 18).Value = "PermanentToleStreet";
            worksheet.Cell(1, 19).Value = "PermanentHouseNumber";
            worksheet.Cell(1, 20).Value = "FatherFirstName";
            worksheet.Cell(1, 21).Value = "FatherLastName";
            worksheet.Cell(1, 22).Value = "FatherOccupation";
            worksheet.Cell(1, 23).Value = "FatherPhone";
            worksheet.Cell(1, 24).Value = "MotherFirstName";
            worksheet.Cell(1, 25).Value = "MotherLastName";
            worksheet.Cell(1, 26).Value = "MotherOccupation";
            worksheet.Cell(1, 27).Value = "MotherPhone";
            worksheet.Cell(1, 28).Value = "PreviousLevelId_1";
            worksheet.Cell(1, 29).Value = "BoardId_1";
            worksheet.Cell(1, 30).Value = "InstituteName_1";
            worksheet.Cell(1, 31).Value = "PassedYear_1";
            worksheet.Cell(1, 32).Value = "Percentage_1";
            worksheet.Cell(1, 33).Value = "ExamRollNumber_1";
            worksheet.Cell(1, 34).Value = "PreviousLevelId_2";
            worksheet.Cell(1, 35).Value = "BoardId_2";
            worksheet.Cell(1, 36).Value = "InstituteName_2";
            worksheet.Cell(1, 37).Value = "PassedYear_2";
            worksheet.Cell(1, 38).Value = "Percentage_2";
            worksheet.Cell(1, 39).Value = "ExamRollNumber_2";
            worksheet.Cell(1, 40).Value = "PreviousLevelId_3";
            worksheet.Cell(1, 41).Value = "BoardId_3";
            worksheet.Cell(1, 42).Value = "InstituteName_3";
            worksheet.Cell(1, 43).Value = "PassedYear_3";
            worksheet.Cell(1, 44).Value = "Percentage_3";
            worksheet.Cell(1, 45).Value = "ExamRollNumber_3";

            for (int col = 1; col <= 45; col++)
            {
                var cell = worksheet.Cell(1, col);
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            int row = 2;
            foreach (var reg in data)
            {
                var guardian = reg.StudentGuardians?.FirstOrDefault();
                var quals = reg.StudentQualifications?.ToList();

                worksheet.Cell(row, 1).Value = reg.FirstName;
                worksheet.Cell(row, 2).Value = reg.LastName;
                worksheet.Cell(row, 3).Value = reg.MiddleName;
                worksheet.Cell(row, 4).Value = reg.Email;
                worksheet.Cell(row, 5).Value = reg.ContactNumber;
                worksheet.Cell(row, 6).Value = reg.DateOfBirthBS;
                worksheet.Cell(row, 7).Value = reg.RegistrationNumber;
                worksheet.Cell(row, 8).Value = reg.AcademicYearId;
                worksheet.Cell(row, 9).Value = reg.LevelId;
                worksheet.Cell(row, 10).Value = reg.CollegeId;
                worksheet.Cell(row, 11).Value = reg.GenderId;
                worksheet.Cell(row, 12).Value = reg.StudentCategoryId;
                worksheet.Cell(row, 13).Value = reg.IsActive ? "Yes" : "No";
                worksheet.Cell(row, 14).Value = reg.FacultyId;
                worksheet.Cell(row, 15).Value = reg.ProgramId;
                worksheet.Cell(row, 16).Value = reg.PermanentAddress?.LocalLevelId;
                worksheet.Cell(row, 17).Value = reg.PermanentAddress?.WardNumber;
                worksheet.Cell(row, 18).Value = reg.PermanentAddress?.ToleStreet;
                worksheet.Cell(row, 19).Value = reg.PermanentAddress?.HouseNumber;
                // Guardian cols 20-27 (import reads separate First/Last cols, entity stores combined — export combined versions)
                worksheet.Cell(row, 20).Value = guardian?.FatherName;
                worksheet.Cell(row, 21).Value = (string?)null;
                worksheet.Cell(row, 22).Value = guardian?.FatherProfession;
                worksheet.Cell(row, 23).Value = guardian?.FatherContactNumber;
                worksheet.Cell(row, 24).Value = guardian?.MotherName;
                worksheet.Cell(row, 25).Value = (string?)null;
                worksheet.Cell(row, 26).Value = guardian?.MotherProfession;
                worksheet.Cell(row, 27).Value = guardian?.MotherContactNumber;
                // Qualification 1 cols 28-33
                worksheet.Cell(row, 28).Value = (quals != null && quals.Count > 0) ? quals[0].PreviousLevelId.ToString() : null;
                worksheet.Cell(row, 29).Value = (quals != null && quals.Count > 0) ? quals[0].BoardId.ToString() : null;
                worksheet.Cell(row, 30).Value = (quals != null && quals.Count > 0) ? quals[0].InstituteName : null;
                worksheet.Cell(row, 31).Value = (quals != null && quals.Count > 0) ? quals[0].PassedYear : null;
                worksheet.Cell(row, 32).Value = (quals != null && quals.Count > 0) ? quals[0].Percentage?.ToString() : null;
                worksheet.Cell(row, 33).Value = (quals != null && quals.Count > 0) ? quals[0].ExamRollNumber : null;
                // Qualification 2 cols 34-39
                worksheet.Cell(row, 34).Value = (quals != null && quals.Count > 1) ? quals[1].PreviousLevelId.ToString() : null;
                worksheet.Cell(row, 35).Value = (quals != null && quals.Count > 1) ? quals[1].BoardId.ToString() : null;
                worksheet.Cell(row, 36).Value = (quals != null && quals.Count > 1) ? quals[1].InstituteName : null;
                worksheet.Cell(row, 37).Value = (quals != null && quals.Count > 1) ? quals[1].PassedYear : null;
                worksheet.Cell(row, 38).Value = (quals != null && quals.Count > 1) ? quals[1].Percentage?.ToString() : null;
                worksheet.Cell(row, 39).Value = (quals != null && quals.Count > 1) ? quals[1].ExamRollNumber : null;
                // Qualification 3 cols 40-45
                worksheet.Cell(row, 40).Value = (quals != null && quals.Count > 2) ? quals[2].PreviousLevelId.ToString() : null;
                worksheet.Cell(row, 41).Value = (quals != null && quals.Count > 2) ? quals[2].BoardId.ToString() : null;
                worksheet.Cell(row, 42).Value = (quals != null && quals.Count > 2) ? quals[2].InstituteName : null;
                worksheet.Cell(row, 43).Value = (quals != null && quals.Count > 2) ? quals[2].PassedYear : null;
                worksheet.Cell(row, 44).Value = (quals != null && quals.Count > 2) ? quals[2].Percentage?.ToString() : null;
                worksheet.Cell(row, 45).Value = (quals != null && quals.Count > 2) ? quals[2].ExamRollNumber : null;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var fileBytes = stream.ToArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"StudentRegistrations_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
        }
    }

    [HttpGet]
    public async Task<JsonResult> GetDistrictsByProvince(int provinceId)
    {
        var districts = await studentRegistrationService.GetDistrictsByProvinceAsync(provinceId);
        return Json(districts);
    }

    [HttpGet]
    public async Task<JsonResult> GetLocalLevelsByDistrict(int districtId)
    {
        var localLevels = await studentRegistrationService.GetLocalLevelsByDistrictAsync(districtId);
        return Json(localLevels);
    }

    [HttpGet]
    public JsonResult ConvertBsToAd(string bsDate)
    {
        if (string.IsNullOrWhiteSpace(bsDate) || bsDate.Length != 10)
            return Json(new { adDate = "" });

        var parts = bsDate.Split('-');
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out var year) ||
            !int.TryParse(parts[1], out var month) ||
            !int.TryParse(parts[2], out var day))
            return Json(new { adDate = "" });

        var adDate = NepaliCalendarHelper.BsToAd(year, month, day);
        if (adDate == null)
            return Json(new { adDate = "" });

        return Json(new { adDate = adDate.Value.ToString("yyyy-MM-dd") });
    }

    [HttpGet]
    public JsonResult ConvertAdToBs(string adDate)
    {
        if (string.IsNullOrWhiteSpace(adDate))
            return Json(new { bsDate = "" });

        if (!DateTime.TryParse(adDate, out var parsed))
            return Json(new { bsDate = "" });

        var (year, month, day) = NepaliCalendarHelper.AdToBs(parsed);
        if (year == 0)
            return Json(new { bsDate = "" });

        return Json(new { bsDate = $"{year:D4}-{month:D2}-{day:D2}" });
    }

    private async Task SaveQualificationsFromFormAsync(int registrationId)
    {
        var previousLevelIds = Request.Form["Qualifications.PreviousLevelId"];
        var boardIds = Request.Form["Qualifications.BoardId"];
        var instituteNames = Request.Form["Qualifications.InstituteName"];
        var passedYears = Request.Form["Qualifications.PassedYear"];
        var percentages = Request.Form["Qualifications.Percentage"];
        var examRollNumbers = Request.Form["Qualifications.ExamRollNumber"];

        if (previousLevelIds.Count == 0) return;

        var qualifications = new List<StudentQualification>();
        var files = Request.Form.Files;

        for (int i = 0; i < previousLevelIds.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(previousLevelIds[i])) continue;

            var q = new StudentQualification
            {
                PreviousLevelId = int.Parse(previousLevelIds[i]),
                BoardId = int.Parse(boardIds[i]),
                InstituteName = instituteNames[i],
                PassedYear = passedYears[i],
                Percentage = decimal.TryParse(percentages[i], out var pct) ? pct : null,
                ExamRollNumber = examRollNumbers[i],
                IsHigherDegree = false,
                IsActive = true
            };

            var file = files.FirstOrDefault(f => f.Name == $"Qualifications.DocumentFile_{i}");
            if (file != null && file.Length > 0)
            {
                q.DocumentPath = await fileUploadHelper.UploadAsync(file, "documents");
            }

            qualifications.Add(q);
        }

        if (qualifications.Count > 0)
        {
            await studentRegistrationService.SaveQualificationsAsync(registrationId, qualifications);
        }
    }

    private async Task SaveGuardiansFromFormAsync(int registrationId)
    {
        var fatherFirstName = Request.Form["FatherFirstName"].ToString();
        var fatherLastName = Request.Form["FatherLastName"].ToString();
        var fatherOccupation = Request.Form["FatherOccupation"].ToString();
        var fatherPhone = Request.Form["FatherPhone"].ToString();
        var motherFirstName = Request.Form["MotherFirstName"].ToString();
        var motherLastName = Request.Form["MotherLastName"].ToString();
        var motherOccupation = Request.Form["MotherOccupation"].ToString();
        var motherPhone = Request.Form["MotherPhone"].ToString();

        if (string.IsNullOrWhiteSpace(fatherFirstName) && string.IsNullOrWhiteSpace(motherFirstName))
        {
            await studentRegistrationService.SaveGuardiansAsync(registrationId, null);
            return;
        }

        var guardian = new StudentGuardian
        {
            FatherName = $"{fatherFirstName} {fatherLastName}".Trim(),
            FatherProfession = fatherOccupation,
            FatherContactNumber = fatherPhone,
            MotherName = $"{motherFirstName} {motherLastName}".Trim(),
            MotherProfession = motherOccupation,
            MotherContactNumber = motherPhone,
            GuardianName = $"{fatherFirstName} {fatherLastName}".Trim(),
            RelationWithStudent = "Father"
        };

        await studentRegistrationService.SaveGuardiansAsync(registrationId, guardian);
    }

    private void PopulateSelectLists(StudentRegistrationSelectListsDto selectLists, StudentRegistration? studentRegistration = null)
    {
        var provinces = studentRegistrationService.GetProvinces();
        ViewBag.Provinces = new SelectList(provinces, "Id", "ProvinceName");

        ViewBag.AcademicYearId = new SelectList(selectLists.AcademicYears, "Id", "Name", studentRegistration?.AcademicYearId);
        ViewBag.LevelId = new SelectList(selectLists.Levels, "Id", "Name", studentRegistration?.LevelId);
        ViewBag.CollegeId = new SelectList(selectLists.Colleges, "Id", "Name", studentRegistration?.CollegeId);
        ViewBag.FacultyId = new SelectList(selectLists.Faculties, "Id", "Name", studentRegistration?.FacultyId);
        ViewBag.ProgramId = new SelectList(selectLists.Programs, "Id", "Name", studentRegistration?.ProgramId);
        ViewBag.GenderId = new SelectList(selectLists.Genders, "Id", "Name", studentRegistration?.GenderId);
        ViewBag.StudentCategoryId = new SelectList(selectLists.StudentCategories, "Id", "Name", studentRegistration?.StudentCategoryId);
        ViewBag.EthnicityId = new SelectList(selectLists.Ethnicities, "Id", "Name", studentRegistration?.EthnicityId);
        ViewBag.LocalLevelId = new SelectList(selectLists.LocalLevels, "Id", "Name");
        ViewBag.BoardId = new SelectList(selectLists.Boards, "Id", "Name");
        ViewBag.PreviousLevelId = new SelectList(selectLists.PreviousLevels, "Id", "Name");
    }

    [HttpGet]
    public async Task<JsonResult> GetCollegesByFaculty(int facultyId)
    {
        var colleges = await context.Colleges
            .Where(c => c.Faculties.Any(f => f.Id == facultyId) && c.Name != null)
            .AsNoTracking()
            .Select(c => new SelectOption { Id = c.Id, Name = c.Name })
            .ToListAsync();
        return Json(colleges);
    }

    [HttpGet]
    public async Task<JsonResult> GetFacultiesByLevel(int levelId)
    {
        var faculties = await studentRegistrationService.GetFacultiesByLevelAsync(levelId);
        return Json(faculties);
    }

    [HttpGet]
    public async Task<JsonResult> GetProgramsByCollege(int collegeId, int? levelId = null)
    {
        var programs = await studentRegistrationService.GetProgramsByCollegeAsync(collegeId, levelId);
        return Json(programs);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            await studentRegistrationService.DeleteStudentRegistrationAsync(id);
            return Json(new { success = true, message = "Student registration deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> GenerateRegistrationNumber(int id)
    {
        try
        {
            var regNumber = await studentRegistrationService.GenerateRegistrationNumberAsync(id);
            if (regNumber == null)
                return Json(new { success = false, message = "Student not found." });

            return Json(new { success = true, registrationNumber = regNumber });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
