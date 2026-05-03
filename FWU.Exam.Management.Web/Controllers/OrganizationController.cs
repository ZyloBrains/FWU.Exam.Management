using System.Security.Claims;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

[Authorize(Roles = Role.SystemAdmin)]
public class OrganizationController : Controller
{
    private const string MustChangePasswordClaimType = "must_change_password";

    private readonly AppDbContext _context;
    private readonly IFileUploadHelper _fileUploadHelper;
    private readonly UserManager<AppUser> _userManager;
    
    public OrganizationController(AppDbContext context, IFileUploadHelper fileUploadHelper, UserManager<AppUser> userManager)
    {
        _context = context;
        _fileUploadHelper = fileUploadHelper;
        _userManager = userManager;
    }

    // Rest of controller methods will be preserved
}
