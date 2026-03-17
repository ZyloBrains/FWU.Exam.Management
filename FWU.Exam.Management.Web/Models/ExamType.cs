using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamTypeId { get; set; }

        [Required, MaxLength(50)]
        public string ExamTypeName { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int Code { get; set; }

        public virtual ICollection<ExamFormFeeRate> ExamFormFeeRates { get; set; }
        public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
        public virtual ICollection<ExamScheduleBatch> ExamScheduleBatches { get; set; }
        public virtual ICollection<ExamScheduleDetail> ExamScheduleDetails { get; set; }
        public virtual ICollection<ExamSubjectRegistration> ExamSubjectRegistrations { get; set; }
    }
}
