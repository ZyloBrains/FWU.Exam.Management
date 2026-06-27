using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class NepaliDate
{
    public int Id { get; set; }

    [Display(Name = "Gregorian Date")]
    public DateTime? GregorianDate { get; set; }

    [MaxLength(10)]
    [Display(Name = "Nepali Date (Short)")]
    public string? NepaliDateShort { get; set; }

    [MaxLength(50)]
    [Display(Name = "Nepali Date (Full)")]
    public string? NepaliDateFull { get; set; }

    [MaxLength(50)]
    [Display(Name = "Nepali Date String")]
    public string? NepaliDateString { get; set; }
}
