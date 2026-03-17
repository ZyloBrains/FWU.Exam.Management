using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class YearPart
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int YearPartId { get; set; }

        public int ProgramPeriodTypeId { get; set; }
        public int Year { get; set; }
        public int Part { get; set; }

        [Required, MaxLength(50)]
        public string YearPartName { get; set; }

        [MaxLength(50)]
        public string Remark { get; set; }

        public bool IsActive { get; set; }
        public bool IsEditable { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [MaxLength(50)]
        public string Code { get; set; }

        [ForeignKey(nameof(ProgramPeriodTypeId))]
        public virtual ProgramPeriodType ProgramPeriodType { get; set; }

        public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
        public virtual ICollection<ProgramYearPart> ProgramYearParts { get; set; }
        public virtual ICollection<StudentProgramYearPart> StudentProgramYearParts { get; set; }
        public virtual ICollection<SubjectDetail> SubjectDetails { get; set; }
        public virtual ICollection<SubjectGroup> SubjectGroups { get; set; }
    }
}
