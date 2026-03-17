using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class StudentAdmission
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentAdmissionId { get; set; }

        public int BatchId { get; set; }
        public int StudentRegistrationId { get; set; }
        public int ProgramId { get; set; }
        public int CollegeId { get; set; }
        public int? SectionId { get; set; }
        public DateTime AdmissionDate { get; set; }
        public int? CheckedBy { get; set; }
        public bool IsCompleted { get; set; }

        [MaxLength(50)]
        public string Cgpa { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [MaxLength(50)]
        public string CollegeRollNumber { get; set; }

        public int? RepeatBatchId { get; set; }
        public int? SubjectGroupId { get; set; }
        public bool HasFeeExemption { get; set; }

        [ForeignKey(nameof(BatchId))]
        public virtual Batch Batch { get; set; }

        [ForeignKey(nameof(StudentRegistrationId))]
        public virtual StudentRegistration StudentRegistration { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual Programs Program { get; set; }

        [ForeignKey(nameof(CollegeId))]
        public virtual College College { get; set; }

        [ForeignKey(nameof(SectionId))]
        public virtual Section Section { get; set; }

        [ForeignKey(nameof(SubjectGroupId))]
        public virtual SubjectGroup SubjectGroup { get; set; }

        public virtual ICollection<StudentProgramYearPart> StudentProgramYearParts { get; set; }
    }
}
