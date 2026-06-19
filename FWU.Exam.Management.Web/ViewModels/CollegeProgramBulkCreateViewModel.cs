using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class CollegeProgramBulkCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "College is required.")]
    public int CollegeId { get; set; }

    public List<CollegeProgramItemViewModel> Programs { get; set; } = new();
}

public class CollegeProgramItemViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Program is required.")]
    public int ProgramId { get; set; }

    public DateTime? AffiliationDate { get; set; }

    [Range(0, int.MaxValue)]
    public int NumberOfStudents { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; } = true;
}
