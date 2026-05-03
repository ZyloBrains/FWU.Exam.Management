using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class NepaliDate
{
    public int Id { get; set; }

    public DateTime? GregorianDate { get; set; }

    [MaxLength(10)]
    public string? NepaliDateShort { get; set; }

    [MaxLength(50)]
    public string? NepaliDateFull { get; set; }

    [MaxLength(50)]
    public string? NepaliDateString { get; set; }
}
