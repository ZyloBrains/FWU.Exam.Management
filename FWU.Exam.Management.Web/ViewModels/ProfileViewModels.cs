namespace FWU.Exam.Management.Web.ViewModels;

public class ProfileBaseViewModel
{
    public string? UserId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? RoleLabel { get; set; }
    public string? Designation { get; set; }
    public string? ProfilePath { get; set; }
    public string? SignaturePath { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public List<string> Roles { get; set; } = [];
    public string? TenantCode { get; set; }
    public string? TenantName { get; set; }
    public string? TenantLogo { get; set; }
    public string? OrganizationName { get; set; }
    public string? OrganizationLogo { get; set; }
    public string? CoverImagePath { get; set; }
    public bool CanUploadSignature { get; set; }
}

public class SuperAdminProfileViewModel : ProfileBaseViewModel
{
    public int TotalTenants { get; set; }
    public int TotalUsers { get; set; }
    public int TotalColleges { get; set; }
    public int TotalStudents { get; set; }
    public int TotalPrograms { get; set; }
    public int ActiveExamSchedules { get; set; }
}

public class FacultyAdminProfileViewModel : ProfileBaseViewModel
{
    public string? FacultyName { get; set; }
    public string? FacultyShortName { get; set; }
    public string? OfficeCode { get; set; }
    public string? FacultyContactNumber { get; set; }
    public string? FacultyAddress { get; set; }
    public string? FacultyEmail { get; set; }
    public int CollegeCount { get; set; }
    public int ProgramCount { get; set; }
    public int StaffCount { get; set; }
    public int ActiveExamScheduleCount { get; set; }
}

public class CollegeAdminProfileViewModel : ProfileBaseViewModel
{
    public string? CollegeName { get; set; }
    public string? CollegeCode { get; set; }
    public string? CollegeShortName { get; set; }
    public string? CollegeEmail { get; set; }
    public string? CollegePhone { get; set; }
    public string? CollegeAddress { get; set; }
    public string? CollegeType { get; set; }
    public string? CollegeWebsite { get; set; }
    public string? PrincipalName { get; set; }
    public int ProgramCount { get; set; }
    public int StudentCount { get; set; }
    public int StaffCount { get; set; }
}

public class StudentProfileDetailViewModel : ProfileBaseViewModel
{
    public int RegistrationId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? NepaliName { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirthBS { get; set; }
    public string? DateOfBirthAD { get; set; }
    public string? Ethnicity { get; set; }
    public string? Category { get; set; }
    public string? BloodGroup { get; set; }
    public string? Nationality { get; set; }
    public string? Religion { get; set; }
    public string? Address { get; set; }
    public string? AcademicYear { get; set; }
    public string? College { get; set; }
    public string? Level { get; set; }
    public string? Program { get; set; }
    public string? ProgramCode { get; set; }
    public string? AdmissionDate { get; set; }
    public string? CollegeRollNumber { get; set; }
    public string? CurrentSemester { get; set; }
    public List<StudentGuardianProfileViewModel> Guardians { get; set; } = [];
    public List<StudentQualificationProfileViewModel> Qualifications { get; set; } = [];
    public int ExamRegistrationCount { get; set; }
    public int AdmitCardCount { get; set; }
    public int PaymentCount { get; set; }
}

public class StudentGuardianProfileViewModel
{
    public string? Relation { get; set; }
    public string? Name { get; set; }
    public string? ContactNumber { get; set; }
    public string? Occupation { get; set; }
}

public class StudentQualificationProfileViewModel
{
    public string? InstituteName { get; set; }
    public string? ProgramName { get; set; }
    public string? PassedYear { get; set; }
    public string? Percentage { get; set; }
}
