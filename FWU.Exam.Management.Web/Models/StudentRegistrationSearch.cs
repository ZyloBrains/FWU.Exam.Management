using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class StudentRegistrationSearch:AuditBase
    {
        [Key]
        public int StudentRegistrationSearchId { get; set; }

        public string SearchCriteria { get; set; }
        public DateTime SearchDate { get; set; }
        public string? UserId { get; set; }

        public string SearchResults { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey(nameof(UserId))]
        [ValidateNever]
        public virtual AppUser? User { get; set; }
        [ValidateNever]
        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
    }
}
