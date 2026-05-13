using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
[Authorize(Roles = "Student")]
public class StudentDashboardController(
    IStudentDashboardService dashboardService,
    UserManager<AppUser> userManager,
    IESewaService esewaService,
    IConfiguration configuration)
    : Controller
{
    public async Task<IActionResult> Profile()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null)
        {
            return View(new StudentProfileViewModel
            {
                FullName = user.FullName ?? user.Email,
                Email = user.Email
            });
        }

        var vm = new StudentProfileViewModel
        {
            RegistrationId = registration.Id,
            RegistrationNumber = registration.RegistrationNumber,
            FullName = $"{registration.FirstName} {registration.MiddleName} {registration.LastName}".Replace("  ", " "),
            NepaliName = registration.NepaliName,
            Gender = registration.Gender?.GenderName,
            DateOfBirthBS = registration.DateOfBirthBS,
            DateOfBirthAD = registration.DateOfBirthAD,
            Ethnicity = registration.Ethnicity?.EthnicityName,
            Category = registration.StudentCategory?.StudentCategoryName,
            ContactNumber = registration.ContactNumber,
            Email = registration.Email,
            PhotoPath = user.ProfilePath,
            BloodGroup = registration.BloodGroup,
            Nationality = registration.Nationality,
            Religion = registration.Religion,
            AcademicYear = registration.AcademicYear?.AcademicYearName,
            Faculty = registration.Faculty?.FacultyName,
            College = registration.College?.Name,
            Level = registration.Level?.LevelName,
            Address = registration.PermanentAddress?.FullAddress
                ?? registration.PermanentAddress?.ToleStreet
        };

        return View(vm);
    }

    public async Task<IActionResult> ExamForms()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null)
        {
            return View(new ExamFormsListViewModel());
        }

        var schedules = await dashboardService.GetExamSchedulesForStudentAsync(registration);
        var forms = new List<ExamFormViewModel>();

        foreach (var schedule in schedules)
        {
            var hasPaid = await dashboardService.HasExistingPaymentAsync(schedule.Id, registration.Id);
            var amount = await dashboardService.GetExamFeeForScheduleAsync(schedule.Id);

            forms.Add(new ExamFormViewModel
            {
                ExamScheduleId = schedule.Id,
                Level = schedule.Program.Level?.LevelName,
                Program = schedule.Program?.ProgramName,
                Semester = $"{schedule.Semester?.Year ?? 0} / {schedule.Semester?.Number ?? 0}",
                ExamScheduleName = schedule.ExamScheduleName,
                Status = hasPaid ? "Paid" : "pending",
                Amount = amount,
                HasPaid = hasPaid
            });
        }

        return View(new ExamFormsListViewModel { ExamForms = forms });
    }

    public async Task<IActionResult> PayExamFee(int examScheduleId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null) return NotFound("Student registration not found.");

        var schedule = await dashboardService.GetExamScheduleByIdAsync(examScheduleId);
        if (schedule == null) return NotFound("Exam schedule not found.");

        var subjects = await dashboardService.GetSubjectOfferingsForScheduleAsync(examScheduleId);
        var examFee = await dashboardService.GetExamFeeForScheduleAsync(examScheduleId);
        var practicalCharge = await dashboardService.GetPracticalChargeForProgramAsync(schedule.ProgramId);
        var paymentTypes = await dashboardService.GetActivePaymentTypesAsync();

        var hasESewa = paymentTypes.Any(pt => pt.PaymentTypeName != null && pt.PaymentTypeName.Contains("esewa", StringComparison.OrdinalIgnoreCase));
        var hasKhalti = paymentTypes.Any(pt => pt.PaymentTypeName != null && pt.PaymentTypeName.Contains("khalti", StringComparison.OrdinalIgnoreCase));
        var hasConnectIPS = paymentTypes.Any(pt => pt.PaymentTypeName != null && pt.PaymentTypeName.Contains("connect", StringComparison.OrdinalIgnoreCase));

        if (paymentTypes.Count == 0)
        {
            hasESewa = !string.IsNullOrEmpty(configuration["ESewa:PostUrl"]);
        }

        var failedSubjectIds = await dashboardService.GetFailedSubjectOfferingIdsAsync(user.Id, schedule.SemesterId);
        var isRegular = failedSubjectIds.Count == 0;

        var failedSet = new HashSet<int>(failedSubjectIds);
        var subjectList = subjects.Select(s => new SubjectFeeDetail
        {
            SubjectOfferingId = s.Id,
            SubjectName = s.SubjectCatalog?.SubjectName,
            SubjectCode = s.SubjectCatalog?.SubjectCode,
            HasTheory = s.HasTheory,
            HasPractical = s.HasPractical,
            ExamFee = examFee,
            PracticalFee = s.HasPractical ? practicalCharge : 0,
            IsSelected = isRegular || failedSet.Contains(s.Id),
            IsFailed = failedSet.Contains(s.Id)
        }).ToList();

        var vm = new ExamPaymentViewModel
        {
            ExamScheduleId = examScheduleId,
            ExamScheduleName = schedule.ExamScheduleName,
            ProgramName = schedule.Program?.ProgramName,
            SemesterName = schedule.Semester?.Name,
            TotalExamFee = examFee,
            HasESewa = hasESewa,
            HasKhalti = hasKhalti,
            HasConnectIPS = hasConnectIPS,
            IsRegular = isRegular,
            Subjects = subjectList,
            SelectedSubjectIds = isRegular ? subjectList.Select(s => s.SubjectOfferingId).ToList() : failedSubjectIds
        };

        vm.TotalPracticalFee = subjectList.Sum(s => s.PracticalFee);
        vm.GrandTotal = vm.TotalExamFee + vm.TotalPracticalFee;

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(int examScheduleId, string paymentMethod, decimal amount, string? selectedSubjectIds)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null) return NotFound("Student registration not found.");

        var invoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}-{registration.Id}";
        var subjectIds = string.IsNullOrEmpty(selectedSubjectIds)
            ? new List<int>()
            : selectedSubjectIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

        var schedule = await dashboardService.GetExamScheduleByIdAsync(examScheduleId);
        var failedSubjectIds = schedule != null
            ? await dashboardService.GetFailedSubjectOfferingIdsAsync(user.Id, schedule.SemesterId)
            : [];
        var isRegular = failedSubjectIds.Count == 0;

        int logId;
        if (isRegular || subjectIds.Count == 0)
        {
            logId = await dashboardService.CreatePaymentRequestLogAsync(
                examScheduleId, registration.Id, amount, paymentMethod, invoiceNumber);
        }
        else
        {
            logId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, amount, paymentMethod, invoiceNumber, subjectIds);
        }

        TempData["SuccessMessage"] = $"Payment request of Rs {amount:N0} via {paymentMethod} has been recorded. Invoice: {invoiceNumber}";
        return RedirectToAction(nameof(ExamForms));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ESewaPayment(int examScheduleId, decimal amount, string? selectedSubjectIds)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null) return NotFound("Student registration not found.");

        var invoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}-{registration.Id}";
        var subjectIds = string.IsNullOrEmpty(selectedSubjectIds)
            ? new List<int>()
            : selectedSubjectIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

        var schedule = await dashboardService.GetExamScheduleByIdAsync(examScheduleId);
        var failedSubjectIds = schedule != null
            ? await dashboardService.GetFailedSubjectOfferingIdsAsync(user.Id, schedule.SemesterId)
            : [];
        var isRegular = failedSubjectIds.Count == 0;

        int logId;
        if (isRegular || subjectIds.Count == 0)
        {
            logId = await dashboardService.CreatePaymentRequestLogAsync(
                examScheduleId, registration.Id, amount, "esewa", invoiceNumber);
        }
        else
        {
            logId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, amount, "esewa", invoiceNumber, subjectIds);
        }

        var transactionUuid = esewaService.GenerateTransactionUuid();
        var successUrl = Url.Action(nameof(ESewaCallback), "StudentDashboard", new { area = "Students", status = "success" }, Request.Scheme);
        var failureUrl = Url.Action(nameof(ESewaCallback), "StudentDashboard", new { area = "Students", status = "failure" }, Request.Scheme);

        var formData = esewaService.GeneratePaymentFormData(amount, transactionUuid, successUrl!, failureUrl!);

        ViewBag.LogId = logId;
        ViewBag.TransactionUuid = transactionUuid;

        return View(formData);
    }

    public async Task<IActionResult> ESewaCallback(string status, string? data)
    {
        if (string.IsNullOrEmpty(data))
        {
            TempData["ErrorMessage"] = "No response data received from eSewa.";
            return RedirectToAction(nameof(PaymentFailure));
        }

        try
        {
            var decodedBytes = Convert.FromBase64String(data);
            var decodedJson = System.Text.Encoding.UTF8.GetString(decodedBytes);
            var response = System.Text.Json.JsonSerializer.Deserialize<ESewaVerifyResponse>(decodedJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response == null)
            {
                TempData["ErrorMessage"] = "Invalid response from eSewa.";
                return RedirectToAction(nameof(PaymentFailure));
            }

            if (!esewaService.VerifyResponseSignature(response))
            {
                TempData["ErrorMessage"] = "Signature verification failed.";
                return RedirectToAction(nameof(PaymentFailure));
            }

            var verified = await esewaService.VerifyTransactionAsync(response.TransactionUuid!, response.TotalAmount);
            if (verified == null || verified.Status != "COMPLETE")
            {
                TempData["ErrorMessage"] = "Transaction verification failed.";
                return RedirectToAction(nameof(PaymentFailure));
            }

            TempData["SuccessMessage"] = "Payment successful!";
            TempData["TransactionCode"] = response.TransactionCode;
            TempData["TransactionUuid"] = response.TransactionUuid;

            return RedirectToAction(nameof(PaymentSuccess));
        }
        catch
        {
            TempData["ErrorMessage"] = "Failed to process eSewa callback.";
            return RedirectToAction(nameof(PaymentFailure));
        }
    }

    public IActionResult PaymentSuccess()
    {
        return View();
    }

    public IActionResult PaymentFailure()
    {
        return View();
    }

    public async Task<IActionResult> Marksheet()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration?.RegistrationNumber == null)
        {
            return View(new List<MarksheetViewModel>());
        }

        var resultRecords = await dashboardService.GetResultRecordsAsync(registration.RegistrationNumber);

        var marksheets = resultRecords.Select(rr => new MarksheetViewModel
        {
            RegistrationNumber = rr.RegistrationNumber,
            StudentName = rr.StudentName,
            Program = rr.Program?.ProgramName,
            ExamSchedule = rr.ExamSchedule?.ExamScheduleName,
            AcademicYear = rr.AcademicYear?.AcademicYearName,
            College = rr.College?.Name,
            TotalGpa = rr.Gpa,
            Result = rr.Result,
            Subjects = new List<MarksheetSubjectViewModel>
            {
                new()
                {
                    SubjectName = "Overall",
                    TheoryMarks = rr.TheoryObtainedMarks,
                    PracticalMarks = rr.PracticalObtainedMarks,
                    TotalMarks = rr.TotalObtainedMarks,
                    Grade = rr.TotalObtainedGrade,
                    GradePoint = rr.TotalGradePoints
                }
            }
        }).ToList();

        return View(marksheets);
    }
}
