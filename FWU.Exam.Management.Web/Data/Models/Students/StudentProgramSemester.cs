using fwu_examination_management_system.Data.Models.Exams;

namespace fwu_examination_management_system.Data.Models.Students;

public class StudentProgramSemester
{
    public int Id { get; set; }

    public int StudentAdmissionId { get; set; }
    public int SemesterId { get; set; }
    public bool IsRunning { get; set; }
    public bool IsActive { get; set; }

    public virtual StudentAdmission? StudentAdmission { get; set; }

    public virtual Semester? Semester { get; set; }
    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; }
    public virtual ICollection<ExamSubjectRegistrationInternal>? ExamSubjectRegistrationInternals { get; set; }
}
