namespace fwu_examination_management_system.Models;
public class Organization
{
    
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OfficeCode { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
}