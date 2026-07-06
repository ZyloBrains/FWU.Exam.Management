using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Ethnicity
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Ethnicity Name")]
    public string? EthnicityName { get; set; }

    [Display(Name = "Is Default")]
    public bool IsDefault { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

        public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
