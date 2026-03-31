using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamSubjectRegistrationInternal:AuditBase
    {
        [Key]
        public int ExamSubjectRegistrationInternalId { get; set; }

        public int EntryAcademicYearId { get; set; }
        public int StudentProgramYearPartId { get; set; }
        public int SubjectDetailId { get; set; }
        public decimal? ObtainedMarksTheoryInternal { get; set; }
        public decimal? ObtainedMarksPracticalInternal { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; }
     
        public int? ExamScheduleId { get; set; }

        [ForeignKey(nameof(EntryAcademicYearId))]
        [ValidateNever]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(StudentProgramYearPartId))]
        [ValidateNever]
        public virtual StudentProgramYearPart StudentProgramYearPart { get; set; }

        [ForeignKey(nameof(SubjectDetailId))]
        [ValidateNever]
        public virtual SubjectDetail SubjectDetail { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        [ValidateNever]
        public virtual ExamSchedule ExamSchedule { get; set; }
    }
}
