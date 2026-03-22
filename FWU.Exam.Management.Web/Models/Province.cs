using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Province
    {
        [Key]
        public int ProvinceId { get; set; }

        [Required, MaxLength(50)]
        public string ProvinceName { get; set; }

        public bool IsActive { get; set; }
        [ValidateNever]
        public virtual ICollection<District> Districts { get; set; }
    }
}
