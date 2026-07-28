using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("genders.view")]
public class GendersController(IGenderService genderService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await genderService.GetGendersAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var gender = await genderService.GetGenderByIdAsync(id.Value);
        if (gender == null) return NotFound();

        return View(gender);
    }

    [RequirePermission("genders.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("genders.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,GenderName,IsActive")] Gender gender)
    {
        if (ModelState.IsValid)
        {
            await genderService.CreateGenderAsync(gender);
            TempData["SuccessMessage"] = "Gender created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(gender);
    }

    [RequirePermission("genders.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var gender = await genderService.GetGenderByIdAsync(id.Value);
        if (gender == null) return NotFound();

        return View(gender);
    }

    [RequirePermission("genders.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,GenderName,IsActive")] Gender gender)
    {
        if (id != gender.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await genderService.UpdateGenderAsync(gender);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await genderService.GenderExistsAsync(gender.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Gender updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(gender);
    }

    [RequirePermission("genders.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var gender = await genderService.GetGenderByIdAsync(id.Value);
        if (gender == null) return NotFound();

        return View(gender);
    }

    [RequirePermission("genders.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await genderService.DeleteGenderAsync(id);
            TempData["SuccessMessage"] = "Gender deleted successfully!";
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

    [RequirePermission("genders.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await genderService.DeleteGenderAsync(id); return Json(new { success = true, message = "Gender deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }
}
