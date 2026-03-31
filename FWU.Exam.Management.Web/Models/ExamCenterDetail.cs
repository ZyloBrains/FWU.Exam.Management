using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamCenterDetail
    {
        [Key]
        public int ExamCenterDetailId { get; set; }

        public int ExamCenterId { get; set; }
        public int CollegeId { get; set; }
        public int? ProgramsId { get; set; }
        public long RollNumberFrom { get; set; }
        public long RollNumberTo { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey(nameof(ExamCenterId))]
        [ValidateNever]
        public virtual ExamCenter ExamCenter { get; set; }

        [ForeignKey(nameof(CollegeId))]
        [ValidateNever]
        public virtual College College { get; set; }

        [ForeignKey(nameof(ProgramsId))]
        [ValidateNever]
        public virtual Programs Program { get; set; }
    }
}
