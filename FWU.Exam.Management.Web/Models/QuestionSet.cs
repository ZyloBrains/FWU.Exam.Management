using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class QuestionSet:AuditBase
    {
        [Key]
        public int QuestionSetId { get; set; }
        [Required, MaxLength(255)]
        public string QuestionSetName { get; set; }
        [MaxLength(1024)]
        public string Description { get; set; }
        public bool IsActive { get; set; }

        [ValidateNever]
        public virtual ICollection<College> Colleges { get; set; }
    }
}
