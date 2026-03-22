using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class PasswordResetLog
    {
        [Key]
        public int PasswordResetLogId { get; set; }

        public string UserId { get; set; }

        [MaxLength(400)]
        public string Browser { get; set; }

        [MaxLength(400)]
        public string Device { get; set; }

        [MaxLength(400)]
        public string IpAddress { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? PasswordChangedDate { get; set; }

        [ForeignKey(nameof(UserId))]
        [ValidateNever]
        public virtual AppUser User { get; set; }
    }
}
