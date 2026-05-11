using System.Security.Claims;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FWU.Exam.Management.Infrastructure.Data.Models;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[Authorize(Roles = Role.SystemAdmin)]
public class OrganizationController(IOrganizationService organizationService, IFileUploadHelper fileUploadHelper, UserManager<AppUser> userManager) : Controller
{
    private const string MustChangePasswordClaimType = "must_change_password";

    // Rest of controller methods will be preserved
}
