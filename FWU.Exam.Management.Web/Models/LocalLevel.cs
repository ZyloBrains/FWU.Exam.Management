using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class LocalLevel
    {
        [Key]
        public int LocalLevelId { get; set; }

        public int DistrictId { get; set; }

        [Required, MaxLength(100)]
        public string LocalLevelName { get; set; }

        [MaxLength(50)]
        public string Remark { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }

        [ForeignKey(nameof(DistrictId))]
        public virtual District District { get; set; }

        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
    }
}
