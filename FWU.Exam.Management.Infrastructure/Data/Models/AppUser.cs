using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FWU.Exam.Management.Infrastructure.Data.Models;
public class AppUser: IdentityUser, IAuditable
{ 
    public string? ProfilePath { get; set; }
    public string? FullName { get; set; }
    public string? Designation { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Remarks { get; set; }

    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
