using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Infrastructure.Services;

/// <summary>
/// Decides which internal marks survive a re-exam registration.
/// External marks of a re-sat leg are always cleared for fresh entry, but
/// internal marks carry forward only when they met/exceeded the subject's
/// internal pass mark; a below-pass internal is cleared so it can be
/// re-entered for the re-examination.
/// </summary>
public static class InternalMarksCarryForwardPolicy
{
    public static float? ResolveTheoryInternal(float? previousInternal, SubjectOffering offering)
    {
        if (!offering.HasInternal || !previousInternal.HasValue)
            return null;

        return previousInternal >= (offering.InternalTheoryPassMarks ?? 0f)
            ? previousInternal.Value
            : null;
    }
}