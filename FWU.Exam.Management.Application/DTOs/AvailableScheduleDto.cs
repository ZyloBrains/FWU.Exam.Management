using System;

namespace FWU.Exam.Management.Application.DTOs;

public class AvailableScheduleDto
{
    public int Id { get; set; }
    public string? ExamScheduleName { get; set; }
    public string? ProgramName { get; set; }
    public string? CollegeName { get; set; }
    public string? AcademicYearName { get; set; }
    public decimal? ExamFee { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? StartDateBs { get; set; }
    public string? EndDateBs { get; set; }
    public string? SemesterName { get; set; }
}
