using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class CollegeProfile:AuditBase
    {
        [Key]
        public int CollegeProfileId { get; set; }
        public int CollegeId { get; set; }

        [Required, MaxLength(1024)]
        public string BankName { get; set; }

        [Required, MaxLength(1024)]
        public string BankBranchName { get; set; }

        [Required, MaxLength(1024)]
        public string BankAccountNumber { get; set; }

        [Required, MaxLength(1024)]
        public string ContactPersonName { get; set; }

        [Required, MaxLength(1024)]
        public string ContactPersonMobileNumber { get; set; }

        [Required, MaxLength(1024)]
        public string ContactPersonEmail { get; set; }

        public int BlankChequeUserAttachmentId { get; set; }
        public int AuditReportUserAttachmentId { get; set; }

        public int? Status { get; set; }

        [ForeignKey(nameof(CollegeId))]
        [ValidateNever]
        public virtual College College { get; set; }

        [ForeignKey(nameof(BlankChequeUserAttachmentId))]
        [ValidateNever]
        public virtual UserAttachment BlankChequeUserAttachment { get; set; }

        [ForeignKey(nameof(AuditReportUserAttachmentId))]
        [ValidateNever]
        public virtual UserAttachment AuditReportUserAttachment { get; set; }
    }
}
