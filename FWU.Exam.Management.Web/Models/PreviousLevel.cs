using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class PreviousLevel
    {
        [Key]
        public int PreviousLevelId { get; set; }

        [Required, MaxLength(100)]
        public string PreviousLevelName { get; set; }

        public int? LevelId { get; set; }
        public int? LevelDisplayOrder { get; set; }

        [MaxLength(1024)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(LevelId))]
        public virtual Level Level { get; set; }

        public virtual ICollection<SchoolType> SchoolTypes { get; set; }
        public virtual ICollection<StudentQualification> StudentQualifications { get; set; }
    }
}
