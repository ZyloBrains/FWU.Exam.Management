using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("backuprestore.manage")]
public class BackupRestoreController(IBackupRestoreService backupRestoreService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var backups = await backupRestoreService.GetBackupsAsync();
        ViewBag.DbName = await backupRestoreService.GetDatabaseNameAsync();
        return View(backups);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Backup(string? backupName)
    {
        try
        {
            var fileName = await backupRestoreService.BackupDatabaseAsync(backupName);
            TempData["SuccessMessage"] = $"Database backup created: {fileName}";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Backup failed: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(string backupFileName)
    {
        try
        {
            var result = await backupRestoreService.RestoreDatabaseAsync(backupFileName);
            TempData["SuccessMessage"] = result;
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Restore failed: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Download(string fileName)
    {
        var backupDir = backupRestoreService.GetBackupDirectory();
        var filePath = Path.Combine(backupDir, fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, "application/octet-stream", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(string fileName)
    {
        try
        {
            var backupDir = backupRestoreService.GetBackupDirectory();
            var filePath = Path.Combine(backupDir, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                TempData["SuccessMessage"] = $"Backup file '{fileName}' deleted.";
            }
            else
            {
                TempData["ErrorMessage"] = "File not found.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Delete failed: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }
}
