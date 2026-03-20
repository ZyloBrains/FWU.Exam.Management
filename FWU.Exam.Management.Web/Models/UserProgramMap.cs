using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class UserProgramMap
    {
        [Key]
        public int UserProgramMapId { get; set; }

        public string UserId { get; set; }
        public int ProgramId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual AppUser User { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual Programs Program { get; set; }
    }
}
