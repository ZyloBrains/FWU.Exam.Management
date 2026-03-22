using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ProgramPeriodType
    {
        [Key]
        public int ProgramPeriodTypeId { get; set; }

        [Required, MaxLength(50)]
        public string ProgramPeriodTypeName { get; set; }

        public decimal? NumberOfMonths { get; set; }
        [ValidateNever]
        public virtual ICollection<Programs> Programs { get; set; }
        [ValidateNever]
        public virtual ICollection<YearPart> YearParts { get; set; }
    }
}
