using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    // 13. SubjectDetail
    [Table("SubjectDetail")]
    public class SubjectDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubjectDetailID { get; set; }
        public int? SubjectGroupID { get; set; }
        public int ProgramID { get; set; }
        public int YearPartID { get; set; }
        [Required]
        [StringLength(50)]
        public string SubjectCode { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string SubjectName { get; set; } = string.Empty;
        public decimal TheoryFullMark { get; set; }
        public decimal TheoryPassMark { get; set; }
        public decimal? PracticalFullMark { get; set; }
        public decimal? PracticalPassMark { get; set; }
        public decimal? InternalTheoryFullMark { get; set; }
        public decimal? InternalTheoryPassMark { get; set; }
        public decimal? InternalPracticalFullMark { get; set; }
        public decimal? InternalPracticalPassMark { get; set; }
        public int? CreditHour { get; set; }
        public bool HasPractical { get; set; }
        public bool HasInternal { get; set; }
        public int? DisplayOrder { get; set; }
        [StringLength(255)]
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsCompulsory { get; set; }
        [StringLength(50)]
        public string? ShortName { get; set; }
        [StringLength(50)]
        public string? ConSubjectCode { get; set; }
        public int SubjectTypeId { get; set; }
        public bool HasTheory { get; set; }
        [StringLength(50)]
        public string? Year { get; set; }
        [StringLength(50)]
        public string? Part { get; set; }
    }

}
