using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class EntryFormat
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Entry Format Name")]
    public string EntryFormatName { get; set; } = string.Empty;

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; } = [];
}
