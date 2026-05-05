using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Board
{
    public int Id { get; set; }
        public int CountryId { get; set; }

    [Required, MaxLength(50)]
    public string? BoardName { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }
        public virtual ICollection<Program>? Programs { get; set; }
        public virtual ICollection<StudentQualification>? StudentQualifications { get; set; }
}
