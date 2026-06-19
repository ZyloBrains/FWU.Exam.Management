using System.Text.Json;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClosedXML.Excel;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
public class EntranceController(IEntranceExamApplicationService service, IExamScheduleService examScheduleService, IESewaService esewaService) : Controller
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
    public IActionResult ESewaPayment()
    {
        var amount = decimal.TryParse(TempData["EsewaAmount"] as string, out var amt) ? amt : 1000m;
        var transactionUuid = TempData["EsewaTransactionUuid"] as string ?? esewaService.GenerateTransactionUuid();

        var successUrl = Url.Action(nameof(ESewaSuccess), "Entrance", new { area = "Exams" }, Request.Scheme);
        var failureUrl = Url.Action(nameof(ESewaFailure), "Entrance", new { area = "Exams" }, Request.Scheme);

        var formData = esewaService.GeneratePaymentFormData(amount, transactionUuid, successUrl!, failureUrl!);

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

            if (!esewaService.VerifyResponseSignature(response, decodedJson))
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
        PopulateStepSelectLists(selectLists);

        ViewBag.VoucherId = voucherId;
        ViewBag.VoucherNumber = voucher.VoucherNumber;
        ViewBag.EntranceFee = voucher.Amount;

        return View(new EntranceExamApplication
        {
            ApplicationVoucherId = voucherId,
            PaymentVerified = true,
            CollegeId = voucher.ExamSchedule?.CollegeId ?? 0
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyStep([Bind("AcademicYearId,CollegeId,ProgramId,FirstName,MiddleName,LastName,NepaliName,DateOfBirthBS,DateOfBirthAD,GenderId,Email,ContactNumber,Phone,FatherName,FatherContact,MotherName,MotherContact,GuardianEmail,FatherProfession,MotherProfession,CitizenshipNo,CitizenshipDistrictId,CitizenshipIssueDateBs,CitizenshipIssueDateAd,BloodGroup,BirthPlace,Country,PostalCode,PreviousSchoolCollege,PreviousLevelId,PreviousPassedYear,PreviousSymbolNumber,PreviousGPA,PreviousDivision,PreviousLevel2Id,PreviousSchoolCollege2,PreviousBoard2,PreviousSymbolNumber2,PreviousPassedYear2,PreviousGPA2,PreviousDivision2,PreviousLevel3Id,PreviousSchoolCollege3,PreviousBoard3,PreviousSymbolNumber3,PreviousPassedYear3,PreviousGPA3,PreviousDivision3")] EntranceExamApplication application, int voucherId)
    {
        var selectLists = await service.GetStepFormSelectListsAsync();

        var permanentLocalLevelId = Request.Form["LocalLevelId"].ToString();
        var permanentWardNumber = Request.Form["WardNumber"].ToString();
        var permanentToleStreet = Request.Form["ToleStreet"].ToString();
        var permanentHouseNumber = Request.Form["HouseNumber"].ToString();

        if (ModelState.IsValid)
        {
            var id = await service.SubmitStepApplicationAsync(application, permanentLocalLevelId, permanentWardNumber, permanentToleStreet, permanentHouseNumber, voucherId);
            return RedirectToAction(nameof(Confirmation), new { id });
        }

        PopulateStepSelectLists(selectLists);
        ViewBag.VoucherId = voucherId;
        return View(application);
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
    public JsonResult GetProvinces()
    {
        var provinces = service.GetProvinces();
        return Json(provinces.Select(p => new { id = p.Id, name = p.ProvinceName }));
    }

    // --- Admin actions ---

    [RequirePermission("entrance.view")]
    public async Task<IActionResult> AdminList(int page = 1, string search = null, string status = null, int? programId = null, int? academicYearId = null, int pageSize = 10)
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
        ViewBag.ProgramIdList = new SelectList(selectLists.Programs, "Id", "Name", programId);
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
        TempData["SuccessMessage"] = "Application approved successfully.";
        return RedirectToAction(nameof(AdminList));
    }

    [HttpPost]
    [RequirePermission("entrance.reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string remarks)
    {
        await service.ReviewApplicationAsync(id, ApplicationStatus.Rejected, remarks);
        TempData["SuccessMessage"] = "Application rejected.";
        return RedirectToAction(nameof(AdminList));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin,DepartmentAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkUnderReview(int id)
    {
        await service.ReviewApplicationAsync(id, ApplicationStatus.UnderReview, null);
        TempData["SuccessMessage"] = "Application marked as under review.";
        return RedirectToAction(nameof(AdminList));
    }

    [HttpGet]
    [RequirePermission("entrance.export")]
    public async Task<IActionResult> ExportToExcel(string search = null, string status = null, int? programId = null, int? academicYearId = null)
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

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin,DepartmentAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertToAdmission(int id)
    {
        try
        {
            var admissionId = await service.ConvertToAdmissionAsync(id);
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

    [RequirePermission("examschedules.create")]
    public async Task<IActionResult> ManageSchedule()
    {
        var selectLists = examScheduleService.GetSelectListData();
        PopulateScheduleDropdowns(selectLists);
        return View(new ExamSchedule
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
    public async Task<IActionResult> ManageSchedule(ExamSchedule model)
    {
        ModelState.Remove(nameof(model.ExamTypeId));
        ModelState.Remove(nameof(model.SemesterId));
        var sl = examScheduleService.GetSelectListData();

        if (ModelState.IsValid)
        {
            model.ExamTypeId = sl.ExamTypes.FirstOrDefault()?.Id ?? 1;
            model.SemesterId = sl.Semesters.FirstOrDefault()?.Id ?? 1;
            model.ExamScheduleCode ??= $"ENT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            await examScheduleService.CreateExamScheduleAsync(model);
            TempData["SuccessMessage"] = "Entrance exam schedule created successfully!";
            return RedirectToAction(nameof(AdminList));
        }

        PopulateScheduleDropdowns(sl, model);
        return View(model);
    }

    [RequirePermission("examschedules.edit")]
    public async Task<IActionResult> EditSchedule(int? id)
    {
        if (id == null) return NotFound();
        var schedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (schedule == null) return NotFound();

        var selectLists = examScheduleService.GetSelectListData();
        PopulateScheduleDropdowns(selectLists, schedule);
        return View("ManageSchedule", schedule);
    }

    [HttpPost]
    [RequirePermission("examschedules.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSchedule(int id, ExamSchedule model)
    {
        if (id != model.Id) return NotFound();

        ModelState.Remove(nameof(model.ExamTypeId));
        ModelState.Remove(nameof(model.SemesterId));
        var sl = examScheduleService.GetSelectListData();

        if (ModelState.IsValid)
        {
            await examScheduleService.UpdateExamScheduleAsync(model);
            TempData["SuccessMessage"] = "Entrance exam schedule updated successfully!";
            return RedirectToAction(nameof(AdminList));
        }

        PopulateScheduleDropdowns(sl, model);
        return View("ManageSchedule", model);
    }

    private void PopulateScheduleDropdowns(ExamScheduleSelectListsDto selectLists, ExamSchedule? model = null)
    {
        ViewBag.ProgramId = new SelectList(selectLists.Programs, "Id", "Name", model?.ProgramId);
        ViewBag.AcademicYearId = new SelectList(selectLists.AcademicYears, "Id", "Name", model?.AcademicYearId);
    }

    private void PopulateStepSelectLists(EntranceExamApplicationSelectListsDto selectLists)
    {
        var provinces = service.GetProvinces();
        ViewBag.Provinces = new SelectList(provinces, "Id", "ProvinceName");
        ViewBag.Districts = new SelectList(selectLists.Districts, "Id", "Name");
        ViewBag.AcademicYearId = new SelectList(selectLists.AcademicYears, "Id", "Name");
        ViewBag.CollegeId = new SelectList(selectLists.Colleges, "Id", "Name");
        ViewBag.ProgramId = new SelectList(selectLists.Programs, "Id", "Name");
        ViewBag.GenderId = new SelectList(selectLists.Genders, "Id", "Name");
        ViewBag.PreviousLevelId = new SelectList(selectLists.PreviousLevels, "Id", "Name");
        ViewBag.CitizenshipDistrictId = new SelectList(selectLists.Districts, "Id", "Name");
    }
}