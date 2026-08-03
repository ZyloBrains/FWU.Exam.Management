using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Country
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Country Name")]
    public string CountryName { get; set; } = string.Empty;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    public virtual ICollection<Board> Boards { get; set; } = [];
}
