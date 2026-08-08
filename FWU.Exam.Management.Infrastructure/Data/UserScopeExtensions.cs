using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Data;

public static class UserScopeExtensions
{
    public static IQueryable<Faculty> ApplyScope(this IQueryable<Faculty> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.FacultyId.HasValue)
            return query.Where(f => f.Id == user.FacultyId.Value);
        return query.Where(f => false);
    }

    public static IQueryable<Semester> ApplyScope(this IQueryable<Semester> query, IUserContext user)
    {
        return query;
    }

    public static IQueryable<College> ApplyScope(this IQueryable<College> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
        {
            var collegeIds = user.FacultyCollegeIds;
            return query.Where(c => collegeIds.Contains(c.Id));
        }
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(c => c.Id == user.CollegeId.Value);
        return query.Where(c => false);
    }

    public static IQueryable<Program> ApplyScope(this IQueryable<Program> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(p => p.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(p => p.CollegePrograms!.Any(cp => cp.CollegeId == user.CollegeId.Value));
        return query.Where(p => false);
    }

    public static IQueryable<SubjectCatalog> ApplyScope(this IQueryable<SubjectCatalog> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(sc => sc.SubjectOfferings!.Any(so =>
                so.Program != null && so.Program.FacultyId == user.FacultyId.Value));
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(sc => sc.SubjectOfferings!.Any(so =>
                so.Program != null && so.Program.CollegePrograms!.Any(cp => cp.CollegeId == user.CollegeId.Value)));
        return query.Where(sc => false);
    }

    public static IQueryable<SubjectOffering> ApplyScope(this IQueryable<SubjectOffering> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(so =>
                so.Program != null && so.Program.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(so =>
                so.Program != null && so.Program.CollegePrograms!.Any(cp => cp.CollegeId == user.CollegeId.Value));
        return query.Where(so => false);
    }

    public static IQueryable<CurriculumVersion> ApplyScope(this IQueryable<CurriculumVersion> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(cv => cv.Program != null && cv.Program.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(cv => cv.Program != null && cv.Program.CollegePrograms!.Any(cp => cp.CollegeId == user.CollegeId.Value));
        return query.Where(cv => false);
    }

    public static IQueryable<StudentRegistration> ApplyScope(this IQueryable<StudentRegistration> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(sr => sr.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(sr => sr.CollegeId == user.CollegeId.Value);
        return query.Where(sr => false);
    }

    public static IQueryable<StudentAdmission> ApplyScope(this IQueryable<StudentAdmission> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(sa =>
                sa.Program != null && sa.Program.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(sa => sa.CollegeId == user.CollegeId.Value);
        return query.Where(sa => false);
    }

    public static IQueryable<ExamRegistration> ApplyScope(this IQueryable<ExamRegistration> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(er =>
                er.Program != null && er.Program.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(er => er.CollegeId == user.CollegeId.Value);
        return query.Where(er => false);
    }

    public static IQueryable<ExamSchedule> ApplyScope(this IQueryable<ExamSchedule> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(es =>
                es.Program != null && es.Program.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(es => es.CollegeId == user.CollegeId.Value);
        return query.Where(es => false);
    }

    public static IQueryable<CollegeProgram> ApplyScope(this IQueryable<CollegeProgram> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(cp =>
                cp.Program != null && cp.Program.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(cp => cp.CollegeId == user.CollegeId.Value);
        return query.Where(cp => false);
    }

    public static IQueryable<AdmitCard> ApplyScope(this IQueryable<AdmitCard> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(ac => ac.ExamRegistration != null &&
                ac.ExamRegistration.Program != null && ac.ExamRegistration.Program.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(ac => ac.ExamRegistration != null && ac.ExamRegistration.CollegeId == user.CollegeId.Value);
        return query.Where(ac => false);
    }

    public static IQueryable<ResultRecord> ApplyScope(this IQueryable<ResultRecord> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(rr =>
                rr.Program != null && rr.Program.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(rr => rr.CollegeId == user.CollegeId.Value);
        return query.Where(rr => false);
    }

    public static IQueryable<RetotalRequest> ApplyScope(this IQueryable<RetotalRequest> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(rr => rr.ExamRegistration != null &&
                rr.ExamRegistration.Program != null && rr.ExamRegistration.Program.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(rr => rr.ExamRegistration != null && rr.ExamRegistration.CollegeId == user.CollegeId.Value);
        return query.Where(rr => false);
    }

    public static IQueryable<AppUser> ApplyScope(this IQueryable<AppUser> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
        {
            var collegeIds = user.FacultyCollegeIds;
            return query.Where(u =>
                u.FacultyId == user.FacultyId.Value ||
                (u.CollegeId != null && collegeIds.Contains(u.CollegeId.Value)));
        }
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(u => u.CollegeId == user.CollegeId.Value);
        return query.Where(u => u.Id == user.UserId);
    }
}
