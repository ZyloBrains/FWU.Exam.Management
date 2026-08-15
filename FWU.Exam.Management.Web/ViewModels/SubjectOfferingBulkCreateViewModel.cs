using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class SubjectOfferingBulkCreateViewModel
{
    [Display(Name = "Program")]
    [Range(1, int.MaxValue, ErrorMessage = "Program is required.")]
    public int ProgramId { get; set; }

    [Display(Name = "Academic Year")]
    [Range(1, int.MaxValue, ErrorMessage = "Academic Year is required.")]
    public int AcademicYearId { get; set; }

    [Display(Name = "Curriculum Version")]
    public int CurriculumVersionId { get; set; }

    public List<SemesterSubjectOfferingGroup> Semesters { get; set; } = new();

    public List<int> RemovedOfferingIds { get; set; } = new();
}

public class SemesterSubjectOfferingGroup
{
    public int SemesterId { get; set; }

    public string? SemesterName { get; set; }

    public List<SubjectOfferingItemViewModel> Subjects { get; set; } = new();
}

public class SubjectOfferingItemViewModel : IValidatableObject
{
    [Display(Name = "Subject")]
    [Range(1, int.MaxValue, ErrorMessage = "Subject is required.")]
    public int SubjectCatalogId { get; set; }

    [Display(Name = "Compulsory")]
    public bool IsCompulsory { get; set; } = true;

    [Display(Name = "Display Order")]
    [Range(1, int.MaxValue, ErrorMessage = "Display order must be greater than 0.")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Has Theory")]
    public bool HasTheory { get; set; } = true;

    [Display(Name = "Has Practical")]
    public bool HasPractical { get; set; }

    [Display(Name = "Has Internal")]
    public bool HasInternal { get; set; }

    [Display(Name = "Theory Full Marks")]
    [Range(0, float.MaxValue, ErrorMessage = "Theory full marks cannot be negative.")]
    public float TheoryFullMarks { get; set; } = 75;

    [Display(Name = "Theory Pass Marks")]
    [Range(0, float.MaxValue, ErrorMessage = "Theory pass marks cannot be negative.")]
    public float TheoryPassMarks { get; set; } = 27;

    [Display(Name = "Practical Full Marks")]
    [Range(0, float.MaxValue, ErrorMessage = "Practical full marks cannot be negative.")]
    public float? PracticalFullMarks { get; set; }

    [Display(Name = "Practical Pass Marks")]
    [Range(0, float.MaxValue, ErrorMessage = "Practical pass marks cannot be negative.")]
    public float? PracticalPassMarks { get; set; }

    [Display(Name = "Internal Theory Full Marks")]
    [Range(0, float.MaxValue, ErrorMessage = "Internal theory full marks cannot be negative.")]
    public float? InternalTheoryFullMarks { get; set; }

    [Display(Name = "Internal Theory Pass Marks")]
    [Range(0, float.MaxValue, ErrorMessage = "Internal theory pass marks cannot be negative.")]
    public float? InternalTheoryPassMarks { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HasTheory && TheoryPassMarks > TheoryFullMarks)
        {
            yield return new ValidationResult(
                "Theory pass marks cannot exceed theory full marks.",
                new[] { nameof(TheoryPassMarks), nameof(TheoryFullMarks) });
        }

        if (HasPractical)
        {
            if (!PracticalFullMarks.HasValue)
            {
                yield return new ValidationResult(
                    "Practical full marks are required when practical is enabled.",
                    new[] { nameof(PracticalFullMarks) });
            }

            if (!PracticalPassMarks.HasValue)
            {
                yield return new ValidationResult(
                    "Practical pass marks are required when practical is enabled.",
                    new[] { nameof(PracticalPassMarks) });
            }

            if (PracticalFullMarks.HasValue && PracticalPassMarks.HasValue && PracticalPassMarks.Value > PracticalFullMarks.Value)
            {
                yield return new ValidationResult(
                    "Practical pass marks cannot exceed practical full marks.",
                    new[] { nameof(PracticalPassMarks), nameof(PracticalFullMarks) });
            }
        }

        if (HasInternal)
        {
            var hasInternalTheory = InternalTheoryFullMarks.HasValue || InternalTheoryPassMarks.HasValue;

            if (!hasInternalTheory)
            {
                yield return new ValidationResult(
                    "Internal marks are required when internal assessment is enabled.",
                    new[]
                    {
                        nameof(InternalTheoryFullMarks),
                        nameof(InternalTheoryPassMarks)
                    });
            }

            if (InternalTheoryFullMarks.HasValue ^ InternalTheoryPassMarks.HasValue)
            {
                yield return new ValidationResult(
                    "Both internal theory full marks and pass marks must be provided together.",
                    new[] { nameof(InternalTheoryFullMarks), nameof(InternalTheoryPassMarks) });
            }

            if (InternalTheoryFullMarks.HasValue && InternalTheoryPassMarks.HasValue && InternalTheoryPassMarks.Value > InternalTheoryFullMarks.Value)
            {
                yield return new ValidationResult(
                    "Internal theory pass marks cannot exceed internal theory full marks.",
                    new[] { nameof(InternalTheoryPassMarks), nameof(InternalTheoryFullMarks) });
            }
        }
    }
}
