using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamType
{
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [Required, MaxLength(30)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;
}

