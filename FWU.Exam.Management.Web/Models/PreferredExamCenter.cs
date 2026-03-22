using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class PreferredExamCenter
    {
        [Key]
        public int PreferredExamCenterId { get; set; }

        [Required, MaxLength(1024)]
        public string Name { get; set; }

        public int? CollegeId { get; set; }

        [MaxLength(1024)]
        public string Remarks { get; set; }

        [ForeignKey(nameof(CollegeId))]
        [ValidateNever]
        public virtual College College { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamRegistrationCenterChange> ExamRegistrationCenterChanges { get; set; }
    }
}
