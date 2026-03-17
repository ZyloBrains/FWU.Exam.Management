using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class StudentQualification
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentQualificationId { get; set; }

        public int StudentRegistrationId { get; set; }
        public int BoardId { get; set; }
        public int PreviousLevelId { get; set; }

        [MaxLength(255)]
        public string ProgramName { get; set; }

        [Required, MaxLength(255)]
        public string InstituteName { get; set; }

        [MaxLength(50)]
        public string PassedYear { get; set; }

        [MaxLength(255)]
        public string Specialization { get; set; }

        public decimal? Percentage { get; set; }

        [MaxLength(50)]
        public string TotalCredits { get; set; }

        [MaxLength(50)]
        public string Remarks { get; set; }

        public bool IsHigherDegree { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [MaxLength(500)]
        public string ExamRollNumber { get; set; }

        [ForeignKey(nameof(StudentRegistrationId))]
        public virtual StudentRegistration StudentRegistration { get; set; }

        [ForeignKey(nameof(BoardId))]
        public virtual Board Board { get; set; }

        [ForeignKey(nameof(PreviousLevelId))]
        public virtual PreviousLevel PreviousLevel { get; set; }
    }
}
