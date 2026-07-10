using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
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

    public static IQueryable<College> ApplyScope(this IQueryable<College> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(c => c.Faculties!.Any(f => f.Id == user.FacultyId.Value));
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(c => c.Id == user.CollegeId.Value);
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(c => c.CollegePrograms!.Any(cp => cp.Program!.DepartmentId == user.DepartmentId.Value));
        return query.Where(c => false);
    }

    public static IQueryable<Department> ApplyScope(this IQueryable<Department> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(d => d.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(d => d.Faculty != null && d.Faculty.Colleges!.Any(c => c.Id == user.CollegeId.Value));
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(d => d.Id == user.DepartmentId.Value);
        return query.Where(d => false);
    }

    public static IQueryable<Program> ApplyScope(this IQueryable<Program> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(p => p.Department != null && p.Department.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(p => p.CollegePrograms!.Any(cp => cp.CollegeId == user.CollegeId.Value));
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(p => p.DepartmentId == user.DepartmentId.Value);
        return query.Where(p => false);
    }

    public static IQueryable<SubjectCatalog> ApplyScope(this IQueryable<SubjectCatalog> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(sc => sc.SubjectOfferings!.Any(so =>
                so.Program != null && so.Program.Department != null && so.Program.Department.FacultyId == user.FacultyId.Value));
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(sc => sc.SubjectOfferings!.Any(so =>
                so.Program != null && so.Program.CollegePrograms!.Any(cp => cp.CollegeId == user.CollegeId.Value)));
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(sc => sc.SubjectOfferings!.Any(so =>
                so.Program != null && so.Program.DepartmentId == user.DepartmentId.Value));
        return query.Where(sc => false);
    }

    public static IQueryable<SubjectOffering> ApplyScope(this IQueryable<SubjectOffering> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(so =>
                so.Program != null && so.Program.Department != null && so.Program.Department.FacultyId == user.FacultyId.Value);
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(so =>
                so.Program != null && so.Program.CollegePrograms!.Any(cp => cp.CollegeId == user.CollegeId.Value));
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(so => so.Program != null && so.Program.DepartmentId == user.DepartmentId.Value);
        return query.Where(so => false);
    }

    public static IQueryable<StudentRegistration> ApplyScope(this IQueryable<StudentRegistration> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(sr =>
                (sr.College != null && sr.College.Faculties!.Any(f => f.Id == user.FacultyId.Value)) ||
                (sr.Department != null && sr.Department.FacultyId == user.FacultyId.Value));
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(sr => sr.CollegeId == user.CollegeId.Value);
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(sr => sr.DepartmentId == user.DepartmentId.Value);
        return query.Where(sr => false);
    }

    public static IQueryable<StudentAdmission> ApplyScope(this IQueryable<StudentAdmission> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(sa =>
                (sa.College != null && sa.College.Faculties!.Any(f => f.Id == user.FacultyId.Value)) ||
                (sa.Program != null && sa.Program.Department != null && sa.Program.Department.FacultyId == user.FacultyId.Value));
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(sa => sa.CollegeId == user.CollegeId.Value);
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(sa => sa.Program != null && sa.Program.DepartmentId == user.DepartmentId.Value);
        return query.Where(sa => false);
    }

    public static IQueryable<ExamRegistration> ApplyScope(this IQueryable<ExamRegistration> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(er =>
                (er.College != null && er.College.Faculties!.Any(f => f.Id == user.FacultyId.Value)) ||
                (er.Program != null && er.Program.Department != null && er.Program.Department.FacultyId == user.FacultyId.Value));
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(er => er.CollegeId == user.CollegeId.Value);
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(er => er.Program != null && er.Program.DepartmentId == user.DepartmentId.Value);
        return query.Where(er => false);
    }

    public static IQueryable<ExamSchedule> ApplyScope(this IQueryable<ExamSchedule> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(es => es.ExamRegistrations!.Any(er =>
                (er.College != null && er.College.Faculties!.Any(f => f.Id == user.FacultyId.Value)) ||
                (er.Program != null && er.Program.Department != null && er.Program.Department.FacultyId == user.FacultyId.Value)));
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(es => es.ExamRegistrations!.Any(er => er.CollegeId == user.CollegeId.Value));
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(es => es.ExamRegistrations!.Any(er => er.Program != null && er.Program.DepartmentId == user.DepartmentId.Value));
        return query.Where(es => false);
    }

    public static IQueryable<CollegeProgram> ApplyScope(this IQueryable<CollegeProgram> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(cp => cp.College != null && cp.College.Faculties!.Any(f => f.Id == user.FacultyId.Value));
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(cp => cp.CollegeId == user.CollegeId.Value);
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(cp => cp.Program != null && cp.Program.DepartmentId == user.DepartmentId.Value);
        return query.Where(cp => false);
    }

    public static IQueryable<AdmitCard> ApplyScope(this IQueryable<AdmitCard> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(ac => ac.ExamRegistration != null &&
                ((ac.ExamRegistration.College != null && ac.ExamRegistration.College.Faculties!.Any(f => f.Id == user.FacultyId.Value)) ||
                 (ac.ExamRegistration.Program != null && ac.ExamRegistration.Program.Department != null && ac.ExamRegistration.Program.Department.FacultyId == user.FacultyId.Value)));
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(ac => ac.ExamRegistration != null && ac.ExamRegistration.CollegeId == user.CollegeId.Value);
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(ac => ac.ExamRegistration != null && ac.ExamRegistration.Program != null && ac.ExamRegistration.Program.DepartmentId == user.DepartmentId.Value);
        return query.Where(ac => false);
    }

    public static IQueryable<ResultRecord> ApplyScope(this IQueryable<ResultRecord> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(rr => rr.College != null && rr.College.Faculties!.Any(f => f.Id == user.FacultyId.Value));
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(rr => rr.CollegeId == user.CollegeId.Value);
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(rr => rr.Program != null && rr.Program.DepartmentId == user.DepartmentId.Value);
        return query.Where(rr => false);
    }

    public static IQueryable<RetotalRequest> ApplyScope(this IQueryable<RetotalRequest> query, IUserContext user)
    {
        if (user.IsSuperAdmin) return query;
        if (user.IsFacultyAdmin && user.FacultyId.HasValue)
            return query.Where(rr => rr.ExamRegistration != null &&
                ((rr.ExamRegistration.College != null && rr.ExamRegistration.College.Faculties!.Any(f => f.Id == user.FacultyId.Value)) ||
                 (rr.ExamRegistration.Program != null && rr.ExamRegistration.Program.Department != null && rr.ExamRegistration.Program.Department.FacultyId == user.FacultyId.Value)));
        if (user.IsCollegeAdmin && user.CollegeId.HasValue)
            return query.Where(rr => rr.ExamRegistration != null && rr.ExamRegistration.CollegeId == user.CollegeId.Value);
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(rr => rr.ExamRegistration != null && rr.ExamRegistration.Program != null && rr.ExamRegistration.Program.DepartmentId == user.DepartmentId.Value);
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
        if (user.IsDepartmentAdmin && user.DepartmentId.HasValue)
            return query.Where(u => u.DepartmentId == user.DepartmentId.Value);
        return query.Where(u => u.Id == user.UserId);
    }
}
