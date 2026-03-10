using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    [Table("ActiveExamSchedule")]
    public class ActiveExamSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ExamScheduleId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        [StringLength(1024)]
        public string Remarks { get; set; }
    }

}
