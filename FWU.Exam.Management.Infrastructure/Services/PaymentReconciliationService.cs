using System.Text.Json;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Helpers;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class PaymentReconciliationService(
    AppDbContext context,
    IESewaService esewaService,
    IKhaltiService khaltiService,
    IStudentDashboardService dashboardService,
    INotificationService notificationService,
    UserManager<AppUser> userManager,
    IAuditLogWriter auditLogWriter,
    ILogger<PaymentReconciliationService> logger) : IPaymentReconciliationService
{
    // Prevents two concurrent "Check All Pending" runs (e.g. two admins clicking
    // at once, or a manual run overlapping the background poller) from double
    // processing the same payments. Static because the service is scoped.
    private static readonly SemaphoreSlim _batchLock = new(1, 1);

    public async Task<(List<PaymentReconciliationListDto> Items, int TotalCount)> GetPendingPaymentsAsync(
        string? search, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .Include(l => l.PaymentType)
            .Include(l => l.ExamSchedule)
            .Where(l => (l.PaymentRequestLogStatus == null || l.PaymentRequestLogStatus == 3)
                     && l.StudentRegistrationId != null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.Trim().ToLower();
            query = query.Where(l =>
                l.InvoiceNumber.ToLower().Contains(lower) ||
                (l.FullName != null && l.FullName.ToLower().Contains(lower)) ||
                (l.MobileNumber != null && l.MobileNumber.Contains(search.Trim())));
        }

        if (fromDate.HasValue)
            query = query.Where(l => l.ForwardedTimestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.ForwardedTimestamp < toDate.Value.Date.AddDays(1));

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(l => l.ForwardedTimestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = logs.Select(l => new PaymentReconciliationListDto
        {
            Id = l.Id,
            InvoiceNumber = l.InvoiceNumber,
            StudentName = string.IsNullOrWhiteSpace(l.FullName) ? "-" : l.FullName,
            Amount = l.Amount,
            Gateway = l.PaymentType?.PaymentTypeName,
            TransactionId = l.TransactionId,
            ForwardedTime = l.ForwardedTimestamp,
            ExamName = l.ExamSchedule?.ExamScheduleName,
            ContactNumber = l.MobileNumber,
            Email = l.Email,
            PaymentRequestLogStatus = l.PaymentRequestLogStatus
        }).ToList();

        return (items, totalCount);
    }

    public async Task<PaymentReconciliationResult> ReconcilePaymentAsync(int logId)
    {
        var log = await context.Set<PaymentRequestLog>()
            .Include(l => l.PaymentType)
            .FirstOrDefaultAsync(l => l.Id == logId);

        if (log == null)
            return new PaymentReconciliationResult { Success = false, Message = "Payment log not found." };

        if (log.PaymentRequestLogStatus == 1)
            return new PaymentReconciliationResult { Success = true, Message = "Payment is already marked as paid." };

        if (log.PaymentRequestLogStatus == 2)
            return new PaymentReconciliationResult { Success = false, Message = "This payment was already closed as terminal (could not be confirmed)." };

        var gateway = log.PaymentType?.PaymentTypeName?.ToLowerInvariant() ?? "";

        try
        {
            if (gateway.Contains("esewa"))
            {
                return await ReconcileESewaAsync(log);
            }

            if (gateway.Contains("khalti"))
            {
                return await ReconcileKhaltiAsync(log);
            }

            return new PaymentReconciliationResult
            {
                Success = false,
                Message = $"Gateway '{gateway}' is not supported for automated reconciliation."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ReconcilePaymentsAsync: exception for logId={LogId}", logId);
            return new PaymentReconciliationResult
            {
                Success = false,
                Message = $"Gateway verification failed: {ex.Message}"
            };
        }
    }

    private async Task<PaymentReconciliationResult> ReconcileESewaAsync(PaymentRequestLog log)
    {
        var transactionUuid = log.TransactionId ?? log.InvoiceNumber;
        if (string.IsNullOrEmpty(transactionUuid))
        {
            await LogResponseAsync(log, null, $"Could not reconcile eSewa payment logId={log.Id}: no transaction UUID stored.", success: false);
            return new PaymentReconciliationResult
            {
                Success = false,
                Message = "No transaction UUID stored for this payment — cannot verify with eSewa."
            };
        }

        var verified = await esewaService.VerifyTransactionAsync(transactionUuid, log.Amount);
        var status = verified?.Status ?? "null";
        var responseJson = JsonSerializer.Serialize(verified);

        if (verified == null || !string.Equals(status, "COMPLETE", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("ReconcileESewaAsync: payment not complete. logId={LogId}, status={Status}", log.Id, status);

            // Terminal eSewa statuses (transaction not found / failed / cancelled)
            // cannot ever be confirmed, so close the log (status 2) exactly like the
            // Khalti path does, so the poller stops revisiting it every cycle.
            if (IsEsewaTerminalStatus(status))
            {
                var message = $"eSewa reports status '{status}' (terminal). Payment is not confirmed and has been closed; no amount was charged.";
                await LogResponseAsync(log, null, message, success: false);
                await MarkLogTerminalAsync(log, message);
                return new PaymentReconciliationResult
                {
                    Success = false,
                    Message = $"eSewa reports status '{status}' — payment is not confirmed and was closed as terminal.",
                    GatewayStatus = status
                };
            }

            await LogResponseAsync(log, null, $"eSewa verification returned status '{status}' (not COMPLETE). Payment is not confirmed.", success: false);
            return new PaymentReconciliationResult
            {
                Success = false,
                Message = $"eSewa reports status '{status}' — payment is not confirmed.",
                GatewayStatus = status
            };
        }

        return await CompletePaymentAsync(log, verified.TransactionCode ?? transactionUuid, responseJson, "Payment verified via eSewa status check (reconciliation).");
    }

    private async Task<PaymentReconciliationResult> ReconcileKhaltiAsync(PaymentRequestLog log)
    {
        var pidx = log.TransactionId;
        if (string.IsNullOrEmpty(pidx))
        {
            await LogResponseAsync(log, null, $"Could not reconcile Khalti payment logId={log.Id}: no pidx stored.", success: false);
            return new PaymentReconciliationResult
            {
                Success = false,
                Message = "No pidx stored for this payment — cannot verify with Khalti."
            };
        }

        var lookup = await khaltiService.LookupPaymentAsync(pidx);
        var status = lookup?.Status ?? "null";
        var responseJson = JsonSerializer.Serialize(new { pidx, lookup });

        if (lookup == null || !string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("ReconcileKhaltiAsync: payment not complete. logId={LogId}, status={Status}", log.Id, status);

            // Terminal statuses (the payment cannot ever be confirmed) should close
            // the log so the background poller stops revisiting it every cycle.
            // Status 2 is the app-wide "closed / never revisited" marker (it is
            // excluded from the pending poll and from open apply-again checks).
            if (KhaltiPaymentStatus.IsTerminalStatus(lookup?.Status))
            {
                var message = $"Khalti reports status '{status}' (terminal). Payment is not confirmed and has been closed; no amount was charged.";
                await LogResponseAsync(log, null, message, success: false);
                await MarkLogTerminalAsync(log, message);
                return new PaymentReconciliationResult
                {
                    Success = false,
                    Message = $"Khalti reports status '{status}' — payment is not confirmed and was closed as terminal.",
                    GatewayStatus = status
                };
            }

            // Non-terminal (still genuinely pending/initiated): keep polling.
            await LogResponseAsync(log, null, $"Khalti lookup returned status '{status}' (not Completed). Payment is not confirmed, will retry later.", success: false);
            return new PaymentReconciliationResult
            {
                Success = false,
                Message = $"Khalti reports status '{status}' — payment is not confirmed yet (will retry).",
                GatewayStatus = status
            };
        }

        return await CompletePaymentAsync(log, lookup.TransactionId ?? pidx, responseJson, "Payment verified via Khalti lookup (reconciliation).");
    }

    private async Task<PaymentReconciliationResult> CompletePaymentAsync(
        PaymentRequestLog log, string transactionCode, string responseData, string responseMessage)
    {
        await CompleteExamRegistrationAsync(log);

        // Update log status.
        await UpdateLogAsync(log, transactionCode, true, responseData, responseMessage);

        // Send notification.
        await SendConfirmationAsync(log, transactionCode);

        await auditLogWriter.LogAsync(ActivityTypes.PaymentReconciled,
            $"Payment reconciled via gateway (Transaction {transactionCode})",
            new { gateway = log.PaymentType?.PaymentTypeName, logId = log.Id, invoice = log.InvoiceNumber, transactionCode, amount = log.Amount },
            AuditSeverity.Info, entityName: "PaymentRequestLog", entityId: log.Id.ToString());

        return new PaymentReconciliationResult
        {
            Success = true,
            Message = "Payment verified and completed successfully.",
            GatewayStatus = "COMPLETE"
        };
    }

    // Replicates the canonical post-payment registration logic (see
    // StudentDashboardController.HandlePostPaymentRegistration). Shared by the
    // gateway reconciliation path and the admin manual-confirm path so both
    // produce the exact same student-facing result (paid status, exam
    // registration, admit-card eligibility).
    private async Task CompleteExamRegistrationAsync(PaymentRequestLog log)
    {
        if (!log.StudentRegistrationId.HasValue || log.ExamScheduleId <= 0)
        {
            logger.LogWarning("CompleteExamRegistrationAsync: cannot create registration for logId={LogId} (StudentRegistrationId={StudentRegId}, ExamScheduleId={ScheduleId})",
                log.Id, log.StudentRegistrationId, log.ExamScheduleId);
            return;
        }

        var userId = await ResolveUserIdAsync(log);
        if (userId == null)
        {
            logger.LogWarning("CompleteExamRegistrationAsync: could not resolve user for logId={LogId} (StudentRegistrationId={StudentRegId})", log.Id, log.StudentRegistrationId);
            return;
        }

        // A confirmed payment on a rejected form with an older confirmed payment
        // is a reapply top-up: revive the rejected registration with this log's
        // subject tokens rather than creating a duplicate.
        if (await dashboardService.TryCompleteApplyAgainTopUpAsync(log.Id, userId))
        {
            logger.LogInformation("CompleteExamRegistrationAsync: reapply top-up completed for logId={LogId}", log.Id);
            return;
        }

        var existing = await dashboardService.HasExistingExamRegistrationAsync(log.ExamScheduleId, userId);
        var selection = ReExamSubjectSelection.Parse(log.SelectedSubjectIds);

        if (!existing && selection.Count > 0)
        {
            await dashboardService.CreateExamRegistrationAsync(
                log.ExamScheduleId, userId, log.Amount, selection.Keys.ToList(), log.StudentRegistrationId.Value, selection);
            logger.LogInformation("CompleteExamRegistrationAsync: created ExamRegistration for logId={LogId}, scheduleId={ScheduleId}, studentRegId={StudentRegId}",
                log.Id, log.ExamScheduleId, log.StudentRegistrationId);
        }
        else if (!existing)
        {
            logger.LogWarning("CompleteExamRegistrationAsync: no subject selection on logId={LogId}; skipping registration creation.", log.Id);
        }
    }

    private async Task<string?> ResolveUserIdAsync(PaymentRequestLog log)
    {
        // Try by email first.
        if (!string.IsNullOrWhiteSpace(log.Email))
        {
            var byEmail = await userManager.FindByEmailAsync(log.Email);
            if (byEmail != null) return byEmail.Id;
        }

        // Fall back to the linked student registration -> admission user.
        if (log.StudentRegistrationId.HasValue)
        {
            var studentReg = await context.StudentRegistrations!
                .AsNoTracking()
                .FirstOrDefaultAsync(sr => sr.Id == log.StudentRegistrationId.Value);

            if (studentReg?.StudentAdmissionId != null)
            {
                var admission = await context.StudentAdmissions!
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sa => sa.Id == studentReg.StudentAdmissionId.Value);
                if (admission != null && !string.IsNullOrEmpty(admission.AppUserId))
                    return admission.AppUserId;
            }

            // Try by registration number == user name.
            if (!string.IsNullOrWhiteSpace(studentReg?.RegistrationNumber))
            {
                var byRegNumber = await userManager.FindByNameAsync(studentReg.RegistrationNumber);
                if (byRegNumber != null) return byRegNumber.Id;
            }
        }

        return null;
    }

    private async Task SendConfirmationAsync(PaymentRequestLog log, string reference)
    {
        try
        {
            var schedule = await dashboardService.GetExamScheduleByIdAsync(log.ExamScheduleId);
            var scheduleName = schedule?.ExamScheduleName ?? $"Exam Schedule #{log.ExamScheduleId}";
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
            logger.LogWarning(ex, "SendConfirmationAsync: failed to send notification for logId={LogId}", log.Id);
        }
    }

    private async Task UpdateLogAsync(PaymentRequestLog log, string transactionId, bool isSuccess, string responseData, string? responseMessage = null)
    {
        log.TransactionId = transactionId;
        log.PaymentRequestLogStatus = isSuccess ? 1 : 0;
        context.Set<PaymentRequestLog>().Update(log);

        context.Set<PaymentResponseLog>().Add(new PaymentResponseLog
        {
            PaymentRequestLogId = log.Id,
            ResponseTimestamp = DateTime.UtcNow,
            IsSuccess = isSuccess,
            ResponseMessage = responseMessage ?? (isSuccess ? "Payment reconciled via gateway" : "Payment failed during reconciliation"),
            FullResponse = responseData
        });

        await context.SaveChangesAsync();
    }

    private async Task LogResponseAsync(PaymentRequestLog log, string? transactionId, string message, bool success)
    {
        await UpdateLogIfNeededAsync(log, transactionId, success, message);
    }

    private async Task UpdateLogIfNeededAsync(PaymentRequestLog log, string? transactionId, bool isSuccess, string message)
    {
        context.Set<PaymentResponseLog>().Add(new PaymentResponseLog
        {
            PaymentRequestLogId = log.Id,
            ResponseTimestamp = DateTime.UtcNow,
            IsSuccess = isSuccess,
            ResponseMessage = message,
            FullResponse = message
        });
        await context.SaveChangesAsync();
    }

    // Terminal Khalti lookup statuses: the payment can never reach "Completed",
    // so there is no point polling it repeatedly. Status 2 is the app-wide
    // "closed / never revisited" marker used for superseded attempts.
    private async Task MarkLogTerminalAsync(PaymentRequestLog log, string? reason)
    {
        log.PaymentRequestLogStatus = 2;
        context.Set<PaymentRequestLog>().Update(log);

        context.Set<PaymentResponseLog>().Add(new PaymentResponseLog
        {
            PaymentRequestLogId = log.Id,
            ResponseTimestamp = DateTime.UtcNow,
            IsSuccess = false,
            ResponseMessage = string.IsNullOrWhiteSpace(reason) ? "Payment closed as terminal (could not be confirmed)." : reason,
            FullResponse = reason ?? "Payment closed as terminal."
        });

        await context.SaveChangesAsync();

        await auditLogWriter.LogAsync(ActivityTypes.PaymentMarkedFailed,
            string.IsNullOrWhiteSpace(reason) ? "Khalti payment closed as terminal (could not be confirmed)." : $"Khalti payment closed as terminal: {reason}",
            new { logId = log.Id, invoice = log.InvoiceNumber, gateway = "khalti", reason, status = log.PaymentRequestLogStatus },
            AuditSeverity.Warning, entityName: "PaymentRequestLog", entityId: log.Id.ToString());
    }

    public async Task<PaymentReconciliationResult> MarkPaymentFailedAsync(int logId, string reason)
    {
        var log = await context.Set<PaymentRequestLog>().FirstOrDefaultAsync(l => l.Id == logId);
        if (log == null)
            return new PaymentReconciliationResult { Success = false, Message = "Payment log not found." };

        if (log.PaymentRequestLogStatus == 1)
            return new PaymentReconciliationResult { Success = true, Message = "Payment is already marked as paid." };

        if (log.PaymentRequestLogStatus == 2)
            return new PaymentReconciliationResult { Success = false, Message = "This payment was already closed as terminal (could not be confirmed)." };

        log.PaymentRequestLogStatus = 0;
        context.Set<PaymentRequestLog>().Update(log);

        context.Set<PaymentResponseLog>().Add(new PaymentResponseLog
        {
            PaymentRequestLogId = logId,
            ResponseTimestamp = DateTime.UtcNow,
            IsSuccess = false,
            ResponseMessage = string.IsNullOrWhiteSpace(reason) ? "Marked as failed by admin." : $"Marked as failed by admin: {reason}",
            FullResponse = string.IsNullOrWhiteSpace(reason) ? "Marked as failed by admin." : reason
        });

        await context.SaveChangesAsync();

        await auditLogWriter.LogAsync(ActivityTypes.PaymentMarkedFailed,
            $"Payment marked as failed by admin. Reason: {reason}",
            new { logId, invoice = log.InvoiceNumber, reason },
            AuditSeverity.Warning, entityName: "PaymentRequestLog", entityId: logId.ToString());

        return new PaymentReconciliationResult { Success = true, Message = "Payment marked as failed." };
    }

    public async Task<PaymentReconciliationResult> ConfirmPaymentManuallyAsync(int logId, string? remark)
    {
        var log = await context.Set<PaymentRequestLog>()
            .Include(l => l.PaymentType)
            .FirstOrDefaultAsync(l => l.Id == logId);
        if (log == null)
            return new PaymentReconciliationResult { Success = false, Message = "Payment log not found." };

        if (log.PaymentRequestLogStatus == 1)
            return new PaymentReconciliationResult { Success = true, Message = "Payment is already marked as paid." };

        if (log.PaymentRequestLogStatus == 2)
            return new PaymentReconciliationResult { Success = false, Message = "This payment was already closed as terminal (could not be confirmed)." };

        // Complete the registration (paid status, exam registration, admit-card
        // eligibility) exactly like the gateway confirmation path, so the student
        // receives the same result as a normally completed payment.
        await CompleteExamRegistrationAsync(log);

        var remarkText = string.IsNullOrWhiteSpace(remark) ? "No remark provided" : remark;
        var reference = log.TransactionId ?? $"ADMIN-{logId}";
        var responseMessage = $"Payment manually confirmed as paid by admin. Remark: {remarkText}";

        await UpdateLogAsync(log, reference, true, remarkText, responseMessage);

        await SendConfirmationAsync(log, reference);

        await auditLogWriter.LogAsync(ActivityTypes.PaymentConfirmedManually,
            $"Payment manually confirmed as paid by admin. Remark: {remarkText}",
            new { logId, invoice = log.InvoiceNumber, gateway = log.PaymentType?.PaymentTypeName, transactionId = log.TransactionId, remark = remarkText, amount = log.Amount },
            AuditSeverity.Info, entityName: "PaymentRequestLog", entityId: logId.ToString());

        return new PaymentReconciliationResult { Success = true, Message = "Payment manually confirmed as paid. The student has been registered and notified." };
    }

    public async Task<int> ReconcilePendingBatchAsync()
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-5);
        var pendingIds = await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .Where(l => (l.PaymentRequestLogStatus == null || l.PaymentRequestLogStatus == 3)
                     && l.StudentRegistrationId != null
                     && l.ForwardedTimestamp < cutoff)
            .OrderBy(l => l.ForwardedTimestamp)
            .Select(l => l.Id)
            .ToListAsync();

        var reconciled = 0;
        foreach (var id in pendingIds)
        {
            var result = await ReconcilePaymentAsync(id);
            if (result.Success) reconciled++;
        }

        logger.LogInformation("ReconcilePendingBatchAsync: processed {Processed} pending payments, {Reconciled} reconciled.", pendingIds.Count, reconciled);
        return reconciled;
    }

    private async Task<(List<PaymentRequestLog> Logs, int Total)> GetReconcileablePendingCoreAsync(int? limit = null)
    {
        // Declared as the non-ordered base interface so .Take(...) (which returns
        // IQueryable, not IOrderedQueryable) can be assigned back to it.
        IQueryable<PaymentRequestLog> query = context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .Include(l => l.PaymentType)
            .Where(l => (l.PaymentRequestLogStatus == null || l.PaymentRequestLogStatus == 3)
                     && l.StudentRegistrationId != null)
            .OrderBy(l => l.ForwardedTimestamp);

        var total = await query.CountAsync();

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return (await query.ToListAsync(), total);
    }

    public async Task<List<PaymentReconciliationListDto>> GetReconcileablePendingAsync()
    {
        var (logs, _) = await GetReconcileablePendingCoreAsync();
        return logs.Select(l => new PaymentReconciliationListDto
        {
            Id = l.Id,
            InvoiceNumber = l.InvoiceNumber,
            StudentName = string.IsNullOrWhiteSpace(l.FullName) ? "-" : l.FullName,
            Amount = l.Amount,
            Gateway = l.PaymentType?.PaymentTypeName,
            TransactionId = l.TransactionId,
            ForwardedTime = l.ForwardedTimestamp,
            ExamName = l.ExamSchedule?.ExamScheduleName,
            ContactNumber = l.MobileNumber,
            Email = l.Email,
            PaymentRequestLogStatus = l.PaymentRequestLogStatus
        }).ToList();
    }

    public async Task<PaymentReconciliationBatchResult> ReconcilePendingWithDetailsAsync()
    {
        var result = new PaymentReconciliationBatchResult();

        if (!await _batchLock.WaitAsync(0))
        {
            result.AlreadyRunning = true;
            return result;
        }

        try
        {
            var (logs, totalPending) = await GetReconcileablePendingCoreAsync();
            result.TotalPending = totalPending;

            foreach (var log in logs)
            {
                var outcome = new PaymentReconciliationOutcome
                {
                    Id = log.Id,
                    InvoiceNumber = log.InvoiceNumber,
                    StudentName = string.IsNullOrWhiteSpace(log.FullName) ? "-" : log.FullName,
                    Amount = log.Amount,
                    Gateway = log.PaymentType?.PaymentTypeName
                };

                var reconcileResult = await ReconcilePaymentAsync(log.Id);

                var status = reconcileResult.GatewayStatus;
                if (reconcileResult.Success)
                {
                    outcome.Outcome = "Confirmed";
                    outcome.Message = reconcileResult.Message;
                    result.Confirmed++;
                }
                else if (KhaltiPaymentStatus.IsTerminalStatus(status) ||
                         IsEsewaTerminalStatus(status))
                {
                    outcome.Outcome = "Expired";
                    outcome.Message = reconcileResult.Message;
                    result.Expired++;
                }
                else if (!string.IsNullOrWhiteSpace(status) &&
                         (string.Equals(status, "Initiated", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase)))
                {
                    outcome.Outcome = "StillPending";
                    outcome.Message = reconcileResult.Message;
                    result.StillPending++;
                }
                else
                {
                    outcome.Outcome = "Failed";
                    outcome.Message = reconcileResult.Message;
                    result.Failed++;
                }

                result.Items.Add(outcome);
            }

            logger.LogInformation(
                "ReconcilePendingWithDetailsAsync: processed {Processed} of {Total} pending payments (confirmed={Confirmed}, expired={Expired}, failed={Failed}, stillPending={StillPending}).",
                result.Items.Count, totalPending, result.Confirmed, result.Expired, result.Failed, result.StillPending);
            return result;
        }
        finally
        {
            _batchLock.Release();
        }
    }

    private static bool IsEsewaTerminalStatus(string? status)
    {
        // NOTE: NOT_FOUND is intentionally NOT terminal. Real prod logs show eSewa
        // can return NOT_FOUND/PENDING for a transaction_uuid and later confirm the
        // SAME uuid as COMPLETE. Closing NOT_FOUND as status 2 would orphan a payment
        // that eSewa subsequently reports as paid. So NOT_FOUND/PENDING stay pending
        // (retried), and only genuinely-terminal outcomes are closed.
        return !string.IsNullOrWhiteSpace(status) &&
               (string.Equals(status, "FAILED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "CANCELED", StringComparison.OrdinalIgnoreCase));
    }
}