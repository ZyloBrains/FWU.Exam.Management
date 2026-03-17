using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class StudentProgramYearPart
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentProgramYearPartId { get; set; }

        public int StudentAdmissionId { get; set; }
        public int AcademicYearId { get; set; }
        public int YearPartId { get; set; }
        public bool IsRunning { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(StudentAdmissionId))]
        public virtual StudentAdmission StudentAdmission { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(YearPartId))]
        public virtual YearPart YearPart { get; set; }

        public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; }
        public virtual ICollection<ExamSubjectRegistrationInternal> ExamSubjectRegistrationInternals { get; set; }
    }
}
