using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Models;

namespace fwu_examination_management_system.Controllers
{
    public class PaymentPracticalSubjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentPracticalSubjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PaymentPracticalSubjects
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PaymentPracticalSubjects.Include(p => p.PaymentRequestLog);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PaymentPracticalSubjects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentPracticalSubjects = await _context.PaymentPracticalSubjects
                .Include(p => p.PaymentRequestLog)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentPracticalSubjects == null)
            {
                return NotFound();
            }

            return View(paymentPracticalSubjects);
        }

        // GET: PaymentPracticalSubjects/Create
        public IActionResult Create()
        {
            ViewData["PaymentRequestLogId"] = new SelectList(_context.PaymentRequestLogs, "PaymentRequestId", "FullName");
            return View();
        }

        // POST: PaymentPracticalSubjects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PaymentRequestLogId,PracticalSubjectsCount,TotalAmount")] PaymentPracticalSubjects paymentPracticalSubjects)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paymentPracticalSubjects);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentRequestLogId"] = new SelectList(_context.PaymentRequestLogs, "PaymentRequestId", "FullName", paymentPracticalSubjects.PaymentRequestLogId);
            return View(paymentPracticalSubjects);
        }

        // GET: PaymentPracticalSubjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentPracticalSubjects = await _context.PaymentPracticalSubjects.FindAsync(id);
            if (paymentPracticalSubjects == null)
            {
                return NotFound();
            }
            ViewData["PaymentRequestLogId"] = new SelectList(_context.PaymentRequestLogs, "PaymentRequestId", "FullName", paymentPracticalSubjects.PaymentRequestLogId);
            return View(paymentPracticalSubjects);
        }

        // POST: PaymentPracticalSubjects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PaymentRequestLogId,PracticalSubjectsCount,TotalAmount")] PaymentPracticalSubjects paymentPracticalSubjects)
        {
            if (id != paymentPracticalSubjects.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paymentPracticalSubjects);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentPracticalSubjectsExists(paymentPracticalSubjects.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentRequestLogId"] = new SelectList(_context.PaymentRequestLogs, "PaymentRequestId", "FullName", paymentPracticalSubjects.PaymentRequestLogId);
            return View(paymentPracticalSubjects);
        }

        // GET: PaymentPracticalSubjects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentPracticalSubjects = await _context.PaymentPracticalSubjects
                .Include(p => p.PaymentRequestLog)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentPracticalSubjects == null)
            {
                return NotFound();
            }

            return View(paymentPracticalSubjects);
        }

        // POST: PaymentPracticalSubjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paymentPracticalSubjects = await _context.PaymentPracticalSubjects.FindAsync(id);
            if (paymentPracticalSubjects != null)
            {
                _context.PaymentPracticalSubjects.Remove(paymentPracticalSubjects);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentPracticalSubjectsExists(int id)
        {
            return _context.PaymentPracticalSubjects.Any(e => e.Id == id);
        }
    }
}
