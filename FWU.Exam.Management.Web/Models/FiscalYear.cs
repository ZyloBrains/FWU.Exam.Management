using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class FiscalYear:AuditBase
    {
        [Key]
        public int FiscalYearId { get; set; }

        [Required, MaxLength(50)]
        public string FiscalYearName { get; set; }

        [Required, MaxLength(10)]
        public string StartDate { get; set; }

        [Required, MaxLength(10)]
        public string EndDate { get; set; }

        public bool IsRunning { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(4)]
        public string FiscalYearCode { get; set; }
    }
}
