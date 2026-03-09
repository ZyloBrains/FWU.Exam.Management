namespace fwu_examination_management_system.Models;
public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string OfficeCode { get; set; }
    public string ContactNumber { get; set; }
    public string Address { get; set; }
    public string Email { get; set; }
    public string? LogoPath { get; set; }
}