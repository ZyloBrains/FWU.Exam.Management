using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Ethnicity
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string? EthnicityName { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }

        public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
