using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClosedXML.Excel;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
public class EntranceController(IEntranceExamApplicationService service) : Controller
{

    // --- Public actions (no auth required) ---

    [AllowAnonymous]
    public async Task<IActionResult> Apply()
    {
  

        var selectLists = await service.GetSelectListsAsync();
        PopulateSelectLists(selectLists);
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply([Bind("AcademicYearId,CollegeId,ProgramId,FirstName,MiddleName,LastName,NepaliName,DateOfBirthBS,DateOfBirthAD,GenderId,Email,ContactNumber,Phone,FatherName,FatherContact,MotherName,MotherContact,PreviousSchoolCollege,PreviousLevelId,PreviousPassedYear,PreviousSymbolNumber,PreviousGPA")] EntranceExamApplication application)
    {
        var selectLists = await service.GetSelectListsAsync();

        var permanentLocalLevelId = Request.Form["LocalLevelId"].ToString();
        var permanentWardNumber = Request.Form["WardNumber"].ToString();
        var permanentToleStreet = Request.Form["ToleStreet"].ToString();
        var permanentHouseNumber = Request.Form["HouseNumber"].ToString();

        if (ModelState.IsValid)
        {
            var id = await service.SubmitApplicationAsync(application, permanentLocalLevelId, permanentWardNumber, permanentToleStreet, permanentHouseNumber);
            return RedirectToAction(nameof(Confirmation), new { id });
        }

        PopulateSelectLists(selectLists);
        return View(application);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Confirmation(int id)
    {
        var application = await service.GetApplicationByIdAsync(id);
        if (application == null) return NotFound();
        return View(application);
    }



    [HttpGet]
    [AllowAnonymous]
    public async Task<JsonResult> GetDistrictsByProvince(int provinceId)
    {
        var districts = await service.GetDistrictsByProvinceAsync(provinceId);
        return Json(districts);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<JsonResult> GetLocalLevelsByDistrict(int districtId)
    {
        var localLevels = await service.GetLocalLevelsByDistrictAsync(districtId);
        return Json(localLevels);
    }

    // --- Admin actions ---

    [RequirePermission("entrance.view")]
    public async Task<IActionResult> Index(int page = 1, string search = null, string status = null, int? programId = null, int? academicYearId = null, int pageSize = 10)
    {
        ApplicationStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, out var parsedStatus))
            statusFilter = parsedStatus;

        var (items, totalCount) = await service.GetPagedApplicationsAsync(search, statusFilter, programId, academicYearId, page, pageSize);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.ProgramId = programId;
        ViewBag.AcademicYearId = academicYearId;

        var selectLists = await service.GetSelectListsAsync();
        ViewBag.ProgramIdList = new SelectList(selectLists.Programs, "Id", "Name", programId);
        ViewBag.AcademicYearIdList = new SelectList(selectLists.AcademicYears, "Id", "Name", academicYearId);

        return View(items);
    }

    [RequirePermission("entrance.view")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var application = await service.GetApplicationByIdAsync(id.Value);
        if (application == null) return NotFound();

        return View(application);
    }

    [HttpPost]
    [RequirePermission("entrance.approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        await service.ReviewApplicationAsync(id, ApplicationStatus.Approved, null);
        TempData["SuccessMessage"] = "Application approved successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [RequirePermission("entrance.reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string remarks)
    {
        await service.ReviewApplicationAsync(id, ApplicationStatus.Rejected, remarks);
        TempData["SuccessMessage"] = "Application rejected.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin,DepartmentAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkUnderReview(int id)
    {
        await service.ReviewApplicationAsync(id, ApplicationStatus.UnderReview, null);
        TempData["SuccessMessage"] = "Application marked as under review.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [RequirePermission("entrance.export")]
    public async Task<IActionResult> ExportToExcel(string search = null, string status = null, int? programId = null, int? academicYearId = null)
    {
        ApplicationStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, out var parsedStatus))
            statusFilter = parsedStatus;

        var data = await service.GetAllApplicationsAsync(search, statusFilter, programId, academicYearId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Entrance Applications");

        worksheet.Cell(1, 1).Value = "Application ID";
        worksheet.Cell(1, 2).Value = "Full Name";
        worksheet.Cell(1, 3).Value = "Email";
        worksheet.Cell(1, 4).Value = "Contact Number";
        worksheet.Cell(1, 5).Value = "Gender";
        worksheet.Cell(1, 6).Value = "Academic Year";
        worksheet.Cell(1, 7).Value = "College";
        worksheet.Cell(1, 8).Value = "Program";
        worksheet.Cell(1, 9).Value = "Status";
        worksheet.Cell(1, 10).Value = "Submitted Date";

        var headerRange = worksheet.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach (var item in data)
        {
            worksheet.Cell(row, 1).Value = item.Id;
            worksheet.Cell(row, 2).Value = (item.FirstName + " " + item.LastName).Trim();
            worksheet.Cell(row, 3).Value = item.Email;
            worksheet.Cell(row, 4).Value = item.ContactNumber;
            worksheet.Cell(row, 5).Value = item.Gender?.GenderName;
            worksheet.Cell(row, 6).Value = item.AcademicYear?.AcademicYearName;
            worksheet.Cell(row, 7).Value = item.College?.Name;
            worksheet.Cell(row, 8).Value = item.Program?.ProgramName;
            worksheet.Cell(row, 9).Value = item.Status.ToString();
            worksheet.Cell(row, 10).Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            row++;
        }

        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileBytes = stream.ToArray();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"EntranceApplications_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    // --- Convert approved application to Student Admission ---

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin,DepartmentAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertToAdmission(int id)
    {
        try
        {
            var admissionId = await service.ConvertToAdmissionAsync(id);
            TempData["SuccessMessage"] = "Application converted to student admission successfully!";
            return RedirectToAction("Details", "StudentAdmissions", new { area = "Students", id = admissionId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // --- Entrance Schedule Management (Admin) ---


    private void PopulateSelectLists(EntranceExamApplicationSelectListsDto selectLists)
    {
        var provinces = service.GetProvinces();
        ViewBag.Provinces = new SelectList(provinces, "Id", "ProvinceName");
        ViewBag.AcademicYearId = new SelectList(selectLists.AcademicYears, "Id", "Name");
        ViewBag.CollegeId = new SelectList(selectLists.Colleges, "Id", "Name");
        ViewBag.ProgramId = new SelectList(selectLists.Programs, "Id", "Name");
        ViewBag.GenderId = new SelectList(selectLists.Genders, "Id", "Name");
        ViewBag.PreviousLevelId = new SelectList(selectLists.PreviousLevels, "Id", "Name");
    }
}
