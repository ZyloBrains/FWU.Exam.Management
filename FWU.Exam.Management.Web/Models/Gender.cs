using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Gender:AuditBase
    {
        [Key]
        public int GenderId { get; set; }

        [Required, MaxLength(50)]
        public string GenderName { get; set; }

        public bool IsActive { get; set; }

        [ValidateNever]
        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
    }
}
