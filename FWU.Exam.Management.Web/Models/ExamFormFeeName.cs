using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamFormFeeName
    {
        [Key]
        public int ExamFormFeeNameId { get; set; }

        [Required, MaxLength(400)]
        public string Name { get; set; }

        public bool? IsCollegeFee { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamFormFeeRate> ExamFormFeeRates { get; set; }
    }
}
