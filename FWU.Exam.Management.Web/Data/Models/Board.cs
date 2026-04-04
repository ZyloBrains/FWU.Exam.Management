using fwu_examination_management_system.Data.Models.Students;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class Board
{
    public int Id { get; set; }
    [ValidateNever]
    public int CountryId { get; set; }

    [Required, MaxLength(50)]
    public string BoardName { get; set; }

    [MaxLength(255)]
    public string Remarks { get; set; }

    public bool IsActive { get; set; }
    [ValidateNever]
    public virtual ICollection<Programs> Programs { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentQualification> StudentQualifications { get; set; }
}
