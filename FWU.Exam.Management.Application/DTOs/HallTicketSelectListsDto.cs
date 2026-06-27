namespace FWU.Exam.Management.Application.DTOs;

public class HallTicketSelectListsDto
{
    public List<SelectOption> ExamSchedules { get; set; } = [];
    public List<SelectOption> ExamRegistrations { get; set; } = [];
    public List<SelectOption> ExamCenters { get; set; } = [];
}
