using System.Collections.Generic;

namespace FWU.Exam.Management.Application.DTOs;

public class StudentRegistrationSelectListsDto
{
    public List<SelectOption> AcademicYears { get; set; } = [];
    public List<SelectOption> Levels { get; set; } = [];
    public List<SelectOption> Faculties { get; set; } = [];
    public List<SelectOption> Colleges { get; set; } = [];
    public List<SelectOption> Genders { get; set; } = [];
    public List<SelectOption> StudentCategories { get; set; } = [];
    public List<SelectOption> Ethnicities { get; set; } = [];
    public List<SelectOption> LocalLevels { get; set; } = [];
}
