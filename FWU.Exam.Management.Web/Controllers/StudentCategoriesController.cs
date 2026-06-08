using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class StudentCategoriesController : Controller
{
    private readonly IStudentCategoryService _studentCategoryService;

    public StudentCategoriesController(IStudentCategoryService studentCategoryService)
    {
        _studentCategoryService = studentCategoryService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "StudentCategoryName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _studentCategoryService.GetStudentCategoriesAsync(page, pageSize, search, sort, sortDir);

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

        var studentCategory = await _studentCategoryService.GetStudentCategoryByIdAsync(id.Value);
        if (studentCategory == null) return NotFound();

        return View(studentCategory);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,StudentCategoryName,IsActive,Remarks")] StudentCategory studentCategory)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _studentCategoryService.CreateStudentCategoryAsync(studentCategory);
                TempData["SuccessMessage"] = "Student category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("StudentCategoryName", ex.Message);
            }
        }
        return View(studentCategory);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var studentCategory = await _studentCategoryService.GetStudentCategoryByIdAsync(id.Value);
        if (studentCategory == null) return NotFound();

        return View(studentCategory);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,StudentCategoryName,IsActive,Remarks")] StudentCategory studentCategory)
    {
        if (id != studentCategory.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _studentCategoryService.UpdateStudentCategoryAsync(studentCategory);
                TempData["SuccessMessage"] = "Student category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _studentCategoryService.StudentCategoryExistsAsync(studentCategory.Id))
                    return NotFound();
                throw;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("StudentCategoryName", ex.Message);
            }
        }
        return View(studentCategory);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var studentCategory = await _studentCategoryService.GetStudentCategoryByIdAsync(id.Value);
        if (studentCategory == null) return NotFound();

        return View(studentCategory);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _studentCategoryService.DeleteStudentCategoryAsync(id);
        TempData["SuccessMessage"] = "Student category deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
