using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class District
    {
        [Key]
        public int DistrictId { get; set; }

        public int ProvinceId { get; set; }

        [MaxLength(50)]
        public string? DistrictCode { get; set; }

        [Required, MaxLength(255)]
        public string DistrictName { get; set; }

        [ForeignKey(nameof(ProvinceId))]
        [ValidateNever]
        public virtual Province? Province { get; set; }

        [ValidateNever]
        public virtual ICollection<College?> Colleges { get; set; }
        [ValidateNever]
        public virtual ICollection<LocalLevel?> LocalLevels { get; set; }
        [ValidateNever]
        public virtual ICollection<StudentRegistration?> StudentRegistrations { get; set; }
    }
}
