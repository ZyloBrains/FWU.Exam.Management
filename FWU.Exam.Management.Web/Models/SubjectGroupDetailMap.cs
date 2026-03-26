using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class SubjectGroupDetailMap
    {
        [Key, Column(Order = 0)]
        public int SubjectGroupId { get; set; }

        [Key, Column(Order = 1)]
        public int SubjectDetailId { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(SubjectGroupId))]
        [ValidateNever]
        public virtual SubjectGroup SubjectGroup { get; set; }

        [ForeignKey(nameof(SubjectDetailId))]
        [ValidateNever]
        public virtual SubjectDetail SubjectDetail { get; set; }
    }
}
