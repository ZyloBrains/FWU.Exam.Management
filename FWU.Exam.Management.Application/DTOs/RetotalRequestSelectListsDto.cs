namespace FWU.Exam.Management.Application.DTOs;

public class RetotalRequestSelectListsDto
{
    public List<SelectOption> ExamSchedules { get; set; } = [];
    public List<SelectOption> Students { get; set; } = [];
    public List<SelectOption> Subjects { get; set; } = [];
}
