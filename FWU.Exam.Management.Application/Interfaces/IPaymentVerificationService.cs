using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IPaymentVerificationService
{
    Task<(List<PaymentVerificationListDto> Items, int TotalCount)> GetPagedAsync(
        string? search, DateTime? fromDate, DateTime? toDate,
        string sort, string sortDir, int page, int pageSize);
    Task<PaymentVerificationListDto?> GetByVoucherNumberAsync(string voucherNumber);
    Task<List<PaymentVerificationListDto>> GetAllForExportAsync(
        string? search, DateTime? fromDate, DateTime? toDate, string sort, string sortDir);
}
