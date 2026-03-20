using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Ethnicity
    {
        [Key]
        public int EthnicityId { get; set; }

        [Required, MaxLength(50)]
        public string EthnicityName { get; set; }

        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
    }
}
