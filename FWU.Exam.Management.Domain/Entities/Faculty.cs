using FWU.Exam.Management.Domain.Entities.Colleges;

namespace FWU.Exam.Management.Domain.Entities;
public class Faculty
{    
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OfficeCode { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public virtual ICollection<College>? Colleges { get; set; }
    public virtual ICollection<Department>? Departments { get; set; }
}
