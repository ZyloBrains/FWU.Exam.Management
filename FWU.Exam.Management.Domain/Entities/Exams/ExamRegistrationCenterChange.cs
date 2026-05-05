namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamRegistrationCenterChange

{

    public int Id { get; set; }
    public int ExamRegistrationId { get; set; }

    public int PreferredExamCenterId { get; set; }
    public DateTime RequestedTimestamp { get; set; }
    public int? CurrentExamCenterId { get; set; }

    public virtual ExamRegistration? ExamRegistration { get; set; }

    public virtual PreferredExamCenter? PreferredExamCenter { get; set; }
}
