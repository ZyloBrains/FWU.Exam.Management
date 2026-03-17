using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamSubjectRegistration
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamSubjectRegistrationId { get; set; }

        public int ExamRegistrationId { get; set; }
        public int SubjectDetailId { get; set; }
        public int ExamTypeId { get; set; }

        [MaxLength(3)]
        public string ObtainedMarksTheory { get; set; }

        [MaxLength(3)]
        public string ObtainedMarksTheoryConfirm { get; set; }

        [MaxLength(3)]
        public string ObtainedMarksPractical { get; set; }

        [MaxLength(3)]
        public string ObtainedMarksPracticalConfirm { get; set; }

        [MaxLength(3)]
        public string GradeLetter { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public bool? IsLooseEntry { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedByTab1 { get; set; }
        public DateTime? CreatedDateTab1 { get; set; }
        public int? ModifiedByTab1 { get; set; }
        public DateTime? ModifiedDateTab1 { get; set; }
        public int? CreatedByTab2 { get; set; }
        public DateTime? CreatedDateTab2 { get; set; }
        public int? ModifiedByTab2 { get; set; }
        public DateTime? ModifiedDateTab2 { get; set; }
        public bool? IsTheoryRegistered { get; set; }
        public bool? IsPracticalRegistered { get; set; }
        public bool? IsExtra { get; set; }

        [ForeignKey(nameof(ExamRegistrationId))]
        public virtual ExamRegistration ExamRegistration { get; set; }

        [ForeignKey(nameof(SubjectDetailId))]
        public virtual SubjectDetail SubjectDetail { get; set; }

        [ForeignKey(nameof(ExamTypeId))]
        public virtual ExamType ExamType { get; set; }

        public virtual ExamSubjectRegistrationExamSession ExamSubjectRegistrationExamSession { get; set; }
    }
}
