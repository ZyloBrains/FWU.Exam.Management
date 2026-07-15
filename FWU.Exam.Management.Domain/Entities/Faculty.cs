using FWU.Exam.Management.Domain.Entities.Colleges;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;
public class Faculty
{    
    public int Id { get; set; }
    [Required, MaxLength(100)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;
    [Required, MaxLength(30)]
    [Display(Name = "Office Code")]
    public string OfficeCode { get; set; } = string.Empty;
    [MaxLength(50)]
    [Display(Name = "Contact Number")]
    public string ContactNumber { get; set; } = string.Empty;
    [MaxLength(255)]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;
    [MaxLength(100)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    [Display(Name = "Logo Path")]
    public string? LogoPath { get; set; }
    [Display(Name = "Controller Signature Path")]
    public string? ControllerSignaturePath { get; set; }
    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public virtual ICollection<College>? Colleges { get; set; }
}

