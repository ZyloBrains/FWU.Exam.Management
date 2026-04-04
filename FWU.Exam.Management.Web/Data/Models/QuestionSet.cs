using fwu_examination_management_system.Data.Models.Colleges;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class QuestionSet
{
    public int Id { get; set; }
    [Required, MaxLength(255)]
    public string QuestionSetName { get; set; }
    [MaxLength(1024)]
    public string Description { get; set; }
    public bool IsActive { get; set; }

    [ValidateNever]
    public virtual ICollection<College> Colleges { get; set; }
}
