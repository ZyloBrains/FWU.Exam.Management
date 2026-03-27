using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ConnectIpsPaymentConfiguration
    {
        [Key]
        public int ConnectIpsPaymentConfigurationId { get; set; }

        [Required, MaxLength(1024)]
        public string GatewayUrl { get; set; }

        [Required, MaxLength(1024)]
        public string MerchantId { get; set; }

        [Required, MaxLength(1024)]
        public string AppId { get; set; }

        [Required, MaxLength(1024)]
        public string AppName { get; set; }

        [Required, MaxLength(1024)]
        public string ValidationApiUrl { get; set; }

        [Required, MaxLength(1024)]
        public string UsernameForValidationApi { get; set; }

        [Required, MaxLength(1024)]
        public string PasswordForValidationApi { get; set; }

        [Required, MaxLength(1024)]
        public string PasswordForCreditorPfx { get; set; }

        [MaxLength(10)]
        public string? TransactionCurrency { get; set; }
    }
}
