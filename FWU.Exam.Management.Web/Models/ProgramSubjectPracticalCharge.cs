using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ProgramSubjectPracticalCharge
    {
        [Key]
        public int ProgramSubjectPracticalChargeId { get; set; }

        public int ProgramsId { get; set; }
        public decimal PracticalSubjectCharge { get; set; }

        [ForeignKey(nameof(ProgramsId))]
        public virtual Programs Program { get; set; }
    }
}
