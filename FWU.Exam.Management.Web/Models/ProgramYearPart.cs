using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ProgramYearPart
    {
        [Key]
        public int ProgramYearPartId { get; set; }

        public int ProgramsId { get; set; }
        public int YearPartId { get; set; }
        public int TotalMarks { get; set; }
        public int TotalPassMarks { get; set; }

        [ForeignKey(nameof(ProgramsId))]
        [ValidateNever]
        public virtual Programs Program { get; set; }

        [ForeignKey(nameof(YearPartId))]
        [ValidateNever]
        public virtual YearPart YearPart { get; set; }
    }
}
