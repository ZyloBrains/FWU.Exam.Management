using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class CollegeType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CollegeTypeId { get; set; }

        [Required, MaxLength(2)]
        public string CollegeTypeCode { get; set; }

        [Required, MaxLength(50)]
        public string CollegeTypeName { get; set; }

        [MaxLength(1024)]
        public string Remarks { get; set; }

        public bool? IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<College> Colleges { get; set; }
        public virtual ICollection<ExamFormFeeRate> ExamFormFeeRates { get; set; }
    }
}
