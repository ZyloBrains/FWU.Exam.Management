using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamRegistrationCenterChange

{

    [Key]
    public int ExamRegistrationCenterChangeId { get; set; }
    public int ExamRegistrationId { get; set; }

    public int PreferredExamCenterId { get; set; }
    public DateTime RequestedTimestamp { get; set; }
    public int? CurrentExamCenterId { get; set; }

    [ForeignKey(nameof(ExamRegistrationId))]
    [ValidateNever]
    public virtual ExamRegistration ExamRegistration { get; set; }

    [ForeignKey(nameof(PreferredExamCenterId))]
    [ValidateNever]
    public virtual PreferredExamCenter PreferredExamCenter { get; set; }
}
