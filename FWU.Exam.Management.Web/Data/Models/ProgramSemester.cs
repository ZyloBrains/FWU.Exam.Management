namespace fwu_examination_management_system.Data.Models;

public class ProgramSemester
{
    public int Id { get; set; }

    public int ProgramsId { get; set; }
    public int SemesterId { get; set; }
    public int TotalMarks { get; set; }
    public int TotalPassMarks { get; set; }

    public virtual Program? Program { get; set; }

    public virtual Semester? Semester { get; set; }
}
