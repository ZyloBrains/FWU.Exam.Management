using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Application.Helpers;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Permissions;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
[Authorize(Roles = Role.Student + "," + Role.SuperAdmin + "," + Role.FacultyAdmin)]
public class StudentDashboardController(
    IStudentDashboardService dashboardService,
    IAdmitCardService admitCardService,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    INotificationService notificationService,
    IESewaService esewaService,
    IKhaltiService khaltiService,
    ILogger<StudentDashboardController> logger,
    FWU.Exam.Management.Web.Helpers.IFileUploadHelper fileUploadHelper,
    AppDbContext context,
    IAuditLogWriter auditLogWriter,
    IGradeCalculationService gradeCalculationService)
    : Controller
{
    public IActionResult Profile()
    {
        return RedirectToAction("Index", "Profile");
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
            try
            {
                var photoPath = await fileUploadHelper.UploadAsync(photo, "uploads/photos", Helpers.FileUploadHelper.MaxPhotoSizeBytes, Helpers.FileUploadHelper.ImageOnlyExtensions);
                if (photoPath != null)
                    user.ProfilePath = photoPath;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Edit", "Profile", new { area = "" });
            }
        }

        if (signature != null && signature.Length > 0)
        {
            try
            {
                var signaturePath = await fileUploadHelper.UploadAsync(signature, "uploads/signatures", Helpers.FileUploadHelper.MaxSignatureSizeBytes, Helpers.FileUploadHelper.ImageOnlyExtensions);
                if (signaturePath != null)
                    user.SignaturePath = signaturePath;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Edit", "Profile", new { area = "" });
            }
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

            var context = new Dictionary<string, string>
            {
                ["UserName"] = user.FullName ?? newEmail,
                ["CallbackUrl"] = callbackUrl ?? string.Empty
            };

            await notificationService.SendAsync(newEmail, null, "change_email", context);

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

        // Non-student accounts keep UserName = Email; students keep UserName = RegistrationNumber
        // so changing email never clobbers the registration number login.
        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains(Role.Student))
        {
            var setUserNameResult = await userManager.SetUserNameAsync(user, email);
            if (!setUserNameResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Error updating username.";
                return RedirectToAction(nameof(Profile));
            }
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

            var context = new Dictionary<string, string>
            {
                ["UserName"] = user.FullName ?? email,
                ["CallbackUrl"] = callbackUrl ?? string.Empty
            };

            await notificationService.SendAsync(email, null, "confirm_email", context);

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

        var missingFields = await GetMissingMandatoryProfileFieldsAsync(user);
        ViewBag.ShowMandatoryProfilePopup = missingFields.Count > 0;
        ViewBag.MissingMandatoryFields = missingFields;

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null)
        {
            return View(new ExamFormsListViewModel());
        }

        var schedules = await dashboardService.GetExamSchedulesForStudentAsync(registration, user.Id);
        schedules = schedules.Where(s => !IsScheduleDeadlinePassed(s)).ToList();

        var forms = new List<ExamFormViewModel>();

        foreach (var schedule in schedules)
        {
            var rejectedOnly = await dashboardService.IsRejectedOnlyForScheduleAsync(schedule.Id, user.Id);
            var hasPaid = !rejectedOnly && await dashboardService.HasExistingPaymentAsync(schedule.Id, registration.Id);
            var hasAdmitCard = hasPaid && await dashboardService.HasAdmitCardForScheduleAsync(schedule.Id, user.Id, registration.Id);
            var admitCardId = hasAdmitCard ? await dashboardService.GetAdmitCardIdForScheduleAsync(schedule.Id, user.Id, registration.Id) : null;
            var rejectionReason = rejectedOnly
                ? await dashboardService.GetLatestRejectionReasonAsync(schedule.Id, user.Id)
                : null;

            forms.Add(new ExamFormViewModel
            {
                ExamScheduleId = schedule.Id,
                Semester = $"Semester {schedule.SemesterInstance?.Semester?.Number ?? 0}",
                ExamScheduleName = schedule.ExamScheduleName,
                HasPaid = hasPaid,
                HasAdmitCard = hasAdmitCard,
                AdmitCardId = admitCardId,
                IsRejected = rejectedOnly,
                RejectionReason = rejectionReason,
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

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null) return View(new List<AdmitCard>());

        var admitCards = await dashboardService.GetAdmitCardsForStudentAsync(user.Id, registration.Id);
        return View(admitCards);
    }

    [HttpGet]
    public async Task<IActionResult> Download(int? id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (id == null) return NotFound();

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null) return NotFound();

        var admitCard = await admitCardService.GetAdmitCardByIdForStudentAsync(id.Value, user.Id, registration.Id);
        if (admitCard == null) return NotFound();

        admitCard.IsDownloaded = true;
        admitCard.DownloadedDate = DateTime.UtcNow;
        await admitCardService.UpdateAdmitCardAsync(admitCard);

        return View("~/Areas/Exams/Views/AdmitCards/PrintAdmitCard.cshtml", admitCard);
    }

    public async Task<IActionResult> PaymentHistory()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null) return View(new List<PaymentRequestLog>());

        var payments = await dashboardService.GetPaymentHistoryForStudentAsync(registration.Id);
        return View(payments);
    }

    public async Task<IActionResult> MySubjects()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
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

        var subjects = await dashboardService.GetSubjectOfferingsForStudentAsync(user.Id, programId);

        return View(subjects);
    }

    public async Task<IActionResult> PayExamFee(int examScheduleId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var missingFields = await GetMissingMandatoryProfileFieldsAsync(user);
        if (missingFields.Count > 0)
        {
            TempData["ErrorMessage"] = "Please complete your mandatory profile details before filling an exam form.";
            return RedirectToAction(nameof(ExamForms));
        }

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null) return NotFound("Student registration not found.");

        var schedule = await dashboardService.GetExamScheduleByIdAsync(examScheduleId);
        if (schedule == null) return NotFound("Exam schedule not found.");

        if (schedule.SemesterInstance == null)
        {
            TempData["ErrorMessage"] = "Exam schedule configuration is incomplete. Please contact support.";
            return RedirectToAction(nameof(ExamForms));
        }

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

        // Rejected forms keep the original confirmed payment; route students to the
        // free re-apply flow instead of charging them again.
        if (await IsRejectedWithConfirmedPaymentAsync(examScheduleId, user.Id, registration.Id))
        {
            TempData["ErrorMessage"] = "You have already paid for this exam form. Please use Apply Again to resubmit it.";
            return RedirectToAction(nameof(ApplyAgain), new { examScheduleId });
        }

        // Direct-URL guard: enforce the same visibility rules as the Exam Forms
        // listing (regular = own semester instance; re-exam = strictly below the
        // student's highest enrolled semester, plus failure/history eligibility).
        if (!await dashboardService.IsScheduleVisibleToStudentAsync(registration, user.Id, examScheduleId))
        {
            TempData["ErrorMessage"] = "You are not eligible for this exam form.";
            return RedirectToAction(nameof(ExamForms));
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
            hasESewa = await context.ESewaConfigurations.AnyAsync();
        }

        var isReExam = dashboardService.IsReExamType(schedule.ExamType?.Name);
        var isRegular = !isReExam;

        List<SubjectFeeDetail> subjectList;
        List<int> selectedSubjectIds;

        if (isRegular)
        {
            subjectList = subjects.Select(s => new SubjectFeeDetail
            {
                SubjectOfferingId = s.Id,
                SubjectName = s.SubjectCatalog?.SubjectName,
                SubjectCode = s.SubjectCatalog?.SubjectCode,
                HasTheory = s.HasTheory,
                HasPractical = s.HasPractical,
                PracticalFee = s.HasPractical ? practicalFee : 0,
                IsSelected = s.IsCompulsory,
                IsFailed = false,
                IsCompulsory = s.IsCompulsory,
                SubjectTypeId = s.SubjectCatalog?.SubjectTypeId ?? 0,
                SubjectTypeName = s.SubjectCatalog?.SubjectType?.Name
            }).ToList();
            selectedSubjectIds = subjectList.Where(s => s.IsCompulsory).Select(s => s.SubjectOfferingId).ToList();
        }
        else
        {
            // Re-exam forms: students with recorded failures get exactly those
            // subjects (their own curriculum version) with the failed exam legs
            // pre-ticked. Students with no result history yet choose freely from
            // THEIR batch curriculum. Each subject offers its available papers:
            // theory and/or practical; a ticked practical leg adds the fee.
            var failedOptions = await dashboardService.GetFailedSubjectOptionsForStudentAsync(examScheduleId, user.Id);
            var knownFailures = failedOptions.Count > 0;
            var failedLegsById = failedOptions.ToDictionary(o => o.SubjectOfferingId, o => o.FailedLegs);

            var selectable = knownFailures
                ? failedOptions.Select(o => o.Offering)
                : await dashboardService.GetReExamSelectableOfferingsAsync(examScheduleId, user.Id);

            subjectList = selectable.Select(s =>
            {
                var failedLegs = failedLegsById.GetValueOrDefault(s.Id);
                return new SubjectFeeDetail
                {
                    SubjectOfferingId = s.Id,
                    SubjectName = s.SubjectCatalog?.SubjectName,
                    SubjectCode = s.SubjectCatalog?.SubjectCode,
                    HasTheory = s.HasTheory,
                    HasPractical = s.HasPractical,
                    PracticalFee = s.HasPractical ? practicalFee : 0,
                    // Free-select mode mirrors the regular form's starting point:
                    // compulsory subjects arrive pre-ticked (both available legs).
                    IsSelected = knownFailures || s.IsCompulsory,
                    IsFailed = knownFailures,
                    FailedTheory = failedLegs.HasFlag(ReExamLegs.Theory),
                    FailedPractical = failedLegs.HasFlag(ReExamLegs.Practical),
                    SelectedTheory = knownFailures
                        ? failedLegs.HasFlag(ReExamLegs.Theory)
                        : s.IsCompulsory && s.HasTheory,
                    SelectedPractical = knownFailures
                        ? failedLegs.HasFlag(ReExamLegs.Practical)
                        : s.IsCompulsory && s.HasPractical,
                    IsCompulsory = s.IsCompulsory,
                    SubjectTypeId = s.SubjectCatalog?.SubjectTypeId ?? 0,
                    SubjectTypeName = s.SubjectCatalog?.SubjectType?.Name
                };
            }).ToList();
            selectedSubjectIds = subjectList.Where(s => s.IsSelected).Select(s => s.SubjectOfferingId).ToList();
        }

        var vm = new ExamPaymentViewModel
        {
            ExamScheduleId = examScheduleId,
            ExamScheduleName = schedule.ExamScheduleName,
            ProgramName = schedule.Program?.ProgramName,
            SemesterName = schedule.SemesterInstance?.Semester?.Name,
            StudentName = user.FullName,
            RegistrationNumber = registration.RegistrationNumber,
            EndDateBs = schedule.EndDateBs,
            AcademicYearName = schedule.SemesterInstance?.AcademicYear?.AcademicYearName,
            ExamTypeName = schedule.ExamType?.Name,
            TotalExamFee = examFee,
            HasESewa = hasESewa,
            HasKhalti = hasKhalti,
            HasConnectIPS = hasConnectIPS,
            IsRegular = isRegular,
            Subjects = subjectList,
            SelectedSubjectIds = selectedSubjectIds,
            PaymentTypes = paymentTypes.Select(pt => new PaymentTypeDetail
            {
                Id = pt.Id,
                Name = pt.PaymentTypeName,
                LogoUrl = pt.LogoUrl
            }).ToList()
        };

        if (!isRegular)
        {
            vm.TotalPracticalFee = subjectList
                .Where(s => vm.SelectedSubjectIds.Contains(s.SubjectOfferingId) && s.SelectedPractical)
                .Sum(s => s.PracticalFee);
        }
        else
        {
            vm.TotalPracticalFee = subjectList.Where(s => vm.SelectedSubjectIds.Contains(s.SubjectOfferingId)).Sum(s => s.PracticalFee);
        }

        if (schedule.ExtendedDate.HasValue && schedule.EndDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var effectiveEnd = DateOnly.FromDateTime(schedule.ExtendedDate.Value);
            if (today > schedule.EndDate.Value && today <= effectiveEnd)
            {
                vm.ExtendedDateCharge = schedule.ExtendedDateCharge ?? 0;
            }
        }

        vm.GrandTotal = vm.TotalExamFee + vm.TotalPracticalFee + vm.ExtendedDateCharge;

        return View(vm);
    }

    private async Task<bool> IsRejectedWithConfirmedPaymentAsync(int examScheduleId, string userId, int studentRegistrationId)
    {
        if (!await dashboardService.IsRejectedOnlyForScheduleAsync(examScheduleId, userId))
            return false;

        return await dashboardService.HasExistingPaymentAsync(examScheduleId, studentRegistrationId);
    }

    [HttpGet]
    public async Task<IActionResult> ApplyAgain(int examScheduleId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var missingFields = await GetMissingMandatoryProfileFieldsAsync(user);
        if (missingFields.Count > 0)
        {
            TempData["ErrorMessage"] = "Please complete your mandatory profile details before filling an exam form.";
            return RedirectToAction(nameof(ExamForms));
        }

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null) return NotFound("Student registration not found.");

        var schedule = await dashboardService.GetExamScheduleByIdAsync(examScheduleId);
        if (schedule == null) return NotFound("Exam schedule not found.");

        if (schedule.SemesterInstance == null)
        {
            TempData["ErrorMessage"] = "Exam schedule configuration is incomplete. Please contact support.";
            return RedirectToAction(nameof(ExamForms));
        }

        if (IsScheduleDeadlinePassed(schedule))
        {
            TempData["ErrorMessage"] = "The deadline for this exam form has passed.";
            return RedirectToAction(nameof(ExamForms));
        }

        if (!await IsRejectedWithConfirmedPaymentAsync(examScheduleId, user.Id, registration.Id))
            return RedirectToAction(nameof(PayExamFee), new { examScheduleId });

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

        var paidLog = await context.PaymentRequestLogs!.AsNoTracking()
            .Where(prl => prl.ExamScheduleId == examScheduleId
                       && prl.StudentRegistrationId == registration.Id
                       && prl.PaymentRequestLogStatus == 1)
            .OrderByDescending(pl => pl.Id)
            .FirstOrDefaultAsync();

        var preSelection = ReExamSubjectSelection.Parse(paidLog?.SelectedSubjectIds);
        if (preSelection.Count == 0)
        {
            // Legacy logs may predate leg-token storage; the rejected form's
            // registered rows are the ground truth of what was selected before.
            foreach (var row in await dashboardService.GetExamSubjectResultsForStudentAsync(user.Id, examScheduleId))
            {
                preSelection[row.SubjectOfferingId] =
                    (row.IsTheoryRegistered == true ? ReExamLegs.Theory : ReExamLegs.None)
                    | (row.IsPracticalRegistered == true ? ReExamLegs.Practical : ReExamLegs.None);
            }
        }

        var subjects = await dashboardService.GetSubjectOfferingsForScheduleAsync(examScheduleId);
        var practicalFee = await dashboardService.GetPracticalSubjectFeeForScheduleAsync(examScheduleId);

        // Reapply supports only gateways the POST flow can actually settle
        // (eSewa / Khalti); anything else configured stays hidden here.
        var supportedPaymentTypes = (await dashboardService.GetActivePaymentTypesAsync())
            .Where(pt => pt.PaymentTypeName != null &&
                         (pt.PaymentTypeName.Contains("esewa", StringComparison.OrdinalIgnoreCase) ||
                          pt.PaymentTypeName.Contains("khalti", StringComparison.OrdinalIgnoreCase)))
            .Select(pt => new PaymentTypeDetail { Id = pt.Id, Name = pt.PaymentTypeName, LogoUrl = pt.LogoUrl })
            .ToList();
        var isReExamForm = dashboardService.IsReExamType(schedule.ExamType?.Name);
        Dictionary<int, ReExamLegs> failedLegsById = new();
        if (isReExamForm)
        {
            subjects = await dashboardService.GetReExamSelectableOfferingsAsync(examScheduleId, user.Id);
            failedLegsById = (await dashboardService.GetFailedSubjectOptionsForStudentAsync(examScheduleId, user.Id))
                .ToDictionary(o => o.SubjectOfferingId, o => o.FailedLegs);
        }
        else
        {
            var failedIds = await dashboardService.GetFailedSubjectOfferingIdsForSemesterAsync(
                user.Id, schedule.SemesterInstance.SemesterId, programId);
            foreach (var id in failedIds)
                failedLegsById[id] = ReExamLegs.Theory;
        }

        var subjectList = subjects.Select(s =>
        {
            var hasExplicitChoice = preSelection.TryGetValue(s.Id, out var preLegs) && preLegs != ReExamLegs.None;
            var failedLegs = failedLegsById.GetValueOrDefault(s.Id);
            var isSelected = preSelection.ContainsKey(s.Id);

            bool selTheory;
            bool selPractical;
            if (hasExplicitChoice)
            {
                selTheory = preLegs.HasFlag(ReExamLegs.Theory);
                selPractical = preLegs.HasFlag(ReExamLegs.Practical);
            }
            else if (preSelection.ContainsKey(s.Id))
            {
                // Legacy plain-id entry: both available papers were registered.
                selTheory = s.HasTheory;
                selPractical = s.HasPractical;
            }
            else
            {
                // Not previously selected: start clean — no compulsory defaulting.
                selTheory = false;
                selPractical = false;
            }

            return new SubjectFeeDetail
            {
                SubjectOfferingId = s.Id,
                SubjectName = s.SubjectCatalog?.SubjectName,
                SubjectCode = s.SubjectCatalog?.SubjectCode,
                HasTheory = s.HasTheory,
                HasPractical = s.HasPractical,
                PracticalFee = s.HasPractical ? practicalFee : 0,
                IsSelected = isSelected,
                IsFailed = failedLegs != ReExamLegs.None,
                FailedTheory = failedLegs.HasFlag(ReExamLegs.Theory),
                FailedPractical = failedLegs.HasFlag(ReExamLegs.Practical),
                SelectedTheory = selTheory,
                SelectedPractical = selPractical,
                IsCompulsory = s.IsCompulsory,
                SubjectTypeId = s.SubjectCatalog?.SubjectTypeId ?? 0,
                SubjectTypeName = s.SubjectCatalog?.SubjectType?.Name
            };
        }).ToList();

        var vm = new ReapplyExamViewModel
        {
            ExamScheduleId = examScheduleId,
            ExamScheduleName = schedule.ExamScheduleName,
            SemesterName = schedule.SemesterInstance?.Semester?.Name,
            ExamTypeName = schedule.ExamType?.Name,
            EndDateBs = schedule.EndDateBs,
            PaidAmount = paidLog?.Amount ?? 0,
            RejectionReason = await dashboardService.GetLatestRejectionReasonAsync(examScheduleId, user.Id),
            Subjects = subjectList,
            PreSelectedSubjectIds = new HashSet<int>(preSelection.Keys),
            ExamFee = await dashboardService.GetExamFeeForScheduleAsync(examScheduleId),
            PracticalFee = practicalFee,
            HasUnpaidTopUp = await dashboardService.HasOpenApplyAgainPaymentAsync(examScheduleId, registration.Id),
            IsPartialForm = isReExamForm,
            PaymentTypes = supportedPaymentTypes
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyAgain(int examScheduleId, string? selectedSubjectIds, string? paymentMethod)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null) return NotFound("Student registration not found.");

        var schedule = await dashboardService.GetExamScheduleByIdAsync(examScheduleId);
        if (schedule == null) return NotFound("Exam schedule not found.");

        if (IsScheduleDeadlinePassed(schedule))
        {
            TempData["ErrorMessage"] = "The deadline for this exam form has passed.";
            return RedirectToAction(nameof(ExamForms));
        }

        if (!await IsRejectedWithConfirmedPaymentAsync(examScheduleId, user.Id, registration.Id))
        {
            TempData["ErrorMessage"] = "This exam form cannot be re-applied.";
            return RedirectToAction(nameof(ExamForms));
        }

        var paidLog = await context.PaymentRequestLogs!.AsNoTracking()
            .Where(prl => prl.ExamScheduleId == examScheduleId
                       && prl.StudentRegistrationId == registration.Id
                       && prl.PaymentRequestLogStatus == 1)
            .OrderByDescending(pl => pl.Id)
            .FirstOrDefaultAsync();

        // Regular-schedule reapply is a locked resubmission: ignore any
        // client-posted selection and restore exactly what the confirmed
        // payment covered. Partial (re-exam) schedules keep per-leg choice.
        if (!dashboardService.IsReExamType(schedule.ExamType?.Name))
        {
            if (paidLog == null || string.IsNullOrWhiteSpace(paidLog.SelectedSubjectIds))
            {
                // Legacy log without subject tokens: default to the schedule's
                // compulsory offerings with their available legs (mirrors the
                // GET view's pre-tick fallback).
                var lockedSelection = new Dictionary<int, ReExamLegs>();
                foreach (var offering in await dashboardService.GetSubjectOfferingsForScheduleAsync(examScheduleId))
                {
                    if (!offering.IsCompulsory) continue;
                    var legs = ReExamLegs.None;
                    if (offering.HasTheory) legs |= ReExamLegs.Theory;
                    if (offering.HasPractical) legs |= ReExamLegs.Practical;
                    lockedSelection[offering.Id] = legs;
                }
                selectedSubjectIds = ReExamSubjectSelection.Format(lockedSelection);
            }
            else
            {
                selectedSubjectIds = paidLog.SelectedSubjectIds;
            }
        }

        var selection = ReExamSubjectSelection.Parse(selectedSubjectIds);
        var subjectIds = selection.Keys.ToList();
        if (subjectIds.Count == 0)
        {
            TempData["ErrorMessage"] = "At least one subject must be selected.";
            return RedirectToAction(nameof(ApplyAgain), new { examScheduleId });
        }

        // Upgrade legacy plain-id tokens to explicit legs so validation, the
        // charge-delta math and every stored log carry unambiguous data.
        selection = await NormalizeLegacySelectionLegsAsync(examScheduleId, selection);

        var selectionValidation = await ValidateSubjectSelectionAsync(examScheduleId, selection);
        if (!selectionValidation.Ok)
        {
            TempData["ErrorMessage"] = selectionValidation.Error;
            return RedirectToAction(nameof(ApplyAgain), new { examScheduleId });
        }

        // Charge-delta provision: the original payment stays with the college.
        // Selecting extra papers (e.g. a practical leg that was skipped the
        // first time) requires paying only the difference before the form can
        // be revived. Reductions are absorbed by the college (no refunds).
        var newTotal = await dashboardService.ComputeSelectionFeeAsync(examScheduleId, selection);
        var previouslyPaid = paidLog?.Amount ?? 0;
        var delta = newTotal - previouslyPaid;

        if (delta > 0)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                TempData["ErrorMessage"] =
                    $"Your updated selection adds Rs {delta:N0} to the Rs {previouslyPaid:N0} already paid. Please choose a payment method for the additional amount.";
                return RedirectToAction(nameof(ApplyAgain), new { examScheduleId });
            }

            var invoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}-{registration.Id}";
            var fullName = registration.FirstName.GetFullName(registration.MiddleName, registration.LastName);

            if (paymentMethod.Contains("esewa", StringComparison.OrdinalIgnoreCase))
            {
                var transactionUuid = esewaService.GenerateTransactionUuid();
                var esewaLogId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                    examScheduleId, registration.Id, delta, "esewa", invoiceNumber, selection,
                    fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD, transactionUuid);
                await dashboardService.SupersedeOpenApplyAgainPaymentsAsync(examScheduleId, registration.Id, esewaLogId);
                logger.LogInformation("Student {UserId} initiated an eSewa reapply top-up of Rs {Delta} for scheduleId={ScheduleId}",
                    user.Id, delta, examScheduleId);
                return await InitiateESewaGatewayAsync(delta, esewaLogId, transactionUuid, invoiceNumber, registration.Id, examScheduleId);
            }

            if (paymentMethod.Contains("khalti", StringComparison.OrdinalIgnoreCase))
            {
                var khaltiLogId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                    examScheduleId, registration.Id, delta, "khalti", invoiceNumber, selection,
                    fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD);
                await dashboardService.SupersedeOpenApplyAgainPaymentsAsync(examScheduleId, registration.Id, khaltiLogId);
                logger.LogInformation("Student {UserId} initiated a Khalti reapply top-up of Rs {Delta} for scheduleId={ScheduleId}",
                    user.Id, delta, examScheduleId);
                return await InitiateKhaltiGatewayAsync(delta, khaltiLogId, invoiceNumber, examScheduleId,
                    schedule.ExamScheduleName, fullName, registration.Email, registration.ContactNumber);
            }

            // Cash / on-counter style methods confirm immediately; the shared
            // post-payment handler detects the top-up context and revives the
            // rejected form instead of creating a duplicate registration.
            var cashLogId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, delta, paymentMethod, invoiceNumber, selection);
            await dashboardService.SupersedeOpenApplyAgainPaymentsAsync(examScheduleId, registration.Id, cashLogId);

            await HandlePostPaymentRegistration(cashLogId);
            await dashboardService.UpdatePaymentRequestLogAsync(cashLogId, invoiceNumber, true,
                $"{{\"method\":\"{paymentMethod}\",\"amount\":{delta}}}",
                $"Additional payment recorded via {paymentMethod}.");
            await auditLogWriter.LogAsync(ActivityTypes.PaymentProcessed,
                $"Additional payment of Rs {delta:N0} via {paymentMethod} recorded (Invoice {invoiceNumber})",
                new { invoiceNumber, amount = delta, method = paymentMethod, examScheduleId, registrationId = registration.Id },
                entityName: "PaymentRequestLog", entityId: cashLogId.ToString());

            await CompleteExamFormSubmissionAsync(cashLogId, invoiceNumber);
            return RedirectToAction(nameof(PaymentSuccess));
        }

        var (success, message) = await dashboardService.ReapplyExamRegistrationAsync(
            examScheduleId, user.Id, registration.Id, subjectIds, selection);

        if (!success)
        {
            logger.LogWarning("ApplyAgain failed for user={UserId}, scheduleId={ScheduleId}: {Message}", user.Id, examScheduleId, message);
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(ApplyAgain), new { examScheduleId });
        }

        logger.LogInformation("Student {UserId} re-applied rejected exam form for scheduleId={ScheduleId}", user.Id, examScheduleId);
        TempData["SuccessMessage"] = message;
        return RedirectToAction(nameof(ExamForms));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(int examScheduleId, string paymentMethod, decimal amount, string? selectedSubjectIds)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var missingFields = await GetMissingMandatoryProfileFieldsAsync(user);
        if (missingFields.Count > 0)
        {
            TempData["ErrorMessage"] = "Please complete your mandatory profile details before filling an exam form.";
            return RedirectToAction(nameof(ExamForms));
        }

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null) return NotFound("Student registration not found.");

        // A rejected form keeps its original confirmed payment; never allow a second
        // payment request for the same schedule.
        if (await IsRejectedWithConfirmedPaymentAsync(examScheduleId, user.Id, registration.Id))
        {
            TempData["ErrorMessage"] = "You have already paid for this exam form. Please use Apply Again to resubmit it.";
            return RedirectToAction(nameof(ApplyAgain), new { examScheduleId });
        }

        var invoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}-{registration.Id}";
        var selection = ReExamSubjectSelection.Parse(selectedSubjectIds);

        var selectionValidation = await ValidateSubjectSelectionAsync(examScheduleId, selection);
        if (!selectionValidation.Ok)
        {
            TempData["ErrorMessage"] = selectionValidation.Error;
            return RedirectToAction(nameof(PayExamFee), new { examScheduleId });
        }

        // Security: the posted amount is never trusted. The charge always comes
        // from the schedule rates applied to the validated selection.
        amount = await dashboardService.ComputeSelectionFeeAsync(examScheduleId, selection);

        int logId;
        if (selection.Count == 0)
        {
            logId = await dashboardService.CreatePaymentRequestLogAsync(
                examScheduleId, registration.Id, amount, paymentMethod, invoiceNumber);
        }
        else
        {
            logId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, amount, paymentMethod, invoiceNumber, selection);
        }

        await HandlePostPaymentRegistration(logId);
        await dashboardService.UpdatePaymentRequestLogAsync(logId, invoiceNumber, true, $"{{\"method\":\"{paymentMethod}\",\"amount\":{amount}}}", $"Payment recorded via {paymentMethod}.");
        await auditLogWriter.LogAsync(ActivityTypes.PaymentProcessed,
            $"Payment of Rs {amount:N0} via {paymentMethod} recorded (Invoice {invoiceNumber})",
            new { invoiceNumber, amount, method = paymentMethod, examScheduleId, registrationId = registration.Id },
            entityName: "PaymentRequestLog", entityId: logId.ToString());

        await CompleteExamFormSubmissionAsync(logId, invoiceNumber);
        return RedirectToAction(nameof(PaymentSuccess));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ESewaPayment(int examScheduleId, decimal amount, string? selectedSubjectIds)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var missingFields = await GetMissingMandatoryProfileFieldsAsync(user);
        if (missingFields.Count > 0)
        {
            TempData["ErrorMessage"] = "Please complete your mandatory profile details before filling an exam form.";
            return RedirectToAction(nameof(ExamForms));
        }

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null) return NotFound("Student registration not found.");

        var invoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}-{registration.Id}";
        var selection = ReExamSubjectSelection.Parse(selectedSubjectIds);

        var selectionValidation = await ValidateSubjectSelectionAsync(examScheduleId, selection);
        if (!selectionValidation.Ok)
        {
            TempData["ErrorMessage"] = selectionValidation.Error;
            return RedirectToAction(nameof(PayExamFee), new { examScheduleId });
        }

        // Security: the posted amount is never trusted (see ProcessPayment).
        amount = await dashboardService.ComputeSelectionFeeAsync(examScheduleId, selection);

        var fullName = registration.FirstName.GetFullName(registration.MiddleName, registration.LastName);

        var transactionUuid = esewaService.GenerateTransactionUuid();

        int logId;
        if (selection.Count == 0)
        {
            logId = await dashboardService.CreatePaymentRequestLogAsync(
                examScheduleId, registration.Id, amount, "esewa", invoiceNumber,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD, transactionUuid);
        }
        else
        {
            logId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, amount, "esewa", invoiceNumber, selection,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD, transactionUuid);
        }

        return await InitiateESewaGatewayAsync(amount, logId, transactionUuid, invoiceNumber, registration.Id, examScheduleId);
    }

    private async Task<IActionResult> InitiateESewaGatewayAsync(
        decimal amount, int logId, string transactionUuid, string invoiceNumber, int studentRegistrationId, int examScheduleId)
    {
        var defaultCallbackUrl = Url.Action(nameof(ESewaCallback), "StudentDashboard", new { area = "Students" }, Request.Scheme)!;
        var successUrl = defaultCallbackUrl;
        var failureUrl = defaultCallbackUrl;

        logger.LogInformation("ESewaPayment: amount={Amount}, transactionUuid={Uuid}, successUrl={SuccessUrl}, failureUrl={FailureUrl}",
            amount, transactionUuid, successUrl, failureUrl);

        var formData = await esewaService.GeneratePaymentFormDataAsync(amount, transactionUuid, successUrl!, failureUrl!);

        HttpContext.Session.SetInt32("ESewaLogId", logId);
        await auditLogWriter.LogAsync(ActivityTypes.PaymentInitiated,
            $"eSewa payment initiated for Rs {amount:N0} (Invoice {invoiceNumber})",
            new { gateway = "esewa", invoiceNumber, amount, examScheduleId, registrationId = studentRegistrationId },
            entityName: "PaymentRequestLog", entityId: logId.ToString());
        ViewBag.LogId = logId;
        ViewBag.TransactionUuid = transactionUuid;

        return View("ESewaPayment", formData);
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

                var uuid = log?.TransactionId;
                if (log != null && !string.IsNullOrEmpty(uuid))
                {
                    try
                    {
                        var status = await esewaService.VerifyTransactionAsync(uuid, log.Amount);
                        if (status != null && string.Equals(status.Status, "COMPLETE", StringComparison.OrdinalIgnoreCase))
                        {
                            logger.LogWarning("ESewaCallback: no callback data but status API reports COMPLETE for logId={LogId}, uuid={Uuid}. Treating as paid.",
                                sessionLogId.Value, uuid);

                            await HandlePostPaymentRegistration(sessionLogId.Value);
                            await dashboardService.UpdatePaymentRequestLogAsync(sessionLogId.Value, status.TransactionCode ?? "", true,
                                System.Text.Json.JsonSerializer.Serialize(status),
                                "Payment verified via eSewa status check (callback carried no data).");
                            await auditLogWriter.LogAsync(ActivityTypes.PaymentVerified,
                                $"eSewa payment verified via status check (Transaction {status.TransactionCode})",
                                new { gateway = "esewa", transactionCode = status.TransactionCode, transactionUuid = uuid, amount = status.TotalAmount },
                                entityName: "PaymentRequestLog", entityId: sessionLogId.Value.ToString());

                            await CompleteExamFormSubmissionAsync(sessionLogId.Value, status.TransactionCode ?? uuid);
                            TempData["TransactionCode"] = status.TransactionCode;
                            TempData["TransactionUuid"] = uuid;
                            return RedirectToAction(nameof(PaymentSuccess));
                        }

                        var statusData = status != null
                            ? $"No response data received from eSewa. QueryString: {Request.QueryString}. Status check: {status.Status}"
                            : $"No response data received from eSewa. QueryString: {Request.QueryString}. Status check: unreachable";
                        await dashboardService.UpdatePaymentRequestLogAsync(sessionLogId.Value, "", false, statusData, "Payment not completed at eSewa.");
                    }
                    catch (Exception statusEx)
                    {
                        logger.LogError(statusEx, "ESewaCallback: status check failed for logId={LogId}", sessionLogId.Value);
                        await dashboardService.UpdatePaymentRequestLogAsync(sessionLogId.Value, "", false,
                            $"No response data received from eSewa. QueryString: {Request.QueryString}", "No response data received from eSewa.");
                    }
                }
                else
                {
                    await dashboardService.UpdatePaymentRequestLogAsync(sessionLogId.Value, "", false, $"No response data received from eSewa. QueryString: {Request.QueryString}", "No response data received from eSewa.");
                }
            }

            await auditLogWriter.LogAsync(ActivityTypes.PaymentVerificationFailed,
                "eSewa callback received no response data",
                new { gateway = "esewa", reason = "no_data" }, AuditSeverity.Warning);
            TempData["ErrorMessage"] = "Payment was cancelled or not completed at eSewa. No amount has been deducted. Please try again.";
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

                await auditLogWriter.LogAsync(ActivityTypes.PaymentVerificationFailed,
                    "eSewa callback returned an invalid response payload",
                    new { gateway = "esewa", reason = "invalid_response" }, AuditSeverity.Warning);
                TempData["ErrorMessage"] = "Invalid response from eSewa.";
                return RedirectToAction(nameof(PaymentFailure));
            }

            logger.LogInformation("ESewaCallback: txCode={TxCode}, status={Status}, totalAmount={Amount}, uuid={Uuid}, productCode={ProductCode}",
                response.TransactionCode, response.Status, response.TotalAmount, response.TransactionUuid, response.ProductCode);

            var sigValid = await esewaService.VerifyResponseSignatureAsync(response, decodedJson);
            logger.LogInformation("ESewaCallback: signatureValid={SigValid}", sigValid);

            if (!sigValid)
            {
                if (sessionLogId.HasValue)
                    await dashboardService.UpdatePaymentRequestLogAsync(sessionLogId.Value, response.TransactionCode ?? "", false, decodedJson, "Signature verification failed via eSewa.");

                await auditLogWriter.LogAsync(ActivityTypes.PaymentVerificationFailed,
                    "eSewa callback signature verification failed",
                    new { gateway = "esewa", reason = "signature_invalid", transactionCode = response.TransactionCode }, AuditSeverity.Error);
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

                await auditLogWriter.LogAsync(ActivityTypes.PaymentVerificationFailed,
                    "eSewa transaction verification failed",
                    new { gateway = "esewa", reason = "transaction_not_complete", status = verified?.Status, transactionCode = response.TransactionCode }, AuditSeverity.Error);
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
                await auditLogWriter.LogAsync(ActivityTypes.PaymentVerified,
                    $"eSewa payment verified (Transaction {response.TransactionCode})",
                    new { gateway = "esewa", transactionCode = response.TransactionCode, transactionUuid = response.TransactionUuid, amount = response.TotalAmount },
                    entityName: "PaymentRequestLog", entityId: resolvedLogId.Value.ToString());
            }

            if (resolvedLogId.HasValue)
            {
                await CompleteExamFormSubmissionAsync(resolvedLogId.Value, response.TransactionCode);
            }
            else
            {
                TempData["SuccessMessage"] = "Payment successful!";
            }
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

        var missingFields = await GetMissingMandatoryProfileFieldsAsync(user);
        if (missingFields.Count > 0)
        {
            TempData["ErrorMessage"] = "Please complete your mandatory profile details before filling an exam form.";
            return RedirectToAction(nameof(ExamForms));
        }

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null) return NotFound("Student registration not found.");

        var invoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}-{registration.Id}";
        var selection = ReExamSubjectSelection.Parse(selectedSubjectIds);

        var selectionValidation = await ValidateSubjectSelectionAsync(examScheduleId, selection);
        if (!selectionValidation.Ok)
        {
            TempData["ErrorMessage"] = selectionValidation.Error;
            return RedirectToAction(nameof(PayExamFee), new { examScheduleId });
        }

        // Security: the posted amount is never trusted (see ProcessPayment).
        amount = await dashboardService.ComputeSelectionFeeAsync(examScheduleId, selection);

        var schedule = await dashboardService.GetExamScheduleByIdAsync(examScheduleId);
        var fullName = registration.FirstName.GetFullName(registration.MiddleName, registration.LastName);

        int logId;
        if (selection.Count == 0)
        {
            logId = await dashboardService.CreatePaymentRequestLogAsync(
                examScheduleId, registration.Id, amount, "khalti", invoiceNumber,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD);
        }
        else
        {
            logId = await dashboardService.CreatePaymentRequestLogWithSubjectsAsync(
                examScheduleId, registration.Id, amount, "khalti", invoiceNumber, selection,
                fullName, registration.Email, registration.ContactNumber, registration.DateOfBirthAD);
        }

        return await InitiateKhaltiGatewayAsync(amount, logId, invoiceNumber, examScheduleId,
            schedule?.ExamScheduleName, fullName, registration.Email, registration.ContactNumber);
    }

    private async Task<IActionResult> InitiateKhaltiGatewayAsync(
        decimal amount, int logId, string invoiceNumber, int examScheduleId, string? examScheduleName,
        string? customerFullName = null, string? customerEmail = null, string? customerPhone = null)
    {
        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var baseUrl = $"{scheme}://{host}";

        var returnUrl = Url.Action(nameof(KhaltiCallback), "StudentDashboard",
            new { area = "Students" }, scheme)!;

        if (string.IsNullOrWhiteSpace(customerEmail))
            customerEmail = null;
        else
        {
            try { _ = new System.Net.Mail.MailAddress(customerEmail); }
            catch { customerEmail = null; }
        }

        var khaltiRequest = new KhaltiInitiateRequest
        {
            ReturnUrl = returnUrl,
            WebsiteUrl = baseUrl,
            Amount = (long)(amount * 100),
            PurchaseOrderId = invoiceNumber,
            PurchaseOrderName = $"Exam Fee - {examScheduleName ?? ""}",
            CustomerInfo = new KhaltiCustomerInfo
            {
                Name = string.IsNullOrWhiteSpace(customerFullName) ? null : customerFullName,
                Email = customerEmail,
                Phone = string.IsNullOrWhiteSpace(customerPhone) ? null : customerPhone
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

            await auditLogWriter.LogAsync(ActivityTypes.PaymentInitiated,
                $"Khalti payment initiated for Rs {amount:N0} (Invoice {invoiceNumber})",
                new { gateway = "khalti", invoiceNumber, amount, examScheduleId, pidx = response.Pidx },
                entityName: "PaymentRequestLog", entityId: logId.ToString());
            logger.LogInformation("Khalti redirecting to: {PaymentUrl}", response.PaymentUrl);

            // Persist the pidx on the payment log so stuck payments can be
            // reconciled later via the gateway lookup API without any session.
            if (!string.IsNullOrEmpty(response.Pidx))
                await dashboardService.UpdatePaymentRequestLogTransactionIdAsync(logId, response.Pidx);

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

                await auditLogWriter.LogAsync(ActivityTypes.PaymentVerificationFailed,
                    $"Khalti payment verification failed (status: {lookup?.Status ?? "Unknown"})",
                    new { gateway = "khalti", pidx, reason = "lookup_not_completed", lookupStatus = lookup?.Status, callbackStatus = status }, AuditSeverity.Error);

                TempData["ErrorMessage"] = GetKhaltiVerificationFailureMessage(lookup?.Status);
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
                await auditLogWriter.LogAsync(ActivityTypes.PaymentVerified,
                    $"Khalti payment verified (Transaction {lookup.TransactionId ?? transaction_id})",
                    new { gateway = "khalti", pidx, transactionId = lookup.TransactionId ?? transaction_id, amount = lookup.TotalAmount },
                    entityName: "PaymentRequestLog", entityId: khaltiResolvedLogId.Value.ToString());
            }

            if (khaltiResolvedLogId.HasValue)
            {
                await CompleteExamFormSubmissionAsync(khaltiResolvedLogId.Value, lookup.TransactionId ?? transaction_id);
            }
            else
            {
                TempData["SuccessMessage"] = "Payment successful!";
            }
            TempData["TransactionCode"] = lookup.TransactionId ?? transaction_id;
            TempData["TransactionUuid"] = pidx;

            return RedirectToAction(nameof(PaymentSuccess));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Khalti callback processing failed");
            if (ex.Message.StartsWith("Khalti configuration is invalid", StringComparison.OrdinalIgnoreCase))
                TempData["ErrorMessage"] = ex.Message;
            else
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

    private static string GetKhaltiVerificationFailureMessage(string? status) =>
        FWU.Exam.Management.Infrastructure.Services.KhaltiPaymentStatus.GetVerificationFailureMessage(status);

    private async Task<List<string>> GetMissingMandatoryProfileFieldsAsync(AppUser user) =>
        await dashboardService.GetMissingMandatoryProfileFieldsAsync(user.Id, user.Email, user.PhoneNumber, user.ProfilePath, user.SignaturePath);

    private async Task<(bool Ok, string? Error)> ValidateSubjectSelectionAsync(int examScheduleId, Dictionary<int, ReExamLegs> selection)
    {
        var schedule = await dashboardService.GetExamScheduleByIdAsync(examScheduleId);
        if (schedule == null)
            return (false, "Exam schedule not found.");

        if (dashboardService.IsReExamType(schedule.ExamType?.Name))
            return await ValidateReExamSubjectSelectionAsync(examScheduleId, selection);

        var subjectIds = selection.Keys.ToList();

        var offerings = await dashboardService.GetSubjectOfferingsForScheduleAsync(examScheduleId);
        var offeringLookup = offerings.ToDictionary(o => o.Id);
        foreach (var id in subjectIds)
        {
            if (!offeringLookup.ContainsKey(id))
                return (false, "Selected subject is not part of this exam schedule.");
        }

        var electiveGroups = offerings
            .Where(o => !o.IsCompulsory && o.SubjectCatalog != null)
            .GroupBy(o => o.SubjectCatalog!.SubjectTypeId);

        foreach (var group in electiveGroups)
        {
            var groupName = group.First().SubjectCatalog?.SubjectType?.Name ?? group.Key.ToString();
            var selectedCount = group.Count(o => subjectIds.Contains(o.Id));
            if (selectedCount == 0)
                return (false, $"Please select at least one elective subject from {groupName}.");
            if (selectedCount > 1)
                return (false, $"Only one subject can be selected from {groupName}.");
        }

        return (true, null);
    }

    // Re-exam forms draw subjects from the student's failed history, which may
    // reference offerings of an older curriculum version than the schedule
    // resolves to. Membership is therefore validated against every offering of
    // the same program + semester number regardless of version. Each chosen leg
    // must actually be offered by that subject (HasTheory/HasPractical) and at
    // least one leg must be ticked per subject. The elective-group rule is
    // skipped (students may sit what they failed).
    private async Task<(bool Ok, string? Error)> ValidateReExamSubjectSelectionAsync(int examScheduleId, Dictionary<int, ReExamLegs> selection)
    {
        if (selection.Count == 0)
            return (false, "Please select at least one subject.");

        var info = await context.ExamSchedules.AsNoTracking()
            .Where(es => es.Id == examScheduleId)
            .Select(es => new { es.ProgramId, SemesterNumber = es.SemesterInstance!.Semester!.Number })
            .FirstOrDefaultAsync();
        if (info == null)
            return (false, "Exam schedule not found.");

        var offerings = await context.SubjectOfferings.AsNoTracking()
            .Where(so => so.ProgramId == info.ProgramId
                      && so.Semester != null && so.Semester.Number == info.SemesterNumber)
            .Select(so => new { so.Id, so.HasTheory, so.HasPractical })
            .ToListAsync();
        var offeringLookup = offerings.ToDictionary(o => o.Id);

        foreach (var (offeringId, legs) in selection)
        {
            if (!offeringLookup.TryGetValue(offeringId, out var offering))
                return (false, "Selected subject is not part of this exam schedule.");

            // Legacy plain-id tokens carry ReExamLegs.None meaning "both
            // available papers" (see ReExamSubjectSelection.Parse) — resolve
            // them instead of rejecting.
            var effectiveLegs = legs == ReExamLegs.None
                ? (offering.HasTheory ? ReExamLegs.Theory : ReExamLegs.None)
                  | (offering.HasPractical ? ReExamLegs.Practical : ReExamLegs.None)
                : legs;

            if (effectiveLegs == ReExamLegs.None || (!effectiveLegs.HasFlag(ReExamLegs.Theory) && !effectiveLegs.HasFlag(ReExamLegs.Practical)))
                return (false, $"Please select at least one exam paper (theory or practical) for every chosen subject.");

            if (effectiveLegs.HasFlag(ReExamLegs.Theory) && !offering.HasTheory)
                return (false, "One of the selected subjects does not offer a theory paper.");
            if (effectiveLegs.HasFlag(ReExamLegs.Practical) && !offering.HasPractical)
                return (false, "One of the selected subjects does not offer a practical paper.");
        }

        return (true, null);
    }

    // Legacy plain-id tokens ("301") parse to ReExamLegs.None meaning "both
    // available papers". Upgrade them to explicit legs so validation, the
    // charge-delta math and every stored log carry unambiguous data.
    private async Task<Dictionary<int, ReExamLegs>> NormalizeLegacySelectionLegsAsync(
        int examScheduleId, Dictionary<int, ReExamLegs> selection)
    {
        var legacyIds = selection.Where(kvp => kvp.Value == ReExamLegs.None)
            .Select(kvp => kvp.Key)
            .ToList();
        if (legacyIds.Count == 0)
            return selection;

        var schedule = await dashboardService.GetExamScheduleByIdAsync(examScheduleId);
        if (schedule == null)
            return selection;

        Dictionary<int, (bool HasTheory, bool HasPractical)> availability;
        if (dashboardService.IsReExamType(schedule.ExamType?.Name))
        {
            // Same membership pool the re-exam validator uses: every offering
            // of the program + semester number regardless of curriculum version.
            var info = await context.ExamSchedules.AsNoTracking()
                .Where(es => es.Id == examScheduleId)
                .Select(es => new { es.ProgramId, SemesterNumber = es.SemesterInstance!.Semester!.Number })
                .FirstOrDefaultAsync();
            if (info == null)
                return selection;

            availability = await context.SubjectOfferings.AsNoTracking()
                .Where(so => so.ProgramId == info.ProgramId
                          && so.Semester != null && so.Semester.Number == info.SemesterNumber)
                .Select(so => new { so.Id, so.HasTheory, so.HasPractical })
                .ToDictionaryAsync(x => x.Id, x => (x.HasTheory, x.HasPractical));
        }
        else
        {
            availability = (await dashboardService.GetSubjectOfferingsForScheduleAsync(examScheduleId))
                .ToDictionary(o => o.Id, o => (o.HasTheory, o.HasPractical));
        }

        foreach (var offeringId in legacyIds)
        {
            if (!availability.TryGetValue(offeringId, out var flags))
                continue;

            var legs = ReExamLegs.None;
            if (flags.HasTheory) legs |= ReExamLegs.Theory;
            if (flags.HasPractical) legs |= ReExamLegs.Practical;
            selection[offeringId] = legs;
        }

        return selection;
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

        // A confirmed payment while the form is rejected with an older confirmed
        // payment is a reapply top-up: revive the rejected registration with this
        // log's subject tokens rather than creating a duplicate registration.
        if (await dashboardService.TryCompleteApplyAgainTopUpAsync(logId, user.Id))
        {
            logger.LogInformation("HandlePostPaymentRegistration: Reapply top-up completed for logId={LogId}", logId);
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

        var selection = ReExamSubjectSelection.Parse(paymentLog.SelectedSubjectIds);

        if (selection.Count == 0)
        {
            logger.LogWarning("HandlePostPaymentRegistration: No subject IDs on logId={LogId} (SelectedSubjectIds={SelectedSubjectIds}). Skipping registration creation.",
                logId, paymentLog.SelectedSubjectIds ?? "null");
            return;
        }

        logger.LogInformation("HandlePostPaymentRegistration: Creating ExamRegistration for logId={LogId}, scheduleId={ScheduleId}, userId={UserId}, subjects={SubjectCount}",
            logId, paymentLog.ExamScheduleId, user.Id, selection.Count);
        await dashboardService.CreateExamRegistrationAsync(paymentLog.ExamScheduleId, user.Id, paymentLog.Amount, selection.Keys.ToList(), paymentLog.StudentRegistrationId.Value, selection);
    }

    private async Task CompleteExamFormSubmissionAsync(int logId, string? reference)
    {
        var log = await dashboardService.GetPaymentLogByIdAsync(logId);
        if (log == null)
        {
            logger.LogWarning("CompleteExamFormSubmission: PaymentRequestLog not found for logId={LogId}", logId);
            return;
        }

        var schedule = await dashboardService.GetExamScheduleByIdAsync(log.ExamScheduleId);
        var scheduleName = schedule?.ExamScheduleName ?? $"Exam Schedule #{log.ExamScheduleId}";

        try
        {
            await notificationService.SendAsync(
                log.Email,
                log.MobileNumber,
                "exam_form_submitted",
                new Dictionary<string, string>
                {
                    ["StudentName"] = log.FullName,
                    ["ExamScheduleName"] = scheduleName,
                    ["Amount"] = log.Amount.ToString("N0"),
                    ["Reference"] = reference ?? string.Empty
                });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send exam form submitted notification for logId={LogId}", log.Id);
        }

        TempData["ExamScheduleName"] = scheduleName;
        TempData["SuccessMessage"] = $"Your exam form for \"{scheduleName}\" has been submitted successfully.";
    }

    [RequirePermission(Permissions.StudentPortalMarksheet)]
    public async Task<IActionResult> MarksheetPrint(int? examScheduleId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration?.RegistrationNumber == null)
        {
            return View(new List<MarksheetViewModel>());
        }

        var resultRecords = await dashboardService.GetResultRecordsAsync(registration.RegistrationNumber);
        var examRegistrations = await dashboardService.GetStudentExamRegistrationsAsync(user.Id);
        var gradePointMap = await GetGradePointMapAsync(registration.ProgramId);

        var marksheets = new List<MarksheetViewModel>();

        foreach (var rr in resultRecords)
        {
            var scheduleId = rr.ExamScheduleId;
            if (scheduleId == null) continue;

            if (examScheduleId.HasValue && scheduleId.Value != examScheduleId.Value) continue;

            var subjects = GetMarksheetSubjects(examRegistrations, scheduleId.Value, gradePointMap);

            marksheets.Add(new MarksheetViewModel
            {
                RegistrationNumber = rr.RegistrationNumber,
                StudentName = rr.StudentName,
                Program = rr.Program?.ProgramName,
                Faculty = rr.Program?.Faculty?.Name,
                ExamSchedule = rr.ExamSchedule?.ExamScheduleName,
                Semester = rr.ExamSchedule?.SemesterInstance?.Semester?.Name,
                Level = rr.ExamSchedule?.Level?.LevelName,
                ExamType = rr.ExamType?.Name,
                AcademicYear = rr.AcademicYear?.AcademicYearName,
                College = registration.College?.Name,
                TotalGpa = rr.Gpa,
                Result = rr.Result,
                TheoryGrade = rr.TheoryObtainedGrade,
                PracticalGrade = rr.PracticalObtainedGrade,
                SymbolNumber = rr.SymbolNumber,
                ExamScheduleId = scheduleId.Value,
                Subjects = subjects
            });
        }

        foreach (var er in examRegistrations)
        {
            if (examScheduleId.HasValue && er.ExamScheduleId != examScheduleId.Value) continue;

            if (!marksheets.Any(m => m.ExamScheduleId == er.ExamScheduleId))
            {
                var subjects = GetMarksheetSubjects(examRegistrations, er.ExamScheduleId, gradePointMap);
                var computedResult = !subjects.Any() ? "Pending"
                    : subjects.Any(s => s.Status == "Fail") ? "Fail"
                    : subjects.All(s => s.Status == "Pass") ? "Pass"
                    : "Pending";
                var totalCredits = subjects.Sum(s => s.CreditHours ?? 0f);
                var totalGradePoints = subjects.Sum(s => s.GradePoint ?? 0m);
                var gpa = totalCredits > 0 ? (totalGradePoints / (decimal)totalCredits) : (decimal?)null;
                marksheets.Add(new MarksheetViewModel
                {
                    RegistrationNumber = registration.RegistrationNumber,
                    StudentName = registration.FirstName.GetFullName(registration.MiddleName, registration.LastName),
                    Program = er.ExamSchedule?.Program?.ProgramName,
                    Faculty = er.ExamSchedule?.Program?.Faculty?.Name,
                    AcademicYear = er.ExamSchedule?.SemesterInstance?.AcademicYear?.AcademicYearName,
                    College = registration.College?.Name,
                    ExamSchedule = er.ExamSchedule?.ExamScheduleName,
                    Semester = er.ExamSchedule?.SemesterInstance?.Semester?.Name,
                    Level = er.ExamSchedule?.Level?.LevelName,
                    ExamType = er.ExamSchedule?.ExamType?.Name,
                    ExamScheduleId = er.ExamScheduleId,
                    SymbolNumber = MarksheetSymbolNumber(er),
                    Result = computedResult,
                    TotalGpa = gpa?.ToString("0.00"),
                    Subjects = subjects
                });
            }
        }

        var sorted = marksheets.OrderByDescending(m => m.ExamScheduleId).ToList();

        ViewBag.StudentRegistration = registration;

        return View("Marksheet", sorted);
    }

    private static string? MarksheetSymbolNumber(ExamRegistration? er)
        => !string.IsNullOrEmpty(er?.ExamRollNumber) ? er.ExamRollNumber : er?.SymbolNumber;

    [RequirePermission(Permissions.StudentPortalMarksheet)]
    public async Task<IActionResult> Marksheet()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);

        // Show all marksheets for logged-in student
        if (registration?.RegistrationNumber == null)
        {
            return View(new List<MarksheetViewModel>());
        }

        var allResultRecords = await dashboardService.GetResultRecordsAsync(registration.RegistrationNumber);
        var allExamRegistrations = await dashboardService.GetStudentExamRegistrationsAsync(user.Id);
        var gradePointMap = await GetGradePointMapAsync(registration.ProgramId);

        var allMarksheets = new List<MarksheetViewModel>();

        foreach (var rr in allResultRecords)
        {
            var scheduleId = rr.ExamScheduleId;
            if (scheduleId == null) continue;

            var subjects = GetMarksheetSubjects(allExamRegistrations, scheduleId.Value, gradePointMap);

            allMarksheets.Add(new MarksheetViewModel
            {
                RegistrationNumber = rr.RegistrationNumber,
                StudentName = rr.StudentName,
                Program = rr.Program?.ProgramName,
                Faculty = rr.Program?.Faculty?.Name,
                ExamSchedule = rr.ExamSchedule?.ExamScheduleName,
                Semester = rr.ExamSchedule?.SemesterInstance?.Semester?.Name,
                SemesterId = rr.ExamSchedule?.SemesterInstance?.Semester?.Id,
                SemesterYear = rr.ExamSchedule?.SemesterInstance?.Semester?.Number ?? 0,
                SemesterNumber = rr.ExamSchedule?.SemesterInstance?.Semester?.Number ?? 0,
                Level = rr.ExamSchedule?.Level?.LevelName,
                ExamType = rr.ExamType?.Name,
                AcademicYear = rr.AcademicYear?.AcademicYearName,
                College = registration.College?.Name,
                TotalGpa = rr.Gpa,
                Result = rr.Result,
                TheoryGrade = rr.TheoryObtainedGrade,
                PracticalGrade = rr.PracticalObtainedGrade,
                SymbolNumber = rr.SymbolNumber,
                ExamScheduleId = scheduleId.Value,
                Subjects = subjects
            });
        }

        foreach (var er in allExamRegistrations)
        {
            if (!allMarksheets.Any(m => m.ExamScheduleId == er.ExamScheduleId))
            {
                var subjects = GetMarksheetSubjects(allExamRegistrations, er.ExamScheduleId, gradePointMap);
                var computedResult = !subjects.Any() ? "Pending"
                    : subjects.Any(s => s.Status == "Fail") ? "Fail"
                    : subjects.All(s => s.Status == "Pass") ? "Pass"
                    : "Pending";
                var totalCredits = subjects.Sum(s => s.CreditHours ?? 0f);
                var totalGradePoints = subjects.Sum(s => s.GradePoint ?? 0m);
                var gpa = totalCredits > 0 ? (totalGradePoints / (decimal)totalCredits) : (decimal?)null;
                allMarksheets.Add(new MarksheetViewModel
                {
                    RegistrationNumber = registration.RegistrationNumber,
                    StudentName = registration.FirstName.GetFullName(registration.MiddleName, registration.LastName),
                    Program = er.ExamSchedule?.Program?.ProgramName,
                    Faculty = er.ExamSchedule?.Program?.Faculty?.Name,
                    AcademicYear = er.ExamSchedule?.SemesterInstance?.AcademicYear?.AcademicYearName,
                    College = registration.College?.Name,
                    ExamSchedule = er.ExamSchedule?.ExamScheduleName,
                    Semester = er.ExamSchedule?.SemesterInstance?.Semester?.Name,
                    SemesterId = er.ExamSchedule?.SemesterInstance?.Semester?.Id,
                    SemesterYear = er.ExamSchedule?.SemesterInstance?.Semester?.Number ?? 0,
                    SemesterNumber = er.ExamSchedule?.SemesterInstance?.Semester?.Number ?? 0,
                    Level = er.ExamSchedule?.Level?.LevelName,
                    ExamType = er.ExamSchedule?.ExamType?.Name,
                    ExamScheduleId = er.ExamScheduleId,
                    SymbolNumber = MarksheetSymbolNumber(er),
                    Result = computedResult,
                    TotalGpa = gpa?.ToString("0.00"),
                    Subjects = subjects
                });
            }
        }

        var sorted = allMarksheets
            .OrderBy(m => m.SemesterYear)
            .ThenBy(m => m.SemesterNumber)
            .ThenBy(m => m.ExamScheduleId)
            .ToList();

        ViewBag.StudentRegistration = registration;

        return View(sorted);
    }

    [RequirePermission(Permissions.RetotalingView)]
    public async Task<IActionResult> RetotalRequests()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null)
            return View(new List<RetotalRequest>());

        var requests = await context.RetotalRequests
            .AsNoTracking()
            .Where(r => r.StudentRegistrationId == registration.Id && r.IsActive)
            .Include(r => r.ExamSubjectResult)
                .ThenInclude(esr => esr!.SubjectOffering)
                    .ThenInclude(so => so!.SubjectCatalog)
            .Include(r => r.ExamRegistration)
            .OrderByDescending(r => r.RequestedDate)
            .ToListAsync();

        var schedules = await dashboardService.GetExamSchedulesByIdsAsync(
            requests.Where(r => r.ExamRegistration != null).Select(r => r.ExamRegistration!.ExamScheduleId));
        foreach (var request in requests)
        {
            if (request.ExamRegistration != null)
                request.ExamRegistration.ExamSchedule = schedules.FirstOrDefault(s => s.Id == request.ExamRegistration.ExamScheduleId);
        }

        return View(requests);
    }

    [RequirePermission(Permissions.RetotalingView)]
    public async Task<IActionResult> RequestRetotal(int? examSubjectResultId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
        if (registration == null) return RedirectToAction(nameof(Profile));

        if (examSubjectResultId.HasValue)
        {
            var result = await context.ExamSubjectResults
                .AsNoTracking()
                .Include(esr => esr.SubjectOffering)
                    .ThenInclude(so => so!.SubjectCatalog)
                .Include(esr => esr.ExamRegistration)
                .FirstOrDefaultAsync(esr => esr.Id == examSubjectResultId.Value);

            if (result != null)
            {
                ViewBag.SubjectName = result.SubjectOffering?.SubjectCatalog?.SubjectName;
                ViewBag.OriginalGrade = result.GradeLetter;
                ViewBag.OriginalMarks = result.ObtainedMarks;

                if (result.ExamRegistration != null)
                {
                    var schedule = await dashboardService.GetExamScheduleByIdAsync(result.ExamRegistration.ExamScheduleId);
                    result.ExamRegistration.ExamSchedule = schedule;
                }

                ViewBag.ExamScheduleName = result.ExamRegistration?.ExamSchedule?.ExamScheduleName;
            }
        }

        ViewBag.ExamSubjectResultId = examSubjectResultId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(Permissions.RetotalingView)]
    public async Task<IActionResult> SubmitRetotalRequest(int examSubjectResultId, string? reason)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var registration = await dashboardService.GetStudentRegistrationByUserIdAsync(user.Id);
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

    private static List<MarksheetSubjectViewModel> GetMarksheetSubjects(
        List<ExamRegistration> examRegistrations,
        int examScheduleId,
        IReadOnlyDictionary<string, decimal> gradePointByLetter)
    {
        var results = examRegistrations
            .Where(er => er.ExamScheduleId == examScheduleId)
            .SelectMany(er => er.ExamSubjectResults ?? Enumerable.Empty<ExamSubjectResult>())
            .Where(esr => esr.IsActive)
            .GroupBy(esr => esr.SubjectOfferingId)
            .Select(g => g.OrderByDescending(esr => esr.Id).First())
            .ToList();

        if (results.Count == 0) return new();

        return results
            .Select(esr =>
            {
                var gradeLetter = esr.GradeLetter?.Trim().ToUpperInvariant();
                var isFailed = gradeLetter is "F" or "NG";
                var isSubmitted = esr.IsSubmitted;
                var hasGrade = !string.IsNullOrEmpty(gradeLetter);

                var creditHours = esr.SubjectOffering?.SubjectCatalog?.CreditHours;
                var gradeValue = hasGrade && gradePointByLetter.TryGetValue(gradeLetter!, out var gradeValueRaw)
                    ? gradeValueRaw
                    : (decimal?)null;
                var gradePoint = gradeValue.HasValue
                    ? (creditHours.HasValue ? gradeValue.Value * creditHours.Value : gradeValue.Value)
                    : (decimal?)null;

                var theoryLetter = esr.GradeLetterTheory?.Trim().ToUpperInvariant();
                var practicalLetter = esr.GradeLetterPractical?.Trim().ToUpperInvariant();
                var theoryGv = !string.IsNullOrEmpty(theoryLetter) && gradePointByLetter.TryGetValue(theoryLetter, out var tv) ? tv : (decimal?)null;
                var practicalGv = !string.IsNullOrEmpty(practicalLetter) && gradePointByLetter.TryGetValue(practicalLetter, out var pv) ? pv : (decimal?)null;

                return new MarksheetSubjectViewModel
                {
                    ExamSubjectResultId = esr.Id,
                    SubjectName = esr.SubjectOffering?.SubjectCatalog?.SubjectName,
                    SubjectCode = esr.SubjectOffering?.SubjectCatalog?.SubjectCode,
                    CreditHours = creditHours,
                    TheoryMarks = esr.ObtainedMarksTheory,
                    PracticalMarks = esr.ObtainedMarksPractical,
                    InternalMarks = esr.ObtainedMarksTheoryInternal,
                    TotalMarks = esr.ObtainedMarks,
                    Grade = gradeLetter,
                    GradeValue = gradeValue,
                    GradePoint = gradePoint,
                    TheoryGrade = theoryLetter,
                    TheoryGradePoint = theoryGv,
                    PracticalGrade = practicalLetter,
                    PracticalGradePoint = practicalGv,
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

    private async Task<Dictionary<string, decimal>> GetGradePointMapAsync(int? programId)
    {
        var map = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        if (programId.HasValue)
        {
            var scheme = gradeCalculationService.ResolveSchemeForProgram(programId.Value);

            if (scheme?.GradeDefinitions != null)
            {
                foreach (var gd in scheme.GradeDefinitions.OrderBy(gd => gd.DisplayOrder))
                {
                    var letter = gd.GradeLetter?.Trim().ToUpperInvariant();
                    if (!string.IsNullOrEmpty(letter))
                    {
                        map.TryAdd(letter, gd.GradePoint);
                    }
                }
            }
        }

        foreach (var standard in StandardGradePoints)
        {
            map.TryAdd(standard.Key, standard.Value);
        }

        return map;
    }

    private static readonly IReadOnlyDictionary<string, decimal> StandardGradePoints = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["A+"] = 4.0m,
        ["A"] = 3.7m,
        ["B+"] = 3.3m,
        ["B"] = 3.0m,
        ["C+"] = 2.7m,
        ["C"] = 2.3m,
        ["D"] = 2.0m,
        ["F"] = 0.0m,
        ["NG"] = 0.0m
    };

    private static bool IsScheduleDeadlinePassed(ExamSchedule schedule)
    {
        var effectiveDate = schedule.ExtendedDate?.Date
            ?? schedule.EndDate?.ToDateTime(TimeOnly.MinValue).Date;

        return effectiveDate.HasValue && DateTime.Now.Date > effectiveDate.Value;
    }
}
