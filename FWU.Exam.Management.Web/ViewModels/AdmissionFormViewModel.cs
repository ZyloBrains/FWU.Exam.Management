using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class AdmissionFormViewModel
{
    [Required(ErrorMessage = "Please select a programme.")]
    [Display(Name = "Programs")]
    public int? ProgrammeId { get; set; }

    [Required(ErrorMessage = "Please select a college.")]
    [Display(Name = "College / Campus")]
    public int CollegeId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(150)]
    [Display(Name = "Name (In BLOCK LETTER)")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Birth Date BS is required.")]
    [MaxLength(10)]
    [Display(Name = "Birth Date BS.")]
    public string DobBS { get; set; } = string.Empty;

    [Required(ErrorMessage = "Birth Date AD is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Birth Date AD.")]
    public DateTime? DobAD { get; set; }

    [Required(ErrorMessage = "Gender is required.")]
    [Display(Name = "Gender")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Citizenship No is required.")]
    [MaxLength(50)]
    [Display(Name = "Citizenship No")]
    public string CitizenshipNo { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "National ID")]
    public string? NationalId { get; set; }

    [Display(Name = "District")]
    public string? CitizenshipDistrict { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of Issue")]
    public DateTime? CitizenshipIssueDate { get; set; }

    [Display(Name = "Blood Group")]
    public string? BloodGroup { get; set; }

    [Display(Name = "Birth Place")]
    public string? BirthPlace { get; set; }

    [Display(Name = "Upload Latest Passport Size Photo")]
    [Required(ErrorMessage = "Please upload a photo.")]
    public IFormFile? Photo { get; set; }

    [Required(ErrorMessage = "Municipality is required.")]
    [MaxLength(150)]
    [Display(Name = "Municipality")]
    public string PermMunicipality { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ward No. is required.")]
    [MaxLength(10)]
    [Display(Name = "Ward No.")]
    public string PermWard { get; set; } = string.Empty;

    [Required(ErrorMessage = "District is required.")]
    [MaxLength(100)]
    [Display(Name = "District")]
    public string PermDistrict { get; set; } = string.Empty;

    [Required(ErrorMessage = "Province is required.")]
    [MaxLength(100)]
    [Display(Name = "Province")]
    public string PermProvince { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required.")]
    [MaxLength(100)]
    [Display(Name = "Country")]
    public string PermCountry { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Postal Code")]
    public string? PermPostalCode { get; set; }

    [MaxLength(20)]
    [Display(Name = "Phone Land-Line")]
    public string? PhoneLandline { get; set; }

    [Required(ErrorMessage = "Mobile No. is required.")]
    [MaxLength(20)]
    [Display(Name = "Mobile No.")]
    public string Mobile { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [MaxLength(100)]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Father Name is required.")]
    [MaxLength(150)]
    [Display(Name = "Father Name")]
    public string FatherName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Father Phone is required.")]
    [MaxLength(20)]
    [Display(Name = "Father Phone")]
    public string FatherPhone { get; set; } = string.Empty;

    [MaxLength(150)]
    [Display(Name = "Father Profession")]
    public string? FatherProfession { get; set; }

    [MaxLength(150)]
    [Display(Name = "Mother Name")]
    public string? MotherName { get; set; }

    [MaxLength(20)]
    [Display(Name = "Mother Phone")]
    public string? MotherPhone { get; set; }

    [MaxLength(100)]
    [EmailAddress]
    [Display(Name = "Guardian Email")]
    public string? GuardianEmail { get; set; }

    [Required]
    public List<AcademicQualificationViewModel> AcademicQualifications { get; set; } = [];

    [Required(ErrorMessage = "Please upload self-attested documents.")]
    [Display(Name = "Documents (Transcript, Character, etc.)")]
    public IFormFile? DocumentsFile { get; set; }

    [Required(ErrorMessage = "Please upload bank voucher.")]
    [Display(Name = "Upload Bank Voucher")]
    public IFormFile? BankVoucherFile { get; set; }
}

public class AcademicQualificationViewModel
{
    [Display(Name = "Examination")]
    public string? Level { get; set; }

    [Display(Name = "Institution/Board/University")]
    public string? BoardUniversity { get; set; }

    [Display(Name = "Symbol No.")]
    public string? SymbolNo { get; set; }

    [Display(Name = "Year")]
    public string? Year { get; set; }

    [Display(Name = "Percent/CGPA")]
    public string? PercentCGPA { get; set; }

    [Display(Name = "Division")]
    public string? Division { get; set; }
}
