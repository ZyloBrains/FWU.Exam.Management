using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Semesters;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectOffering
{
    public int Id { get; set; }

    public int SubjectCatalogId { get; set; }
    public int ProgramId { get; set; }
    public int SemesterId { get; set; }

    public bool IsCompulsory { get; set; }
    public int DisplayOrder { get; set; }

    public bool HasTheory { get; set; }
    public bool HasPractical { get; set; }
    public bool HasInternal { get; set; }

    public float TheoryFullMarks { get; set; }
    public float TheoryPassMarks { get; set; }
    public float? PracticalFullMarks { get; set; }
    public float? PracticalPassMarks { get; set; }
    public float? InternalTheoryFullMarks { get; set; }
    public float? InternalTheoryPassMarks { get; set; }
    public float? InternalPracticalFullMarks { get; set; }
    public float? InternalPracticalPassMarks { get; set; }

    public virtual SubjectCatalog? SubjectCatalog { get; set; }
    public virtual Program? Program { get; set; }
    public virtual Semester? Semester { get; set; }
}
