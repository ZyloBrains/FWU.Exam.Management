using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class CollegeProgramBulkCreateViewModel
{
    [Display(Name = "College")]
    [Range(1, int.MaxValue, ErrorMessage = "College is required.")]
    public int CollegeId { get; set; }

    public List<CollegeProgramItemViewModel> Programs { get; set; } = new();
}

public class CollegeProgramItemViewModel
{
    [Display(Name = "Program")]
    [Range(1, int.MaxValue, ErrorMessage = "Program is required.")]
    public int ProgramId { get; set; }

    [Display(Name = "Affiliation Date")]
    public DateTime? AffiliationDate { get; set; }

    [Display(Name = "Number Of Students")]
    [Range(0, int.MaxValue)]
    public int NumberOfStudents { get; set; }

    [Display(Name = "Remarks")]
    [MaxLength(1024)]
    public string? Remarks { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
