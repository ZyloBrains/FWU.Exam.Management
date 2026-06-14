using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
[Authorize(Roles = "Student")]
public class StudentDashboardController(
    IStudentDashboardService dashboardService,
    UserManager<AppUser> userManager,
    IESewaService esewaService,
    IKhaltiService khaltiService,
    IConfiguration configuration,
    ILogger<StudentDashboardController> logger,
    FWU.Exam.Management.Web.Helpers.IFileUploadHelper fileUploadHelper)
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
            FullName = string.Join(" ", new[] { registration.FirstName, registration.MiddleName, registration.LastName }.Where(x => !string.IsNullOrEmpty(x))),
            NepaliName = registration.NepaliName,
            Gender = registration.Gender?.GenderName,
            DateOfBirthBS = registration.DateOfBirthBS,
            DateOfBirthAD = registration.DateOfBirthAD,
            Ethnicity = registration.Ethnicity?.EthnicityName,
            Category = registration.StudentCategory?.StudentCategoryName,
            ContactNumber = registration.ContactNumber,
            Email = registration.Email,
            PhotoPath = user.ProfilePath,
            SignaturePath = user.SignaturePath,
            BloodGroup = registration.BloodGroup,
            Nationality = registration.Nationality,
            Religion = registration.Religion,
            AcademicYear = registration.AcademicYear?.AcademicYearName,
            Department = registration.Department?.DepartmentName,
            College = registration.College?.Name,
            Level = registration.Level?.LevelName,
            Address = registration.PermanentAddress?.FullAddress
                ?? registration.PermanentAddress?.ToleStreet
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(IFormFile? photo, IFormFile? signature)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (photo != null && photo.Length > 0)
        {
            var photoPath = await fileUploadHelper.UploadAsync(photo, "uploads/photos");
            if (photoPath != null)
                user.ProfilePath = photoPath;
        }

        if (signature != null && signature.Length > 0)
        {
            var signaturePath = await fileUploadHelper.UploadAsync(signature, "uploads/signatures");
            if (signaturePath != null)
                user.SignaturePath = signaturePath;
        }

        await userManager.UpdateAsync(user);
        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
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

        var schedules = await dashboardService.GetExamSchedulesForStudentAsync(registration, user.Id);
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

    public async Task<IActionResult> MySubjects()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null) return NotFound("Student registration not found.");

        var admission = await dashboardService.GetStudentAdmissionByUserIdAsync(user.Id);
        int programId;

        if (admission != null)
        {
            programId = admission.ProgramsId;
        }
        else if (registration.ProgramId.HasValue)
        {
            programId = registration.ProgramId.Value;
        }
        else
        {
            return NotFound("No program assigned.");
        }

        var subjects = await dashboardService.GetSubjectOfferingsByProgramAsync(programId);

        return View(subjects);
    }

    public async Task<IActionResult> PayExamFee(int examScheduleId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null) return NotFound("Student registration not found.");

        var schedule = await dashboardService.GetExamScheduleByIdAsync(examScheduleId);
        if (schedule == null) return NotFound("Exam schedule not found.");

        var admission = await dashboardService.GetStudentAdmissionByUserIdAsync(user.Id);
        if (admission == null || schedule.ProgramId != admission.ProgramsId)
            return Forbid();

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
            IsCompulsory = s.IsCompulsory,
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
            TotalExamFee = subjectList.Sum(s => s.ExamFee),
            HasESewa = hasESewa,
            HasKhalti = hasKhalti,
            HasConnectIPS = hasConnectIPS,
            IsRegular = isRegular,
            Subjects = subjectList,
            SelectedSubjectIds = isRegular ? subjectList.Select(s => s.SubjectOfferingId).ToList() : failedSubjectIds,
            PaymentTypes = paymentTypes.Select(pt => new PaymentTypeDetail
            {
                Id = pt.Id,
                Name = pt.PaymentTypeName,
                LogoUrl = pt.LogoUrl
            }).ToList()
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

        await dashboardService.CreateExamRegistrationAsync(examScheduleId, user.Id, amount, subjectIds);

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

        var fullName = $"{registration.FirstName} {registration.MiddleName} {registration.LastName}".Replace("  ", " ");

        int logId;
        if (isRegular || subjectIds.Count == 0)
        {
            logId = await dashboardService.CreatePaymentRequestLogAsync(
                examScheduleId, registration.Id, amount, "esewa", invoiceNumber,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD, registration.CollegeId);
        }
        else
        {
            logId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, amount, "esewa", invoiceNumber, subjectIds,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD, registration.CollegeId);
        }

        var transactionUuid = esewaService.GenerateTransactionUuid();
        var successUrl = Url.Action(nameof(ESewaCallback), "StudentDashboard", new { area = "Students", status = "success", logId }, Request.Scheme);
        var failureUrl = Url.Action(nameof(ESewaCallback), "StudentDashboard", new { area = "Students", status = "failure", logId }, Request.Scheme);

        var formData = esewaService.GeneratePaymentFormData(amount, transactionUuid, successUrl!, failureUrl!);

        ViewBag.LogId = logId;
        ViewBag.TransactionUuid = transactionUuid;

        return View(formData);
    }

    public async Task<IActionResult> ESewaCallback(string status, string? data, int? logId)
    {
        if (string.IsNullOrEmpty(data))
        {
            if (logId.HasValue)
                await dashboardService.UpdatePaymentRequestLogAsync(logId.Value, "", false, "No response data received from eSewa.", "No response data received from eSewa.");

            TempData["ErrorMessage"] = "No response data received from eSewa.";
            return RedirectToAction(nameof(PaymentFailure));
        }

        try
        {
            var log = logId.HasValue ? await dashboardService.GetPaymentRequestLogByIdAsync(logId.Value) : null;
            if (log != null) TempData["ExamScheduleId"] = log.ExamScheduleId;

            var decodedBytes = Convert.FromBase64String(data);
            var decodedJson = System.Text.Encoding.UTF8.GetString(decodedBytes);
            var response = System.Text.Json.JsonSerializer.Deserialize<ESewaVerifyResponse>(decodedJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response == null)
            {
                if (logId.HasValue)
                    await dashboardService.UpdatePaymentRequestLogAsync(logId.Value, "", false, "Invalid response from eSewa.", "Invalid response from eSewa.");

                TempData["ErrorMessage"] = "Invalid response from eSewa.";
                return RedirectToAction(nameof(PaymentFailure));
            }

            if (!esewaService.VerifyResponseSignature(response, decodedJson))
            {
                if (logId.HasValue)
                    await dashboardService.UpdatePaymentRequestLogAsync(logId.Value, response.TransactionCode ?? "", false, decodedJson, "Signature verification failed via eSewa.");

                TempData["ErrorMessage"] = "Signature verification failed.";
                return RedirectToAction(nameof(PaymentFailure));
            }

            var verified = await esewaService.VerifyTransactionAsync(response.TransactionUuid!, response.TotalAmount);
            var verifyData = verified != null
                ? System.Text.Json.JsonSerializer.Serialize(verified)
                : "null";
            var combinedData = $"{{\"callback\":{decodedJson},\"verification\":{verifyData}}}";

            if (verified == null || verified.Status != "COMPLETE")
            {
                if (logId.HasValue)
                    await dashboardService.UpdatePaymentRequestLogAsync(logId.Value, response.TransactionCode ?? "", false, combinedData, "Transaction verification failed via eSewa.");

                TempData["ErrorMessage"] = "Transaction verification failed.";
                return RedirectToAction(nameof(PaymentFailure));
            }

            if (logId.HasValue)
            {
                await dashboardService.UpdatePaymentRequestLogAsync(logId.Value, response.TransactionCode ?? "", true, combinedData, "Payment verified via eSewa.");
                await HandlePostPaymentRegistration(logId.Value);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KhaltiPayment(int examScheduleId, decimal amount, string? selectedSubjectIds)
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

        var fullName = $"{registration.FirstName} {registration.MiddleName} {registration.LastName}".Replace("  ", " ");

        int logId;
        if (isRegular || subjectIds.Count == 0)
        {
            logId = await dashboardService.CreatePaymentRequestLogAsync(
                examScheduleId, registration.Id, amount, "khalti", invoiceNumber,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD, registration.CollegeId);
        }
        else
        {
            logId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, amount, "khalti", invoiceNumber, subjectIds,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD, registration.CollegeId);
        }

        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var baseUrl = $"{scheme}://{host}";

        var returnUrl = Url.Action(nameof(KhaltiCallback), "StudentDashboard",
            new { area = "Students", logId }, scheme)!;

        var khaltiRequest = new KhaltiInitiateRequest
        {
            ReturnUrl = returnUrl,
            WebsiteUrl = baseUrl,
            Amount = (long)(amount * 100),
            PurchaseOrderId = invoiceNumber,
            PurchaseOrderName = $"Exam Fee - {schedule?.ExamScheduleName ?? ""}",
            CustomerInfo = new KhaltiCustomerInfo
            {
                Name = fullName,
                Email = registration.Email,
                Phone = registration.ContactNumber
            }
        };

        try
        {
            logger.LogInformation("Initiating Khalti payment: amount={Amount}, invoice={Invoice}, returnUrl={ReturnUrl}, websiteUrl={WebsiteUrl}",
                amount, invoiceNumber, returnUrl, baseUrl);

            var response = await khaltiService.InitiatePaymentAsync(khaltiRequest);
            if (response?.PaymentUrl == null)
            {
                TempData["ErrorMessage"] = "Khalti did not return a payment URL. Please try again.";
                return RedirectToAction(nameof(PayExamFee), new { examScheduleId });
            }

            logger.LogInformation("Khalti redirecting to: {PaymentUrl}", response.PaymentUrl);
            return Redirect(response.PaymentUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Khalti payment initiation failed");
            TempData["ErrorMessage"] = $"Khalti payment failed: {ex.Message}";
            return RedirectToAction(nameof(PayExamFee), new { examScheduleId });
        }
    }

    public async Task<IActionResult> KhaltiCallback(string? pidx, string? status, string? transaction_id, string? purchase_order_id, int? logId)
    {
        if (string.IsNullOrEmpty(pidx))
        {
            if (logId.HasValue)
                await dashboardService.UpdatePaymentRequestLogAsync(logId.Value, "", false, "No pidx received from Khalti.", "No pidx received from Khalti.");

            TempData["ErrorMessage"] = "No payment identifier received from Khalti.";
            return RedirectToAction(nameof(PaymentFailure));
        }

        try
        {
            var log = logId.HasValue ? await dashboardService.GetPaymentRequestLogByIdAsync(logId.Value) : null;
            if (log != null) TempData["ExamScheduleId"] = log.ExamScheduleId;

            logger.LogInformation("Khalti callback received: pidx={Pidx}, status={Status}, transaction_id={TransactionId}, purchase_order_id={PurchaseOrderId}",
                pidx, status, transaction_id, purchase_order_id);

            var lookup = await khaltiService.LookupPaymentAsync(pidx!);
            var responseData = System.Text.Json.JsonSerializer.Serialize(new
            {
                pidx,
                callback_status = status,
                callback_transaction_id = transaction_id,
                lookup
            });

            if (lookup == null || lookup.Status != "Completed")
            {
                logger.LogWarning("Khalti payment verification failed: status={LookupStatus}, callback_status={CallbackStatus}", lookup?.Status, status);
                if (logId.HasValue)
                    await dashboardService.UpdatePaymentRequestLogAsync(logId.Value, transaction_id ?? "", false, responseData, "Payment verification failed via Khalti.");

                TempData["ErrorMessage"] = $"Payment verification failed. Status: {lookup?.Status ?? "Unknown"}";
                return RedirectToAction(nameof(PaymentFailure));
            }

            logger.LogInformation("Khalti payment successful: transaction_id={TransactionId}", lookup.TransactionId);
            if (logId.HasValue)
            {
                await dashboardService.UpdatePaymentRequestLogAsync(logId.Value, lookup.TransactionId ?? transaction_id ?? "", true, responseData, "Payment verified via Khalti.");
                await HandlePostPaymentRegistration(logId.Value);
            }

            TempData["SuccessMessage"] = "Payment successful!";
            TempData["TransactionCode"] = lookup.TransactionId ?? transaction_id;
            TempData["TransactionUuid"] = pidx;

            return RedirectToAction(nameof(PaymentSuccess));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Khalti callback processing failed");
            TempData["ErrorMessage"] = "Failed to process Khalti callback.";
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

        private async Task HandlePostPaymentRegistration(int logId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return;

            var paymentLog = await dashboardService.GetPaymentLogByIdAsync(logId);
            if (paymentLog == null) return;

            var schedule = await dashboardService.GetExamScheduleByIdAsync(paymentLog.ExamScheduleId);
            if (schedule == null) return;

            var subjects = await dashboardService.GetSubjectOfferingsForScheduleAsync(paymentLog.ExamScheduleId);
            var failedSubjectIds = await dashboardService.GetFailedSubjectOfferingIdsAsync(user.Id, schedule.SemesterId);
            var isRegular = failedSubjectIds.Count == 0;

            List<int> subjectIds;
            if (isRegular)
            {
                subjectIds = subjects.Where(s => s.IsCompulsory).Select(s => s.Id).ToList();
            }
            else
            {
                subjectIds = failedSubjectIds;
            }

            if (subjectIds.Count == 0) return;

            await dashboardService.CreateExamRegistrationAsync(paymentLog.ExamScheduleId, user.Id, paymentLog.Amount, subjectIds);
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
