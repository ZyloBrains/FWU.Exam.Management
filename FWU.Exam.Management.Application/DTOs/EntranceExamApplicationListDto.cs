using System;
using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Application.DTOs;

public class EntranceExamApplicationListDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? ContactNumber { get; set; }
    public string? AcademicYear { get; set; }
    public string? College { get; set; }
    public string? Program { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
