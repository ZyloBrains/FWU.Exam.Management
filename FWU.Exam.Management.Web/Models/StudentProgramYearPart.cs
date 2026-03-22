using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class StudentProgramYearPart
    {
        [Key]
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
        [ValidateNever]
        public virtual StudentAdmission StudentAdmission { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        [ValidateNever]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(YearPartId))]
        [ValidateNever]
        public virtual YearPart YearPart { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamSubjectRegistrationInternal> ExamSubjectRegistrationInternals { get; set; }
    }
}
