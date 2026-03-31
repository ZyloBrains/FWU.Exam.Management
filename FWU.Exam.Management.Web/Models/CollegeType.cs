using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class CollegeType:AuditBase
    {
        [Key]
        public int CollegeTypeId { get; set; }

        [Required, MaxLength(2)]
        public string CollegeTypeCode { get; set; }

        [Required, MaxLength(50)]
        public string CollegeTypeName { get; set; }

        [MaxLength(1024)]
        public string? Remarks { get; set; }

        public bool? IsDefault { get; set; }
        public bool IsActive { get; set; }

        [ValidateNever]
        public virtual ICollection<College> Colleges { get; set; }

        [ValidateNever]
        public virtual ICollection<ExamFormFeeRate> ExamFormFeeRates { get; set; }
    }
}
