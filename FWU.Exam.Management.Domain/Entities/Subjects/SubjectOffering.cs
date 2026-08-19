using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Subjects;

public class SubjectOffering : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Subject Catalog")]
    public int SubjectCatalogId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Program")]
    public int ProgramId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Semester")]
    public int SemesterId { get; set; }

    [Display(Name = "Curriculum Version")]
    public int? CurriculumVersionId { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Is Compulsory")]
    public bool IsCompulsory { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Has Theory")]
    public bool HasTheory { get; set; }

    [Display(Name = "Has Practical")]
    public bool HasPractical { get; set; }

    [Display(Name = "Has Internal")]
    public bool HasInternal { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Theory Full Marks")]
    public float TheoryFullMarks { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Theory Pass Marks")]
    public float TheoryPassMarks { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Practical Full Marks")]
    public float? PracticalFullMarks { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Practical Pass Marks")]
    public float? PracticalPassMarks { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Internal Theory Full Marks")]
    public float? InternalTheoryFullMarks { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Internal Theory Pass Marks")]
    public float? InternalTheoryPassMarks { get; set; }

    public virtual SubjectCatalog? SubjectCatalog { get; set; }
    public virtual Program? Program { get; set; }
    public virtual Semester? Semester { get; set; }
    public virtual CurriculumVersion? CurriculumVersion { get; set; }
    public virtual ICollection<ExamSlot> ExamSlots { get; set; } = [];
}
