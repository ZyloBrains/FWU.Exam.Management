using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class StudentRegistrationSearch
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentRegistrationSearchId { get; set; }

        public string SearchCriteria { get; set; }
        public DateTime SearchDate { get; set; }
        public string? UserId { get; set; }

        public string SearchResults { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual AppUser? User { get; set; }

        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
    }
}
