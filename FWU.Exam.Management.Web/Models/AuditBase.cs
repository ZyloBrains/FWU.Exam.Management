using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    [NotMapped]
    public  class AuditBase
    {
        [ScaffoldColumn(false)]
        public string? CreatedBy { get; set; }
        [ScaffoldColumn(false)]
        public DateTime? CreatedDate { get; set; }=DateTime.UtcNow;
        [ScaffoldColumn(false)]
        public string? UpdatedBy { get; set; }
        [ScaffoldColumn(false)]
        public DateTime? UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
