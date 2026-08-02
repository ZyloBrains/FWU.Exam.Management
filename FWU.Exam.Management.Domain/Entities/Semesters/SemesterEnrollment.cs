using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Semesters;

public class SemesterEnrollment : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Student Admission")]
    public int StudentAdmissionId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Semester")]
    public int SemesterId { get; set; }

    [Display(Name = "Enrollment Status")]
    public StudentEnrollmentStatus EnrollmentStatus { get; set; }

    [Display(Name = "Enrollment Type")]
    public EnrollmentType EnrollmentType { get; set; }

    [Display(Name = "Payment Status")]
    public PaymentStatus PaymentStatus { get; set; }

    [Display(Name = "Enrolled Date")]
    public DateTime EnrolledDate { get; set; }

    [Display(Name = "Drop Date")]
    public DateTime? DropDate { get; set; }

    [MaxLength(500)]
    [Display(Name = "Drop Reason")]
    public string? DropReason { get; set; }

    [Display(Name = "Semester Result Date")]
    public DateTime? SemesterResultDate { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Total Credits")]
    public double TotalCredits { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Grade Points")]
    public double GradePoints { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Total Fee")]
    public double TotalFee { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Paid Amount")]
    public double PaidAmount { get; set; }

    [Display(Name = "Deficiency")]
    public bool Deficiency { get; set; }

    [Display(Name = "Result Status")]
    public ResultStatus ResultStatus { get; set; }

    public virtual StudentAdmission? StudentAdmission { get; set; }
    public virtual Semester? Semester { get; set; }
    public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; } = [];
}
