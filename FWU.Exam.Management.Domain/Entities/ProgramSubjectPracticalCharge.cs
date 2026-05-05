namespace FWU.Exam.Management.Domain.Entities;

public class ProgramSubjectPracticalCharge
{
    public int Id { get; set; }

    public int ProgramsId { get; set; }
    public decimal PracticalSubjectCharge { get; set; }

    public virtual Program? Program { get; set; }
}
