using fwu_examination_management_system.Data;
using fwu_examination_management_system.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers
{
    [Authorize]
    public class BankVoucherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BankVoucherController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpGet]
        public IActionResult Verify()
        {
            return View(new VoucherVerificationViewModel());
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(VoucherVerificationViewModel model)
        {
            model.HasSearched = true;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var voucherNo = model.VoucherNumber.Trim();
            var paymentRequest = await GetPaymentRequestByVoucherNoAsync(voucherNo);

            var applicationVoucher = await _context.ApplicationVouchers
                .AsNoTracking()
                .Include(x => x.ExamSchedule)
                    .ThenInclude(x => x.ExamScheduleParent)
                .Include(x => x.StudentRegistration)
                    .ThenInclude(x => x.College)
                .Where(x => EF.Functions.ILike(x.VoucherNumber, voucherNo))
                .OrderByDescending(x => x.Timestamp)
                .ThenByDescending(x => x.VoucherDate)
                .FirstOrDefaultAsync();

            if (applicationVoucher == null)
            {
                applicationVoucher = await _context.ApplicationVouchers
                    .AsNoTracking()
                    .Include(x => x.ExamSchedule)
                        .ThenInclude(x => x.ExamScheduleParent)
                    .Include(x => x.StudentRegistration)
                        .ThenInclude(x => x.College)
                    .Where(x => EF.Functions.ILike(x.VoucherNumber, $"%{voucherNo}%"))
                    .OrderByDescending(x => x.Timestamp)
                    .ThenByDescending(x => x.VoucherDate)
                    .FirstOrDefaultAsync();
            }

            if (applicationVoucher != null)
            {
                var rollNo = await GetRollNoAsync(applicationVoucher.StudentRegistrationId, applicationVoucher.ExamScheduleId);

                model.Result = new VoucherVerificationResultViewModel
                {
                    VoucherNumber = applicationVoucher.VoucherNumber,
                    Amount = applicationVoucher.Amount,
                    PaymentGateway = paymentRequest?.PaymentType?.PaymentTypeName
                                     ?? applicationVoucher.Branch
                                     ?? string.Empty,
                    RequestedTime = paymentRequest?.ForwardedTimestamp
                                    ?? applicationVoucher.Timestamp
                                    ?? applicationVoucher.VoucherDate,
                    StudentName = !string.IsNullOrWhiteSpace(paymentRequest?.FullName)
                        ? paymentRequest.FullName
                        : applicationVoucher.StudentName,
                    CollegeName = applicationVoucher.StudentRegistration?.College?.CollegeName
                                 ?? paymentRequest?.College?.CollegeName
                                 ?? string.Empty,
                    RollNo = rollNo ?? string.Empty,
                    ContactNo = paymentRequest?.MobileNumber
                               ?? applicationVoucher.ContactNumber
                               ?? string.Empty,
                    ExamName = BuildExamDisplayName(
                        applicationVoucher.ExamSchedule?.ExamScheduleParent?.ExamScheduleParentName,
                        applicationVoucher.ExamSchedule?.ExamScheduleName)
                };

                return View(model);
            }

            var bankVoucher = await _context.BankVouchers
                .AsNoTracking()
                .Include(x => x.College)
                .Include(x => x.Bank)
                .Include(x => x.ExamScheduleParent)
                .Where(x => EF.Functions.ILike(x.VoucherNumber, voucherNo))
                .OrderByDescending(x => x.VoucherDate)
                .FirstOrDefaultAsync();

            if (bankVoucher == null)
            {
                bankVoucher = await _context.BankVouchers
                    .AsNoTracking()
                    .Include(x => x.College)
                    .Include(x => x.Bank)
                    .Include(x => x.ExamScheduleParent)
                    .Where(x => EF.Functions.ILike(x.VoucherNumber, $"%{voucherNo}%"))
                    .OrderByDescending(x => x.VoucherDate)
                    .FirstOrDefaultAsync();
            }

            if (bankVoucher == null)
            {
                model.Result = null;
                return View(model);
            }

            var studentRegistrationId = paymentRequest?.StudentRegistrationId;
            var examScheduleId = paymentRequest?.ExamScheduleId;
            var rollNoFromRequest = await GetRollNoAsync(studentRegistrationId, examScheduleId);

            model.Result = new VoucherVerificationResultViewModel
            {
                VoucherNumber = bankVoucher.VoucherNumber,
                Amount = bankVoucher.VoucherAmount,
                PaymentGateway = paymentRequest?.PaymentType?.PaymentTypeName
                                 ?? bankVoucher.Bank?.BankName
                                 ?? string.Empty,
                RequestedTime = paymentRequest?.ForwardedTimestamp
                                ?? bankVoucher.VoucherDate,
                StudentName = paymentRequest?.FullName ?? string.Empty,
                CollegeName = paymentRequest?.College?.CollegeName
                             ?? bankVoucher.College?.CollegeName
                             ?? string.Empty,
                RollNo = rollNoFromRequest ?? string.Empty,
                ContactNo = paymentRequest?.MobileNumber ?? string.Empty,
                ExamName = examScheduleId.HasValue
                    ? await GetExamDisplayNameByExamScheduleIdAsync(examScheduleId.Value)
                    : (bankVoucher.ExamScheduleParent?.ExamScheduleParentName ?? string.Empty)
            };

            return View(model);
        }

        private async Task<string?> GetRollNoAsync(int? studentRegistrationId, int? examScheduleId)
        {
            if (!studentRegistrationId.HasValue || !examScheduleId.HasValue)
            {
                return null;
            }

            return await (from reg in _context.ExamRegistrations.AsNoTracking()
                          join sp in _context.StudentProgramYearParts.AsNoTracking() on reg.StudentProgramYearPartId equals sp.StudentProgramYearPartId
                          join sa in _context.StudentAdmissions.AsNoTracking() on sp.StudentAdmissionId equals sa.StudentAdmissionId
                          where sa.StudentRegistrationId == studentRegistrationId.Value
                                && reg.ExamScheduleId == examScheduleId.Value
                                && reg.ExamRollNumber != null
                                && reg.ExamRollNumber != string.Empty
                          orderby reg.ExamRegistrationId descending
                          select reg.ExamRollNumber)
                .FirstOrDefaultAsync();
        }

        private async Task<string> GetExamDisplayNameByExamScheduleIdAsync(int examScheduleId)
        {
            var exam = await _context.ExamSchedules
                .AsNoTracking()
                .Include(x => x.ExamScheduleParent)
                .Where(x => x.ExamScheduleId == examScheduleId)
                .Select(x => new
                {
                    x.ExamScheduleName,
                    ParentName = x.ExamScheduleParent != null ? x.ExamScheduleParent.ExamScheduleParentName : string.Empty
                })
                .FirstOrDefaultAsync();

            if (exam == null)
            {
                return string.Empty;
            }

            return BuildExamDisplayName(exam.ParentName, exam.ExamScheduleName);
        }

        private async Task<Models.PaymentRequestLog?> GetPaymentRequestByVoucherNoAsync(string voucherNo)
        {
            var request = await _context.PaymentRequestLogs
                .AsNoTracking()
                .Include(x => x.PaymentType)
                .Include(x => x.College)
                .Where(x => EF.Functions.ILike(x.InvoiceNumber, voucherNo))
                .OrderByDescending(x => x.ForwardedTimestamp)
                .FirstOrDefaultAsync();

            if (request != null)
            {
                return request;
            }

            return await _context.PaymentRequestLogs
                .AsNoTracking()
                .Include(x => x.PaymentType)
                .Include(x => x.College)
                .Where(x => EF.Functions.ILike(x.InvoiceNumber, $"%{voucherNo}%"))
                .OrderByDescending(x => x.ForwardedTimestamp)
                .FirstOrDefaultAsync();
        }

        private static string BuildExamDisplayName(string? examParentName, string? examScheduleName)
        {
            var parent = examParentName ?? string.Empty;
            var schedule = examScheduleName ?? string.Empty;

            return !string.IsNullOrWhiteSpace(parent) && !string.IsNullOrWhiteSpace(schedule)
                ? $"{parent} - {schedule}"
                : (!string.IsNullOrWhiteSpace(parent) ? parent : schedule);
        }
    }
}
