using System.Collections.Generic;

namespace FWU.Exam.Management.Application.DTOs;

public class EntranceExamApplicationSelectListsDto
{
    public List<SelectOption>? AcademicYears { get; set; }
    public List<SelectOption>? Colleges { get; set; }
    public List<SelectOption>? Programs { get; set; }
    public List<SelectOption>? Genders { get; set; }
    public List<SelectOption>? PreviousLevels { get; set; }
    public List<SelectOption>? Provinces { get; set; }
    public List<SelectOption>? Districts { get; set; }
    public List<SelectOption>? LocalLevels { get; set; }
}
