using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class PeriodType
    {
        [Key]
        public int PeriodTypeId { get; set; }

        [Required, MaxLength(50)]
        public string PeriodTypeName { get; set; }

        public decimal? NumberOfMonths { get; set; }
        public bool? IsActive { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
