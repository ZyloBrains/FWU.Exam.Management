using FWU.Exam.Management.Domain.Entities.Colleges;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class EntranceSchedule
{
    public int Id { get; set; }

    public int AcademicYearId { get; set; }
    public int ProgramId { get; set; }
    public int CollegeId { get; set; }

    public DateTime FormOpenDate { get; set; }
    public DateTime FormCloseDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual Program? Program { get; set; }
    public virtual College? College { get; set; }
}
