using System.Collections.Generic;

namespace FWU.Exam.Management.Application.DTOs;

public class ExamRegistrationSelectListsDto
{
    public List<SelectOption> ExamSchedules { get; set; } = [];
    public List<SelectOption> Colleges { get; set; } = [];
    public List<SelectOption> AcademicYears { get; set; } = [];
    public List<SelectOption> Programs { get; set; } = [];
    public List<SelectOption> ExamCenters { get; set; } = [];
}
