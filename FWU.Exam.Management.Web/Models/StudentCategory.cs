using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class StudentCategory
    {
        [Key]
        public int StudentCategoryId { get; set; }

        [Required, MaxLength(50)]
        public string StudentCategoryName { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
    }
}
