using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamFormFeeName
{
    public int Id { get; set; }

    [Required, MaxLength(400)]
    public string? Name { get; set; }

    public bool? IsCollegeFee { get; set; }
    public virtual ICollection<ExamFormFeeRate>? ExamFormFeeRates { get; set; }
}
