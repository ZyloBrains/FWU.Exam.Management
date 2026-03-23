using fwu_examination_management_system.Data;
using fwu_examination_management_system.Models;
using fwu_examination_management_system.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.Include(u => u.Organization).ToListAsync();
            var model = new List<UserListItemViewModel>();
            foreach (var user in users)
            {
                model.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    OrganizationName = user.Organization?.Name,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }
            return View(model);
        }

        // GET: User/Details/id
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.Users
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            ViewBag.Roles = await _userManager.GetRolesAsync(user);
            return View(user);
        }

        // GET: User/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Organizations = new SelectList(await _context.Organizations.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!model.OrganizationId.HasValue)
            {
                ModelState.AddModelError(nameof(model.OrganizationId), "Organization is required.");
            }

            if (model.OrganizationId.HasValue)
            {
                var organizationExists = await _context.Organizations.AnyAsync(o => o.Id == model.OrganizationId.Value);
                if (!organizationExists)
                {
                    ModelState.AddModelError(nameof(model.OrganizationId), "Selected organization is invalid.");
                }
            }

            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    OrganizationId = model.OrganizationId,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (await _roleManager.RoleExistsAsync("Student") && !await _userManager.IsInRoleAsync(user, "Student"))
                        await _userManager.AddToRoleAsync(user, "Student");

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Organizations = new SelectList(await _context.Organizations.ToListAsync(), "Id", "Name", model.OrganizationId);
            return View(model);
        }

        // GET: User/Edit/id
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                OrganizationId = user.OrganizationId
            };

            ViewBag.Organizations = new SelectList(await _context.Organizations.ToListAsync(), "Id", "Name", model.OrganizationId);
            return View(model);
        }

        // POST: User/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound();

                user.Email = model.Email;
                user.UserName = model.Email;
                user.OrganizationId = model.OrganizationId;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Organizations = new SelectList(await _context.Organizations.ToListAsync(), "Id", "Name", model.OrganizationId);
            return View(model);
        }

        // GET: User/Delete/id
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.Users
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: User/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
                await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }

        // GET: User/AssignRoles/id
        public async Task<IActionResult> AssignRoles(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var allRoles = await _roleManager.Roles
                .Where(r => r.Name != "Student")
                .ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new AssignRolesViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email ?? string.Empty,
                Roles = allRoles.Select(r => new RoleAssignmentItem
                {
                    RoleName = r.Name ?? string.Empty,
                    IsAssigned = userRoles.Contains(r.Name ?? string.Empty)
                }).ToList()
            };

            return View(model);
        }

        // POST: User/AssignRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoles(AssignRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles
                .Where(r => r.IsAssigned && !string.Equals(r.RoleName, "Student", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.RoleName)
                .ToList();

            if (!selectedRoles.Contains("Student", StringComparer.OrdinalIgnoreCase))
            {
                selectedRoles.Add("Student");
            }

            var toAdd = selectedRoles.Except(currentRoles).ToList();
            var toRemove = currentRoles.Except(selectedRoles).ToList();

            if (toAdd.Count > 0)
                await _userManager.AddToRolesAsync(user, toAdd);

            if (toRemove.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, toRemove);

            return RedirectToAction(nameof(Index));
        }
    }
}
