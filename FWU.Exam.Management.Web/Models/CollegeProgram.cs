using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class CollegeProgram
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CollegeProgramId { get; set; }

        public int CollegeId { get; set; }
        public int ProgramId { get; set; }
        public DateTime? AffiliationDate { get; set; }
        public int NumberOfStudents { get; set; }

        [MaxLength(1024)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(CollegeId))]
        public virtual College College { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual Programs Program { get; set; }
    }
}
