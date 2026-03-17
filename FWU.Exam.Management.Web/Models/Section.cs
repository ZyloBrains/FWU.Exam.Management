using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Section
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SectionId { get; set; }

        [Required, MaxLength(100)]
        public string SectionName { get; set; }

        public int? ProgramId { get; set; }
        public int? BatchId { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual Programs Program { get; set; }

        [ForeignKey(nameof(BatchId))]
        public virtual Batch Batch { get; set; }

        public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; }
    }
}
