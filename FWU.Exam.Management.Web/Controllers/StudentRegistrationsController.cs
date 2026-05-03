using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models.Students;
using fwu_examination_management_system.Data.Models.Location;
using fwu_examination_management_system.Data.Enums;
using OfficeOpenXml;
using fwu_examination_management_system.Data.Enums;

namespace fwu_examination_management_system.Controllers;

public class StudentRegistrationsController : Controller
{
    private readonly AppDbContext _context;

    public StudentRegistrationsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var studentRegistrations = await _context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.District)
            .Include(s => s.StudentCategory)
            .OrderByDescending(s => s.Id)
            .ToListAsync();
        return View(studentRegistrations);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var studentRegistration = await _context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.District)
            .Include(s => s.StudentCategory)
            .Include(s => s.Ethnicity)
            .Include(s => s.LocalLevel)
            .Include(s => s.IndexGroup)
            .Include(s => s.EntryFormat)
            .Include(s => s.PhotoAttachment)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (studentRegistration == null)
        {
            return NotFound();
        }

        return View(studentRegistration);
    }

    public IActionResult Create()
    {
        PopulateSelectLists();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("LevelId,FacultyId,CollegeId,RegistrationNumber,FirstName,MiddleName,LastName,NepaliName,ContactNumber,Phone,Email,DateOfBirthBs,DateOfBirthAd,GenderId,IndexGroupId,BloodGroup,Nationality,Religion,IsActive,StudentRegistrationIndex,StudentCategoryId,VerifiedBy,VerifiedDate,PhotoAttachmentId,EthnicityId,EntranceRollNumber,EntryFormatId,IsRegistrationNumberGenerated,RowIndex,PreviousAcademicYear,PreviousSymbolNumber,StudentRegistrationSearchId,AcademicYearId,SemesterId")] StudentRegistration studentRegistration)
    {
        // Create Permanent Address
        var permanentLocalLevelId = Request.Form["LocalLevelId"].ToString();
        var permanentWardNumber = Request.Form["WardNumber"].ToString();
        var permanentToleStreet = Request.Form["ToleStreet"].ToString();
        var permanentHouseNumber = Request.Form["HouseNumber"].ToString();

        if (!string.IsNullOrEmpty(permanentLocalLevelId))
        {
            var permanentAddress = new Address
            {
                LocalLevelId = int.Parse(permanentLocalLevelId),
                WardNumber = string.IsNullOrEmpty(permanentWardNumber) ? null : int.Parse(permanentWardNumber),
                ToleStreet = permanentToleStreet,
                HouseNumber = permanentHouseNumber,
                AddressType = AddressType.Permanent,
                IsActive = true
            };
            _context.Addresses.Add(permanentAddress);
            await _context.SaveChangesAsync();
            studentRegistration.PermanentAddressId = permanentAddress.Id;
        }

        if (ModelState.IsValid)
        {
            _context.Add(studentRegistration);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Student registration created successfully!";
            return RedirectToAction(nameof(Index));
        }
        PopulateSelectLists(studentRegistration);
        return View(studentRegistration);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var studentRegistration = await _context.StudentRegistrations.FindAsync(id);
        if (studentRegistration == null)
        {
            return NotFound();
        }
        PopulateSelectLists(studentRegistration);
        return View(studentRegistration);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LevelId,FacultyId,CollegeId,RegistrationNumber,FirstName,MiddleName,LastName,NepaliName,ContactNumber,Phone,Email,DateOfBirthBs,DateOfBirthAd,GenderId,IndexGroupId,BloodGroup,Nationality,Religion,IsActive,StudentRegistrationIndex,StudentCategoryId,VerifiedBy,VerifiedDate,PhotoAttachmentId,EthnicityId,EntranceRollNumber,EntryFormatId,IsRegistrationNumberGenerated,RowIndex,PreviousAcademicYear,PreviousSymbolNumber,StudentRegistrationSearchId,AcademicYearId,SemesterId,PermanentAddressId")] StudentRegistration studentRegistration)
    {
        if (id != studentRegistration.Id)
        {
            return NotFound();
        }

        // Update Permanent Address if provided
        var permanentLocalLevelId = Request.Form["LocalLevelId"].ToString();
        var permanentWardNumber = Request.Form["WardNumber"].ToString();
        var permanentToleStreet = Request.Form["ToleStreet"].ToString();
        var permanentHouseNumber = Request.Form["HouseNumber"].ToString();

        if (!string.IsNullOrEmpty(permanentLocalLevelId))
        {
            var address = await _context.Addresses.FindAsync(studentRegistration.PermanentAddressId);
            if (address == null)
            {
                address = new Address
                {
                    LocalLevelId = int.Parse(permanentLocalLevelId),
                    WardNumber = string.IsNullOrEmpty(permanentWardNumber) ? null : int.Parse(permanentWardNumber),
                    ToleStreet = permanentToleStreet,
                    HouseNumber = permanentHouseNumber,
                    AddressType = AddressType.Permanent,
                    IsActive = true
                };
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                studentRegistration.PermanentAddressId = address.Id;
            }
            else
            {
                address.LocalLevelId = int.Parse(permanentLocalLevelId);
                address.WardNumber = string.IsNullOrEmpty(permanentWardNumber) ? null : int.Parse(permanentWardNumber);
                address.ToleStreet = permanentToleStreet;
                address.HouseNumber = permanentHouseNumber;
                _context.Update(address);
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(studentRegistration);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Student registration updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentRegistrationExists(studentRegistration.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        PopulateSelectLists(studentRegistration);
        return View(studentRegistration);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var studentRegistration = await _context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.District)
            .Include(s => s.StudentCategory)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (studentRegistration == null)
        {
            return NotFound();
        }

        return View(studentRegistration);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var studentRegistration = await _context.StudentRegistrations.FindAsync(id);
        if (studentRegistration != null)
        {
            _context.StudentRegistrations.Remove(studentRegistration);
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Student registration deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    private bool StudentRegistrationExists(int id)
    {
        return _context.StudentRegistrations.Any(e => e.Id == id);
    }

    private void PopulateSelectLists(StudentRegistration? studentRegistration = null)
    {
        ViewData["AcademicYearId"] = new SelectList(_context.AcademicYears.Where(ay => ay.AcademicYearName != null).ToList(), "Id", "AcademicYearName", studentRegistration?.AcademicYearId);        
        ViewData["LevelId"] = new SelectList(_context.Levels.Where(l => l.LevelName != null).ToList(), "Id", "LevelName", studentRegistration?.LevelId);
        ViewData["FacultyId"] = new SelectList(_context.Faculties.Where(f => f.FacultyName != null).ToList(), "Id", "FacultyName", studentRegistration?.FacultyId);
        ViewData["CollegeId"] = new SelectList(_context.Colleges.Where(c => c.Name != null).ToList(), "Id", "Name", studentRegistration?.CollegeId);
        ViewData["GenderId"] = new SelectList(_context.Genders.Where(g => g.GenderName != null).ToList(), "Id", "GenderName", studentRegistration?.GenderId);
        ViewData["StudentCategoryId"] = new SelectList(_context.StudentCategories.Where(sc => sc.StudentCategoryName != null).ToList(), "Id", "StudentCategoryName", studentRegistration?.StudentCategoryId);
        ViewData["EthnicityId"] = new SelectList(_context.Ethnicities.Where(e => e.EthnicityName != null).ToList(), "Id", "EthnicityName", studentRegistration?.EthnicityId);
        ViewData["LocalLevelId"] = new SelectList(_context.LocalLevels.Where(ll => ll.LocalLevelName != null).ToList(), "Id", "LocalLevelName", studentRegistration?.LocalLevelId);
        ViewData["IndexGroupId"] = new SelectList(_context.IndexGroups.Where(ig => ig.IndexGroupName != null).ToList(), "Id", "IndexGroupName", studentRegistration?.IndexGroupId);
        ViewData["EntryFormatId"] = new SelectList(_context.EntryFormats.Where(ef => ef.EntryFormatName != null).ToList(), "Id", "EntryFormatName", studentRegistration?.EntryFormatId);
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedData(string searchTerm = "", int page = 1, int pageSize = 10)
    {
        var query = _context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(s => 
                (s.RegistrationNumber != null && s.RegistrationNumber.ToLower().Contains(lowerSearchTerm)) ||
                (s.FirstName != null && s.FirstName.ToLower().Contains(lowerSearchTerm)) ||
                (s.LastName != null && s.LastName.ToLower().Contains(lowerSearchTerm)) ||
                (s.Email != null && s.Email.ToLower().Contains(lowerSearchTerm)) ||
                (s.ContactNumber != null && s.ContactNumber.ToLower().Contains(lowerSearchTerm)));
        }

        var totalCount = await query.CountAsync();
        var skip = (page - 1) * pageSize;

        var data = await query
            .OrderByDescending(s => s.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(s => new
            {
                id = s.Id,
                registrationNumber = s.RegistrationNumber ?? "-",
                fullName = $"{s.FirstName} {s.LastName}".Trim(),
                academicYear = s.AcademicYear != null ? s.AcademicYear.AcademicYearName : "-",
                level = s.Level != null ? s.Level.LevelName : "-",
                college = s.College != null ? s.College.Name : "-",
                category = s.StudentCategory != null ? s.StudentCategory.StudentCategoryName : "-",
                contactNumber = s.ContactNumber ?? "-",
                email = s.Email ?? "-",
                status = s.IsActive ? "Active" : "Inactive"
            })
            .ToListAsync();

        return Json(new { data, totalCount });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var registration = await _context.StudentRegistrations.FindAsync(id);
        if (registration == null)
            return NotFound();

        registration.IsActive = status == "Approved";
        await _context.SaveChangesAsync();

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
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;

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
                                FirstName = worksheet.Cells[row, 1].Value?.ToString() ?? "",
                                LastName = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                                MiddleName = worksheet.Cells[row, 3].Value?.ToString(),
                                Email = worksheet.Cells[row, 4].Value?.ToString(),
                                ContactNumber = worksheet.Cells[row, 5].Value?.ToString(),
                                DateOfBirthBs = worksheet.Cells[row, 6].Value?.ToString() ?? "",
                                RegistrationNumber = worksheet.Cells[row, 7].Value?.ToString(),
                                AcademicYearId = int.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out var ayId) ? ayId : 0,
                                LevelId = int.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out var lvlId) ? lvlId : 0,
                                CollegeId = int.TryParse(worksheet.Cells[row, 10].Value?.ToString(), out var collId) ? collId : 0,
                                FacultyId = int.TryParse(worksheet.Cells[row, 11].Value?.ToString(), out var facId) ? facId : 0,
                                GenderId = int.TryParse(worksheet.Cells[row, 12].Value?.ToString(), out var genderId) ? genderId : 0,
                                StudentCategoryId = int.TryParse(worksheet.Cells[row, 13].Value?.ToString(), out var catId) ? catId : 0,
                                IsActive = true
                            };

                            if (registration.AcademicYearId == 0 || registration.LevelId == 0 || 
                                registration.CollegeId == 0 || registration.FacultyId == 0)
                            {
                                errors.Add($"Row {row}: Missing required IDs (AcademicYear, Level, College, or Faculty)");
                                continue;
                            }

                            _context.StudentRegistrations.Add(registration);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Row {row}: {ex.Message}");
                        }
                    }

                    await _context.SaveChangesAsync();
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
        var query = _context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.College)
            .Include(s => s.StudentCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(s => 
                (s.RegistrationNumber != null && s.RegistrationNumber.ToLower().Contains(lowerSearchTerm)) ||
                (s.FirstName != null && s.FirstName.ToLower().Contains(lowerSearchTerm)) ||
                (s.LastName != null && s.LastName.ToLower().Contains(lowerSearchTerm)));
        }

        var data = await query.OrderByDescending(s => s.Id).ToListAsync();

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Student Registrations");

            // Add headers
            worksheet.Cells[1, 1].Value = "Registration No";
            worksheet.Cells[1, 2].Value = "First Name";
            worksheet.Cells[1, 3].Value = "Middle Name";
            worksheet.Cells[1, 4].Value = "Last Name";
            worksheet.Cells[1, 5].Value = "Email";
            worksheet.Cells[1, 6].Value = "Contact Number";
            worksheet.Cells[1, 7].Value = "Date of Birth (BS)";
            worksheet.Cells[1, 8].Value = "Academic Year";
            worksheet.Cells[1, 9].Value = "Level";
            worksheet.Cells[1, 10].Value = "College";
            worksheet.Cells[1, 11].Value = "Faculty";
            worksheet.Cells[1, 12].Value = "Category";
            worksheet.Cells[1, 13].Value = "Active";

            // Make header bold
            for (int col = 1; col <= 13; col++)
            {
                worksheet.Cells[1, col].Style.Font.Bold = true;
                worksheet.Cells[1, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Add data rows
            int row = 2;
            foreach (var reg in data)
            {
                worksheet.Cells[row, 1].Value = reg.RegistrationNumber;
                worksheet.Cells[row, 2].Value = reg.FirstName;
                worksheet.Cells[row, 3].Value = reg.MiddleName;
                worksheet.Cells[row, 4].Value = reg.LastName;
                worksheet.Cells[row, 5].Value = reg.Email;
                worksheet.Cells[row, 6].Value = reg.ContactNumber;
                worksheet.Cells[row, 7].Value = reg.DateOfBirthBs;
                worksheet.Cells[row, 8].Value = reg.AcademicYear?.AcademicYearName;
                worksheet.Cells[row, 9].Value = reg.Level?.LevelName;
                worksheet.Cells[row, 10].Value = reg.College?.Name;
                worksheet.Cells[row, 11].Value = reg.Faculty?.FacultyName;
                worksheet.Cells[row, 12].Value = reg.StudentCategory?.StudentCategoryName;
                worksheet.Cells[row, 13].Value = reg.IsActive ? "Yes" : "No";
                row++;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            var fileBytes = package.GetAsByteArray();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"StudentRegistrations_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
}
