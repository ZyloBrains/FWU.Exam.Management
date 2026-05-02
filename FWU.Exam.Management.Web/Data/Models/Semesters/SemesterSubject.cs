using fwu_examination_management_system.Data.Models.Subjects;

namespace fwu_examination_management_system.Data.Models.Semesters;

public class SemesterSubject
{
    public int SemesterId { get; set; }
    public int SubjectDetailId { get; set; }

    public virtual Semester? Semester { get; set; }
    public virtual SubjectDetail? SubjectDetail { get; set; }
}
