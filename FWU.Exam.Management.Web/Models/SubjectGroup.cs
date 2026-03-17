using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class SubjectGroup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubjectGroupId { get; set; }

        public int ProgramId { get; set; }

        [Required, MaxLength(250)]
        public string SubjectGroupName { get; set; }

        [Required, MaxLength(250)]
        public string SubjectGroupShortName { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int YearPartId { get; set; }
        public bool? IsExtraAllowed { get; set; }
        public bool? IsCompulsory { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual Programs Program { get; set; }

        [ForeignKey(nameof(YearPartId))]
        public virtual YearPart YearPart { get; set; }

        public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; }
        public virtual ICollection<SubjectDetail> SubjectDetails { get; set; }
        public virtual ICollection<SubjectGroupDetailMap> SubjectGroupDetailMaps { get; set; }
    }
}
