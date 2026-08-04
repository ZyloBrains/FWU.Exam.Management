using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities;

public class ResultRecord : ITenantScoped
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Tenant")]
    public int TenantId { get; set; }

    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Program")]
    public int ProgramsId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Type")]
    public int ExamTypeId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "College")]
    public int CollegeId { get; set; }

    [Required, MaxLength(3)]
    [Display(Name = "Year")]
    public string Year { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    [Display(Name = "Part")]
    public string Part { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "Registration Number")]
    public string? RegistrationNumber { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Symbol Number")]
    public string SymbolNumber { get; set; } = string.Empty;

    [MaxLength(1)]
    [Display(Name = "Alphabet")]
    public string? Alphabet { get; set; }

    [Required, MaxLength(10)]
    [Display(Name = "Date of Birth (BS)")]
    public string DateOfBirthBs { get; set; } = string.Empty;

    [MaxLength(10)]
    [Display(Name = "Sex")]
    public string? Sex { get; set; }

    [MaxLength(5)]
    [Display(Name = "Theory Obtained Marks")]
    public string? TheoryObtainedMarks { get; set; }

    [MaxLength(5)]
    [Display(Name = "Internal Obtained Marks")]
    public string? InternalObtainedMarks { get; set; }

    [MaxLength(5)]
    [Display(Name = "Practical Obtained Marks")]
    public string? PracticalObtainedMarks { get; set; }

    [MaxLength(5)]
    [Display(Name = "Theory Obtained Grade")]
    public string? TheoryObtainedGrade { get; set; }

    [MaxLength(5)]
    [Display(Name = "Internal Obtained Grade")]
    public string? InternalObtainedGrade { get; set; }

    [MaxLength(5)]
    [Display(Name = "Practical Obtained Grade")]
    public string? PracticalObtainedGrade { get; set; }

    [MaxLength(5)]
    [Display(Name = "Total Obtained Marks")]
    public string? TotalObtainedMarks { get; set; }

    [MaxLength(5)]
    [Display(Name = "Total Obtained Grade")]
    public string? TotalObtainedGrade { get; set; }

    [MaxLength(5)]
    [Display(Name = "Total Grade Points")]
    public string? TotalGradePoints { get; set; }

    [MaxLength(4)]
    [Display(Name = "GPA")]
    public string? Gpa { get; set; }

    [MaxLength(50)]
    [Display(Name = "Result")]
    public string? Result { get; set; }

    [MaxLength(255)]
    [Display(Name = "Student Name")]
    public string? StudentName { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Result Record Master")]
    public int ResultRecordMasterId { get; set; }

    [Display(Name = "Exam Schedule")]
    public int? ExamScheduleId { get; set; }

    [Display(Name = "Created Date")]
    public DateTime? CreatedDate { get; set; }

    [Display(Name = "Is Published")]
    public bool IsPublished { get; set; }

    [Display(Name = "Published Date")]
    public DateTime? PublishedDate { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual Program? Program { get; set; }
    public virtual ExamType? ExamType { get; set; }
    public virtual College? College { get; set; }
    public virtual ExamSchedule? ExamSchedule { get; set; }
}
