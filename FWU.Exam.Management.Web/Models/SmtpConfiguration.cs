using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class SmtpConfiguration
    {
        [Key]
        public int SmtpConfigurationId { get; set; }

        [Required, MaxLength(1024)]
        public string Host { get; set; }

        [Required, MaxLength(1024)]
        public string From { get; set; }

        public int Port { get; set; }

        [Required, MaxLength(1024)]
        public string UserName { get; set; }

        [Required, MaxLength(1024)]
        public string Password { get; set; }

        public bool EnableSsl { get; set; }
    }
}
