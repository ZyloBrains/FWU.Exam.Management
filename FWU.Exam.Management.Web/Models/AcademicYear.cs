using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class AcademicYear
    {
        [Key]
        public int AcademicYearId { get; set; } // primary key auto-incremented

        public int AcademicYearCode { get; set; }

        [MaxLength(50)]
        public string AcademicYearCodeNepali { get; set; }

        [Required, MaxLength(50)]
        public string AcademicYearName { get; set; }

        [Required, MaxLength(50)]
        public string AcademicYearNameNepali { get; set; }

        [MaxLength(50)]
        public string Remark { get; set; }

        public bool IsRunning { get; set; }
        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<Batch> Batches { get; set; }
        public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; }
        public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
    }
}
