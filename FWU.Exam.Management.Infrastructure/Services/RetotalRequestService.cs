using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class RetotalRequestService(AppDbContext context, IUserContext userContext) : IRetotalRequestService
{
    public async Task<(List<RetotalRequest> Items, int TotalCount)> GetRetotalRequestsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search, sort, sortDir).ApplyScope(userContext);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new RetotalRequest
            {
                Id = e.Id,
                ExamSubjectResultId = e.ExamSubjectResultId,
                StudentRegistrationId = e.StudentRegistrationId,
                ExamRegistrationId = e.ExamRegistrationId,
                RequestedDate = e.RequestedDate,
                Reason = e.Reason,
                Status = e.Status,
                OriginalGradeLetter = e.OriginalGradeLetter,
                OriginalObtainedMarks = e.OriginalObtainedMarks,
                RetotalledGradeLetter = e.RetotalledGradeLetter,
                RetotalledObtainedMarks = e.RetotalledObtainedMarks,
                ReviewedByUsername = e.ReviewedByUsername,
                ReviewedDate = e.ReviewedDate,
                AdminRemarks = e.AdminRemarks,
                FeeAmount = e.FeeAmount,
                FeePaid = e.FeePaid,
                IsActive = e.IsActive,
                ExamSubjectResult = e.ExamSubjectResult,
                StudentRegistration = e.StudentRegistration,
                ExamRegistration = e.ExamRegistration
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<RetotalRequest>> GetFilteredItemsAsync(string? search)
    {
        var query = BuildQuery(search, "Id", "asc").ApplyScope(userContext);
        return await query
            .Select(e => new RetotalRequest
            {
                Id = e.Id,
                ExamSubjectResultId = e.ExamSubjectResultId,
                StudentRegistrationId = e.StudentRegistrationId,
                ExamRegistrationId = e.ExamRegistrationId,
                RequestedDate = e.RequestedDate,
                Reason = e.Reason,
                Status = e.Status,
                OriginalGradeLetter = e.OriginalGradeLetter,
                OriginalObtainedMarks = e.OriginalObtainedMarks,
                ReviewedByUsername = e.ReviewedByUsername,
                ReviewedDate = e.ReviewedDate,
                AdminRemarks = e.AdminRemarks,
                FeeAmount = e.FeeAmount,
                FeePaid = e.FeePaid,
                IsActive = e.IsActive,
                ExamSubjectResult = e.ExamSubjectResult,
                StudentRegistration = e.StudentRegistration,
                ExamRegistration = e.ExamRegistration
            })
            .ToListAsync();
    }

    public async Task<RetotalRequest?> GetRetotalRequestByIdAsync(int id)
    {
        return await context.RetotalRequests
            .AsNoTracking()
            .Include(r => r.ExamSubjectResult)
                .ThenInclude(esr => esr.SubjectOffering)
            .Include(r => r.StudentRegistration)
            .Include(r => r.ExamRegistration)
                .ThenInclude(er => er.ExamSchedule)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task CreateRetotalRequestAsync(RetotalRequest retotalRequest)
    {
        context.RetotalRequests.Add(retotalRequest);
        await context.SaveChangesAsync();
    }

    public async Task UpdateRetotalRequestAsync(RetotalRequest retotalRequest)
    {
        var existing = await context.RetotalRequests.FindAsync(retotalRequest.Id);
        if (existing != null)
        {
            retotalRequest.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(retotalRequest);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteRetotalRequestAsync(int id)
    {
        var retotalRequest = await context.RetotalRequests.FindAsync(id);
        if (retotalRequest != null)
        {
            retotalRequest.IsActive = false;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> RetotalRequestExistsAsync(int id)
    {
        return await context.RetotalRequests.AnyAsync(e => e.Id == id);
    }

    public async Task MarkUnderReviewAsync(int id, string reviewedBy)
    {
        var request = await context.RetotalRequests.FindAsync(id);
        if (request != null && request.Status == RetotalStatus.Pending)
        {
            request.Status = RetotalStatus.UnderReview;
            request.ReviewedByUsername = reviewedBy;
            request.ReviewedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task ApproveRetotalRequestAsync(int id, string? retotalledGradeLetter, float? retotalledMarks, string? adminRemarks, string reviewedBy)
    {
        var request = await context.RetotalRequests.FindAsync(id);
        if (request != null && request.Status == RetotalStatus.UnderReview)
        {
            request.Status = RetotalStatus.Approved;
            request.RetotalledGradeLetter = retotalledGradeLetter;
            request.RetotalledObtainedMarks = retotalledMarks;
            request.AdminRemarks = adminRemarks;
            request.ReviewedByUsername = reviewedBy;
            request.ReviewedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task RejectRetotalRequestAsync(int id, string? adminRemarks, string reviewedBy)
    {
        var request = await context.RetotalRequests.FindAsync(id);
        if (request != null && (request.Status == RetotalStatus.Pending || request.Status == RetotalStatus.UnderReview))
        {
            request.Status = RetotalStatus.Rejected;
            request.AdminRemarks = adminRemarks;
            request.ReviewedByUsername = reviewedBy;
            request.ReviewedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task<RetotalRequestSelectListsDto> GetSelectListDataAsync(RetotalRequest? retotalRequest = null)
    {
        var examSchedules = await context.ExamSchedules.AsNoTracking().ToListAsync();
        var students = await context.StudentRegistrations.AsNoTracking().ToListAsync();
        var subjects = await context.SubjectCatalogs.AsNoTracking().ToListAsync();

        return new RetotalRequestSelectListsDto
        {
            ExamSchedules = examSchedules.Select(es => new SelectOption { Id = es.Id, Name = es.ExamScheduleName }).ToList(),
            Students = students.Select(s => new SelectOption { Id = s.Id, Name = $"{s.FirstName} {s.LastName}" }).ToList(),
            Subjects = subjects.Select(s => new SelectOption { Id = s.Id, Name = s.SubjectName }).ToList()
        };
    }

    private IQueryable<RetotalRequest> BuildQuery(string? search, string sort, string sortDir)
    {
        IQueryable<RetotalRequest> query = context.RetotalRequests.AsNoTracking()
            .Include(r => r.ExamSubjectResult)
            .Include(r => r.StudentRegistration)
            .Include(r => r.ExamRegistration);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(r =>
                (r.Reason != null && r.Reason.Contains(search)) ||
                (r.ExamSubjectResult != null && r.ExamSubjectResult.GradeLetter != null && r.ExamSubjectResult.GradeLetter.Contains(search)) ||
                (r.StudentRegistration != null && r.StudentRegistration.FirstName != null && r.StudentRegistration.FirstName.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "status" => descending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
            "date" => descending ? query.OrderByDescending(r => r.RequestedDate) : query.OrderBy(r => r.RequestedDate),
            "student" => descending
                ? query.OrderByDescending(r => r.StudentRegistration != null ? r.StudentRegistration.FirstName : string.Empty)
                : query.OrderBy(r => r.StudentRegistration != null ? r.StudentRegistration.FirstName : string.Empty),
            _ => descending ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id)
        };
    }
}
