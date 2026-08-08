using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("ethnicities.view")]
public class EthnicitiesController(IEthnicityService ethnicityService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await ethnicityService.GetEthnicitiesAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    [RequirePermission("ethnicities.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("ethnicities.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,EthnicityName,IsDefault,IsActive")] Ethnicity ethnicity)
    {
        if (ModelState.IsValid)
        {
            await ethnicityService.CreateEthnicityAsync(ethnicity);
            TempData["SuccessMessage"] = "Ethnicity created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(ethnicity);
    }

    [RequirePermission("ethnicities.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var ethnicity = await ethnicityService.GetEthnicityByIdAsync(id.Value);
        if (ethnicity == null) return NotFound();

        return View(ethnicity);
    }

    [RequirePermission("ethnicities.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,EthnicityName,IsDefault,IsActive")] Ethnicity ethnicity)
    {
        if (id != ethnicity.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await ethnicityService.UpdateEthnicityAsync(ethnicity);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ethnicityService.EthnicityExistsAsync(ethnicity.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Ethnicity updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(ethnicity);
    }

    [RequirePermission("ethnicities.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var ethnicity = await ethnicityService.GetEthnicityByIdAsync(id.Value);
        if (ethnicity == null) return NotFound();

        return View(ethnicity);
    }

    [RequirePermission("ethnicities.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await ethnicityService.DeleteEthnicityAsync(id);
            TempData["SuccessMessage"] = "Ethnicity deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this record because it is referenced by other records. Please remove or reassign dependent records first.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while deleting: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [RequirePermission("ethnicities.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await ethnicityService.DeleteEthnicityAsync(id); return Json(new { success = true, message = "Ethnicity deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }
}
