using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

public class CollegesController : Controller
{
    private readonly ICollegeService _collegeService;

    public CollegesController(ICollegeService collegeService)
    {
        _collegeService = collegeService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "DisplayOrder", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _collegeService.GetCollegesAsync(page, pageSize, search, sort, sortDir);

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
        var items = await _collegeService.GetFilteredItemsAsync(search, sort, sortDir);

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
                          $"{c.EstablishedDate?.ToString("yyyy-MM-dd")}," +
                          $"{c.ClosedDate?.ToString("yyyy-MM-dd")}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", $"Colleges_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    public async Task<IActionResult> ExportToPdf(string search = null, string sort = "DisplayOrder", string sortDir = "asc")
    {
        var items = await _collegeService.GetFilteredItemsAsync(search, sort, sortDir);
        return View("PrintPdf", items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var college = await _collegeService.GetCollegeByIdAsync(id.Value);
        if (college == null) return NotFound();

        return View(college);
    }

    public async Task<IActionResult> Create()
    {
        var collegeTypes = await _collegeService.GetCollegeTypesAsync();
        ViewData["CollegeTypeId"] = new SelectList(collegeTypes, "Id", "Code");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Code,Name,CollegeNameNepali,ShortName,EstablishedDate,ClosedDate,Website,Email,Phone1,Phone2,PrincipalName,PrincipalContactNumber,Fax,Remarks,IsExamCenterOnly,IsActive,AllocatedAmount,DisplayOrder,CollegeTypeId,CollegeProfileId")] College college)
    {
        var localLevelId = Request.Form["LocalLevelId"].ToString();
        var wardNumber = Request.Form["WardNumber"].ToString();
        var toleStreet = Request.Form["ToleStreet"].ToString();
        var houseNumber = Request.Form["HouseNumber"].ToString();

        if (ModelState.IsValid)
        {
            await _collegeService.CreateCollegeAsync(college, localLevelId, wardNumber, toleStreet, houseNumber);
            return RedirectToAction(nameof(Index));
        }

        var collegeTypes = await _collegeService.GetCollegeTypesAsync();
        ViewData["CollegeTypeId"] = new SelectList(collegeTypes, "Id", "Code", college.CollegeTypeId);
        return View(college);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var college = await _collegeService.GetCollegeByIdAsync(id.Value);
        if (college == null) return NotFound();

        var collegeTypes = await _collegeService.GetCollegeTypesAsync();
        ViewData["CollegeTypeId"] = new SelectList(collegeTypes, "Id", "Code", college.CollegeTypeId);
        return View(college);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
                await _collegeService.UpdateCollegeAsync(college, localLevelId, wardNumber, toleStreet, houseNumber);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _collegeService.CollegeExistsAsync(college.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        var collegeTypes = await _collegeService.GetCollegeTypesAsync();
        ViewData["CollegeTypeId"] = new SelectList(collegeTypes, "Id", "Code", college.CollegeTypeId);
        return View(college);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var college = await _collegeService.GetCollegeByIdAsync(id.Value);
        if (college == null) return NotFound();

        return View(college);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _collegeService.DeleteCollegeAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
