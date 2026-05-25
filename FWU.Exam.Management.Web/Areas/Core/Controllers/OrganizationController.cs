using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin")]
public class OrganizationController(IOrganizationService organizationService, IFileUploadHelper fileUploadHelper) : Controller
{
    public async Task<IActionResult> Index()
    {
        var organizations = await organizationService.GetAllOrganizationsAsync();
        return View(organizations);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var organization = await organizationService.GetOrganizationByIdAsync(id.Value);
        if (organization == null) return NotFound();

        return View(organization);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Organization organization, IFormFile? logoFile)
    {
        if (ModelState.IsValid)
        {
            if (logoFile != null)
            {
                organization.LogoPath = await fileUploadHelper.UploadAsync(logoFile);
            }

            await organizationService.CreateOrganizationAsync(organization);
            return RedirectToAction(nameof(Index));
        }

        return View(organization);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var organization = await organizationService.GetOrganizationByIdAsync(id.Value);
        if (organization == null) return NotFound();

        return View(organization);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Organization organization, IFormFile? logoFile)
    {
        if (id != organization.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (logoFile != null)
                {
                    organization.LogoPath = await fileUploadHelper.UploadAsync(logoFile);
                }

                await organizationService.UpdateOrganizationAsync(organization);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await organizationService.OrganizationExistsAsync(organization.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        return View(organization);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var organization = await organizationService.GetOrganizationByIdAsync(id.Value);
        if (organization == null) return NotFound();

        return View(organization);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await organizationService.DeleteOrganizationAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
