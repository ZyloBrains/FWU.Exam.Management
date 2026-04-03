using fwu_examination_management_system.Data.Models.Exams;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Colleges;

public class CollegeType
{
    [Key]
    public int CollegeTypeId { get; set; }

    [Required, MaxLength(2)]
    public string CollegeTypeCode { get; set; }

    [Required, MaxLength(50)]
    public string CollegeTypeName { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }

    public bool? IsDefault { get; set; }
    public bool IsActive { get; set; }

    [ValidateNever]
    public virtual ICollection<College> Colleges { get; set; }

    [ValidateNever]
    public virtual ICollection<ExamFormFeeRate> ExamFormFeeRates { get; set; }
}
