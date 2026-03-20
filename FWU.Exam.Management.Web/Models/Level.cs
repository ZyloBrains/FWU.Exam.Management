
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Level
    {
        [Key]
        public int LevelId { get; set; }

        [MaxLength(2)]
        public string LevelCode { get; set; }

        [Required, MaxLength(50)]
        public string LevelName { get; set; }

        public int? LevelDisplayOrder { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool? IsRunning { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
        public virtual ICollection<Programs> Programs { get; set; }
        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
    }
}
