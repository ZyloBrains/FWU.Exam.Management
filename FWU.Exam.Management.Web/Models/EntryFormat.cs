using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class EntryFormat:AuditBase
    {
        [Key]
        public int EntryFormatId { get; set; }

        [Required, MaxLength(100)]
        public string EntryFormatName { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }

        [ValidateNever]
        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
    }
}
