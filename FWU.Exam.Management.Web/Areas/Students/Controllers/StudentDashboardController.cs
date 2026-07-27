using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
[Authorize(Roles = "Student,SuperAdmin,SystemAdmin")]
public class StudentDashboardController(
    IStudentDashboardService dashboardService,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IEmailSender emailSender,
    IESewaService esewaService,
    IKhaltiService khaltiService,
    IConfiguration configuration,
    ILogger<StudentDashboardController> logger,
    FWU.Exam.Management.Web.Helpers.IFileUploadHelper fileUploadHelper,
    IRetotalRequestService retotalRequestService,
    AppDbContext context,
    IWebHostEnvironment env)
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
            PermanentAddressId = registration.PermanentAddressId,
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
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
            PhotoPath = user.ProfilePath,
            SignaturePath = user.SignaturePath,
            BloodGroup = registration.BloodGroup,
            Nationality = registration.Nationality,
            Religion = registration.Religion,
            AcademicYear = registration.AcademicYear?.AcademicYearName,
            College = registration.College?.Name,
            Level = registration.Level?.LevelName,
            Address = registration.PermanentAddress?.FullAddress
                ?? registration.PermanentAddress?.ToleStreet
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        IFormFile? photo,
        IFormFile? signature,
        string? contactNumber,
        string? bloodGroup,
        string? nationality,
        string? religion,
        string? address,
        int? permanentAddressId)
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

        var registration = await context.StudentRegistrations
            .FirstOrDefaultAsync(sr => sr.Email == user.Email);
        if (registration != null)
        {
            registration.ContactNumber = contactNumber;
            registration.BloodGroup = bloodGroup;
            registration.Nationality = nationality;
            registration.Religion = religion;

            if (permanentAddressId.HasValue)
            {
                var existingAddress = await context.Addresses.FindAsync(permanentAddressId.Value);
                if (existingAddress != null)
                {
                    existingAddress.FullAddress = address;
                }
            }

            await context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEmail(string newEmail)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (string.IsNullOrWhiteSpace(newEmail))
        {
            TempData["ErrorMessage"] = "Please enter a valid email address.";
            return RedirectToAction(nameof(Profile));
        }

        if (!newEmail.Contains('@') || !new EmailAddressAttribute().IsValid(newEmail))
        {
            TempData["ErrorMessage"] = "Please enter a valid email address.";
            return RedirectToAction(nameof(Profile));
        }

        var currentEmail = await userManager.GetEmailAsync(user);
        if (string.Equals(currentEmail, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            TempData["InfoMessage"] = "This is already your current email address.";
            return RedirectToAction(nameof(Profile));
        }

        var existingUser = await userManager.FindByEmailAsync(newEmail);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            TempData["ErrorMessage"] = "This email address is already in use by another account.";
            return RedirectToAction(nameof(Profile));
        }

        var existingRegistration = await context.StudentRegistrations
            .FirstOrDefaultAsync(sr => sr.Email == newEmail && sr.Id != context.StudentRegistrations
                .Where(s => s.Email == user.Email).Select(s => s.Id).FirstOrDefault());
        if (existingRegistration != null)
        {
            TempData["ErrorMessage"] = "This email address is already in use by another student.";
            return RedirectToAction(nameof(Profile));
        }

        try
        {
            var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action(
                "ConfirmEmailChange",
                "StudentDashboard",
                new { area = "Students", userId = userId, email = newEmail, code = code },
                protocol: Request.Scheme);

            await emailSender.SendEmailAsync(
                newEmail,
                "Confirm your email",
                EmailTemplateHelper.ChangeEmail(user.FullName ?? newEmail, callbackUrl));

            TempData["SuccessMessage"] = "A verification link has been sent to your new email address. Please check your inbox and verify. You can continue using your current email to login until verification is complete.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification email to {Email}", newEmail);
            TempData["ErrorMessage"] = "Failed to send verification email. Please try again later.";
        }

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string code)
    {
        if (userId == null || email == null || code == null)
        {
            return RedirectToAction(nameof(Profile));
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Profile));
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await userManager.ChangeEmailAsync(user, email, code);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "Error confirming email. The link may have expired or is invalid.";
            return RedirectToAction(nameof(Profile));
        }

        var setUserNameResult = await userManager.SetUserNameAsync(user, email);
        if (!setUserNameResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Error updating username.";
            return RedirectToAction(nameof(Profile));
        }

        var registration = await context.StudentRegistrations
            .FirstOrDefaultAsync(sr => sr.RegistrationNumber == user.UserName
                                   || (sr.Email != null && sr.Email == user.Email));
        if (registration != null)
        {
            registration.Email = email;
            await context.SaveChangesAsync();
        }

        await signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Email verified successfully! You can now use this email to login.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendEmailVerification()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var email = await userManager.GetEmailAsync(user);
        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "No email address found on your account.";
            return RedirectToAction(nameof(Profile));
        }

        if (await userManager.IsEmailConfirmedAsync(user))
        {
            TempData["InfoMessage"] = "Your email is already verified.";
            return RedirectToAction(nameof(Profile));
        }

        try
        {
            var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            await emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                EmailTemplateHelper.ConfirmEmail(user.FullName ?? email, callbackUrl));

            TempData["SuccessMessage"] = "Verification email sent. Please check your inbox.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to resend verification email to {Email}", email);
            TempData["ErrorMessage"] = "Failed to send verification email. Please try again later.";
        }

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
        schedules = schedules.Where(s => !IsScheduleDeadlinePassed(s)).ToList();

        var forms = new List<ExamFormViewModel>();

        foreach (var schedule in schedules)
        {
            var hasPaid = await dashboardService.HasExistingPaymentAsync(schedule.Id, registration.Id);
            var hasAdmitCard = hasPaid && await dashboardService.HasAdmitCardForScheduleAsync(schedule.Id, user.Id, registration.Id);
            var admitCardId = hasAdmitCard ? await dashboardService.GetAdmitCardIdForScheduleAsync(schedule.Id, user.Id, registration.Id) : null;

            forms.Add(new ExamFormViewModel
            {
                ExamScheduleId = schedule.Id,
                Semester = $"{schedule.Semester?.Year ?? 0} / {schedule.Semester?.Number ?? 0}",
                ExamScheduleName = schedule.ExamScheduleName,
                HasPaid = hasPaid,
                HasAdmitCard = hasAdmitCard,
                AdmitCardId = admitCardId,
                EndDateBs = schedule.EndDateBs,
                ExtendedDateBs = schedule.ExtendedDate.HasValue ? schedule.ExtendedDate.Value.ToString("yyyy-MM-dd") : null,
                AdmissionCardReleaseDate = schedule.AdmissionCardReleaseDate
            });
        }

        return View(new ExamFormsListViewModel { ExamForms = forms });
    }

    public async Task<IActionResult> AdmitCards()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null) return View(new List<AdmitCard>());

        var admitCards = await dashboardService.GetAdmitCardsForStudentAsync(user.Id, registration.Id);
        return View(admitCards);
    }

    public async Task<IActionResult> PaymentHistory()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null) return View(new List<PaymentRequestLog>());

        var payments = await dashboardService.GetPaymentHistoryForStudentAsync(registration.Id);
        return View(payments);
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

        if (IsScheduleDeadlinePassed(schedule))
        {
            TempData["ErrorMessage"] = "The deadline for this exam form has passed.";
            return RedirectToAction(nameof(ExamForms));
        }

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
            return Forbid();
        }

        if (schedule.ProgramId != programId)
            return Forbid();

        var hasExistingRegistration = await dashboardService.HasExistingExamRegistrationAsync(examScheduleId, user.Id);
        if (hasExistingRegistration)
        {
            TempData["ErrorMessage"] = "You have already submitted this exam form.";
            return RedirectToAction(nameof(ExamForms));
        }

        var isSupplementary = schedule.ExamType?.Name == "Supplementary";
        if (isSupplementary)
        {
            var hasFailed = await dashboardService.HasFailedSubjectsInSemesterAsync(user.Id, schedule.SemesterId);
            if (!hasFailed)
            {
                TempData["ErrorMessage"] = "You are not eligible for this supplementary exam.";
                return RedirectToAction(nameof(ExamForms));
            }
        }
        else
        {
            if (schedule.Semester != null && schedule.Semester.Number > 1)
            {
                var prevSubmitted = await dashboardService.HasSubmittedPreviousSemesterExamFormAsync(user.Id, schedule.SemesterId, programId);
                if (!prevSubmitted)
                {
                    TempData["ErrorMessage"] = "Please submit the previous semester exam form first.";
                    return RedirectToAction(nameof(ExamForms));
                }
            }
        }

        var subjects = await dashboardService.GetSubjectOfferingsForScheduleAsync(examScheduleId);
        var examFee = await dashboardService.GetExamFeeForScheduleAsync(examScheduleId);
        var practicalFee = await dashboardService.GetPracticalSubjectFeeForScheduleAsync(examScheduleId);
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
            PracticalFee = s.HasPractical ? practicalFee : 0,
            IsSelected = isRegular || failedSet.Contains(s.Id),
            IsFailed = failedSet.Contains(s.Id),
            IsCompulsory = s.IsCompulsory
        }).ToList();

        var vm = new ExamPaymentViewModel
        {
            ExamScheduleId = examScheduleId,
            ExamScheduleName = schedule.ExamScheduleName,
            ProgramName = schedule.Program?.ProgramName,
            SemesterName = schedule.Semester?.Name,
            StudentName = user.FullName,
            RegistrationNumber = registration.RegistrationNumber,
            EndDateBs = schedule.EndDateBs,
            AcademicYearName = schedule.AcademicYear?.AcademicYearName,
            ExamTypeName = schedule.ExamType?.Name,
            TotalExamFee = examFee,
            HasESewa = hasESewa,
            HasKhalti = hasKhalti,
            HasConnectIPS = hasConnectIPS,
            IsRegular = isRegular,
            Subjects = subjectList,
            SelectedSubjectIds = isRegular ? subjectList.Where(s => s.IsCompulsory).Select(s => s.SubjectOfferingId).ToList() : failedSubjectIds,
            PaymentTypes = paymentTypes.Select(pt => new PaymentTypeDetail
            {
                Id = pt.Id,
                Name = pt.PaymentTypeName,
                LogoUrl = pt.LogoUrl
            }).ToList()
        };

        vm.TotalPracticalFee = subjectList.Where(s => vm.SelectedSubjectIds.Contains(s.SubjectOfferingId)).Sum(s => s.PracticalFee);
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

        int logId;
        if (subjectIds.Count == 0)
        {
            logId = await dashboardService.CreatePaymentRequestLogAsync(
                examScheduleId, registration.Id, amount, paymentMethod, invoiceNumber);
        }
        else
        {
            logId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, amount, paymentMethod, invoiceNumber, subjectIds);
        }

        await HandlePostPaymentRegistration(logId);
        await dashboardService.UpdatePaymentRequestLogAsync(logId, invoiceNumber, true, $"{{\"method\":\"{paymentMethod}\",\"amount\":{amount}}}", $"Payment recorded via {paymentMethod}.");

        TempData["SuccessMessage"] = $"Payment of Rs {amount:N0} via {paymentMethod} completed successfully. Invoice: {invoiceNumber}";
        return RedirectToAction(nameof(PaymentSuccess));
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

        var fullName = $"{registration.FirstName} {registration.MiddleName} {registration.LastName}".Replace("  ", " ");

        int logId;
        if (subjectIds.Count == 0)
        {
            logId = await dashboardService.CreatePaymentRequestLogAsync(
                examScheduleId, registration.Id, amount, "esewa", invoiceNumber,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD);
        }
        else
        {
            logId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, amount, "esewa", invoiceNumber, subjectIds,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD);
        }

        var transactionUuid = esewaService.GenerateTransactionUuid();
        var defaultCallbackUrl = Url.Action(nameof(ESewaCallback), "StudentDashboard", new { area = "Students" }, Request.Scheme)!;
        var successUrl = !string.IsNullOrWhiteSpace(configuration["ESewa:SuccessUrl"]) ? configuration["ESewa:SuccessUrl"]! : defaultCallbackUrl;
        var failureUrl = !string.IsNullOrWhiteSpace(configuration["ESewa:FailureUrl"]) ? configuration["ESewa:FailureUrl"]! : defaultCallbackUrl;

        logger.LogInformation("ESewaPayment: amount={Amount}, transactionUuid={Uuid}, successUrl={SuccessUrl}, failureUrl={FailureUrl}",
            amount, transactionUuid, successUrl, failureUrl);

        var formData = esewaService.GeneratePaymentFormData(amount, transactionUuid, successUrl!, failureUrl!);

        HttpContext.Session.SetInt32("ESewaLogId", logId);
        ViewBag.LogId = logId;
        ViewBag.TransactionUuid = transactionUuid;

        return View(formData);
    }

    public async Task<IActionResult> ESewaCallback(string? data)
    {
        var sessionLogId = HttpContext.Session.GetInt32("ESewaLogId");
        HttpContext.Session.Remove("ESewaLogId");

        logger.LogInformation("ESewaCallback hit: sessionLogId={SessionLogId}, dataPresent={DataPresent}, queryString={QueryString}",
            sessionLogId, !string.IsNullOrEmpty(data), Request.QueryString);

        if (string.IsNullOrEmpty(data))
        {
            if (sessionLogId.HasValue)
            {
                var log = await dashboardService.GetPaymentLogByIdAsync(sessionLogId.Value);
                if (log != null) TempData["ExamScheduleId"] = log.ExamScheduleId;
                await dashboardService.UpdatePaymentRequestLogAsync(sessionLogId.Value, "", false, $"No response data received from eSewa. QueryString: {Request.QueryString}", "No response data received from eSewa.");
            }

            TempData["ErrorMessage"] = "No response data received from eSewa.";
            return RedirectToAction(nameof(PaymentFailure));
        }

        try
        {
            var log = sessionLogId.HasValue ? await dashboardService.GetPaymentLogByIdAsync(sessionLogId.Value) : null;
            if (log != null) TempData["ExamScheduleId"] = log.ExamScheduleId;

            logger.LogInformation("ESewaCallback: raw data length={Length}, first100={Preview}", data.Length, data.Length > 100 ? data[..100] : data);

            var base64 = data.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            var decodedBytes = Convert.FromBase64String(base64);
            var decodedJson = System.Text.Encoding.UTF8.GetString(decodedBytes);
            logger.LogInformation("ESewaCallback: decodedJson={Json}", decodedJson);

            var response = System.Text.Json.JsonSerializer.Deserialize<ESewaVerifyResponse>(decodedJson,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                });

            if (response == null)
            {
                logger.LogWarning("ESewaCallback: deserialized response is null");
                if (sessionLogId.HasValue)
                    await dashboardService.UpdatePaymentRequestLogAsync(sessionLogId.Value, "", false, decodedJson, "Invalid response from eSewa.");

                TempData["ErrorMessage"] = "Invalid response from eSewa.";
                return RedirectToAction(nameof(PaymentFailure));
            }

            logger.LogInformation("ESewaCallback: txCode={TxCode}, status={Status}, totalAmount={Amount}, uuid={Uuid}, productCode={ProductCode}",
                response.TransactionCode, response.Status, response.TotalAmount, response.TransactionUuid, response.ProductCode);

            var sigValid = esewaService.VerifyResponseSignature(response, decodedJson);
            logger.LogInformation("ESewaCallback: signatureValid={SigValid}", sigValid);

            if (!sigValid)
            {
                if (sessionLogId.HasValue)
                    await dashboardService.UpdatePaymentRequestLogAsync(sessionLogId.Value, response.TransactionCode ?? "", false, decodedJson, "Signature verification failed via eSewa.");

                TempData["ErrorMessage"] = "Signature verification failed.";
                return RedirectToAction(nameof(PaymentFailure));
            }

            var verified = await esewaService.VerifyTransactionAsync(response.TransactionUuid!, response.TotalAmount);
            var verifyData = verified != null
                ? System.Text.Json.JsonSerializer.Serialize(verified)
                : "null";
            var combinedData = $"{{\"callback\":{decodedJson},\"verification\":{verifyData}}}";

            logger.LogInformation("ESewaCallback: verifyResult={Status}", verified?.Status ?? "null");

            if (verified == null || verified.Status != "COMPLETE")
            {
                if (sessionLogId.HasValue)
                    await dashboardService.UpdatePaymentRequestLogAsync(sessionLogId.Value, response.TransactionCode ?? "", false, combinedData, "Transaction verification failed via eSewa.");

                TempData["ErrorMessage"] = "Transaction verification failed.";
                return RedirectToAction(nameof(PaymentFailure));
            }

            var resolvedLogId = sessionLogId;
            if (!resolvedLogId.HasValue)
            {
                logger.LogWarning("ESewaCallback: Session log ID lost. Attempting fallback lookup by student registration.");
                var callbackUser = await userManager.GetUserAsync(User);
                if (callbackUser != null)
                {
                    var registration = await dashboardService.GetStudentRegistrationByEmailAsync(callbackUser.Email ?? "");
                    if (registration != null)
                    {
                        var pendingLog = await dashboardService.FindPendingPaymentLogByStudentAsync(registration.Id);
                        if (pendingLog != null)
                        {
                            resolvedLogId = pendingLog.Id;
                            logger.LogInformation("ESewaCallback: Fallback lookup found logId={LogId} for studentRegId={StudentRegId}", resolvedLogId, registration.Id);
                        }
                    }
                }
            }

            if (resolvedLogId.HasValue)
            {
                await HandlePostPaymentRegistration(resolvedLogId.Value);
                await dashboardService.UpdatePaymentRequestLogAsync(resolvedLogId.Value, response.TransactionCode ?? "", true, combinedData, "Payment verified via eSewa.");
            }

            TempData["SuccessMessage"] = "Payment successful!";
            TempData["TransactionCode"] = response.TransactionCode;
            TempData["TransactionUuid"] = response.TransactionUuid;

            return RedirectToAction(nameof(PaymentSuccess));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ESewaCallback: exception processing callback. queryString={QueryString}", Request.QueryString);
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
        var fullName = $"{registration.FirstName} {registration.MiddleName} {registration.LastName}".Replace("  ", " ");

        int logId;
        if (subjectIds.Count == 0)
        {
            logId = await dashboardService.CreatePaymentRequestLogAsync(
                examScheduleId, registration.Id, amount, "khalti", invoiceNumber,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD);
        }
        else
        {
            logId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, amount, "khalti", invoiceNumber, subjectIds,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD);
        }

        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var baseUrl = $"{scheme}://{host}";

        var returnUrl = Url.Action(nameof(KhaltiCallback), "StudentDashboard",
            new { area = "Students" }, scheme)!;

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
            HttpContext.Session.SetInt32("KhaltiLogId", logId);
            return Redirect(response.PaymentUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Khalti payment initiation failed");
            TempData["ErrorMessage"] = $"Khalti payment failed: {ex.Message}";
            return RedirectToAction(nameof(PayExamFee), new { examScheduleId });
        }
    }

    public async Task<IActionResult> KhaltiCallback(string? pidx, string? status, string? transaction_id, string? purchase_order_id)
    {
        var sessionLogId = HttpContext.Session.GetInt32("KhaltiLogId");
        HttpContext.Session.Remove("KhaltiLogId");

        logger.LogInformation("KhaltiCallback hit: sessionLogId={SessionLogId}, pidx={Pidx}, status={Status}", sessionLogId, pidx, status);

        if (string.IsNullOrEmpty(pidx))
        {
            if (sessionLogId.HasValue)
                await dashboardService.UpdatePaymentRequestLogAsync(sessionLogId.Value, "", false, "No pidx received from Khalti.", "No pidx received from Khalti.");

            TempData["ErrorMessage"] = "No payment identifier received from Khalti.";
            return RedirectToAction(nameof(PaymentFailure));
        }

        try
        {
            var log = sessionLogId.HasValue ? await dashboardService.GetPaymentLogByIdAsync(sessionLogId.Value) : null;
            if (log != null) TempData["ExamScheduleId"] = log.ExamScheduleId;

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
                if (sessionLogId.HasValue)
                    await dashboardService.UpdatePaymentRequestLogAsync(sessionLogId.Value, transaction_id ?? "", false, responseData, "Payment verification failed via Khalti.");

                TempData["ErrorMessage"] = $"Payment verification failed. Status: {lookup?.Status ?? "Unknown"}";
                return RedirectToAction(nameof(PaymentFailure));
            }

            logger.LogInformation("Khalti payment successful: transaction_id={TransactionId}", lookup.TransactionId);
            var khaltiResolvedLogId = sessionLogId;
            if (!khaltiResolvedLogId.HasValue && !string.IsNullOrEmpty(purchase_order_id))
            {
                logger.LogWarning("KhaltiCallback: Session log ID lost. Attempting fallback lookup by invoice={Invoice}", purchase_order_id);
                var invoiceLog = await dashboardService.GetPaymentLogByInvoiceNumberAsync(purchase_order_id);
                if (invoiceLog != null)
                {
                    khaltiResolvedLogId = invoiceLog.Id;
                    logger.LogInformation("KhaltiCallback: Fallback lookup found logId={LogId} for invoice={Invoice}", khaltiResolvedLogId, purchase_order_id);
                }
            }

            if (khaltiResolvedLogId.HasValue)
            {
                await HandlePostPaymentRegistration(khaltiResolvedLogId.Value);
                await dashboardService.UpdatePaymentRequestLogAsync(khaltiResolvedLogId.Value, lookup.TransactionId ?? transaction_id ?? "", true, responseData, "Payment verified via Khalti.");
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DemoPayment(int examScheduleId, decimal amount, string? selectedSubjectIds)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null) return NotFound("Student registration not found.");

        var demoType = await context.Set<FWU.Exam.Management.Domain.Entities.Payments.PaymentType>()
            .FirstOrDefaultAsync(pt => pt.PaymentTypeName == "Demo");
        if (demoType == null)
        {
            demoType = new FWU.Exam.Management.Domain.Entities.Payments.PaymentType
            {
                PaymentTypeName = "Demo",
                LogoUrl = "",
                IsActive = true
            };
            context.Set<FWU.Exam.Management.Domain.Entities.Payments.PaymentType>().Add(demoType);
            await context.SaveChangesAsync();
        }

        var invoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}-{registration.Id}";
        var subjectIds = string.IsNullOrEmpty(selectedSubjectIds)
            ? new List<int>()
            : selectedSubjectIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

        var schedule = await dashboardService.GetExamScheduleByIdAsync(examScheduleId);
        var fullName = $"{registration.FirstName} {registration.MiddleName} {registration.LastName}".Replace("  ", " ");

        int logId;
        if (subjectIds.Count == 0)
        {
            logId = await dashboardService.CreatePaymentRequestLogAsync(
                examScheduleId, registration.Id, amount, "demo", invoiceNumber,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD);
        }
        else
        {
            logId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, amount, "demo", invoiceNumber, subjectIds,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD);
        }

        await HandlePostPaymentRegistration(logId);
        await dashboardService.UpdatePaymentRequestLogAsync(logId, $"DEMO-{DateTime.Now:yyyyMMddHHmmss}", true, "{\"method\":\"demo\",\"note\":\"Demo payment for workflow testing\"}", "Demo payment approved.");

        TempData["SuccessMessage"] = "Demo payment completed successfully!";
        TempData["TransactionCode"] = $"DEMO-{DateTime.Now:yyyyMMddHHmmss}";
        return RedirectToAction(nameof(PaymentSuccess));
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
        if (user == null)
        {
            logger.LogWarning("HandlePostPaymentRegistration: User not found for logId={LogId}", logId);
            return;
        }

        var paymentLog = await dashboardService.GetPaymentLogByIdAsync(logId);
        if (paymentLog == null)
        {
            logger.LogWarning("HandlePostPaymentRegistration: PaymentLog not found for logId={LogId}", logId);
            return;
        }

        if (!paymentLog.StudentRegistrationId.HasValue)
        {
            logger.LogWarning("HandlePostPaymentRegistration: StudentRegistrationId is null on logId={LogId}", logId);
            return;
        }

        var existingRegistration = await dashboardService.HasExistingExamRegistrationAsync(
            paymentLog.ExamScheduleId, user.Id);
        if (existingRegistration)
        {
            logger.LogWarning("HandlePostPaymentRegistration: Existing payment found for scheduleId={ScheduleId}, studentRegId={StudentRegId}. Skipping registration creation.",
                paymentLog.ExamScheduleId, paymentLog.StudentRegistrationId.Value);
            return;
        }

        var subjectIds = string.IsNullOrEmpty(paymentLog.SelectedSubjectIds)
            ? new List<int>()
            : paymentLog.SelectedSubjectIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

        if (subjectIds.Count == 0)
        {
            logger.LogWarning("HandlePostPaymentRegistration: No subject IDs on logId={LogId} (SelectedSubjectIds={SelectedSubjectIds}). Skipping registration creation.",
                logId, paymentLog.SelectedSubjectIds ?? "null");
            return;
        }

        logger.LogInformation("HandlePostPaymentRegistration: Creating ExamRegistration for logId={LogId}, scheduleId={ScheduleId}, userId={UserId}, subjects={SubjectCount}",
            logId, paymentLog.ExamScheduleId, user.Id, subjectIds.Count);
        await dashboardService.CreateExamRegistrationAsync(paymentLog.ExamScheduleId, user.Id, paymentLog.Amount, subjectIds, paymentLog.StudentRegistrationId.Value);
    }

    public async Task<IActionResult> MarksheetPrint(int? examScheduleId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration?.RegistrationNumber == null)
        {
            return View(new List<MarksheetViewModel>());
        }

        var resultRecords = await dashboardService.GetResultRecordsAsync(registration.RegistrationNumber);
        var examRegistrations = await dashboardService.GetStudentExamRegistrationsAsync(user.Id);

        var marksheets = new List<MarksheetViewModel>();

        foreach (var rr in resultRecords)
        {
            var scheduleId = rr.ExamScheduleId;
            if (scheduleId == null) continue;

            if (examScheduleId.HasValue && scheduleId.Value != examScheduleId.Value) continue;

            var subjects = GetMarksheetSubjects(examRegistrations, scheduleId.Value);

            marksheets.Add(new MarksheetViewModel
            {
                RegistrationNumber = rr.RegistrationNumber,
                StudentName = rr.StudentName,
                Program = rr.Program?.ProgramName,
                ExamSchedule = rr.ExamSchedule?.ExamScheduleName,
                AcademicYear = rr.AcademicYear?.AcademicYearName,
                College = rr.College?.Name,
                TotalGpa = rr.Gpa,
                Result = rr.Result,
                ExamScheduleId = scheduleId.Value,
                Subjects = subjects
            });
        }

        foreach (var er in examRegistrations)
        {
            if (examScheduleId.HasValue && er.ExamScheduleId != examScheduleId.Value) continue;

            if (!marksheets.Any(m => m.ExamScheduleId == er.ExamScheduleId))
            {
                var subjects = GetMarksheetSubjects(examRegistrations, er.ExamScheduleId);
                marksheets.Add(new MarksheetViewModel
                {
                    RegistrationNumber = registration.RegistrationNumber,
                    StudentName = $"{registration.FirstName} {registration.MiddleName} {registration.LastName}".Replace("  ", " "),
                    ExamSchedule = er.ExamSchedule?.ExamScheduleName,
                    ExamScheduleId = er.ExamScheduleId,
                    Result = "Pending",
                    Subjects = subjects
                });
            }
        }

        var sorted = marksheets.OrderByDescending(m => m.ExamScheduleId).ToList();

        ViewBag.StudentRegistration = registration;
        ViewBag.SymbolNumber = sorted.Count > 0 ? examRegistrations.FirstOrDefault(er => er.ExamScheduleId == sorted.First().ExamScheduleId)?.SymbolNumber : null;

        return View(sorted);
    }

    public async Task<IActionResult> Marksheet()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");

        // Show all marksheets for logged-in student
        if (registration?.RegistrationNumber == null)
        {
            return View(new List<MarksheetViewModel>());
        }

        var allResultRecords = await dashboardService.GetResultRecordsAsync(registration.RegistrationNumber);
        var allExamRegistrations = await dashboardService.GetStudentExamRegistrationsAsync(user.Id);

        var allMarksheets = new List<MarksheetViewModel>();

        foreach (var rr in allResultRecords)
        {
            var scheduleId = rr.ExamScheduleId;
            if (scheduleId == null) continue;

            var subjects = GetMarksheetSubjects(allExamRegistrations, scheduleId.Value);

            allMarksheets.Add(new MarksheetViewModel
            {
                RegistrationNumber = rr.RegistrationNumber,
                StudentName = rr.StudentName,
                Program = rr.Program?.ProgramName,
                ExamSchedule = rr.ExamSchedule?.ExamScheduleName,
                AcademicYear = rr.AcademicYear?.AcademicYearName,
                College = rr.College?.Name,
                TotalGpa = rr.Gpa,
                Result = rr.Result,
                ExamScheduleId = scheduleId.Value,
                Subjects = subjects
            });
        }

        foreach (var er in allExamRegistrations)
        {
            if (!allMarksheets.Any(m => m.ExamScheduleId == er.ExamScheduleId))
            {
                var subjects = GetMarksheetSubjects(allExamRegistrations, er.ExamScheduleId);
                allMarksheets.Add(new MarksheetViewModel
                {
                    RegistrationNumber = registration.RegistrationNumber,
                    StudentName = $"{registration.FirstName} {registration.MiddleName} {registration.LastName}".Replace("  ", " "),
                    ExamSchedule = er.ExamSchedule?.ExamScheduleName,
                    ExamScheduleId = er.ExamScheduleId,
                    Result = "Pending",
                    Subjects = subjects
                });
            }
        }

        ViewBag.StudentRegistration = registration;
        ViewBag.SymbolNumber = allExamRegistrations.FirstOrDefault()?.SymbolNumber;

        return View(allMarksheets.OrderByDescending(m => m.ExamScheduleId).ToList());
    }

    public async Task<IActionResult> RetotalRequests()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null)
            return View(new List<RetotalRequest>());

        var requests = await context.RetotalRequests
            .AsNoTracking()
            .Where(r => r.StudentRegistrationId == registration.Id && r.IsActive)
            .Include(r => r.ExamSubjectResult)
                .ThenInclude(esr => esr.SubjectOffering)
                    .ThenInclude(so => so.SubjectCatalog)
            .Include(r => r.ExamRegistration)
                .ThenInclude(er => er.ExamSchedule)
            .OrderByDescending(r => r.RequestedDate)
            .ToListAsync();

        return View(requests);
    }

    public async Task<IActionResult> RequestRetotal(int? examSubjectResultId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null) return RedirectToAction(nameof(Profile));

        if (examSubjectResultId.HasValue)
        {
            var result = await context.ExamSubjectResults
                .AsNoTracking()
                .Include(esr => esr.SubjectOffering)
                    .ThenInclude(so => so.SubjectCatalog)
                .Include(esr => esr.ExamRegistration)
                    .ThenInclude(er => er.ExamSchedule)
                .FirstOrDefaultAsync(esr => esr.Id == examSubjectResultId.Value);

            if (result != null)
            {
                ViewBag.SubjectName = result.SubjectOffering?.SubjectCatalog?.SubjectName;
                ViewBag.OriginalGrade = result.GradeLetter;
                ViewBag.OriginalMarks = result.ObtainedMarks;
                ViewBag.ExamScheduleName = result.ExamRegistration?.ExamSchedule?.ExamScheduleName;
            }
        }

        ViewBag.ExamSubjectResultId = examSubjectResultId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitRetotalRequest(int examSubjectResultId, string? reason)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByEmailAsync(user.Email ?? "");
        if (registration == null) return RedirectToAction(nameof(Profile));

        var result = await context.ExamSubjectResults
            .AsNoTracking()
            .FirstOrDefaultAsync(esr => esr.Id == examSubjectResultId);

        if (result == null) return NotFound();

        var existingRequest = await context.RetotalRequests
            .AnyAsync(r => r.ExamSubjectResultId == examSubjectResultId
                && r.StudentRegistrationId == registration.Id
                && r.IsActive
                && r.Status != RetotalStatus.Rejected);

        if (existingRequest)
        {
            TempData["ErrorMessage"] = "A pending retotal request already exists for this subject.";
            return RedirectToAction(nameof(RetotalRequests));
        }

        var retotalRequest = new RetotalRequest
        {
            ExamSubjectResultId = examSubjectResultId,
            StudentRegistrationId = registration.Id,
            ExamRegistrationId = result.ExamRegistrationId,
            RequestedDate = DateTime.UtcNow,
            Reason = reason,
            Status = RetotalStatus.Pending,
            OriginalGradeLetter = result.GradeLetter,
            OriginalObtainedMarks = result.ObtainedMarks,
            FeeAmount = 500,
            FeePaid = false,
            IsActive = true
        };

        context.RetotalRequests.Add(retotalRequest);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Retotal request submitted successfully. Please pay the fee to proceed.";
        return RedirectToAction(nameof(RetotalRequests));
    }

    private static List<MarksheetSubjectViewModel> GetMarksheetSubjects(List<ExamRegistration> examRegistrations, int examScheduleId)
    {
        var registration = examRegistrations.FirstOrDefault(er => er.ExamScheduleId == examScheduleId);
        if (registration?.ExamSubjectResults == null || !registration.ExamSubjectResults.Any())
            return new();

        return registration.ExamSubjectResults
            .Where(esr => esr.IsActive)
            .Select(esr =>
            {
                var gradeLetter = esr.GradeLetter?.Trim().ToUpperInvariant();
                var isFailed = gradeLetter is "F" or "NG";
                var isSubmitted = esr.IsSubmitted;
                var hasGrade = !string.IsNullOrEmpty(gradeLetter);

                return new MarksheetSubjectViewModel
                {
                    ExamSubjectResultId = esr.Id,
                    SubjectName = esr.SubjectOffering?.SubjectCatalog?.SubjectName,
                    SubjectCode = esr.SubjectOffering?.SubjectCatalog?.SubjectCode,
                    TheoryMarks = esr.ObtainedMarksTheory,
                    PracticalMarks = esr.ObtainedMarksPractical,
                    InternalMarks = esr.ObtainedMarksTheoryInternal,
                    TotalMarks = esr.ObtainedMarks,
                    Grade = gradeLetter,
                    IsPassed = hasGrade && !isFailed,
                    Status = !hasGrade ? "Pending"
                        : isSubmitted && isFailed ? "Fail"
                        : isSubmitted && !isFailed ? "Pass"
                        : "Pending"
                };
            })
            .OrderBy(s => s.SubjectName)
            .ToList();
    }

    private static bool IsScheduleDeadlinePassed(ExamSchedule schedule)
    {
        var effectiveDeadline = schedule.ExtendedDate
            ?? schedule.EndDate?.ToDateTime(TimeOnly.MinValue);

        return effectiveDeadline.HasValue && DateTime.Now > effectiveDeadline.Value;
    }
}
