using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using Microsoft.Extensions.Logging;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class EntranceExamApplicationService(AppDbContext context, UserManager<AppUser> userManager, IEmailService emailService, ISmsService smsService) : IEntranceExamApplicationService
{
    private const string EntranceExamTypeCode = "4";

    public async Task<int> SubmitApplicationAsync(EntranceExamApplication application, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber)
    {
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
            context.Addresses.Add(permanentAddress);
            await context.SaveChangesAsync();
            application.PermanentAddressId = permanentAddress.Id;
        }

        application.Status = ApplicationStatus.Submitted;
        application.CreatedAt = DateTime.UtcNow;

        context.EntranceExamApplications.Add(application);
        await context.SaveChangesAsync();

        await SendEntranceSubmissionNotificationsAsync(application);

        return application.Id;
    }

    public async Task<EntranceExamApplication?> GetApplicationByIdAsync(int id)
    {
        return await context.EntranceExamApplications
            .Include(a => a.AcademicYear)
            .Include(a => a.College)
            .Include(a => a.Program)
            .Include(a => a.Gender)
            .Include(a => a.PermanentAddress)
                .ThenInclude(pa => pa != null ? pa.LocalLevel : null)
                    .ThenInclude(ll => ll != null ? ll.District : null)
                        .ThenInclude(d => d != null ? d.Province : null)
            .Include(a => a.PreviousLevel)
            .Include(a => a.PreviousLevel2)
            .Include(a => a.PreviousLevel3)
            .Include(a => a.CitizenshipDistrict)
            .Include(a => a.ApplicationVoucher)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<EntranceExamApplication?> GetApplicationByVoucherIdAsync(int voucherId)
    {
        return await context.EntranceExamApplications
            .Include(a => a.AcademicYear)
            .Include(a => a.College)
            .Include(a => a.Program)
            .Include(a => a.Gender)
            .Include(a => a.PermanentAddress)
                .ThenInclude(pa => pa != null ? pa.LocalLevel : null)
                    .ThenInclude(ll => ll != null ? ll.District : null)
                        .ThenInclude(d => d != null ? d.Province : null)
            .Include(a => a.PreviousLevel)
            .Include(a => a.PreviousLevel2)
            .Include(a => a.PreviousLevel3)
            .Include(a => a.CitizenshipDistrict)
            .Include(a => a.ApplicationVoucher)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ApplicationVoucherId == voucherId);
    }

    public async Task<(List<EntranceExamApplicationListDto> Data, int TotalCount)> GetPagedApplicationsAsync(string? search, ApplicationStatus? status, int? programId, int? academicYearId, int page, int pageSize)
    {
        var query = context.EntranceExamApplications
            .Include(a => a.AcademicYear)
            .Include(a => a.College)
            .Include(a => a.Program)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            query = query.Where(a =>
                (a.FirstName != null && a.FirstName.ToLower().Contains(lowerSearch)) ||
                (a.LastName != null && a.LastName.ToLower().Contains(lowerSearch)) ||
                (a.Email != null && a.Email.ToLower().Contains(lowerSearch)) ||
                (a.ContactNumber != null && a.ContactNumber.ToLower().Contains(lowerSearch)));
        }

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (programId.HasValue)
            query = query.Where(a => a.ProgramId == programId.Value);

        if (academicYearId.HasValue)
            query = query.Where(a => a.AcademicYearId == academicYearId.Value);

        var totalCount = await query.CountAsync();
        var skip = (page - 1) * pageSize;

        var data = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(a => new EntranceExamApplicationListDto
            {
                Id = a.Id,
                FullName = (a.FirstName + " " + a.LastName).Trim(),
                Email = a.Email ?? "-",
                ContactNumber = a.ContactNumber ?? "-",
                AcademicYear = a.AcademicYear != null ? a.AcademicYear.AcademicYearName : "-",
                College = a.College != null ? a.College.Name : "-",
                Program = a.Program != null ? a.Program.ProgramName : "-",
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task ReviewApplicationAsync(int id, ApplicationStatus status, string? remarks)
    {
        var application = await context.EntranceExamApplications.FindAsync(id);
        if (application != null)
        {
            application.Status = status;
            application.ReviewDate = DateTime.UtcNow;
            application.ReviewRemarks = remarks;
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteApplicationAsync(int id)
    {
        var application = await context.EntranceExamApplications.FindAsync(id);
        if (application != null)
        {
            context.EntranceExamApplications.Remove(application);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ApplicationExistsAsync(int id)
    {
        return await context.EntranceExamApplications.AnyAsync(a => a.Id == id);
    }

    public async Task<EntranceExamApplicationSelectListsDto> GetSelectListsAsync()
    {
        var academicYears = await context.AcademicYears.Where(ay => ay.AcademicYearName != null && ay.IsActive).AsNoTracking().ToListAsync();
        var colleges = await context.Colleges.Where(c => c.Name != null && c.IsActive).AsNoTracking().ToListAsync();
        var programs = await context.Programs.Where(p => p.ProgramName != null && p.IsActive).AsNoTracking().ToListAsync();
        var genders = await context.Genders.Where(g => g.GenderName != null && g.IsActive).AsNoTracking().ToListAsync();
        var previousLevels = await context.PreviousLevels.Where(pl => pl.PreviousLevelName != null && pl.IsActive).AsNoTracking().ToListAsync();
        var provinces = await context.Provinces.AsNoTracking().ToListAsync();

        return new EntranceExamApplicationSelectListsDto
        {
            AcademicYears = academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName }).ToList(),
            Colleges = colleges.Select(c => new SelectOption { Id = c.Id, Name = c.Name }).ToList(),
            Programs = programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName }).ToList(),
            Genders = genders.Select(g => new SelectOption { Id = g.Id, Name = g.GenderName }).ToList(),
            PreviousLevels = previousLevels.Select(pl => new SelectOption { Id = pl.Id, Name = pl.PreviousLevelName }).ToList(),
            Provinces = provinces.Select(p => new SelectOption { Id = p.Id, Name = p.ProvinceName }).ToList(),
        };
    }

    public async Task<List<SelectOption>> GetDistrictsByProvinceAsync(int provinceId)
    {
        return await context.Districts
            .Where(d => d.ProvinceId == provinceId && d.IsActive)
            .Select(d => new SelectOption { Id = d.Id, Name = d.DistrictName })
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetLocalLevelsByDistrictAsync(int districtId)
    {
        return await context.LocalLevels
            .Where(l => l.DistrictId == districtId && l.IsActive)
            .Select(l => new SelectOption { Id = l.Id, Name = l.LocalLevelName })
            .ToListAsync();
    }

    public List<Province> GetProvinces()
    {
        return context.Provinces.AsNoTracking().ToList();
    }

    public async Task<List<EntranceExamApplication>> GetAllApplicationsAsync(string? search, ApplicationStatus? status, int? programId, int? academicYearId)
    {
        var query = context.EntranceExamApplications
            .Include(a => a.AcademicYear)
            .Include(a => a.College)
            .Include(a => a.Program)
            .Include(a => a.Gender)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            query = query.Where(a =>
                (a.FirstName != null && a.FirstName.ToLower().Contains(lowerSearch)) ||
                (a.LastName != null && a.LastName.ToLower().Contains(lowerSearch)) ||
                (a.Email != null && a.Email.ToLower().Contains(lowerSearch)) ||
                (a.ContactNumber != null && a.ContactNumber.ToLower().Contains(lowerSearch)));
        }

        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (programId.HasValue) query = query.Where(a => a.ProgramId == programId.Value);
        if (academicYearId.HasValue) query = query.Where(a => a.AcademicYearId == academicYearId.Value);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<int> ConvertToAdmissionAsync(int applicationId)
    {
        var application = await context.EntranceExamApplications
            .Include(a => a.PermanentAddress)
            .Include(a => a.College)
            .Include(a => a.Program)
                .ThenInclude(p => p!.Level)
            .FirstOrDefaultAsync(a => a.Id == applicationId)
            ?? throw new InvalidOperationException("Application not found.");

        if (application.Status != ApplicationStatus.Approved)
            throw new InvalidOperationException("Only approved applications can be converted to admission.");

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var existingRegistration = await context.StudentRegistrations
                .FirstOrDefaultAsync(sr => sr.Email == application.Email);

            int studentRegistrationId;
            if (existingRegistration != null)
            {
                studentRegistrationId = existingRegistration.Id;
            }
            else
            {
                var registration = new StudentRegistration
                {
                    AcademicYearId = application.AcademicYearId,
                    CollegeId = application.CollegeId,
                    ProgramId = application.ProgramId,
                    LevelId = application.Program?.LevelId ?? 0,
                    DepartmentId = application.Program?.DepartmentId ?? 0,
                    FirstName = application.FirstName,
                    MiddleName = application.MiddleName,
                    LastName = application.LastName!,
                    NepaliName = application.NepaliName,
                    DateOfBirthBS = application.DateOfBirthBS,
                    DateOfBirthAD = application.DateOfBirthAD,
                    Email = application.Email,
                    ContactNumber = application.ContactNumber,
                    Phone = application.Phone,
                    GenderId = application.GenderId,
                    PermanentAddressId = application.PermanentAddressId,
                    StudentCategoryId = 1,
                    IsActive = true,
                    EntranceRollNumber = application.Id.ToString()
                };

                context.StudentRegistrations.Add(registration);
                await context.SaveChangesAsync();
                studentRegistrationId = registration.Id;
            }

            var existingAdmission = await context.StudentAdmissions
                .FirstOrDefaultAsync(sa => sa.AppUserId != null
                    && context.StudentRegistrations.Any(sr => sr.Id == studentRegistrationId && sr.Email == application.Email)
                    && sa.ProgramsId == application.ProgramId
                    && sa.CollegeId == application.CollegeId);

            if (existingAdmission != null)
            {
                await transaction.CommitAsync();
                return existingAdmission.Id;
            }

            var appUser = await userManager.FindByEmailAsync(application.Email);
            var admission = new StudentAdmission
            {
                TenantId = application.TenantId,
                ProgramsId = application.ProgramId,
                CollegeId = application.CollegeId,
                AdmissionDate = DateTime.UtcNow,
                IsActive = true,
                IsCompleted = false,
                AppUserId = appUser?.Id,
                CollegeRollNumber = await GenerateCollegeRollNumberAsync(application.CollegeId, application.ProgramId)
            };

            context.StudentAdmissions.Add(admission);
            await context.SaveChangesAsync();

            await transaction.CommitAsync();
            return admission.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<string> GenerateCollegeRollNumberAsync(int collegeId, int programId)
    {
        var college = await context.Colleges.AsNoTracking().FirstOrDefaultAsync(c => c.Id == collegeId);
        var program = await context.Programs.AsNoTracking().FirstOrDefaultAsync(p => p.Id == programId);
        var collegeCode = college?.Code ?? "CLG";
        var programCode = program?.ShortName ?? "PROG";
        var year = DateTime.Now.Year.ToString();
        var runningYear = await context.AcademicYears
            .Where(ay => ay.IsRunning)
            .Select(ay => ay.AcademicYearName)
            .FirstOrDefaultAsync() ?? year;

        var count = await context.StudentAdmissions
            .CountAsync(sa => sa.CollegeId == collegeId && sa.ProgramsId == programId) + 1;

        return $"{collegeCode}/{programCode}/{runningYear}/{count:D4}";
    }

    public async Task<bool> IsExamScheduleOpenAsync(int programId, int collegeId, int academicYearId)
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var entranceTypeId = await context.ExamTypes
            .Where(et => et.Code == EntranceExamTypeCode)
            .Select(et => et.Id)
            .FirstOrDefaultAsync();
        return await context.ExamSchedules
            .AnyAsync(es => es.ProgramId == programId
                && es.CollegeId == collegeId
                && es.AcademicYearId == academicYearId
                && es.IsActive
                && es.StartDate <= now
                && es.EndDate >= now
                && es.ExamTypeId == entranceTypeId);
    }

    public async Task<ApplicationVoucher?> VerifyPaymentAsync(string transactionCode, string fullName, string contactNumber)
    {
        if (string.IsNullOrWhiteSpace(transactionCode)) return null;

        var code = transactionCode.Trim();

        // 1. Try by VoucherNumber exact match
        var voucher = await context.ApplicationVouchers
            .Include(v => v.ExamSchedule)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VoucherNumber != null && v.VoucherNumber == code);

        if (voucher != null)
        {
            if (MatchesNameAndPhone(voucher, fullName, contactNumber))
                return voucher;
            return null;
        }

        // 2. Try by last 6 chars of VoucherNumber
        var last6 = code.Length >= 6 ? code[^6..] : code;
        var vouchers = await context.ApplicationVouchers
            .Include(v => v.ExamSchedule)
            .AsNoTracking()
            .Where(v => v.VoucherNumber != null && v.VoucherNumber.Length >= last6.Length)
            .ToListAsync();

        var match = vouchers.FirstOrDefault(v =>
            v.VoucherNumber != null &&
            v.VoucherNumber[^last6.Length..] == last6);

        if (match != null && MatchesNameAndPhone(match, fullName, contactNumber))
            return match;

        // 3. Try by eSewa transaction code (stored in PaymentRequestLog.TransactionId)
        var paymentLog = await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(prl => prl.TransactionId == code);

        if (paymentLog?.ExamScheduleId != null)
        {
            var voucherBySchedule = await context.ApplicationVouchers
                .Include(v => v.ExamSchedule)
                .AsNoTracking()
                .FirstOrDefaultAsync(v =>
                    v.ExamScheduleId == paymentLog.ExamScheduleId &&
                    v.StudentName != null &&
                    v.StudentName == paymentLog.FullName &&
                    v.ContactNumber != null &&
                    v.ContactNumber == paymentLog.MobileNumber);

            if (voucherBySchedule != null && MatchesNameAndPhone(voucherBySchedule, fullName, contactNumber))
                return voucherBySchedule;
        }

        return null;
    }

    private static bool MatchesNameAndPhone(ApplicationVoucher voucher, string fullName, string contactNumber)
    {
        var nameMatch = string.IsNullOrWhiteSpace(fullName) ||
            (voucher.StudentName != null &&
             voucher.StudentName.Contains(fullName.Trim(), StringComparison.OrdinalIgnoreCase));

        var phoneMatch = string.IsNullOrWhiteSpace(contactNumber) ||
            (voucher.ContactNumber != null &&
             voucher.ContactNumber.Contains(contactNumber.Trim()));

        return nameMatch && phoneMatch;
    }

    public async Task<EntranceExamApplicationSelectListsDto> GetStepFormSelectListsAsync()
    {
        var programs = await context.Programs.Where(p => p.ProgramName != null && p.IsActive).AsNoTracking().ToListAsync();
        var colleges = await context.Colleges.Where(c => c.Name != null && c.IsActive).AsNoTracking().ToListAsync();
        var academicYears = await context.AcademicYears.Where(ay => ay.AcademicYearName != null && ay.IsActive).AsNoTracking().ToListAsync();
        var genders = await context.Genders.Where(g => g.GenderName != null && g.IsActive).AsNoTracking().ToListAsync();
        var previousLevels = await context.PreviousLevels.Where(pl => pl.PreviousLevelName != null && pl.IsActive).AsNoTracking().ToListAsync();
        var provinces = await context.Provinces.AsNoTracking().ToListAsync();
        var districts = await context.Districts.Where(d => d.IsActive).AsNoTracking().ToListAsync();

        return new EntranceExamApplicationSelectListsDto
        {
            Programs = programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName }).ToList(),
            Colleges = colleges.Select(c => new SelectOption { Id = c.Id, Name = c.Name }).ToList(),
            AcademicYears = academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName }).ToList(),
            Genders = genders.Select(g => new SelectOption { Id = g.Id, Name = g.GenderName }).ToList(),
            PreviousLevels = previousLevels.Select(pl => new SelectOption { Id = pl.Id, Name = pl.PreviousLevelName }).ToList(),
            Provinces = provinces.Select(p => new SelectOption { Id = p.Id, Name = p.ProvinceName }).ToList(),
            Districts = districts.Select(d => new SelectOption { Id = d.Id, Name = d.DistrictName }).ToList(),
        };
    }

    public async Task<int> SubmitStepApplicationAsync(EntranceExamApplication application, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber, int voucherId)
    {
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
            context.Addresses.Add(permanentAddress);
            await context.SaveChangesAsync();
            application.PermanentAddressId = permanentAddress.Id;
        }

        application.ApplicationVoucherId = voucherId;
        application.PaymentVerified = true;
        application.Status = ApplicationStatus.Submitted;
        application.CreatedAt = DateTime.UtcNow;

        context.EntranceExamApplications.Add(application);
        await context.SaveChangesAsync();

        await SendEntranceSubmissionNotificationsAsync(application);

        return application.Id;
    }

    private async Task SendEntranceSubmissionNotificationsAsync(EntranceExamApplication application)
    {
        var fullName = $"{application.FirstName} {application.LastName}".Trim();
        var program = await context.Programs.Where(p => p.Id == application.ProgramId).Select(p => p.ProgramName).FirstOrDefaultAsync();
        var college = await context.Colleges.Where(c => c.Id == application.CollegeId).Select(c => c.Name).FirstOrDefaultAsync();

        try
        {
            if (!string.IsNullOrWhiteSpace(application.Email))
            {
                var emailBody = $@"
                    <h3>Dear {fullName},</h3>
                    <p>Your entrance exam application has been submitted successfully.</p>
                    <p><strong>Details:</strong></p>
                    <ul>
                        <li><strong>College:</strong> {college}</li>
                        <li><strong>Program:</strong> {program}</li>
                        <li><strong>Application ID:</strong> {application.Id}</li>
                        <li><strong>Date:</strong> {application.CreatedAt:yyyy-MM-dd}</li>
                    </ul>
                    <p>You will be notified once your application is reviewed.</p>
                    <br/>
                    <p>Regards,<br/>Far-Western University</p>";
                await emailService.SendEmailAsync(application.Email, "Entrance Exam Application Submitted", emailBody);
            }
        }
        catch { }

        try
        {
            var phone = application.ContactNumber ?? application.Phone;
            if (!string.IsNullOrWhiteSpace(phone))
            {
                var smsMessage = $"Dear {fullName}, your entrance application for {program} at {college} has been submitted successfully. Application ID: {application.Id}. - FWU";
                await smsService.SendSmsAsync(phone, smsMessage);
            }
        }
        catch { }
    }

    public async Task<int> UpdateStepApplicationAsync(EntranceExamApplication application, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber, int voucherId, int applicationId)
    {
        var existing = await context.EntranceExamApplications
            .Include(a => a.PermanentAddress)
            .FirstOrDefaultAsync(a => a.Id == applicationId)
            ?? throw new InvalidOperationException("Application not found.");

        if (existing.Status != ApplicationStatus.Submitted && existing.Status != ApplicationStatus.Rejected)
            throw new InvalidOperationException("Application cannot be edited in its current state.");

        existing.AcademicYearId = application.AcademicYearId;
        existing.CollegeId = application.CollegeId;
        existing.ProgramId = application.ProgramId;
        existing.FirstName = application.FirstName;
        existing.MiddleName = application.MiddleName;
        existing.LastName = application.LastName;
        existing.NepaliName = application.NepaliName;
        existing.DateOfBirthBS = application.DateOfBirthBS;
        existing.DateOfBirthAD = application.DateOfBirthAD;
        existing.GenderId = application.GenderId;
        existing.Email = application.Email;
        existing.ContactNumber = application.ContactNumber;
        existing.Phone = application.Phone;
        existing.FatherName = application.FatherName;
        existing.FatherContact = application.FatherContact;
        existing.FatherProfession = application.FatherProfession;
        existing.MotherName = application.MotherName;
        existing.MotherContact = application.MotherContact;
        existing.MotherProfession = application.MotherProfession;
        existing.GuardianEmail = application.GuardianEmail;
        existing.CitizenshipNo = application.CitizenshipNo;
        existing.CitizenshipDistrictId = application.CitizenshipDistrictId;
        existing.CitizenshipIssueDateBs = application.CitizenshipIssueDateBs;
        existing.CitizenshipIssueDateAd = application.CitizenshipIssueDateAd;
        existing.BloodGroup = application.BloodGroup;
        existing.BirthPlace = application.BirthPlace;
        existing.Country = application.Country;
        existing.PostalCode = application.PostalCode;
        existing.PreviousLevelId = application.PreviousLevelId;
        existing.PreviousSchoolCollege = application.PreviousSchoolCollege;
        existing.PreviousPassedYear = application.PreviousPassedYear;
        existing.PreviousSymbolNumber = application.PreviousSymbolNumber;
        existing.PreviousGPA = application.PreviousGPA;
        existing.PreviousDivision = application.PreviousDivision;
        existing.PreviousLevel2Id = application.PreviousLevel2Id;
        existing.PreviousSchoolCollege2 = application.PreviousSchoolCollege2;
        existing.PreviousBoard2 = application.PreviousBoard2;
        existing.PreviousSymbolNumber2 = application.PreviousSymbolNumber2;
        existing.PreviousPassedYear2 = application.PreviousPassedYear2;
        existing.PreviousGPA2 = application.PreviousGPA2;
        existing.PreviousDivision2 = application.PreviousDivision2;
        existing.PreviousLevel3Id = application.PreviousLevel3Id;
        existing.PreviousSchoolCollege3 = application.PreviousSchoolCollege3;
        existing.PreviousBoard3 = application.PreviousBoard3;
        existing.PreviousSymbolNumber3 = application.PreviousSymbolNumber3;
        existing.PreviousPassedYear3 = application.PreviousPassedYear3;
        existing.PreviousGPA3 = application.PreviousGPA3;
        existing.PreviousDivision3 = application.PreviousDivision3;

        if (existing.Status == ApplicationStatus.Rejected)
            existing.Status = ApplicationStatus.Submitted;

        if (!string.IsNullOrEmpty(permanentLocalLevelId))
        {
            var newAddress = new Address
            {
                LocalLevelId = int.Parse(permanentLocalLevelId),
                WardNumber = string.IsNullOrEmpty(permanentWardNumber) ? null : int.Parse(permanentWardNumber),
                ToleStreet = permanentToleStreet,
                HouseNumber = permanentHouseNumber,
                AddressType = AddressType.Permanent,
                IsActive = true
            };

            if (existing.PermanentAddressId.HasValue)
            {
                context.Addresses.Remove(existing.PermanentAddress!);
            }

            context.Addresses.Add(newAddress);
            await context.SaveChangesAsync();
            existing.PermanentAddressId = newAddress.Id;
        }

        if (!string.IsNullOrEmpty(application.PhotoPath))
            existing.PhotoPath = application.PhotoPath;

        if (!string.IsNullOrEmpty(application.DocumentsPath))
            existing.DocumentsPath = application.DocumentsPath;

        if (!string.IsNullOrEmpty(application.VoucherPath))
            existing.VoucherPath = application.VoucherPath;

        await context.SaveChangesAsync();
        return existing.Id;
    }

    public async Task<List<SelectOption>> GetCollegesByProgramAsync(int programId)
    {
        return await context.CollegePrograms
            .Where(cp => cp.ProgramId == programId && cp.College.IsActive)
            .Select(cp => new SelectOption { Id = cp.College.Id, Name = cp.College.Name })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetDistrictsAsync()
    {
        return await context.Districts
            .Where(d => d.IsActive)
            .Select(d => new SelectOption { Id = d.Id, Name = d.DistrictName })
            .ToListAsync();
    }

    public async Task<decimal?> GetEntranceFeeForProgramAsync(int programId, int academicYearId)
    {
        return await context.ExamSchedules
            .Where(es => es.ProgramId == programId && es.AcademicYearId == academicYearId && es.IsActive)
            .Select(es => es.ExamFee)
            .FirstOrDefaultAsync();
    }

    public async Task<List<AvailableScheduleDto>> GetAvailableExamSchedulesAsync()
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var entranceTypeId = await context.ExamTypes
            .Where(et => et.Code == EntranceExamTypeCode)
            .Select(et => et.Id)
            .FirstOrDefaultAsync();
        return await context.ExamSchedules
            .Include(es => es.Program)
            .Include(es => es.College)
            .Include(es => es.AcademicYear)
            .Include(es => es.Semester)
            .Where(es => es.IsActive && es.EndDate >= now && es.ExamFee != null && es.ExamTypeId == entranceTypeId)
            .OrderBy(es => es.EndDate)
            .Select(es => new AvailableScheduleDto
            {
                Id = es.Id,
                ExamScheduleName = es.ExamScheduleName,
                ProgramName = es.Program != null ? es.Program.ProgramName : null,
                CollegeName = es.College != null ? es.College.Name : null,
                AcademicYearName = es.AcademicYear != null ? es.AcademicYear.AcademicYearName : null,
                SemesterName = es.Semester != null ? es.Semester.Name : null,
                ExamFee = es.ExamFee,
                StartDate = es.StartDate,
                EndDate = es.EndDate,
                StartDateBs = es.StartDateBs,
                EndDateBs = es.EndDateBs
            })
            .ToListAsync();
    }

    public async Task<ApplicationVoucher?> GetVoucherByIdAsync(int voucherId)
    {
        return await context.ApplicationVouchers
            .Include(v => v.ExamSchedule)
                .ThenInclude(es => es!.Program)
            .Include(v => v.ExamSchedule)
                .ThenInclude(es => es!.College)
            .Include(v => v.ExamSchedule)
                .ThenInclude(es => es!.AcademicYear)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == voucherId);
    }

    public async Task<List<PaymentType>> GetActivePaymentTypesAsync()
    {
        return await context.Set<PaymentType>()
            .Where(pt => pt.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> HasExistingVoucherAsync(int scheduleId, string studentName, string contactNumber)
    {
        return await context.ApplicationVouchers
            .AnyAsync(v => v.ExamScheduleId == scheduleId
                && v.StudentName == studentName
                && v.ContactNumber == contactNumber);
    }

    public async Task<int> CreateEsewaPaymentLogAsync(int scheduleId, string studentName, string contactNumber, int paymentTypeId, string transactionUuid)
    {
        var schedule = await context.ExamSchedules.FindAsync(scheduleId);
        if (schedule == null || !schedule.IsActive || schedule.ExamFee == null)
            return 0;

        var paymentRequest = new PaymentRequestLog
        {
            TenantId = schedule.TenantId,
            InvoiceNumber = transactionUuid,
            FullName = studentName,
            MobileNumber = contactNumber,
            FullRequestContent = "{}",
            Amount = schedule.ExamFee.Value,
            ForwardedTimestamp = DateTime.UtcNow,
            PaymentTypeId = paymentTypeId,
            ExamScheduleId = scheduleId,
            StudentCount = 1
        };

        context.PaymentRequestLogs.Add(paymentRequest);
        await context.SaveChangesAsync();
        return paymentRequest.Id;
    }

    public async Task<int?> GetPaymentLogIdByTransactionUuidAsync(string transactionUuid)
    {
        var log = await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(prl => prl.InvoiceNumber == transactionUuid);
        return log?.Id;
    }

    public async Task LogEsewaResponseAsync(int logId, string? transactionCode, bool isSuccess, string responseData, string? responseMessage = null)
    {
        var log = await context.Set<PaymentRequestLog>().FindAsync(logId);
        if (log == null) return;

        log.TransactionId = transactionCode;
        log.PaymentRequestLogStatus = isSuccess ? 1 : 0;

        context.Set<PaymentResponseLog>().Add(new PaymentResponseLog
        {
            PaymentRequestLogId = logId,
            IsSuccess = isSuccess,
            ResponseMessage = responseMessage,
            FullResponse = responseData,
            ResponseTimestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    public async Task<ApplicationVoucher?> CompleteEsewaPaymentAsync(int logId, decimal amount)
    {
        var log = await context.Set<PaymentRequestLog>().FindAsync(logId);
        if (log == null) return null;

        if (log.PaymentRequestLogStatus == 1)
            return await context.ApplicationVouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.ExamScheduleId == log.ExamScheduleId
                    && v.StudentName == log.FullName
                    && v.ContactNumber == log.MobileNumber);

        var schedule = await context.ExamSchedules.FindAsync(log.ExamScheduleId);
        if (schedule == null) return null;

        var voucherNumber = $"VCH-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        log.PaymentRequestLogStatus = 1;

        var voucher = new ApplicationVoucher
        {
            TenantId = schedule.TenantId,
            VoucherNumber = voucherNumber,
            StudentName = log.FullName,
            ContactNumber = log.MobileNumber,
            Amount = amount,
            VoucherDate = DateTime.UtcNow,
            Timestamp = DateTime.UtcNow,
            ExamScheduleId = log.ExamScheduleId
        };

        context.ApplicationVouchers.Add(voucher);
        await context.SaveChangesAsync();

        return voucher;
    }

    public async Task<string?> GetPaymentTypeNameByIdAsync(int paymentTypeId)
    {
        var pt = await context.Set<PaymentType>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == paymentTypeId);
        return pt?.PaymentTypeName;
    }

    public async Task<ApplicationVoucher?> InitiatePaymentAsync(int scheduleId, string studentName, string contactNumber, int paymentTypeId)
    {
        var schedule = await context.ExamSchedules.FindAsync(scheduleId);
        if (schedule == null || !schedule.IsActive || schedule.ExamFee == null)
            return null;

        var voucherNumber = $"VCH-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var voucher = new ApplicationVoucher
        {
            TenantId = schedule.TenantId,
            VoucherNumber = voucherNumber,
            StudentName = studentName,
            ContactNumber = contactNumber,
            Amount = schedule.ExamFee.Value,
            VoucherDate = DateTime.UtcNow,
            Timestamp = DateTime.UtcNow,
            ExamScheduleId = scheduleId
        };

        context.ApplicationVouchers.Add(voucher);
        await context.SaveChangesAsync();

        var paymentRequest = new PaymentRequestLog
        {
            TenantId = schedule.TenantId,
            InvoiceNumber = voucherNumber,
            FullName = studentName,
            MobileNumber = contactNumber,
            FullRequestContent = "{}",
            Amount = schedule.ExamFee.Value,
            ForwardedTimestamp = DateTime.UtcNow,
            PaymentTypeId = paymentTypeId,
            ExamScheduleId = scheduleId,
            StudentCount = 1
        };

        context.PaymentRequestLogs.Add(paymentRequest);
        await context.SaveChangesAsync();

        return voucher;
    }
}