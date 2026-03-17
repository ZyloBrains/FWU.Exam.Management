using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ESewaConfiguration
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(256)]
        public string PostUrl { get; set; }

        [MaxLength(50)]
        public string ProductCode { get; set; }

        [MaxLength(256)]
        public string SecretKey { get; set; }

        [MaxLength(256)]
        public string SuccessUrl { get; set; }

        public decimal ServiceChargeAmount { get; set; }

        [MaxLength(256)]
        public string VerifyUrl { get; set; }
    }
}
