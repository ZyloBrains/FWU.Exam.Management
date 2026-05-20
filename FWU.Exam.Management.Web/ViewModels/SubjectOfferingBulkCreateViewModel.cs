using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class SubjectOfferingBulkCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Program is required.")]
    public int ProgramId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Semester is required.")]
    public int SemesterId { get; set; }

    public List<SubjectOfferingItemViewModel> Subjects { get; set; } = new();
}

public class SubjectOfferingItemViewModel : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Subject is required.")]
    public int SubjectCatalogId { get; set; }

    public bool IsCompulsory { get; set; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "Display order must be greater than 0.")]
    public int DisplayOrder { get; set; }

    public bool HasTheory { get; set; } = true;
    public bool HasPractical { get; set; }
    public bool HasInternal { get; set; }

    [Range(0, float.MaxValue, ErrorMessage = "Theory full marks cannot be negative.")]
    public float TheoryFullMarks { get; set; } = 75;

    [Range(0, float.MaxValue, ErrorMessage = "Theory pass marks cannot be negative.")]
    public float TheoryPassMarks { get; set; } = 27;

    [Range(0, float.MaxValue, ErrorMessage = "Practical full marks cannot be negative.")]
    public float? PracticalFullMarks { get; set; }

    [Range(0, float.MaxValue, ErrorMessage = "Practical pass marks cannot be negative.")]
    public float? PracticalPassMarks { get; set; }

    [Range(0, float.MaxValue, ErrorMessage = "Internal theory full marks cannot be negative.")]
    public float? InternalTheoryFullMarks { get; set; }

    [Range(0, float.MaxValue, ErrorMessage = "Internal theory pass marks cannot be negative.")]
    public float? InternalTheoryPassMarks { get; set; }

    [Range(0, float.MaxValue, ErrorMessage = "Internal practical full marks cannot be negative.")]
    public float? InternalPracticalFullMarks { get; set; }

    [Range(0, float.MaxValue, ErrorMessage = "Internal practical pass marks cannot be negative.")]
    public float? InternalPracticalPassMarks { get; set; }

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
            var hasInternalPractical = InternalPracticalFullMarks.HasValue || InternalPracticalPassMarks.HasValue;

            if (!hasInternalTheory && !hasInternalPractical)
            {
                yield return new ValidationResult(
                    "Internal marks are required when internal assessment is enabled.",
                    new[]
                    {
                        nameof(InternalTheoryFullMarks),
                        nameof(InternalTheoryPassMarks),
                        nameof(InternalPracticalFullMarks),
                        nameof(InternalPracticalPassMarks)
                    });
            }

            if (InternalTheoryFullMarks.HasValue ^ InternalTheoryPassMarks.HasValue)
            {
                yield return new ValidationResult(
                    "Both internal theory full marks and pass marks must be provided together.",
                    new[] { nameof(InternalTheoryFullMarks), nameof(InternalTheoryPassMarks) });
            }

            if (InternalPracticalFullMarks.HasValue ^ InternalPracticalPassMarks.HasValue)
            {
                yield return new ValidationResult(
                    "Both internal practical full marks and pass marks must be provided together.",
                    new[] { nameof(InternalPracticalFullMarks), nameof(InternalPracticalPassMarks) });
            }

            if (InternalTheoryFullMarks.HasValue && InternalTheoryPassMarks.HasValue && InternalTheoryPassMarks.Value > InternalTheoryFullMarks.Value)
            {
                yield return new ValidationResult(
                    "Internal theory pass marks cannot exceed internal theory full marks.",
                    new[] { nameof(InternalTheoryPassMarks), nameof(InternalTheoryFullMarks) });
            }

            if (InternalPracticalFullMarks.HasValue && InternalPracticalPassMarks.HasValue && InternalPracticalPassMarks.Value > InternalPracticalFullMarks.Value)
            {
                yield return new ValidationResult(
                    "Internal practical pass marks cannot exceed internal practical full marks.",
                    new[] { nameof(InternalPracticalPassMarks), nameof(InternalPracticalFullMarks) });
            }
        }
    }
}
