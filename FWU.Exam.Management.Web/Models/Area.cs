
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Area:AuditBase
    {
        [Key]
        public int AreaId { get; set; }

        [Required, MaxLength(100)]
        public string AreaName { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }

        [ValidateNever]
        public virtual ICollection<College> Colleges { get; set; }
    }
}
