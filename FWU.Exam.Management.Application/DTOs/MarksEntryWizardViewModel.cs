namespace FWU.Exam.Management.Application.DTOs;

public enum MarksEntryMode
{
    Internal,
    Theory,
    Practical
}

public class MarksEntryWizardViewModel
{
    public MarksEntryMode Mode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-pen-alt";
    public string ControllerBase { get; set; } = string.Empty;
    public string SaveAction { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public bool IsFacultyAdmin { get; set; }
    public bool IsCollegeAdmin { get; set; }
    public List<SelectOption> Faculties { get; set; } = [];
    public List<SelectOption> Colleges { get; set; } = [];
}
