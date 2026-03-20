using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Batch
    {
        [Key]
        public int BatchId { get; set; }

        public int AcademicYearId { get; set; }

        [Required, MaxLength(50)]
        public string BatchName { get; set; }

        [MaxLength(50)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; }
        public virtual ICollection<ExamScheduleBatch> ExamScheduleBatches { get; set; }
    }
}
