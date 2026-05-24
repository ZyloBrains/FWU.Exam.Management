using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Domain.Entities;

public class Tenant
{    
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OfficeCode { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public TenantType TenantType { get; set; } = TenantType.Standard;
    public bool IsActive { get; set; } = true;
}
