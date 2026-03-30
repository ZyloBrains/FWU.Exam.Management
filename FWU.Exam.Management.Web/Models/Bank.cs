using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Bank:AuditBase
    {
        [Key]
        public int BankId { get; set; }

        [Required, MaxLength(100)]
        public string BankName { get; set; }

        [MaxLength(25)]
        public string? BankCode { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        [ValidateNever]
        public virtual ICollection<BankVoucher> BankVouchers { get; set; }
    }
}
