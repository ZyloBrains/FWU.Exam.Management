namespace fwu_examination_management_system.Data.Models;

public class ProgramYearPart
{
    public int Id { get; set; }

    public int ProgramsId { get; set; }
    public int TotalMarks { get; set; }
    public int TotalPassMarks { get; set; }

    public virtual Program? Program { get; set; }
}
