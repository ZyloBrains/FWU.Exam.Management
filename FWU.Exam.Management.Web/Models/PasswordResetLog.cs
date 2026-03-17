using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class PasswordResetLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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
        public virtual AppUser User { get; set; }
    }
}
