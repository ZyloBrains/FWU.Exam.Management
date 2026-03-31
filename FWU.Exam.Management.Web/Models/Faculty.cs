using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Faculty:AuditBase
    {
        [Key]
        public int FacultyId { get; set; }

        [Required, MaxLength(10)]
        public string FacultyCode { get; set; }

        [Required, MaxLength(200)]
        public string FacultyName { get; set; }

        [MaxLength(50)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; }

        [ValidateNever]
        public virtual ICollection<Programs> Programs { get; set; }
        [ValidateNever]
        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
    }
}
