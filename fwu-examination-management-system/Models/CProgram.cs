using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models {

   [Table("Program")]
    public class CProgram
    {
        [Required]
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        [Required]
        [Column("LevelId")]
        public int LevelId { get; set; }
        [Required]
        [Column("FacultyId")]
        public int FacultyId { get; set; }
        //[Required]
        [StringLength(100)]
        [Column("ProgramName")]
        public  string? ProgramName { get; set; }
        [StringLength(100)]
        [Column("ShortName")]
        public string? ShortName { get; set; }
        [StringLength(100)]
        [Column("Duration")]
        public int Duration { get; set; }
        //[StringLength(100)]
        [Column("IsActive")]
        public bool IsActive { get; set; }
    }
}
