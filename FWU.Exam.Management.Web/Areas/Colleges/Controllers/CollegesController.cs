using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Text;

namespace FWU.Exam.Management.Web.Areas.Colleges.Controllers;

[Area("Colleges")]
[RequirePermission("colleges.view")]
public class CollegesController(ICollegeService collegeService, UserManager<AppUser> userManager, AppDbContext context) : Controller
{
    private async Task<int?> GetCurrentUserFacultyIdAsync()
    {
        var user = await userManager.GetUserAsync(User);
        return user?.FacultyId;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "DisplayOrder", string sortDir = "asc", int pageSize = 10)
    {
        int? orgId = null;
        if (User.IsInRole(Role.FacultyAdmin))
        {
            orgId = await GetCurrentUserFacultyIdAsync();
        }

        var (items, totalCount) = await collegeService.GetCollegesAsync(page, pageSize, search, sort, sortDir, orgId);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    public async Task<IActionResult> ExportToCsv(string search = null, string sort = "DisplayOrder", string sortDir = "asc")
    {
        int? orgId = User.IsInRole(Role.FacultyAdmin) ? await GetCurrentUserFacultyIdAsync() : null;
        var items = await collegeService.GetFilteredItemsAsync(search, sort, sortDir, orgId);

        var sb = new StringBuilder();
        sb.AppendLine("College Code,College Name,College Name (Nepali),Short Name,District,Municipality/VDC,Ward No.,House No.,Website,Email,Phone 1,Phone 2,Principal Name,Principal Contact,Fax,Remarks,Is Exam Center Only,Is Active,College Type,Allocated Amount,Area,Display Order,Established Date,Closed Date");

        foreach (var c in items)
        {
            sb.AppendLine($"{EscapeCsv(c.Code.ToString())}," +
                          $"{EscapeCsv(c.Name)}," +
                          $"{EscapeCsv(c.CollegeNameNepali)}," +
                          $"{EscapeCsv(c.ShortName)}," +
                          $"{EscapeCsv(c.Address?.LocalLevel?.District?.DistrictName)}," +
                          $"{EscapeCsv(c.Address?.LocalLevel?.LocalLevelName)}," +
                          $"{c.Address?.WardNumber}," +
                          $"{EscapeCsv(c.Address?.HouseNumber)}," +
                          $"{EscapeCsv(c.Website)}," +
                          $"{EscapeCsv(c.Email)}," +
                          $"{EscapeCsv(c.Phone1)}," +
                          $"{EscapeCsv(c.Phone2)}," +
                          $"{EscapeCsv(c.PrincipalName)}," +
                          $"{EscapeCsv(c.PrincipalContactNumber)}," +
                          $"{EscapeCsv(c.Fax)}," +
                          $"{EscapeCsv(c.Remarks)}," +
                          $"{(c.IsExamCenterOnly ? "Yes" : "No")}," +
                          $"{(c.IsActive ? "Active" : "Inactive")}," +
                          $"{EscapeCsv(c.CollegeType?.Code)}," +
                          $"{c.AllocatedAmount}," +
                          $"{EscapeCsv(c.Address?.ToleStreet)}," +
                          $"{c.DisplayOrder}," +
                          $"{c.EstablishedDate.ToString("yyyy-MM-dd")}," +
                          $"{c.ClosedDate?.ToString("yyyy-MM-dd")}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", $"Colleges_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    public async Task<IActionResult> ExportToPdf(string search = null, string sort = "DisplayOrder", string sortDir = "asc")
    {
        int? orgId = User.IsInRole(Role.FacultyAdmin) ? await GetCurrentUserFacultyIdAsync() : null;
        var items = await collegeService.GetFilteredItemsAsync(search, sort, sortDir, orgId);
        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(string search = null, string sort = "DisplayOrder", string sortDir = "asc")
    {
        int? orgId = User.IsInRole(Role.FacultyAdmin) ? await GetCurrentUserFacultyIdAsync() : null;
        var items = await collegeService.GetFilteredItemsAsync(search, sort, sortDir, orgId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Colleges");

        var headers = new[] { "College Code", "College Name", "College Name (Nepali)", "Short Name", "District", "Municipality/VDC", "Ward No.", "House No.", "Website", "Email", "Phone 1", "Phone 2", "Principal Name", "Principal Contact", "Fax", "Remarks", "Is Exam Center Only", "Is Active", "College Type", "Allocated Amount", "Area", "Display Order", "Established Date", "Closed Date" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var c in items)
        {
            worksheet.Cell(row, 1).Value = c.Code.ToString();
            worksheet.Cell(row, 2).Value = c.Name ?? "";
            worksheet.Cell(row, 3).Value = c.CollegeNameNepali ?? "";
            worksheet.Cell(row, 4).Value = c.ShortName ?? "";
            worksheet.Cell(row, 5).Value = c.Address?.LocalLevel?.District?.DistrictName ?? "";
            worksheet.Cell(row, 6).Value = c.Address?.LocalLevel?.LocalLevelName ?? "";
            worksheet.Cell(row, 7).Value = c.Address?.WardNumber?.ToString() ?? "";
            worksheet.Cell(row, 8).Value = c.Address?.HouseNumber ?? "";
            worksheet.Cell(row, 9).Value = c.Website ?? "";
            worksheet.Cell(row, 10).Value = c.Email ?? "";
            worksheet.Cell(row, 11).Value = c.Phone1 ?? "";
            worksheet.Cell(row, 12).Value = c.Phone2 ?? "";
            worksheet.Cell(row, 13).Value = c.PrincipalName ?? "";
            worksheet.Cell(row, 14).Value = c.PrincipalContactNumber ?? "";
            worksheet.Cell(row, 15).Value = c.Fax ?? "";
            worksheet.Cell(row, 16).Value = c.Remarks ?? "";
            worksheet.Cell(row, 17).Value = c.IsExamCenterOnly ? "Yes" : "No";
            worksheet.Cell(row, 18).Value = c.IsActive ? "Active" : "Inactive";
            worksheet.Cell(row, 19).Value = c.CollegeType?.Code ?? "";
            worksheet.Cell(row, 20).Value = c.AllocatedAmount?.ToString() ?? "";
            worksheet.Cell(row, 21).Value = c.Address?.ToleStreet ?? "";
            worksheet.Cell(row, 22).Value = c.DisplayOrder?.ToString() ?? "";
            worksheet.Cell(row, 23).Value = c.EstablishedDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 24).Value = c.ClosedDate?.ToString("yyyy-MM-dd") ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Colleges_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var college = await collegeService.GetCollegeByIdAsync(id.Value);
        if (college == null) return NotFound();

        return View(college);
    }

    [RequirePermission("colleges.create")]
    public async Task<IActionResult> Create()
    {
        var collegeTypes = await collegeService.GetCollegeTypesAsync();
        await this.PopulateSelectLists();
        ViewData["CollegeTypeId"] = new SelectList(collegeTypes, "Id", "Code");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("colleges.create")]
    public async Task<IActionResult> Create([Bind("Id,Code,Name,CollegeNameNepali,ShortName,EstablishedDate,ClosedDate,Website,Email,Phone1,Phone2,PrincipalName,PrincipalContactNumber,Fax,Remarks,IsExamCenterOnly,IsActive,AllocatedAmount,DisplayOrder,CollegeTypeId,CollegeProfileId")] College college)
    {
        var localLevelId = Request.Form["LocalLevelId"].ToString();
        var wardNumber = Request.Form["WardNumber"].ToString();
        var toleStreet = Request.Form["ToleStreet"].ToString();
        var houseNumber = Request.Form["HouseNumber"].ToString();

        if (ModelState.IsValid)
        {
            try
            {
                if (User.IsInRole(Role.FacultyAdmin))
                {
                    var facultyId = await GetCurrentUserFacultyIdAsync();
                    if (facultyId.HasValue)
                    {
                        var faculty = new Faculty { Id = facultyId.Value };
                        context.Faculties.Attach(faculty);
                        college.Faculties = new List<Faculty> { faculty };
                    }
                }
                await collegeService.CreateCollegeAsync(college, localLevelId, wardNumber, toleStreet, houseNumber);
                TempData["SuccessMessage"] = "College created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the college: {ex.Message}");
            }
        }

        var collegeTypes = await collegeService.GetCollegeTypesAsync();
        ViewData["CollegeTypeId"] = new SelectList(collegeTypes, "Id", "Code", college.CollegeTypeId);
        await this.PopulateSelectLists();
        return View(college);
    }

    [RequirePermission("colleges.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var college = await collegeService.GetCollegeByIdAsync(id.Value);
        if (college == null) return NotFound();

        var collegeTypes = await collegeService.GetCollegeTypesAsync();
        ViewData["CollegeTypeId"] = new SelectList(collegeTypes, "Id", "Code", college.CollegeTypeId);
        await this.PopulateSelectLists();
        return View(college);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("colleges.edit")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,CollegeNameNepali,ShortName,EstablishedDate,ClosedDate,Website,Email,Phone1,Phone2,PrincipalName,PrincipalContactNumber,Fax,Remarks,IsExamCenterOnly,IsActive,AllocatedAmount,DisplayOrder,CollegeTypeId,CollegeProfileId,AddressId")] College college)
    {
        if (id != college.Id) return NotFound();

        var localLevelId = Request.Form["LocalLevelId"].ToString();
        var wardNumber = Request.Form["WardNumber"].ToString();
        var toleStreet = Request.Form["ToleStreet"].ToString();
        var houseNumber = Request.Form["HouseNumber"].ToString();

        if (ModelState.IsValid)
        {
            try
            {
                await collegeService.UpdateCollegeAsync(college, localLevelId, wardNumber, toleStreet, houseNumber);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await collegeService.CollegeExistsAsync(college.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        var collegeTypes = await collegeService.GetCollegeTypesAsync();
        ViewData["CollegeTypeId"] = new SelectList(collegeTypes, "Id", "Code", college.CollegeTypeId);
        return View(college);
    }

    [RequirePermission("colleges.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var college = await collegeService.GetCollegeByIdAsync(id.Value);
        if (college == null) return NotFound();

        return View(college);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission("colleges.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await collegeService.DeleteCollegeAsync(id);
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<JsonResult> GetDistrictsByProvince(int provinceId)
    {
        var districts = await collegeService.GetDistrictsByProvinceAsync(provinceId);
        return Json(districts);
    }

    [HttpGet]
    public async Task<JsonResult> GetLocalLevelsByDistrict(int districtId)
    {
        var localLevels = await collegeService.GetLocalLevelsByDistrictAsync(districtId);
        return Json(localLevels);
    }

    private async Task PopulateSelectLists()
    {
        var provinces = await collegeService.GetProvincesAsync();
        ViewBag.Provinces = new SelectList(provinces, "Id", "ProvinceName");

        // This will be implemented based on the selectLists object
        // For now, using ViewData as in the original
    }
        [RequirePermission("colleges.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await collegeService.DeleteCollegeAsync(id); return Json(new { success = true, message = "College deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
