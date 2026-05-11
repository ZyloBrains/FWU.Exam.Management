using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Students;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
public class StudentRegistrationsController(IStudentRegistrationService studentRegistrationService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var studentRegistrations = await studentRegistrationService.GetAllStudentRegistrationsAsync();
        return View(studentRegistrations);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var studentRegistration = await studentRegistrationService.GetStudentRegistrationByIdAsync(id.Value);
        if (studentRegistration == null) return NotFound();

        return View(studentRegistration);
    }

    public async Task<IActionResult> Create()
    {
        var selectLists = await studentRegistrationService.GetSelectListDataAsync();
        PopulateSelectLists(selectLists, null);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("LevelId,FacultyId,CollegeId,RegistrationNumber,FirstName,MiddleName,LastName,NepaliName,ContactNumber,Phone,Email,DateOfBirthBs,DateOfBirthAd,GenderId,IndexGroupId,BloodGroup,Nationality,Religion,IsActive,StudentRegistrationIndex,StudentCategoryId,VerifiedBy,VerifiedDate,PhotoAttachmentId,EthnicityId,EntranceRollNumber,EntryFormatId,IsRegistrationNumberGenerated,RowIndex,PreviousAcademicYear,PreviousSymbolNumber,StudentRegistrationSearchId,AcademicYearId,SemesterId")] StudentRegistration studentRegistration)
    {
        var permanentLocalLevelId = Request.Form["LocalLevelId"].ToString();
        var permanentWardNumber = Request.Form["WardNumber"].ToString();
        var permanentToleStreet = Request.Form["ToleStreet"].ToString();
        var permanentHouseNumber = Request.Form["HouseNumber"].ToString();

        if (ModelState.IsValid)
        {
            await studentRegistrationService.CreateStudentRegistrationAsync(studentRegistration, permanentLocalLevelId, permanentWardNumber, permanentToleStreet, permanentHouseNumber);
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

        var selectLists = await studentRegistrationService.GetSelectListDataAsync(studentRegistration);
        PopulateSelectLists(selectLists, studentRegistration);
        return View(studentRegistration);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LevelId,FacultyId,CollegeId,RegistrationNumber,FirstName,MiddleName,LastName,NepaliName,ContactNumber,Phone,Email,DateOfBirthBs,DateOfBirthAd,GenderId,IndexGroupId,BloodGroup,Nationality,Religion,IsActive,StudentRegistrationIndex,StudentCategoryId,VerifiedBy,VerifiedDate,PhotoAttachmentId,EthnicityId,EntranceRollNumber,EntryFormatId,IsRegistrationNumberGenerated,RowIndex,PreviousAcademicYear,PreviousSymbolNumber,StudentRegistrationSearchId,AcademicYearId,SemesterId,PermanentAddressId")] StudentRegistration studentRegistration)
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
        var (data, totalCount) = await studentRegistrationService.GetPagedDataAsync(searchTerm, page, pageSize);
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
            return BadRequest("No file uploaded");

        try
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;

                    if (rowCount < 2)
                        return BadRequest("Excel file is empty");

                    int successCount = 0;
                    var errors = new List<string>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var registration = new StudentRegistration
                            {
                                FirstName = worksheet.Cells[row, 1].Value?.ToString() ?? "",
                                LastName = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                                MiddleName = worksheet.Cells[row, 3].Value?.ToString(),
                                Email = worksheet.Cells[row, 4].Value?.ToString(),
                                ContactNumber = worksheet.Cells[row, 5].Value?.ToString(),
                                DateOfBirthBS = worksheet.Cells[row, 6].Value?.ToString() ?? "",
                                RegistrationNumber = worksheet.Cells[row, 7].Value?.ToString(),
                                AcademicYearId = int.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out var ayId) ? ayId : 0,
                                CollegeId = int.TryParse(worksheet.Cells[row, 10].Value?.ToString(), out var collId) ? collId : 0,
                                FacultyId = int.TryParse(worksheet.Cells[row, 11].Value?.ToString(), out var facId) ? facId : 0,
                                GenderId = int.TryParse(worksheet.Cells[row, 12].Value?.ToString(), out var genderId) ? genderId : 0,
                                StudentCategoryId = int.TryParse(worksheet.Cells[row, 13].Value?.ToString(), out var catId) ? catId : 0,
                                IsActive = true
                            };

                            if (registration.AcademicYearId == 0 || registration.LevelId == 0 ||
                                registration.CollegeId == 0 || registration.FacultyId == 0)
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

                    return Ok(new { message = $"Imported {successCount} records successfully", errors });
                }
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Error processing file: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(string searchTerm = "")
    {
        var data = await studentRegistrationService.GetAllStudentRegistrationsAsync();

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Student Registrations");

            worksheet.Cells[1, 1].Value = "Registration No";
            worksheet.Cells[1, 2].Value = "First Name";
            worksheet.Cells[1, 3].Value = "Middle Name";
            worksheet.Cells[1, 4].Value = "Last Name";
            worksheet.Cells[1, 5].Value = "Email";
            worksheet.Cells[1, 6].Value = "Contact Number";
            worksheet.Cells[1, 7].Value = "Date of Birth (BS)";
            worksheet.Cells[1, 8].Value = "Academic Year";
            worksheet.Cells[1, 9].Value = "Level";
            worksheet.Cells[1, 10].Value = "College";
            worksheet.Cells[1, 11].Value = "Faculty";
            worksheet.Cells[1, 12].Value = "Category";
            worksheet.Cells[1, 13].Value = "Active";

            for (int col = 1; col <= 13; col++)
            {
                worksheet.Cells[1, col].Style.Font.Bold = true;
                worksheet.Cells[1, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int row = 2;
            foreach (var reg in data)
            {
                worksheet.Cells[row, 1].Value = reg.RegistrationNumber;
                worksheet.Cells[row, 2].Value = reg.FirstName;
                worksheet.Cells[row, 3].Value = reg.MiddleName;
                worksheet.Cells[row, 4].Value = reg.LastName;
                worksheet.Cells[row, 5].Value = reg.Email;
                worksheet.Cells[row, 6].Value = reg.ContactNumber;
                worksheet.Cells[row, 7].Value = reg.DateOfBirthBS;
                worksheet.Cells[row, 8].Value = reg.AcademicYear?.AcademicYearName;
                worksheet.Cells[row, 9].Value = reg.Level?.LevelName;
                worksheet.Cells[row, 10].Value = reg.College?.Name;
                worksheet.Cells[row, 11].Value = reg.Faculty?.FacultyName;
                worksheet.Cells[row, 12].Value = reg.StudentCategory?.StudentCategoryName;
                worksheet.Cells[row, 13].Value = reg.IsActive ? "Yes" : "No";
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var fileBytes = package.GetAsByteArray();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"StudentRegistrations_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
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

    private void PopulateSelectLists(StudentRegistrationSelectListsDto selectLists, StudentRegistration? studentRegistration = null)
    {
        var provinces = studentRegistrationService.GetProvinces();
        ViewBag.Provinces = new SelectList(provinces, "Id", "ProvinceName");
        ViewBag.AcademicYearId = new SelectList(selectLists.AcademicYears, "Id", "Name", studentRegistration?.AcademicYearId);
        ViewBag.LevelId = new SelectList(selectLists.Levels, "Id", "Name", studentRegistration?.LevelId);
        ViewBag.CollegeId = new SelectList(selectLists.Colleges, "Id", "Name", studentRegistration?.CollegeId);
        ViewBag.FacultyId = new SelectList(selectLists.Faculties, "Id", "Name", studentRegistration?.FacultyId);
        ViewBag.GenderId = new SelectList(selectLists.Genders, "Id", "Name", studentRegistration?.GenderId);
        ViewBag.StudentCategoryId = new SelectList(selectLists.StudentCategories, "Id", "Name", studentRegistration?.StudentCategoryId);
        ViewBag.EthnicityId = new SelectList(selectLists.Ethnicities, "Id", "Name", studentRegistration?.EthnicityId);
    }
}
