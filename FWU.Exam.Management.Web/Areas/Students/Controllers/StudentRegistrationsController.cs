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

using Microsoft.AspNetCore.Authorization;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin,DepartmentAdmin")]
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

    private async Task<int?> GetUserDepartmentIdAsync()
    {
        var user = await userManager.GetUserAsync(User);
        return user?.DepartmentId;
    }

    public async Task<IActionResult> Index(int? departmentId = null)
    {
        var collegeIds = await GetUserCollegeIdsAsync();
        var userDeptId = await GetUserDepartmentIdAsync();

        if (User.IsInRole(Role.DepartmentAdmin) && userDeptId.HasValue)
        {
            departmentId = userDeptId.Value;
        }

        if (departmentId.HasValue)
        {
            var deptRegistrations = await studentRegistrationService.GetAllStudentRegistrationsAsync(collegeIds.Count > 0 ? collegeIds : null);
            deptRegistrations = deptRegistrations.Where(r => r.DepartmentId == departmentId.Value).ToList();
            return View(deptRegistrations);
        }

        var studentRegistrations = await studentRegistrationService.GetAllStudentRegistrationsAsync(collegeIds.Count > 0 ? collegeIds : null);
        return View(studentRegistrations);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var studentRegistration = await studentRegistrationService.GetStudentRegistrationByIdAsync(id.Value);
        if (studentRegistration == null) return NotFound();

        ViewBag.Qualifications = await studentRegistrationService.GetQualificationsByRegistrationAsync(id.Value);
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
    public async Task<IActionResult> Create([Bind("LevelId,DepartmentId,FacultyId,CollegeId,ProgramId,RegistrationNumber,FirstName,MiddleName,LastName,NepaliName,ContactNumber,Phone,Email,DateOfBirthBS,DateOfBirthAD,GenderId,IndexGroupId,BloodGroup,Nationality,Religion,IsActive,StudentRegistrationIndex,StudentCategoryId,VerifiedBy,VerifiedDate,PhotoAttachmentId,EthnicityId,EntranceRollNumber,EntryFormatId,IsRegistrationNumberGenerated,RowIndex,PreviousAcademicYear,PreviousSymbolNumber,StudentRegistrationSearchId,AcademicYearId,SemesterId")] StudentRegistration studentRegistration)
    {
        var permanentLocalLevelId = Request.Form["LocalLevelId"].ToString();
        var permanentWardNumber = Request.Form["WardNumber"].ToString();
        var permanentToleStreet = Request.Form["ToleStreet"].ToString();
        var permanentHouseNumber = Request.Form["HouseNumber"].ToString();

        if (ModelState.IsValid)
        {
            var registrationId = await studentRegistrationService.CreateStudentRegistrationAsync(studentRegistration, permanentLocalLevelId, permanentWardNumber, permanentToleStreet, permanentHouseNumber);
            await SaveQualificationsFromFormAsync(registrationId);
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

        var selectLists = await studentRegistrationService.GetSelectListDataAsync(studentRegistration);
        PopulateSelectLists(selectLists, studentRegistration);
        return View(studentRegistration);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LevelId,DepartmentId,FacultyId,CollegeId,ProgramId,RegistrationNumber,FirstName,MiddleName,LastName,NepaliName,ContactNumber,Phone,Email,DateOfBirthBS,DateOfBirthAD,GenderId,IndexGroupId,BloodGroup,Nationality,Religion,IsActive,StudentRegistrationIndex,StudentCategoryId,VerifiedBy,VerifiedDate,PhotoAttachmentId,EthnicityId,EntranceRollNumber,EntryFormatId,IsRegistrationNumberGenerated,RowIndex,PreviousAcademicYear,PreviousSymbolNumber,StudentRegistrationSearchId,AcademicYearId,SemesterId,PermanentAddressId")] StudentRegistration studentRegistration)
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
        var userDeptId = await GetUserDepartmentIdAsync();
        var (data, totalCount) = await studentRegistrationService.GetPagedDataAsync(searchTerm, page, pageSize, collegeIds.Count > 0 ? collegeIds : null);

        if (User.IsInRole(Role.DepartmentAdmin) && userDeptId.HasValue)
        {
            data = data.Where(d => d.DepartmentId == userDeptId.Value).ToList();
            totalCount = data.Count;
        }

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

                    int successCount = 0;
                    var errors = new List<string>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var registration = new StudentRegistration
                            {
                                FirstName = worksheet.Cell(row, 1).GetString(),
                                LastName = worksheet.Cell(row, 2).GetString(),
                                MiddleName = worksheet.Cell(row, 3).GetString(),
                                Email = worksheet.Cell(row, 4).GetString(),
                                ContactNumber = worksheet.Cell(row, 5).GetString(),
                                DateOfBirthBS = worksheet.Cell(row, 6).GetString(),
                                RegistrationNumber = worksheet.Cell(row, 7).GetString(),
                                AcademicYearId = int.TryParse(worksheet.Cell(row, 8).GetString(), out var ayId) ? ayId : 0,
                                LevelId = int.TryParse(worksheet.Cell(row, 9).GetString(), out var levelId) ? levelId : 0,
                                CollegeId = int.TryParse(worksheet.Cell(row, 10).GetString(), out var collId) ? collId : 0,
                                DepartmentId = int.TryParse(worksheet.Cell(row, 11).GetString(), out var facId) ? facId : 0,
                                GenderId = int.TryParse(worksheet.Cell(row, 12).GetString(), out var genderId) ? genderId : 0,
                                StudentCategoryId = int.TryParse(worksheet.Cell(row, 13).GetString(), out var catId) ? catId : 0,
                                FacultyId = int.TryParse(worksheet.Cell(row, 15).GetString(), out var facultyIdVal) ? facultyIdVal : null,
                                ProgramId = int.TryParse(worksheet.Cell(row, 16).GetString(), out var progId) ? progId : null,
                                IsActive = true
                            };

                            if (registration.AcademicYearId == 0 || registration.LevelId == 0 ||
                                registration.CollegeId == 0 || registration.DepartmentId == 0)
                            {
                                errors.Add($"Row {row}: Missing required IDs (AcademicYear, Level, College, or Faculty)");
                                continue;
                            }

                            await studentRegistrationService.CreateStudentRegistrationAsync(registration, null, null, null, null);
                            successCount++;
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
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error processing file: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(string searchTerm = "")
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
            worksheet.Cell(1, 11).Value = "DepartmentId";
            worksheet.Cell(1, 12).Value = "GenderId";
            worksheet.Cell(1, 13).Value = "StudentCategoryId";
            worksheet.Cell(1, 14).Value = "Active";
            worksheet.Cell(1, 15).Value = "FacultyId";
            worksheet.Cell(1, 16).Value = "ProgramId";

            for (int col = 1; col <= 16; col++)
            {
                var cell = worksheet.Cell(1, col);
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            int row = 2;
            foreach (var reg in data)
            {
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
                worksheet.Cell(row, 11).Value = reg.DepartmentId;
                worksheet.Cell(row, 12).Value = reg.GenderId;
                worksheet.Cell(row, 13).Value = reg.StudentCategoryId;
                worksheet.Cell(row, 14).Value = reg.IsActive ? "Yes" : "No";
                worksheet.Cell(row, 15).Value = reg.FacultyId;
                worksheet.Cell(row, 16).Value = reg.ProgramId;
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

    private void PopulateSelectLists(StudentRegistrationSelectListsDto selectLists, StudentRegistration? studentRegistration = null)
    {
        var provinces = studentRegistrationService.GetProvinces();
        ViewBag.Provinces = new SelectList(provinces, "Id", "ProvinceName");

        ViewBag.AcademicYearId = new SelectList(selectLists.AcademicYears, "Id", "Name", studentRegistration?.AcademicYearId);
        ViewBag.LevelId = new SelectList(selectLists.Levels, "Id", "Name", studentRegistration?.LevelId);
        ViewBag.DepartmentId = new SelectList(selectLists.Departments, "Id", "Name", studentRegistration?.DepartmentId);
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
    public async Task<JsonResult> GetDepartmentsByCollege(int collegeId)
    {
        var departments = await studentRegistrationService.GetDepartmentsByCollegeAsync(collegeId);
        return Json(departments);
    }

    [HttpGet]
    public async Task<JsonResult> GetProgramsByCollege(int collegeId, int? levelId = null, int? departmentId = null)
    {
        var programs = await studentRegistrationService.GetProgramsByCollegeAsync(collegeId, levelId, departmentId);
        return Json(programs);
    }
}
