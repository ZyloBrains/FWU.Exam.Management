using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Application.DTOs;

/// <summary>
/// A subject a student failed, together with the specific exam legs
/// (theory/practical) that were failed — drives the partial-form
/// per-leg selection.
/// </summary>
public class FailedSubjectOption
{
    public SubjectOffering Offering { get; set; } = null!;
    public Helpers.ReExamLegs FailedLegs { get; set; }

    public int SubjectOfferingId => Offering.Id;
}
