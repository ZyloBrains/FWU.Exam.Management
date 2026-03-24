using fwu_examination_management_system.Data;
using fwu_examination_management_system.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers
{
    [Authorize]
    public class BankVoucherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BankVoucherController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpGet]
        public IActionResult Verify()
        {
            return View(new VoucherVerificationViewModel());
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(VoucherVerificationViewModel model)
        {
            model.HasSearched = true;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var voucherNo = model.VoucherNumber.Trim();

            model.Results = await _context.BankVouchers
                .Include(x => x.College)
                .Include(x => x.AcademicYear)
                .Include(x => x.Bank)
                .Where(x => x.VoucherNumber == voucherNo)
                .OrderByDescending(x => x.VoucherDate)
                .ToListAsync();

            return View(model);
        }
    }
}
