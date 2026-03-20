using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class KhaltiConfiguration
    {
        [Key]
        public int KhaltiConfigurationId { get; set; }

        [MaxLength(400)]
        public string ReturnUrl { get; set; }

        [MaxLength(400)]
        public string WebsiteUrl { get; set; }

        public decimal? Amount { get; set; }

        [MaxLength(400)]
        public string ProductName { get; set; }

        [MaxLength(400)]
        public string AuthorizationKey { get; set; }

        public int ServiceCharge { get; set; }

        [MaxLength(400)]
        public string PostUrl { get; set; }

        [MaxLength(400)]
        public string VerifyUrl { get; set; }
    }
}
