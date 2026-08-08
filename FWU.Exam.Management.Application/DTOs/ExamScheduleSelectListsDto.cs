using System.Collections.Generic;

namespace FWU.Exam.Management.Application.DTOs;

public class ExamScheduleSelectListsDto
{
    public List<SelectOption> AcademicYears { get; set; } = [];
    public List<SelectOption> ExamTypes { get; set; } = [];
    public List<SelectOption> Programs { get; set; } = [];
    public List<SelectOption> Semesters { get; set; } = [];
    public List<SelectOption> CurriculumVersions { get; set; } = [];
}
