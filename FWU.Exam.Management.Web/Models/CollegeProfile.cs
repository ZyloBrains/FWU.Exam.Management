using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class CollegeProfile
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
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

        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(CollegeId))]
        public virtual College College { get; set; }

        [ForeignKey(nameof(BlankChequeUserAttachmentId))]
        public virtual UserAttachment BlankChequeUserAttachment { get; set; }

        [ForeignKey(nameof(AuditReportUserAttachmentId))]
        public virtual UserAttachment AuditReportUserAttachment { get; set; }
    }
}
