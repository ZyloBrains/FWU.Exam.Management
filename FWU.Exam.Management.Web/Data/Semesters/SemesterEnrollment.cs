using fwu_examination_management_system.Data.Enums;
using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Students;

namespace fwu_examination_management_system.Data.Semesters;

public class SemesterEnrollment
{
    public int Id { get; set; }

    public int StudentAdmissionId { get; set; }
    public int SemesterId { get; set; }
    public StudentEnrollmentStatus EnrollmentStatus { get; set; }
    public EnrollmentType EnrollmentType { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime EnrolledDate { get; set; }
    public DateTime? DropDate { get; set; }
    public string? DropReason { get; set; }
    public DateTime? SemesterResultDate { get; set; }
    public double TotalCredits { get; set; }
    public double GradePoints { get; set; }
    public double TotalFee { get; set; }
    public double PaidAmount { get; set; }
    public bool Deficiency { get; set; }
    public ResultStatus ResultStatus { get; set; }

    public virtual StudentAdmission? StudentAdmission { get; set; }

    public virtual Semester? Semester { get; set; }
    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; }
    public virtual ICollection<ExamSubjectRegistrationInternal>? ExamSubjectRegistrationInternals { get; set; }
}
