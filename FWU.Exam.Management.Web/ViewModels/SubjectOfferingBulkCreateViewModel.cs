namespace FWU.Exam.Management.Web.ViewModels;

public class SubjectOfferingBulkCreateViewModel
{
    public int ProgramId { get; set; }
    public int SemesterId { get; set; }
    public List<SubjectOfferingItemViewModel> Subjects { get; set; } = new();
}

public class SubjectOfferingItemViewModel
{
    public int SubjectCatalogId { get; set; }
    public bool IsCompulsory { get; set; } = true;
    public int DisplayOrder { get; set; }
    public bool HasTheory { get; set; } = true;
    public bool HasPractical { get; set; }
    public bool HasInternal { get; set; }
    public float TheoryFullMarks { get; set; } = 75;
    public float TheoryPassMarks { get; set; } = 27;
    public float? PracticalFullMarks { get; set; }
    public float? PracticalPassMarks { get; set; }
    public float? InternalTheoryFullMarks { get; set; }
    public float? InternalTheoryPassMarks { get; set; }
    public float? InternalPracticalFullMarks { get; set; }
    public float? InternalPracticalPassMarks { get; set; }
}
