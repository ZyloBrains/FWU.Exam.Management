using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Board
{
    public int Id { get; set; }
        public int CountryId { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Board Name")]
    public string? BoardName { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
        public virtual ICollection<Program>? Programs { get; set; }
        public virtual ICollection<StudentQualification>? StudentQualifications { get; set; }
}
