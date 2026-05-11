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

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var registration = await _context.StudentRegistrations
            .Include(s => s.PermanentAddress)
            .Include(s => s.StudentGuardians)
            .Include(s => s.StudentQualifications)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (registration == null) return NotFound();

        var program = await _context.Programs
            .Where(p => p.LevelId == registration.LevelId && p.FacultyId == registration.FacultyId)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        var model = new AdmissionFormViewModel
        {
            ProgrammeId = program?.Id,
            CollegeId = registration.CollegeId,
            Name = string.Join(" ", new[] { registration.FirstName, registration.MiddleName, registration.LastName }.Where(x => !string.IsNullOrEmpty(x))),
            DobBS = registration.DateOfBirthBS ?? "",
            DobAD = DateTime.TryParse(registration.DateOfBirthAD, out var dobAd) ? dobAd : null,
            Gender = registration.GenderId.ToString(),
            CitizenshipNo = registration.CitizenshipNo ?? "",
            CitizenshipDistrict = registration.CitizenshipDistrict ?? "",
            CitizenshipIssueDate = registration.CitizenshipIssueDate,
            NationalId = registration.NationalId,
            BloodGroup = registration.BloodGroup,
            BirthPlace = registration.BirthPlace,
            PermMunicipality = "",
            PermWard = "",
            PermDistrict = "",
            PermProvince = "",
            PermCountry = "",
            PhoneLandline = registration.Phone,
            Mobile = registration.ContactNumber ?? "",
            Email = registration.Email ?? "",
            FatherName = "",
            FatherPhone = "",
            FatherProfession = "",
            MotherName = "",
            MotherPhone = "",
            GuardianEmail = "",
            AcademicQualifications = []
        };

        if (registration.PermanentAddress != null)
        {
            var parts = registration.PermanentAddress.FullAddress?.Split(',', StringSplitOptions.TrimEntries) ?? [];
            if (parts.Length >= 5)
            {
                model.PermMunicipality = parts[0];
                model.PermWard = parts[1].Replace("Ward ", "");
                model.PermDistrict = parts[2];
                model.PermProvince = parts[3];
                model.PermCountry = parts[4];
            }
        }

        var guardian = registration.StudentGuardians?.FirstOrDefault();
        if (guardian != null)
        {
            model.FatherName = guardian.FatherName ?? "";
            model.FatherPhone = guardian.FatherContactNumber ?? "";
            model.FatherProfession = guardian.FatherProfession ?? "";
            model.MotherName = guardian.MotherName ?? "";
            model.MotherPhone = guardian.MotherContactNumber ?? "";
            model.GuardianEmail = guardian.GuardianEmail ?? "";
        }

        foreach (var q in registration.StudentQualifications ?? [])
        {
            model.AcademicQualifications.Add(new AcademicQualificationViewModel
            {
                Level = q.ProgramName,
                BoardUniversity = q.InstituteName,
                SymbolNo = q.ExamRollNumber,
                Year = q.PassedYear,
                PercentCGPA = q.Percentage?.ToString(),
                Division = q.Division
            });
        }

        while (model.AcademicQualifications.Count < 4)
            model.AcademicQualifications.Add(new AcademicQualificationViewModel());

        await PopulateSelectListsAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdmissionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return View(model);
        }

        var registration = await _context.StudentRegistrations
            .Include(s => s.PermanentAddress)
            .Include(s => s.StudentGuardians)
            .Include(s => s.StudentQualifications)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (registration == null) return NotFound();

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

        registration.LevelId = program.LevelId;
        registration.FacultyId = program.FacultyId;
        registration.CollegeId = model.CollegeId;
        registration.AcademicYearId = runningYear?.Id ?? 0;
        registration.FirstName = nameParts.first;
        registration.MiddleName = nameParts.middle;
        registration.LastName = nameParts.last;
        registration.DateOfBirthBS = model.DobBS;
        registration.DateOfBirthAD = model.DobAD?.ToString("yyyy-MM-dd");
        registration.GenderId = int.TryParse(model.Gender, out var gId) ? gId : 0;
        registration.ContactNumber = model.Mobile?.Length > 20 ? model.Mobile.Substring(0, 20) : model.Mobile;
        registration.Phone = model.PhoneLandline?.Length > 15 ? model.PhoneLandline.Substring(0, 15) : model.PhoneLandline;
        registration.Email = model.Email;
        registration.CitizenshipNo = model.CitizenshipNo;
        registration.CitizenshipDistrict = model.CitizenshipDistrict;
        registration.CitizenshipIssueDate = model.CitizenshipIssueDate;
        registration.BirthPlace = model.BirthPlace;
        registration.NationalId = model.NationalId;
        registration.BloodGroup = model.BloodGroup;

        var localLevel = await _context.LocalLevels
            .Where(ll => ll.LocalLevelName.Contains(model.PermMunicipality) || model.PermMunicipality.Contains(ll.LocalLevelName))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (registration.PermanentAddress == null)
        {
            registration.PermanentAddress = new Address();
            _context.Addresses.Add(registration.PermanentAddress);
            await _context.SaveChangesAsync();
            registration.PermanentAddressId = registration.PermanentAddress.Id;
        }

        registration.PermanentAddress.LocalLevelId = localLevel?.Id ?? 1;
        registration.PermanentAddress.FullAddress = $"{model.PermMunicipality}, Ward {model.PermWard}, {model.PermDistrict}, {model.PermProvince}, {model.PermCountry}";

        var guardian = registration.StudentGuardians?.FirstOrDefault();
        if (guardian == null)
        {
            guardian = new StudentGuardian { StudentRegistrationId = registration.Id };
            _context.StudentGuardians.Add(guardian);
        }

        guardian.FatherName = model.FatherName;
        guardian.FatherContactNumber = model.FatherPhone;
        guardian.FatherProfession = model.FatherProfession;
        guardian.MotherName = model.MotherName ?? "";
        guardian.MotherContactNumber = model.MotherPhone;
        guardian.GuardianEmail = model.GuardianEmail;
        guardian.GuardianName = model.FatherName;

        var existingQuals = registration.StudentQualifications?.ToList() ?? [];
        _context.StudentQualifications.RemoveRange(existingQuals);

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

            _context.StudentQualifications.Add(new StudentQualification
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
            });
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Admission form updated successfully!";
        return RedirectToAction(nameof(Index));
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

    [HttpGet]
    public async Task<IActionResult> GetDistrictsByProvince(int provinceId)
    {
        var districts = await _context.Districts
            .Where(d => d.ProvinceId == provinceId && d.IsActive)
            .AsNoTracking()
            .Select(d => new { id = d.Id, name = d.DistrictName })
            .ToListAsync();
        return Json(districts);
    }

    [HttpGet]
    public async Task<IActionResult> GetLocalLevelsByDistrict(int districtId)
    {
        var localLevels = await _context.LocalLevels
            .Where(ll => ll.DistrictId == districtId && ll.IsActive)
            .AsNoTracking()
            .Select(ll => new { id = ll.Id, name = ll.LocalLevelName })
            .ToListAsync();
        return Json(localLevels);
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
