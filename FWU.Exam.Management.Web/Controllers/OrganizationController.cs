using System.Security.Claims;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Controllers;

[Authorize(Roles = Role.SystemAdmin)]
public class OrganizationController : Controller
{
    private const string MustChangePasswordClaimType = "must_change_password";

    private readonly IOrganizationService _organizationService;
    private readonly IFileUploadHelper _fileUploadHelper;
    private readonly UserManager<AppUser> _userManager;
    
    public OrganizationController(IOrganizationService organizationService, IFileUploadHelper fileUploadHelper, UserManager<AppUser> userManager)
    {
        _organizationService = organizationService;
        _fileUploadHelper = fileUploadHelper;
        _userManager = userManager;
    }

    // Rest of controller methods will be preserved
}
