using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Students;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;


namespace FWU.Exam.Management.Web.Controllers;

public class StudentRegistrationsController : Controller
{
    private readonly IStudentRegistrationService _studentRegistrationService;

    public StudentRegistrationsController(IStudentRegistrationService studentRegistrationService)
    {
        _studentRegistrationService = studentRegistrationService;
    }

    public async Task<IActionResult> Index()
    {
        var studentRegistrations = await _studentRegistrationService.GetAllStudentRegistrationsAsync();
        return View(studentRegistrations);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var studentRegistration = await _studentRegistrationService.GetStudentRegistrationByIdAsync(id.Value);
        if (studentRegistration == null) return NotFound();

        return View(studentRegistration);
    }

    public async Task<IActionResult> Create()
    {
        var selectLists = await _studentRegistrationService.GetSelectListDataAsync();
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
            await _studentRegistrationService.CreateStudentRegistrationAsync(studentRegistration, permanentLocalLevelId, permanentWardNumber, permanentToleStreet, permanentHouseNumber);
            TempData["SuccessMessage"] = "Student registration created successfully!";
            return RedirectToAction(nameof(Index));
        }

        var selectLists = await _studentRegistrationService.GetSelectListDataAsync();
        PopulateSelectLists(selectLists, studentRegistration);
        return View(studentRegistration);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var studentRegistration = await _studentRegistrationService.GetStudentRegistrationByIdAsync(id.Value);
        if (studentRegistration == null) return NotFound();

        var selectLists = await _studentRegistrationService.GetSelectListDataAsync(studentRegistration);
        PopulateSelectLists(selectLists, studentRegistration);
        return View(studentRegistration);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LevelId,FacultyId,CollegeId,RegistrationNumber,FirstName,MiddleName,LastName,NepaliName,ContactNumber,Phone,Email,DateOfBirthBS,DateOfBirthAD,GenderId,IndexGroupId,BloodGroup,Nationality,Religion,IsActive,StudentRegistrationIndex,StudentCategoryId,VerifiedBy,VerifiedDate,PhotoAttachmentId,EthnicityId,EntranceRollNumber,EntryFormatId,IsRegistrationNumberGenerated,RowIndex,PreviousAcademicYear,PreviousSymbolNumber,StudentRegistrationSearchId,AcademicYearId,SemesterId,PermanentAddressId")] StudentRegistration studentRegistration)
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
                await _studentRegistrationService.UpdateStudentRegistrationAsync(studentRegistration, permanentLocalLevelId, permanentWardNumber, permanentToleStreet, permanentHouseNumber);
                TempData["SuccessMessage"] = "Student registration updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _studentRegistrationService.StudentRegistrationExistsAsync(studentRegistration.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        var selectLists = await _studentRegistrationService.GetSelectListDataAsync(studentRegistration);
        PopulateSelectLists(selectLists, studentRegistration);
        return View(studentRegistration);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var studentRegistration = await _studentRegistrationService.GetStudentRegistrationByIdAsync(id.Value);
        if (studentRegistration == null) return NotFound();

        return View(studentRegistration);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _studentRegistrationService.DeleteStudentRegistrationAsync(id);
        TempData["SuccessMessage"] = "Student registration deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedData(string searchTerm = "", int page = 1, int pageSize = 10)
    {
        var (data, totalCount) = await _studentRegistrationService.GetPagedDataAsync(searchTerm, page, pageSize);
        return Json(new { data, totalCount });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        await _studentRegistrationService.UpdateStatusAsync(id, status == "Approved");
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
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

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
                                FirstName = worksheet.Cell(row, 1).GetString(),
                                LastName = worksheet.Cell(row, 2).GetString(),
                                MiddleName = worksheet.Cell(row, 3).GetString(),
                                Email = worksheet.Cell(row, 4).GetString(),
                                ContactNumber = worksheet.Cell(row, 5).GetString(),
                                DateOfBirthBS = worksheet.Cell(row, 6).GetString(),
                                RegistrationNumber = worksheet.Cell(row, 7).GetString(),
                                AcademicYearId = int.TryParse(worksheet.Cell(row, 8).GetString(), out var ayId) ? ayId : 0,
                                CollegeId = int.TryParse(worksheet.Cell(row, 10).GetString(), out var collId) ? collId : 0,
                                FacultyId = int.TryParse(worksheet.Cell(row, 11).GetString(), out var facId) ? facId : 0,
                                GenderId = int.TryParse(worksheet.Cell(row, 12).GetString(), out var genderId) ? genderId : 0,
                                StudentCategoryId = int.TryParse(worksheet.Cell(row, 13).GetString(), out var catId) ? catId : 0,
                                IsActive = true
                            };

