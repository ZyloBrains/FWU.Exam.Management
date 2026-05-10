using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Web.Helpers;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

public class AdmissionFormsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IFileUploadHelper _fileUploadHelper;

    public AdmissionFormsController(AppDbContext context, IFileUploadHelper fileUploadHelper)
    {
        _context = context;
        _fileUploadHelper = fileUploadHelper;
    }

    public async Task<IActionResult> Index()
    {
        var registrations = await _context.StudentRegistrations
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentGuardians)
            .AsNoTracking()
            .OrderByDescending(s => s.Id)
            .ToListAsync();
        return View(registrations);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateSelectListsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdmissionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return View(model);
        }

        var program = await _context.Programs.AsNoTracking().FirstOrDefaultAsync(p => p.Id == model.ProgrammeId);
        if (program == null)
        {
            ModelState.AddModelError(nameof(model.ProgrammeId), "Selected programme not found.");
            await PopulateSelectListsAsync();
            return View(model);
        }

        var runningYear = await _context.AcademicYears.Where(y => y.IsRunning).AsNoTracking().FirstOrDefaultAsync();

        await _fileUploadHelper.UploadImageAsync(model.Photo, "uploads/photos");
        await _fileUploadHelper.UploadDocumentAsync(model.DocumentsFile, "uploads/documents");
        await _fileUploadHelper.UploadDocumentAsync(model.BankVoucherFile, "uploads/vouchers");

        var nameParts = SplitName(model.Name);

        var registration = new StudentRegistration
        {
            LevelId = program.LevelId,
            FacultyId = program.FacultyId,
            CollegeId = model.CollegeId,
            AcademicYearId = runningYear?.Id ?? 0,
            FirstName = nameParts.first,
            MiddleName = nameParts.middle,
            LastName = nameParts.last,
            DateOfBirthBS = model.DobBS,
            DateOfBirthAD = model.DobAD?.ToString("yyyy-MM-dd"),
            GenderId = int.TryParse(model.Gender, out var gId) ? gId : 0,
            ContactNumber = model.Mobile?.Length > 20 ? model.Mobile.Substring(0, 20) : model.Mobile,
            Phone = model.PhoneLandline?.Length > 15 ? model.PhoneLandline.Substring(0, 15) : model.PhoneLandline,
            Email = model.Email,
            CitizenshipNo = model.CitizenshipNo,
            CitizenshipDistrict = model.CitizenshipDistrict,
            CitizenshipIssueDate = model.CitizenshipIssueDate,
            BirthPlace = model.BirthPlace,
            NationalId = model.NationalId,
            BloodGroup = model.BloodGroup,
            IsActive = true,
            StudentCategoryId = 1
        };

        var localLevel = await _context.LocalLevels
            .Where(ll => ll.LocalLevelName.Contains(model.PermMunicipality) || model.PermMunicipality.Contains(ll.LocalLevelName))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        var address = new Address
        {
            LocalLevelId = localLevel?.Id ?? 1,
            FullAddress = $"{model.PermMunicipality}, Ward {model.PermWard}, {model.PermDistrict}, {model.PermProvince}, {model.PermCountry}",
            AddressType = AddressType.Permanent,
            IsActive = true
        };
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();
        registration.PermanentAddressId = address.Id;

        _context.StudentRegistrations.Add(registration);
        await _context.SaveChangesAsync();

        var guardian = new StudentGuardian
        {
            StudentRegistrationId = registration.Id,
            FatherName = model.FatherName,
            FatherContactNumber = model.FatherPhone,
            FatherProfession = model.FatherProfession,
            MotherName = model.MotherName ?? "",
            MotherContactNumber = model.MotherPhone,
            GuardianEmail = model.GuardianEmail,
            GuardianName = model.FatherName
        };
        _context.StudentGuardians.Add(guardian);

        var boards = await _context.Boards.AsNoTracking().ToListAsync();
        var prevLevels = await _context.PreviousLevels.AsNoTracking().ToListAsync();

        foreach (var q in model.AcademicQualifications)
        {
            if (string.IsNullOrWhiteSpace(q.Level)) continue;

            var board = boards.FirstOrDefault(b =>
                !string.IsNullOrWhiteSpace(q.BoardUniversity) &&
                (b.BoardName?.Contains(q.BoardUniversity) == true || q.BoardUniversity.Contains(b.BoardName ?? "")));
            var prevLevel = prevLevels.FirstOrDefault(pl =>
                pl.PreviousLevelName?.Contains(q.Level) == true);

            var qualification = new StudentQualification
            {
                StudentRegistrationId = registration.Id,
                BoardId = board?.Id ?? 1,
                PreviousLevelId = prevLevel?.Id ?? 1,
                InstituteName = q.BoardUniversity,
                ProgramName = q.Level,
                PassedYear = q.Year,
                ExamRollNumber = q.SymbolNo,
                Percentage = decimal.TryParse(q.PercentCGPA, out var pct) ? pct : null,
                Division = q.Division,
                IsActive = true
            };
            _context.StudentQualifications.Add(qualification);
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Admission form submitted successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var registration = await _context.StudentRegistrations
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.PermanentAddress)
            .Include(s => s.StudentGuardians)
            .Include(s => s.StudentQualifications)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (registration == null) return NotFound();

        return View(registration);
    }

    private async Task PopulateSelectListsAsync()
    {
        var programs = await _context.Programs.Where(p => p.IsActive).AsNoTracking().ToListAsync();
        ViewBag.Programs = new SelectList(programs, "Id", "ProgramName");

        var genders = await _context.Genders.Where(g => g.IsActive).AsNoTracking().ToListAsync();
        ViewBag.Genders = new SelectList(genders, "Id", "GenderName");

        var provinces = await _context.Provinces.AsNoTracking().ToListAsync();
        ViewBag.Provinces = new SelectList(provinces, "Id", "ProvinceName");

        var colleges = await _context.Colleges.Where(c => c.IsActive).AsNoTracking().ToListAsync();
        ViewBag.Colleges = new SelectList(colleges, "Id", "Name");
    }

    private static (string first, string? middle, string last) SplitName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => ("", null, ""),
            1 => (parts[0], null, ""),
            2 => (parts[0], null, parts[1]),
            _ => (parts[0], string.Join(" ", parts[1..^1]), parts[^1])
        };
    }
}
