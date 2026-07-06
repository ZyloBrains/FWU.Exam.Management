using System.Collections.Generic;

namespace FWU.Exam.Management.Application.DTOs;

public class ExamSubjectResultSelectListsDto
{
    public List<SelectOption> ExamRegistrations { get; set; } = [];
    public List<SelectOption> SubjectOfferings { get; set; } = [];
    public List<SelectOption> ExamTypes { get; set; } = [];
    public List<SelectOption> ExamSchedules { get; set; } = [];
}
