using System.Text;
using System.Text.Json;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
public class EntranceController(IEntranceExamApplicationService service, IExamScheduleService examScheduleService, IESewaService esewaService, IFileUploadHelper fileUploadHelper, IAuditLogWriter auditLogWriter, AppDbContext context) : Controller
{

    // --- Public actions (no auth required) ---

    [AllowAnonymous]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(VerifyPayment));
    }

    [AllowAnonymous]
    public IActionResult VerifyPayment()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyPayment(string transactionCode, string fullName, string contactNumber)
    {
        if (string.IsNullOrWhiteSpace(transactionCode))
        {
            TempData["ErrorMessage"] = "Please enter a transaction code or voucher number.";
            return View();
        }

        var voucher = await service.VerifyPaymentAsync(transactionCode, fullName ?? "", contactNumber ?? "");
        if (voucher == null)
        {
            TempData["ErrorMessage"] = "Invalid credentials. Please verify your transaction code, name, and phone number.";
            return View();
        }

        return RedirectToAction(nameof(ApplyStep), new { voucherId = voucher.Id });
    }

    [AllowAnonymous]
    public async Task<IActionResult> AvailableSchedules()
    {
        var schedules = await service.GetAvailableExamSchedulesAsync();
        return View(schedules);
    }

    [AllowAnonymous]
    public async Task<IActionResult> InitiatePayment(int scheduleId)
    {
        var schedules = await service.GetAvailableExamSchedulesAsync();
        var schedule = schedules.FirstOrDefault(s => s.Id == scheduleId);
        if (schedule == null)
        {
            TempData["ErrorMessage"] = "Exam schedule not found or no longer available.";
            return RedirectToAction(nameof(AvailableSchedules));
        }

        var paymentTypes = await service.GetActivePaymentTypesAsync();
        ViewBag.PaymentTypes = new SelectList(paymentTypes, "Id", "PaymentTypeName");
        ViewBag.Schedule = schedule;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InitiatePayment(int scheduleId, string studentName, string contactNumber, int paymentTypeId)
    {
        if (string.IsNullOrWhiteSpace(studentName))
        {
            TempData["ErrorMessage"] = "Please enter your name.";
            return RedirectToAction(nameof(InitiatePayment), new { scheduleId });
        }

        if (string.IsNullOrWhiteSpace(contactNumber))
        {
            TempData["ErrorMessage"] = "Please enter your phone number.";
            return RedirectToAction(nameof(InitiatePayment), new { scheduleId });
        }

        var existing = await service.HasExistingVoucherAsync(scheduleId, studentName, contactNumber);
        if (existing)
        {
            TempData["SuccessMessage"] = "You already have a payment record. Enter your voucher code to continue.";
            return RedirectToAction(nameof(VerifyPayment));
        }

        var schedule = (await service.GetAvailableExamSchedulesAsync()).FirstOrDefault(s => s.Id == scheduleId);
        var amount = schedule?.ExamFee ?? 1000;
        var transactionUuid = esewaService.GenerateTransactionUuid();

        var logId = await service.CreateEsewaPaymentLogAsync(scheduleId, studentName, contactNumber, paymentTypeId, transactionUuid);
        if (logId == 0)
        {
            TempData["ErrorMessage"] = "Unable to process payment. Please try again.";
            return RedirectToAction(nameof(AvailableSchedules));
        }

        TempData["EsewaAmount"] = amount.ToString("F2");
        TempData["EsewaTransactionUuid"] = transactionUuid;

        return RedirectToAction(nameof(ESewaPayment));
    }

    [AllowAnonymous]
    public async Task<IActionResult> ESewaPayment()
    {
        var amount = decimal.TryParse(TempData["EsewaAmount"] as string, out var amt) ? amt : 1000m;
        var transactionUuid = TempData["EsewaTransactionUuid"] as string ?? esewaService.GenerateTransactionUuid();

        var successUrl = Url.Action(nameof(ESewaSuccess), "Entrance", new { area = "Exams" }, Request.Scheme);
        var failureUrl = Url.Action(nameof(ESewaFailure), "Entrance", new { area = "Exams" }, Request.Scheme);

        var formData = await esewaService.GeneratePaymentFormDataAsync(amount, transactionUuid, successUrl!, failureUrl!);

        return View(formData);
    }

    [AllowAnonymous]
    public async Task<IActionResult> ESewaSuccess(string? data)
    {
        if (string.IsNullOrEmpty(data))
        {
            TempData["ErrorMessage"] = "Payment was processed but no response data was returned. If you completed the payment, please contact support with your transaction details.";
            return RedirectToAction(nameof(VerifyPayment));
        }

        try
        {
            var decodedBytes = Convert.FromBase64String(data);
            var decodedJson = System.Text.Encoding.UTF8.GetString(decodedBytes);
            var response = JsonSerializer.Deserialize<ESewaVerifyResponse>(decodedJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString });

            if (response == null)
            {
                TempData["ErrorMessage"] = "Invalid response from eSewa.";
                return RedirectToAction(nameof(VerifyPayment));
            }

            if (!await esewaService.VerifyResponseSignatureAsync(response, decodedJson))
            {
                await LogEsewaCallback(null, response.TransactionCode, false, decodedJson, "Signature verification failed");
                TempData["ErrorMessage"] = "Signature verification failed.";
                return RedirectToAction(nameof(VerifyPayment));
            }

            var verified = await esewaService.VerifyTransactionAsync(response.TransactionUuid!, response.TotalAmount);
            var verifyData = verified != null ? JsonSerializer.Serialize(verified) : "null";
            var combinedData = $"{{\"callback\":{decodedJson},\"verification\":{verifyData}}}";

            if (verified == null || verified.Status != "COMPLETE")
            {
                await LogEsewaCallback(null, response.TransactionCode, false, combinedData, "Transaction verification failed");
                TempData["ErrorMessage"] = "Transaction verification failed.";
                return RedirectToAction(nameof(VerifyPayment));
            }

            var logId = await service.GetPaymentLogIdByTransactionUuidAsync(response.TransactionUuid!);
            if (logId == null)
            {
                await LogEsewaCallback(null, response.TransactionCode, true, combinedData, "Payment log not found");
                TempData["ErrorMessage"] = "Payment log not found. Please contact support.";
                return RedirectToAction(nameof(VerifyPayment));
            }

            var voucher = await service.CompleteEsewaPaymentAsync(logId.Value, response.TotalAmount);
            if (voucher == null)
            {
                await LogEsewaCallback(logId, response.TransactionCode, false, combinedData, "Failed to record payment");
                TempData["ErrorMessage"] = "Failed to record payment. Please contact support.";
                return RedirectToAction(nameof(VerifyPayment));
            }

            await LogEsewaCallback(logId, response.TransactionCode, true, combinedData, "Payment verified via eSewa");
            await auditLogWriter.LogAsync(ActivityTypes.PaymentVerified,
                $"Entrance exam fee payment verified via eSewa (Transaction {response.TransactionCode})",
                new { gateway = "esewa", transactionCode = response.TransactionCode, transactionUuid = response.TransactionUuid, amount = response.TotalAmount, voucherId = voucher.Id, voucherNumber = voucher.VoucherNumber },
                entityName: "EntranceExamApplication", entityId: voucher.Id.ToString());

            TempData["SuccessMessage"] = "Payment successful!";
            TempData["TransactionCode"] = response.TransactionCode;
            TempData["TransactionUuid"] = response.TransactionUuid;
            TempData["VoucherNumber"] = voucher.VoucherNumber;
            TempData["VoucherId"] = voucher.Id;

            return RedirectToAction(nameof(PaymentSuccess));
        }
        catch (FormatException)
        {
            TempData["ErrorMessage"] = "Invalid response format from eSewa. Please try again.";
            return RedirectToAction(nameof(VerifyPayment));
        }
        catch
        {
            TempData["ErrorMessage"] = "Failed to process eSewa callback.";
            return RedirectToAction(nameof(VerifyPayment));
        }
    }

    private async Task LogEsewaCallback(int? logId, string? transactionCode, bool isSuccess, string responseData, string? message)
    {
        if (logId.HasValue)
        {
            await service.LogEsewaResponseAsync(logId.Value, transactionCode, isSuccess, responseData, message);
        }
        else
        {
            var uuid = "";
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(responseData);
                uuid = doc.RootElement.TryGetProperty("callback", out var callback)
                    ? callback.TryGetProperty("transaction_uuid", out var tu) ? tu.GetString() ?? "" : ""
                    : doc.RootElement.TryGetProperty("transaction_uuid", out var tu2) ? tu2.GetString() ?? "" : "";
            }
            catch { }

            if (!string.IsNullOrEmpty(uuid))
            {
                var lid = await service.GetPaymentLogIdByTransactionUuidAsync(uuid);
                if (lid.HasValue)
                    await service.LogEsewaResponseAsync(lid.Value, transactionCode, isSuccess, responseData, message);
            }
        }
    }

    [AllowAnonymous]
    public IActionResult ESewaFailure()
    {
        TempData["ErrorMessage"] = "Payment was cancelled or failed. Please try again.";
        return RedirectToAction(nameof(VerifyPayment));
    }

    [AllowAnonymous]
    public IActionResult PaymentSuccess()
    {
        return View();
    }

    [AllowAnonymous]
    public async Task<IActionResult> ApplyStep(int voucherId)
    {
        var voucher = await service.GetVoucherByIdAsync(voucherId);
        if (voucher == null)
        {
            TempData["ErrorMessage"] = "Invalid payment reference. Please verify payment first.";
            return RedirectToAction(nameof(VerifyPayment));
        }

        var selectLists = await service.GetStepFormSelectListsAsync();
        await PopulateStepSelectListsAsync(selectLists);

        var schedule = voucher.ExamSchedule;
        ViewBag.VoucherId = voucherId;
        ViewBag.VoucherNumber = voucher.VoucherNumber;
        ViewBag.EntranceFee = voucher.Amount;
        ViewBag.SelectedProgram = schedule?.Program?.ProgramName;
        ViewBag.SelectedCollege = schedule?.College?.Name;
        ViewBag.SelectedAcademicYear = schedule?.SemesterInstance?.AcademicYear?.AcademicYearName;

        // Check for existing application linked to this voucher
        var existing = await service.GetApplicationByVoucherIdAsync(voucherId);
        if (existing != null)
        {
            if (existing.Status == ApplicationStatus.Approved || existing.Status == ApplicationStatus.UnderReview)
            {
                TempData["InfoMessage"] = $"This application is already {existing.Status.ToString().ToLower()} and cannot be edited.";
                return RedirectToAction(nameof(Confirmation), new { id = existing.Id });
            }

            ViewBag.IsEditing = true;
            ViewBag.ExistingStatus = existing.Status.ToString();
            return View(existing);
        }

        return View(new EntranceExamApplication
        {
            ApplicationVoucherId = voucherId,
            PaymentVerified = true,
            AcademicYearId = schedule?.SemesterInstance?.AcademicYearId ?? 0,
            CollegeId = schedule?.CollegeId ?? 0,
            ProgramId = schedule?.ProgramId ?? 0
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyStep([Bind("AcademicYearId,CollegeId,ProgramId,FirstName,MiddleName,LastName,NepaliName,DateOfBirthBS,DateOfBirthAD,GenderId,Email,ContactNumber,Phone,FatherName,FatherContact,MotherName,MotherContact,GuardianEmail,FatherProfession,MotherProfession,CitizenshipNo,CitizenshipDistrictId,CitizenshipIssueDateBs,CitizenshipIssueDateAd,BloodGroup,BirthPlace,Country,PostalCode,PreviousSchoolCollege,PreviousLevelId,PreviousPassedYear,PreviousSymbolNumber,PreviousGPA,PreviousDivision,PreviousLevel2Id,PreviousSchoolCollege2,PreviousBoard2,PreviousSymbolNumber2,PreviousPassedYear2,PreviousGPA2,PreviousDivision2,PreviousLevel3Id,PreviousSchoolCollege3,PreviousBoard3,PreviousSymbolNumber3,PreviousPassedYear3,PreviousGPA3,PreviousDivision3")] EntranceExamApplication application, int voucherId,
        IFormFile? PhotoFile, IFormFile? DocumentsFile, IFormFile? VoucherFile)
    {
        var selectLists = await service.GetStepFormSelectListsAsync();

        var permanentLocalLevelId = Request.Form["LocalLevelId"].ToString();
        var permanentWardNumber = Request.Form["WardNumber"].ToString();
        var permanentToleStreet = Request.Form["ToleStreet"].ToString();
        var permanentHouseNumber = Request.Form["HouseNumber"].ToString();

        // Validate file uploads
        var fileErrors = ValidateUploadedFiles(PhotoFile, DocumentsFile, VoucherFile);
        foreach (var error in fileErrors)
            ModelState.AddModelError("", error);

        if (ModelState.IsValid)
        {
            // Handle file uploads
            try
            {
                application.PhotoPath = await fileUploadHelper.UploadAsync(PhotoFile, "entrance/photos", Helpers.FileUploadHelper.MaxPhotoSizeBytes, Helpers.FileUploadHelper.ImageOnlyExtensions);
                application.DocumentsPath = await fileUploadHelper.UploadAsync(DocumentsFile, "entrance/documents", Helpers.FileUploadHelper.MaxDocumentSizeBytes, Helpers.FileUploadHelper.DocumentAllowedExtensions);
                application.VoucherPath = await fileUploadHelper.UploadAsync(VoucherFile, "entrance/vouchers", Helpers.FileUploadHelper.MaxDocumentSizeBytes, Helpers.FileUploadHelper.DocumentAllowedExtensions);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateStepSelectListsAsync(selectLists);
                ViewBag.VoucherId = voucherId;
                return View(application);
            }

            // Check if editing an existing application
            var existing = await service.GetApplicationByVoucherIdAsync(voucherId);
            if (existing != null && (existing.Status == ApplicationStatus.Submitted || existing.Status == ApplicationStatus.Rejected))
            {
                await service.UpdateStepApplicationAsync(application, permanentLocalLevelId, permanentWardNumber, permanentToleStreet, permanentHouseNumber, voucherId, existing.Id);
                await auditLogWriter.LogAsync(ActivityTypes.EntranceUpdated,
                    $"Entrance application {existing.Id} updated",
                    new { applicationId = existing.Id, voucherId, name = $"{application.FirstName} {application.LastName}".Trim() }, entityName: "EntranceExamApplication", entityId: existing.Id.ToString());
                TempData["SuccessMessage"] = "Application updated successfully.";
                return RedirectToAction(nameof(Confirmation), new { id = existing.Id });
            }

            var id = await service.SubmitStepApplicationAsync(application, permanentLocalLevelId, permanentWardNumber, permanentToleStreet, permanentHouseNumber, voucherId);
            await auditLogWriter.LogAsync(ActivityTypes.EntranceSubmitted,
                $"Entrance application {id} submitted",
                new { applicationId = id, voucherId, name = $"{application.FirstName} {application.LastName}".Trim() }, entityName: "EntranceExamApplication", entityId: id.ToString());
            return RedirectToAction(nameof(Confirmation), new { id });
        }

        await PopulateStepSelectListsAsync(selectLists);
        ViewBag.VoucherId = voucherId;

        var schedule = (await service.GetVoucherByIdAsync(voucherId))?.ExamSchedule;
        ViewBag.SelectedProgram = schedule?.Program?.ProgramName;
        ViewBag.SelectedCollege = schedule?.College?.Name;
        ViewBag.SelectedAcademicYear = schedule?.SemesterInstance?.AcademicYear?.AcademicYearName;

        return View(application);
    }

    private List<string> ValidateUploadedFiles(IFormFile? photo, IFormFile? documents, IFormFile? voucher)
    {
        var errors = new List<string>();
        var allowedImageExts = new[] { ".jpg", ".jpeg", ".png" };
        var allowedDocExts = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };

        if (photo != null && photo.Length > 0)
        {
            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!allowedImageExts.Contains(ext))
                errors.Add("Photo must be a JPG or PNG file.");
            if (photo.Length > Helpers.FileUploadHelper.MaxPhotoSizeBytes)
                errors.Add("Photo must be less than 500 KB.");
        }

        if (documents != null && documents.Length > 0)
        {
            var ext = Path.GetExtension(documents.FileName).ToLowerInvariant();
            if (!allowedDocExts.Contains(ext))
                errors.Add("Documents must be PDF, JPG, PNG, or DOC/DOCX file.");
            if (documents.Length > Helpers.FileUploadHelper.MaxDocumentSizeBytes)
                errors.Add("Documents must be less than 2MB.");
        }

        if (voucher != null && voucher.Length > 0)
        {
            var ext = Path.GetExtension(voucher.FileName).ToLowerInvariant();
            if (!allowedDocExts.Contains(ext))
                errors.Add("Voucher must be PDF, JPG, or DOC/DOCX file.");
            if (voucher.Length > Helpers.FileUploadHelper.MaxDocumentSizeBytes)
                errors.Add("Voucher must be less than 2MB.");
        }

        return errors;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Confirmation(int id)
    {
        var application = await service.GetApplicationByIdAsync(id);
        if (application == null) return NotFound();
        return View(application);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<JsonResult> GetDistrictsByProvince(int provinceId)
    {
        var districts = await service.GetDistrictsByProvinceAsync(provinceId);
        return Json(districts);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<JsonResult> GetLocalLevelsByDistrict(int districtId)
    {
        var localLevels = await service.GetLocalLevelsByDistrictAsync(districtId);
        return Json(localLevels);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<JsonResult> CheckFormStatus(int programId, int collegeId, int academicYearId)
    {
        var isOpen = await service.IsExamScheduleOpenAsync(programId, collegeId, academicYearId);
        var fee = await service.GetEntranceFeeForProgramAsync(programId, academicYearId);
        return Json(new { isOpen, fee });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<JsonResult> GetAllDistricts()
    {
        var districts = await service.GetDistrictsAsync();
        return Json(districts);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<JsonResult> GetProvinces()
    {
        var provinces = await service.GetProvincesAsync();
        return Json(provinces.Select(p => new { id = p.Id, name = p.ProvinceName }));
    }

    // --- Admin actions ---

    [RequirePermission("entrance.view")]
    public async Task<IActionResult> AdminList(int page = 1, string? search = null, string? status = null, int? programId = null, int? academicYearId = null, int pageSize = 10)
    {
        ApplicationStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, out var parsedStatus))
            statusFilter = parsedStatus;

        var (items, totalCount) = await service.GetPagedApplicationsAsync(search, statusFilter, programId, academicYearId, page, pageSize);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.ProgramId = programId;
        ViewBag.AcademicYearId = academicYearId;

        var selectLists = await service.GetSelectListsAsync();
        ViewBag.ProgramIdList = new SelectList(selectLists.Programs, "Id", "ProgramName", programId);
        ViewBag.AcademicYearIdList = new SelectList(selectLists.AcademicYears, "Id", "Name", academicYearId);

        return View(items);
    }

    [RequirePermission("entrance.view")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var application = await service.GetApplicationByIdAsync(id.Value);
        if (application == null) return NotFound();

        return View(application);
    }

    [HttpPost]
    [RequirePermission("entrance.approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        await service.ReviewApplicationAsync(id, ApplicationStatus.Approved, null);
        await auditLogWriter.LogAsync(ActivityTypes.EntranceApproved, $"Entrance application {id} approved", new { applicationId = id }, entityName: "EntranceExamApplication", entityId: id.ToString());
        TempData["SuccessMessage"] = "Application approved successfully.";
        return RedirectToAction(nameof(AdminList));
    }

    [HttpPost]
    [RequirePermission("entrance.reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string remarks)
    {
        await service.ReviewApplicationAsync(id, ApplicationStatus.Rejected, remarks);
        await auditLogWriter.LogAsync(ActivityTypes.EntranceRejected, $"Entrance application {id} rejected", new { applicationId = id, remarks }, entityName: "EntranceExamApplication", entityId: id.ToString());
        TempData["SuccessMessage"] = "Application rejected.";
        return RedirectToAction(nameof(AdminList));
    }

    [HttpPost]
    [Authorize(Roles = Role.BackOfficeRoles)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkUnderReview(int id)
    {
        await service.ReviewApplicationAsync(id, ApplicationStatus.UnderReview, null);
        await auditLogWriter.LogAsync(ActivityTypes.EntranceUnderReview, $"Entrance application {id} marked as under review", new { applicationId = id }, entityName: "EntranceExamApplication", entityId: id.ToString());
        TempData["SuccessMessage"] = "Application marked as under review.";
        return RedirectToAction(nameof(AdminList));
    }

    [HttpGet]
    [RequirePermission("entrance.export")]
    public async Task<IActionResult> ExportToExcel(string? search = null, string? status = null, int? programId = null, int? academicYearId = null)
    {
        ApplicationStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, out var parsedStatus))
            statusFilter = parsedStatus;

        var data = await service.GetAllApplicationsAsync(search, statusFilter, programId, academicYearId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Entrance Applications");

        worksheet.Cell(1, 1).Value = "Application ID";
        worksheet.Cell(1, 2).Value = "Full Name";
        worksheet.Cell(1, 3).Value = "Email";
        worksheet.Cell(1, 4).Value = "Contact Number";
        worksheet.Cell(1, 5).Value = "Gender";
        worksheet.Cell(1, 6).Value = "Academic Year";
        worksheet.Cell(1, 7).Value = "College";
        worksheet.Cell(1, 8).Value = "Program";
        worksheet.Cell(1, 9).Value = "Status";
        worksheet.Cell(1, 10).Value = "Submitted Date";

        var headerRange = worksheet.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach (var item in data)
        {
            worksheet.Cell(row, 1).Value = item.Id;
            worksheet.Cell(row, 2).Value = (item.FirstName + " " + item.LastName).Trim();
            worksheet.Cell(row, 3).Value = item.Email;
            worksheet.Cell(row, 4).Value = item.ContactNumber;
            worksheet.Cell(row, 5).Value = item.Gender?.GenderName;
            worksheet.Cell(row, 6).Value = item.AcademicYear?.AcademicYearName;
            worksheet.Cell(row, 7).Value = item.College?.Name;
            worksheet.Cell(row, 8).Value = item.Program?.ProgramName;
            worksheet.Cell(row, 9).Value = item.Status.ToString();
            worksheet.Cell(row, 10).Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            row++;
        }

        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileBytes = stream.ToArray();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"EntranceApplications_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpGet]
    [RequirePermission("entrance.export")]
    public async Task<IActionResult> ExportToCsv(string? search = null, string? status = null, int? programId = null, int? academicYearId = null)
    {
        ApplicationStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, out var parsedStatus))
            statusFilter = parsedStatus;

        var data = await service.GetAllApplicationsAsync(search, statusFilter, programId, academicYearId);

        var sb = new StringBuilder();
        sb.AppendLine("Application ID,Full Name,Email,Contact Number,Gender,Academic Year,College,Program,Status,Submitted Date");
        foreach (var item in data)
        {
            sb.AppendLine($"{item.Id},{(item.FirstName + " " + item.LastName).Trim().EscapeCsv()},{(item.Email ?? "").EscapeCsv()},{(item.ContactNumber ?? "").EscapeCsv()},{(item.Gender?.GenderName ?? "").EscapeCsv()},{(item.AcademicYear?.AcademicYearName ?? "").EscapeCsv()},{(item.College?.Name ?? "").EscapeCsv()},{(item.Program?.ProgramName ?? "").EscapeCsv()},{(item.Status.ToString() ?? "").EscapeCsv()},{item.CreatedAt:yyyy-MM-dd HH:mm}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", $"EntranceApplications_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet]
    [RequirePermission("entrance.export")]
    public async Task<IActionResult> ExportToPdf(string? search = null, string? status = null, int? programId = null, int? academicYearId = null)
    {
        ApplicationStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, out var parsedStatus))
            statusFilter = parsedStatus;

        var data = await service.GetAllApplicationsAsync(search, statusFilter, programId, academicYearId);
        return View("PrintPdf", data);
    }

    [HttpPost]
    [Authorize(Roles = Role.BackOfficeRoles)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertToAdmission(int id)
    {
        try
        {
            var admissionId = await service.ConvertToAdmissionAsync(id);
            await auditLogWriter.LogAsync(ActivityTypes.EntranceConvertedToAdmission, $"Entrance application {id} converted to admission {admissionId}", new { applicationId = id, admissionId }, entityName: "StudentAdmission", entityId: admissionId.ToString());
            TempData["SuccessMessage"] = "Application converted to student admission successfully!";
            return RedirectToAction("Details", "StudentAdmissions", new { area = "Students", id = admissionId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // --- Entrance Schedule Management (Admin) ---

    [RequirePermission("examschedules.view")]
    public async Task<IActionResult> ManageSchedule(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await examScheduleService.GetExamSchedulesAsync(page, pageSize, search, sort, sortDir, "Entrance");

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    [RequirePermission("examschedules.create")]
    public async Task<IActionResult> CreateSchedule()
    {
        var selectLists = await examScheduleService.GetSelectListDataAsync();
        PopulateScheduleDropdowns(selectLists);
        return View("ScheduleForm", new ExamSchedule
        {
            IsActive = true,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            PublishedDate = DateTime.UtcNow
        });
    }

    [HttpPost]
    [RequirePermission("examschedules.create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSchedule(ExamSchedule model)
    {
        ModelState.Remove(nameof(model.ExamTypeId));
        ModelState.Remove(nameof(model.SemesterInstanceId));
        var sl = await examScheduleService.GetSelectListDataAsync();

        if (ModelState.IsValid)
        {
            model.ExamTypeId = sl.ExamTypes.FirstOrDefault(et => et.Name == "Entrance")?.Id ?? 1;
            model.SemesterInstanceId = await context.SemesterInstances.AsNoTracking()
                .Where(si => si.ProgramId == model.ProgramId && si.Semester != null)
                .Include(si => si.Semester)
                .OrderBy(si => si.Semester!.Number)
                .Select(si => si.Id)
                .FirstOrDefaultAsync();
            if (model.SemesterInstanceId == 0) model.SemesterInstanceId = 1;
            model.ExamScheduleCode ??= $"ENT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            try
            {
                await examScheduleService.CreateExamScheduleAsync(model);
                await auditLogWriter.LogAsync(ActivityTypes.ExamScheduleCreated, $"Entrance exam schedule created (Code {model.ExamScheduleCode})", new { scheduleId = model.Id, code = model.ExamScheduleCode, programId = model.ProgramId, semesterInstanceId = model.SemesterInstanceId, type = "Entrance" }, entityName: "ExamSchedule", entityId: model.Id.ToString());
                TempData["SuccessMessage"] = "Entrance exam schedule created successfully!";
                return RedirectToAction(nameof(ManageSchedule));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
        }

        PopulateScheduleDropdowns(sl, model);
        return View("ScheduleForm", model);
    }

    [RequirePermission("examschedules.edit")]
    public async Task<IActionResult> EditSchedule(int? id)
    {
        if (id == null) return NotFound();
        var schedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (schedule == null) return NotFound();

        var selectLists = await examScheduleService.GetSelectListDataAsync();
        PopulateScheduleDropdowns(selectLists, schedule);
        return View("ScheduleForm", schedule);
    }

    [HttpPost]
    [RequirePermission("examschedules.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSchedule(int id, ExamSchedule model)
    {
        if (id != model.Id) return NotFound();

        ModelState.Remove(nameof(model.ExamTypeId));
        ModelState.Remove(nameof(model.SemesterInstanceId));
        var sl = await examScheduleService.GetSelectListDataAsync();

        if (ModelState.IsValid)
        {
            model.ExamTypeId = sl.ExamTypes.FirstOrDefault(et => et.Name == "Entrance")?.Id ?? 1;
            model.SemesterInstanceId = await context.SemesterInstances.AsNoTracking()
                .Where(si => si.ProgramId == model.ProgramId && si.Semester != null)
                .Include(si => si.Semester)
                .OrderBy(si => si.Semester!.Number)
                .Select(si => si.Id)
                .FirstOrDefaultAsync();
            if (model.SemesterInstanceId == 0) model.SemesterInstanceId = 1;
            try
            {
                await examScheduleService.UpdateExamScheduleAsync(model);
                await examScheduleService.DeactivateExpiredSchedulesAsync();
                await auditLogWriter.LogAsync(ActivityTypes.ExamScheduleUpdated, $"Entrance exam schedule {model.Id} updated", new { scheduleId = model.Id, code = model.ExamScheduleCode, programId = model.ProgramId, semesterInstanceId = model.SemesterInstanceId, type = "Entrance" }, entityName: "ExamSchedule", entityId: model.Id.ToString());
                TempData["SuccessMessage"] = "Entrance exam schedule updated successfully!";
                return RedirectToAction(nameof(ManageSchedule));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
        }

        PopulateScheduleDropdowns(sl, model);
 
        return View("ScheduleForm", model);
    }

    [RequirePermission("examschedules.view")]
    public async Task<IActionResult> ScheduleDetails(int? id)
    {
        if (id == null) return NotFound();
        var schedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (schedule == null) return NotFound();
        return View(schedule);
    }

    [RequirePermission("examschedules.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteScheduleAjax(int id)
    {
        try
        {
            await examScheduleService.DeleteExamScheduleAsync(id);
            await auditLogWriter.LogAsync(ActivityTypes.ExamScheduleDeleted, $"Entrance exam schedule {id} deleted", new { scheduleId = id, type = "Entrance" }, entityName: "ExamSchedule", entityId: id.ToString());
            return Json(new { success = true, message = "Entrance schedule deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private void PopulateScheduleDropdowns(ExamScheduleSelectListsDto selectLists, ExamSchedule? model = null)
    {
        ViewBag.ProgramId = new SelectList(selectLists.Programs, "Id", "Name", model?.ProgramId);
        ViewBag.SemesterInstanceId = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text", model?.SemesterInstanceId);
    }

    private async Task PopulateStepSelectListsAsync(EntranceExamApplicationSelectListsDto selectLists)
    {
        var provinces = await service.GetProvincesAsync();
        ViewBag.Provinces = new SelectList(provinces, "Id", "ProvinceName");
        ViewBag.Districts = new SelectList(selectLists.Districts, "Id", "Name");
        ViewBag.AcademicYearId = new SelectList(selectLists.AcademicYears, "Id", "Name");
        ViewBag.CollegeId = new SelectList(selectLists.Colleges, "Id", "Name");
        ViewBag.ProgramId = new SelectList(selectLists.Programs, "Id", "ProgramName");
        ViewBag.GenderId = new SelectList(selectLists.Genders, "Id", "Name");
        ViewBag.PreviousLevelId = new SelectList(selectLists.PreviousLevels, "Id", "Name");
        ViewBag.CitizenshipDistrictId = new SelectList(selectLists.Districts, "Id", "Name");
    }
}