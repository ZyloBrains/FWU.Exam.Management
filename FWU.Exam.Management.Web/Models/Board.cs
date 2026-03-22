using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Board
    {
        [Key]
        public int BoardId { get; set; }
        [ValidateNever]
        public int CountryId { get; set; }

        [Required, MaxLength(50)]
        public string BoardName { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        [ValidateNever]
        public virtual ICollection<Programs> Programs { get; set; }
        [ValidateNever]
        public virtual ICollection<StudentQualification> StudentQualifications { get; set; }
    }
}
