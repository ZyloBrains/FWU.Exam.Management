using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Region
    {
        [Key]
        public int RegionId { get; set; }

        [Required, MaxLength(2)]
        public string RegionCode { get; set; }

        [Required, MaxLength(100)]
        public string RegionName { get; set; }

        [MaxLength(55)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
