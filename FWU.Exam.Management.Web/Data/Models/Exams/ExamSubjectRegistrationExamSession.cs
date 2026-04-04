namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamSubjectRegistrationExamSession
{
    public int Id { get; set; }
    public int ExamSubjectRegistrationId { get; set; }

    public DateTime ExamStartedDateTime { get; set; }
    public bool IsSubmitted { get; set; }
    public decimal? ObtainedMarks { get; set; }
    public DateTime? ExamSubmittedDateTime { get; set; }
    public bool? IsAutoSubmitted { get; set; }
    public DateTime LastStatusSyncDateTime { get; set; }

    public virtual ExamSubjectRegistration? ExamSubjectRegistration { get; set; }
}
