using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Colleges;

public class CollegeProgram
{
    public int Id { get; set; }

    public DateTime? AffiliationDate { get; set; }
    public int NumberOfStudents { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }

    public int CollegeId { get; set; }
    public virtual College? College { get; set; }

    public int ProgramId { get; set; }
    public virtual Programs? Program { get; set; }
}
