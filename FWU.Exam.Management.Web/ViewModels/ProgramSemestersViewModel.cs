using System.Collections.Generic;

namespace FWU.Exam.Management.Web.ViewModels;

public class ProgramSemestersViewModel
{
    public int ProgramId { get; set; }
    public string? ProgramCode { get; set; }
    public string? ProgramName { get; set; }
    public List<int> AssignedSemesterIds { get; set; } = new();
    public List<ProgramSemesterGroup> Groups { get; set; } = new();
}

public class ProgramSemesterGroup
{
    public string? Title { get; set; }
    public List<ProgramSemesterItem> Semesters { get; set; } = new();
}

public class ProgramSemesterItem
{
    public int SemesterId { get; set; }
    public string? Display { get; set; }
    public string? FacultyName { get; set; }
}
