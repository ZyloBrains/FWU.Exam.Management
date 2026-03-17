using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ProgramSubjectPracticalCharge
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProgramId { get; set; }
        public decimal PracticalSubjectCharge { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual Programs Program { get; set; }
    }
}
