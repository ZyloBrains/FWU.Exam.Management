using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class PaymentVerificationService(AppDbContext context, IUserContext userContext) : IPaymentVerificationService
{
    public async Task<(List<PaymentVerificationListDto> Items, int TotalCount)> GetPagedAsync(
        string? search, DateTime? fromDate, DateTime? toDate,
        string sort, string sortDir, int page, int pageSize)
    {
        var query = BuildQuery(search, fromDate, toDate, sort, sortDir);

        var totalCount = await query.CountAsync();
        var skip = (page - 1) * pageSize;

        var vouchers = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        var paymentInfo = await ResolveGatewayPaymentsAsync(vouchers);

        var items = vouchers.Select(v => MapToDto(v, paymentInfo.GetValueOrDefault(v.Id))).ToList();
        return (items, totalCount);
    }

    public async Task<PaymentVerificationListDto?> GetByVoucherNumberAsync(string voucherNumber)
    {
        if (string.IsNullOrWhiteSpace(voucherNumber)) return null;

        var code = voucherNumber.Trim();

        // 1. Exact voucher number match
        var voucher = await BuildBaseQuery().FirstOrDefaultAsync(v => v.VoucherNumber == code);
        if (voucher != null)
            return await MapSingleAsync(voucher);

        // 2. Last 6 characters of the voucher number
        if (code.Length >= 6)
        {
            var last6 = code[^6..];
            var candidates = await BuildBaseQuery()
                .Where(v => v.VoucherNumber != null && v.VoucherNumber.Length >= 6)
                .ToListAsync();
            var suffixMatch = candidates.FirstOrDefault(v => v.VoucherNumber![^6..] == last6);
            if (suffixMatch != null)
                return await MapSingleAsync(suffixMatch);
        }

        // 3. Successful gateway transaction id / invoice / pidx -> payment request log
        var log = await FindPaymentLogByCodeAsync(code);
        if (log != null)
            return await MapFromLogAsync(log);

        return null;
    }

    private async Task<PaymentRequestLog?> FindPaymentLogByCodeAsync(string code)
    {
        var logs = context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .Include(l => l.PaymentType)
            .Include(l => l.StudentRegistration)
            .Include(l => l.College)
            .Include(l => l.ExamSchedule).ThenInclude(s => s!.Program)
            .Include(l => l.ExamSchedule).ThenInclude(s => s!.College)
            .Include(l => l.ExamSchedule).ThenInclude(s => s!.SemesterInstance).ThenInclude(si => si!.AcademicYear);

        // Khalti/E-Sewa transaction id written on callback verification
        var byTransaction = await logs
            .OrderByDescending(l => l.ForwardedTimestamp)
            .FirstOrDefaultAsync(l => l.TransactionId == code);
        if (byTransaction != null) return byTransaction;

        // purchase_order_id / invoice number used at initiation
        var byInvoice = await logs
            .OrderByDescending(l => l.ForwardedTimestamp)
            .FirstOrDefaultAsync(l => l.InvoiceNumber == code);
        if (byInvoice != null) return byInvoice;

        // pidx or any other reference kept inside the stored gateway response payload
        var responseLogId = await context.Set<PaymentResponseLog>()
            .AsNoTracking()
            .Where(r => r.FullResponse.Contains(code))
            .OrderByDescending(r => r.ResponseTimestamp)
            .Select(r => (int?)r.PaymentRequestLogId)
            .FirstOrDefaultAsync();
        if (responseLogId == null) return null;

        return await logs.FirstOrDefaultAsync(l => l.Id == responseLogId.Value);
    }

    private async Task<PaymentVerificationListDto> MapFromLogAsync(PaymentRequestLog log)
    {
        var info = new GatewayPaymentInfo(log.TransactionId, log.PaymentType?.PaymentTypeName, log.ForwardedTimestamp);

        // Prefer the canonical voucher when one exists for this payment
        var voucher = await BuildBaseQuery().FirstOrDefaultAsync(v =>
            v.ExamScheduleId == log.ExamScheduleId
            && v.StudentName == log.FullName
            && v.ContactNumber == log.MobileNumber);
        if (voucher != null)
            return MapToDto(voucher, info);

        return new PaymentVerificationListDto
        {
            Id = log.Id,
            VoucherNumber = log.InvoiceNumber,
            StudentName = string.IsNullOrWhiteSpace(log.FullName) ? "-" : log.FullName,
            Amount = log.Amount,
            VoucherDate = log.ForwardedTimestamp,
            ContactNumber = log.MobileNumber ?? "-",
            Branch = null,
            TransactionCode = log.TransactionId,
            PaymentGateway = log.PaymentType?.PaymentTypeName,
            RequestedTime = log.ForwardedTimestamp,
            RollNumber = log.StudentRegistration != null
                ? (log.StudentRegistration.EntranceRollNumber ?? log.StudentRegistration.RegistrationNumber)
                : null,
            ExamName = log.ExamSchedule?.ExamScheduleName,
            AcademicYear = log.ExamSchedule?.SemesterInstance?.AcademicYear?.AcademicYearName,
            Program = log.ExamSchedule?.Program?.ProgramName,
            College = log.StudentRegistration?.College?.Name ?? log.ExamSchedule?.College?.Name ?? log.College?.Name
        };
    }

    public async Task<List<PaymentVerificationListDto>> GetAllForExportAsync(
        string? search, DateTime? fromDate, DateTime? toDate, string sort, string sortDir)
    {
        var query = BuildQuery(search, fromDate, toDate, sort, sortDir);
        var vouchers = await query.ToListAsync();
        var paymentInfo = await ResolveGatewayPaymentsAsync(vouchers);
        return vouchers.Select(v => MapToDto(v, paymentInfo.GetValueOrDefault(v.Id))).ToList();
    }

    private IQueryable<ApplicationVoucher> BuildBaseQuery() =>
        context.ApplicationVouchers
            .Include(v => v.StudentRegistration).ThenInclude(sr => sr!.College)
            .Include(v => v.ExamSchedule).ThenInclude(s => s!.Program)
            .Include(v => v.ExamSchedule).ThenInclude(s => s!.College)
            .Include(v => v.ExamSchedule).ThenInclude(s => s!.SemesterInstance).ThenInclude(si => si!.AcademicYear)
            .AsNoTracking();

    private IQueryable<ApplicationVoucher> BuildQuery(
        string? search, DateTime? fromDate, DateTime? toDate,
        string sort, string sortDir)
    {
        var query = BuildBaseQuery();

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
                query = query.Where(v => v.ExamSchedule != null && v.ExamSchedule.CollegeId == userContext.CollegeId.Value);
            else if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
                query = query.Where(v => v.ExamSchedule != null && v.ExamSchedule.Program != null && v.ExamSchedule.Program.FacultyId == userContext.FacultyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.Trim().ToLower();
            var trimmedSearch = search.Trim();
            var txnVoucherIds = GetTransactionMatchedVoucherIds(trimmedSearch);

            query = query.Where(v =>
                ((v.VoucherNumber != null && v.VoucherNumber.ToLower().Contains(lowerSearch)) ||
                 (v.StudentName != null && v.StudentName.ToLower().Contains(lowerSearch)) ||
                 (v.ContactNumber != null && v.ContactNumber.Contains(trimmedSearch))) ||
                txnVoucherIds.Contains(v.Id));
        }

        if (fromDate.HasValue)
            query = query.Where(v => v.VoucherDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(v => v.VoucherDate < toDate.Value.Date.AddDays(1));

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return query;
    }

    private List<int> GetTransactionMatchedVoucherIds(string search)
    {
        var matches = context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .Where(l => l.TransactionId != null && l.TransactionId.Contains(search))
            .Select(l => new { l.ExamScheduleId, l.FullName, l.MobileNumber })
            .Distinct()
            .Take(50)
            .ToList();

        if (matches.Count == 0) return [];

        var scheduleIds = matches.Select(m => m.ExamScheduleId).ToList();
        var names = matches.Select(m => m.FullName).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();
        var phones = matches.Select(m => m.MobileNumber).Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();

        return context.ApplicationVouchers
            .AsNoTracking()
            .Where(v => scheduleIds.Contains(v.ExamScheduleId)
                     && (names.Count == 0 || (v.StudentName != null && names.Contains(v.StudentName)))
                     && (phones.Count == 0 || (v.ContactNumber != null && phones.Contains(v.ContactNumber))))
            .Select(v => v.Id)
            .ToList();
    }

    private sealed record GatewayPaymentInfo(string? TransactionCode, string? GatewayName, DateTime? RequestedTime);

    private async Task<Dictionary<int, GatewayPaymentInfo>> ResolveGatewayPaymentsAsync(List<ApplicationVoucher> vouchers)
    {
        var map = new Dictionary<int, GatewayPaymentInfo>();
        if (vouchers.Count == 0) return map;

        var scheduleIds = vouchers.Select(v => v.ExamScheduleId).Distinct().ToList();
        var studentNames = vouchers.Select(v => v.StudentName).Where(n => n != null).Select(n => n!).Distinct().ToList();
        var contactNumbers = vouchers.Select(v => v.ContactNumber).Where(c => c != null).Select(c => c!).Distinct().ToList();

        var logs = await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .Include(l => l.PaymentType)
            .Where(l => scheduleIds.Contains(l.ExamScheduleId)
                     && (studentNames.Count == 0 || (l.FullName != null && studentNames.Contains(l.FullName)))
                     && (contactNumbers.Count == 0 || (l.MobileNumber != null && contactNumbers.Contains(l.MobileNumber))))
            .OrderByDescending(l => l.ForwardedTimestamp)
            .Select(l => new { l.Id, l.ExamScheduleId, l.FullName, l.MobileNumber, l.TransactionId, l.ForwardedTimestamp, GatewayName = l.PaymentType != null ? l.PaymentType.PaymentTypeName : null })
            .ToListAsync();

        foreach (var v in vouchers)
        {
            var log = logs.FirstOrDefault(l =>
                l.ExamScheduleId == v.ExamScheduleId &&
                l.FullName == v.StudentName &&
                l.MobileNumber == v.ContactNumber);
            map[v.Id] = new GatewayPaymentInfo(log?.TransactionId, log?.GatewayName, log?.ForwardedTimestamp);
        }

        return map;
    }

    private async Task<PaymentVerificationListDto> MapSingleAsync(ApplicationVoucher voucher)
    {
        var info = await ResolveGatewayPaymentsAsync([voucher]);
        return MapToDto(voucher, info.GetValueOrDefault(voucher.Id));
    }

    private static PaymentVerificationListDto MapToDto(ApplicationVoucher v, GatewayPaymentInfo? gatewayPayment)
    {
        gatewayPayment ??= new GatewayPaymentInfo(null, null, null);

        return new PaymentVerificationListDto
        {
            Id = v.Id,
            VoucherNumber = v.VoucherNumber ?? "-",
            StudentName = v.StudentName ?? "-",
            Amount = v.Amount,
            VoucherDate = v.VoucherDate,
            ContactNumber = v.ContactNumber ?? "-",
            Branch = v.Branch,
            TransactionCode = gatewayPayment.TransactionCode,
            PaymentGateway = gatewayPayment.GatewayName,
            RequestedTime = gatewayPayment.RequestedTime ?? v.Timestamp ?? v.VoucherDate,
            RollNumber = v.StudentRegistration != null
                ? (v.StudentRegistration.EntranceRollNumber ?? v.StudentRegistration.RegistrationNumber)
                : null,
            ExamName = v.ExamSchedule?.ExamScheduleName,
            AcademicYear = v.ExamSchedule?.SemesterInstance?.AcademicYear?.AcademicYearName,
            Program = v.ExamSchedule?.Program?.ProgramName,
            College = v.ExamSchedule?.College?.Name ?? v.StudentRegistration?.College?.Name
        };
    }

    private static Expression<Func<ApplicationVoucher, object>> GetSortProperty(string sort) =>
        sort.ToLower() switch
        {
            "vouchernumber" => v => v.VoucherNumber ?? "",
            "studentname" => v => v.StudentName ?? "",
            "amount" => v => v.Amount,
            _ => v => v.VoucherDate ?? DateTime.MinValue
        };
}
