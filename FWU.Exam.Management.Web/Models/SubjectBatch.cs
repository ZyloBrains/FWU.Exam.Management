using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class SubjectBatch
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubjectBatchId { get; set; }

        [Required, MaxLength(255)]
        public string SubjectBatchName { get; set; }

        public int EffectiveAcademicYearId { get; set; }
        public int ProgramId { get; set; }

        [MaxLength(1024)]
        public string Remarks { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(EffectiveAcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual Programs Program { get; set; }
    }
}
