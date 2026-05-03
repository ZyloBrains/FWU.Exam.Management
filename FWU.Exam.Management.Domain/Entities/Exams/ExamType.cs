using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamType
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public int Code { get; set; }
}
