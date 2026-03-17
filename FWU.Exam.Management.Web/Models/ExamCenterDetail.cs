using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamCenterDetail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamCenterDetailId { get; set; }

        public int ExamCenterId { get; set; }
        public int CollegeId { get; set; }
        public int? ProgramId { get; set; }
        public long RollNumberFrom { get; set; }
        public long RollNumberTo { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(ExamCenterId))]
        public virtual ExamCenter ExamCenter { get; set; }

        [ForeignKey(nameof(CollegeId))]
        public virtual College College { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual Programs Program { get; set; }
    }
}