                            if (registration.AcademicYearId == 0 || registration.LevelId == 0 ||
                                registration.CollegeId == 0 || registration.FacultyId == 0)
                            {
                                errors.Add($"Row {row}: Missing required IDs (AcademicYear, Level, College, or Faculty)");
                                continue;
                            }

                            await _studentRegistrationService.CreateStudentRegistrationAsync(registration, null, null, null, null);
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
        var data = await _studentRegistrationService.GetAllStudentRegistrationsAsync();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Student Registrations");

            worksheet.Cell(1, 1).Value = "Registration No";
            worksheet.Cell(1, 2).Value = "First Name";
            worksheet.Cell(1, 3).Value = "Middle Name";
            worksheet.Cell(1, 4).Value = "Last Name";
            worksheet.Cell(1, 5).Value = "Email";
            worksheet.Cell(1, 6).Value = "Contact Number";
            worksheet.Cell(1, 7).Value = "Date of Birth (BS)";
            worksheet.Cell(1, 8).Value = "Academic Year";
            worksheet.Cell(1, 9).Value = "Level";
            worksheet.Cell(1, 10).Value = "College";
            worksheet.Cell(1, 11).Value = "Faculty";
            worksheet.Cell(1, 12).Value = "Category";
            worksheet.Cell(1, 13).Value = "Active";

            for (int col = 1; col <= 13; col++)
            {
                var cell = worksheet.Cell(1, col);
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            int row = 2;
            foreach (var reg in data)
            {
                worksheet.Cell(row, 1).Value = reg.RegistrationNumber;
                worksheet.Cell(row, 2).Value = reg.FirstName;
                worksheet.Cell(row, 3).Value = reg.MiddleName;
                worksheet.Cell(row, 4).Value = reg.LastName;
                worksheet.Cell(row, 5).Value = reg.Email;
                worksheet.Cell(row, 6).Value = reg.ContactNumber;
                worksheet.Cell(row, 7).Value = reg.DateOfBirthBS;
                worksheet.Cell(row, 8).Value = reg.AcademicYear?.AcademicYearName;
                worksheet.Cell(row, 9).Value = reg.Level?.LevelName;
                worksheet.Cell(row, 10).Value = reg.College?.Name;
                worksheet.Cell(row, 11).Value = reg.Faculty?.FacultyName;
                worksheet.Cell(row, 12).Value = reg.StudentCategory?.StudentCategoryName;
                worksheet.Cell(row, 13).Value = reg.IsActive ? "Yes" : "No";
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
        var districts = await _studentRegistrationService.GetDistrictsByProvinceAsync(provinceId);
        return Json(districts);
    }

    [HttpGet]
    public async Task<JsonResult> GetLocalLevelsByDistrict(int districtId)
    {
        var localLevels = await _studentRegistrationService.GetLocalLevelsByDistrictAsync(districtId);
        return Json(localLevels);
    }

    private void PopulateSelectLists(StudentRegistrationSelectListsDto selectLists, StudentRegistration? studentRegistration = null)
    {
        var provinces = _studentRegistrationService.GetProvinces();
        ViewBag.Provinces = new SelectList(provinces, "Id", "ProvinceName");


        ViewBag.AcademicYearId = new SelectList(selectLists.AcademicYears, "Id", "Name", studentRegistration?.AcademicYearId);
        ViewBag.LevelId = new SelectList(selectLists.Levels, "Id", "Name", studentRegistration?.LevelId);
        ViewBag.CollegeId = new SelectList(selectLists.Colleges, "Id", "Name", studentRegistration?.CollegeId);
        ViewBag.FacultyId = new SelectList(selectLists.Faculties, "Id", "Name", studentRegistration?.FacultyId);
        ViewBag.GenderId = new SelectList(selectLists.Genders, "Id", "Name", studentRegistration?.GenderId);
        ViewBag.StudentCategoryId = new SelectList(selectLists.StudentCategories, "Id", "Name", studentRegistration?.StudentCategoryId);
        ViewBag.EthnicityId = new SelectList(selectLists.Ethnicities, "Id", "Name", studentRegistration?.EthnicityId);
        ViewBag.LocalLevelId = new SelectList(selectLists.LocalLevels, "Id", "Name");
    }
}
