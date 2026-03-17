using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{

    public class SubjectDetail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubjectDetailId { get; set; }

        public int? SubjectGroupId { get; set; }
        public int ProgramId { get; set; }
        public int YearPartId { get; set; }

        [Required, MaxLength(50)]
        public string SubjectCode { get; set; }

        [Required, MaxLength(100)]
        public string SubjectName { get; set; }

        public decimal TheoryFullMarks { get; set; }
        public decimal TheoryPassMarks { get; set; }
        public decimal? PracticalFullMarks { get; set; }
        public decimal? PracticalPassMarks { get; set; }
        public decimal? InternalTheoryFullMarks { get; set; }
        public decimal? InternalTheoryPassMarks { get; set; }
        public decimal? InternalPracticalFullMarks { get; set; }
        public decimal? InternalPracticalPassMarks { get; set; }
        public int? CreditHours { get; set; }
        public bool HasPractical { get; set; }
        public bool HasInternal { get; set; }
        public int? DisplayOrder { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool IsCompulsory { get; set; }

        [MaxLength(50)]
        public string ShortName { get; set; }

        [MaxLength(50)]
        public string ConcurrentSubjectCode { get; set; }

        public int SubjectTypeId { get; set; }
        public bool HasTheory { get; set; }

        [MaxLength(50)]
        public string Year { get; set; }

        [MaxLength(50)]
        public string Part { get; set; }

        [ForeignKey(nameof(SubjectGroupId))]
        public virtual SubjectGroup SubjectGroup { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual Programs Program { get; set; }

        [ForeignKey(nameof(YearPartId))]
        public virtual YearPart YearPart { get; set; }

        [ForeignKey(nameof(SubjectTypeId))]
        public virtual SubjectType SubjectType { get; set; }

        public virtual ICollection<ExamScheduleDetail> ExamScheduleDetails { get; set; }
        public virtual ICollection<ExamSubjectRegistration> ExamSubjectRegistrations { get; set; }
        public virtual ICollection<ExamSubjectRegistrationInternal> ExamSubjectRegistrationInternals { get; set; }
        public virtual ICollection<ResultRecord> ResultRecords { get; set; }
        public virtual ICollection<SubjectGroupDetailMap> SubjectGroupDetailMaps { get; set; }
    }
}
