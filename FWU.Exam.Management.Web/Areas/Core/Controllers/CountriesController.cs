using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("countries.view")]
public class CountriesController(ICountryService countryService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await countryService.GetCountriesAsync(page, pageSize, search, sort, sortDir);

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

        var country = await countryService.GetCountryByIdAsync(id.Value);
        if (country == null) return NotFound();

        return View(country);
    }

    [RequirePermission("countries.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("countries.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,CountryName,IsActive")] Country country)
    {
        if (ModelState.IsValid)
        {
            await countryService.CreateCountryAsync(country);
            TempData["SuccessMessage"] = "Country created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(country);
    }

    [RequirePermission("countries.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var country = await countryService.GetCountryByIdAsync(id.Value);
        if (country == null) return NotFound();

        return View(country);
    }

    [RequirePermission("countries.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CountryName,IsActive")] Country country)
    {
        if (id != country.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await countryService.UpdateCountryAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await countryService.CountryExistsAsync(country.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Country updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(country);
    }

    [RequirePermission("countries.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var country = await countryService.GetCountryByIdAsync(id.Value);
        if (country == null) return NotFound();

        return View(country);
    }

    [RequirePermission("countries.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await countryService.DeleteCountryAsync(id);
            TempData["SuccessMessage"] = "Country deleted successfully!";
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

    [RequirePermission("countries.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await countryService.DeleteCountryAsync(id); return Json(new { success = true, message = "Country deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }
}
