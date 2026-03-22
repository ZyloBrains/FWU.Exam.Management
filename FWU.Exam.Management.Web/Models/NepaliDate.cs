using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class NepaliDate
    {
        [Key]
        public int NepaliDateId { get; set; }

        public DateTime? GregorianDate { get; set; }

        [MaxLength(10)]
        public string NepaliDateShort { get; set; }

        [MaxLength(50)]
        public string NepaliDateFull { get; set; }

        [MaxLength(50)]
        public string NepaliDateString { get; set; }
    }
}
