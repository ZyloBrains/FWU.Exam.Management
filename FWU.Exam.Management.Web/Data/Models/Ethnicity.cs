using fwu_examination_management_system.Data.Models.Students;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class Ethnicity
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string EthnicityName { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }

    [ValidateNever]
    public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
}
