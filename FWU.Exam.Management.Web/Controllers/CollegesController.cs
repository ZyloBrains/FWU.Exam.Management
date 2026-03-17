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
    public class CollegesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CollegesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Colleges
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Colleges.Include(c => c.Area).Include(c => c.CollegeType).Include(c => c.District).Include(c => c.QuestionSet);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Colleges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var college = await _context.Colleges
                .Include(c => c.Area)
                .Include(c => c.CollegeType)
                .Include(c => c.District)
                .Include(c => c.QuestionSet)
                .FirstOrDefaultAsync(m => m.CollegeId == id);
            if (college == null)
            {
                return NotFound();
            }

            return View(college);
        }

        // GET: Colleges/Create
        public IActionResult Create()
        {
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaName");
            ViewData["CollegeTypeId"] = new SelectList(_context.CollegeTypes, "CollegeTypeId", "CollegeTypeCode");
            ViewData["DistrictId"] = new SelectList(_context.Districts, "DistrictId", "DistrictName");
            ViewData["QuestionSetId"] = new SelectList(_context.QuestionSets, "QuestionSetId", "QuestionSetName");
            return View();
        }

        // POST: Colleges/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CollegeId,CollegeCode,CollegeName,CollegeNameNepali,ShortName,EstablishedDate,ClosedDate,DistrictId,MunicipalityVdc,WardNumber,HouseNumber,Website,Email,Phone1,Phone2,PrincipalName,PrincipalContactNumber,Fax,Remarks,IsExamCenterOnly,IsActive,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,CollegeTypeId,AllocatedAmount,AreaId,DisplayOrder,QuestionSetId")] College college)
        {
            if (ModelState.IsValid)
            {
                _context.Add(college);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaName", college.AreaId);
            ViewData["CollegeTypeId"] = new SelectList(_context.CollegeTypes, "CollegeTypeId", "CollegeTypeCode", college.CollegeTypeId);
            ViewData["DistrictId"] = new SelectList(_context.Districts, "DistrictId", "DistrictName", college.DistrictId);
            ViewData["QuestionSetId"] = new SelectList(_context.QuestionSets, "QuestionSetId", "QuestionSetName", college.QuestionSetId);
            return View(college);
        }

        // GET: Colleges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var college = await _context.Colleges.FindAsync(id);
            if (college == null)
            {
                return NotFound();
            }
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaName", college.AreaId);
            ViewData["CollegeTypeId"] = new SelectList(_context.CollegeTypes, "CollegeTypeId", "CollegeTypeCode", college.CollegeTypeId);
            ViewData["DistrictId"] = new SelectList(_context.Districts, "DistrictId", "DistrictName", college.DistrictId);
            ViewData["QuestionSetId"] = new SelectList(_context.QuestionSets, "QuestionSetId", "QuestionSetName", college.QuestionSetId);
            return View(college);
        }

        // POST: Colleges/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CollegeId,CollegeCode,CollegeName,CollegeNameNepali,ShortName,EstablishedDate,ClosedDate,DistrictId,MunicipalityVdc,WardNumber,HouseNumber,Website,Email,Phone1,Phone2,PrincipalName,PrincipalContactNumber,Fax,Remarks,IsExamCenterOnly,IsActive,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,CollegeTypeId,AllocatedAmount,AreaId,DisplayOrder,QuestionSetId")] College college)
        {
            if (id != college.CollegeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(college);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CollegeExists(college.CollegeId))
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
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaName", college.AreaId);
            ViewData["CollegeTypeId"] = new SelectList(_context.CollegeTypes, "CollegeTypeId", "CollegeTypeCode", college.CollegeTypeId);
            ViewData["DistrictId"] = new SelectList(_context.Districts, "DistrictId", "DistrictName", college.DistrictId);
            ViewData["QuestionSetId"] = new SelectList(_context.QuestionSets, "QuestionSetId", "QuestionSetName", college.QuestionSetId);
            return View(college);
        }

        // GET: Colleges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var college = await _context.Colleges
                .Include(c => c.Area)
                .Include(c => c.CollegeType)
                .Include(c => c.District)
                .Include(c => c.QuestionSet)
                .FirstOrDefaultAsync(m => m.CollegeId == id);
            if (college == null)
            {
                return NotFound();
            }

            return View(college);
        }

        // POST: Colleges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var college = await _context.Colleges.FindAsync(id);
            if (college != null)
            {
                _context.Colleges.Remove(college);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CollegeExists(int id)
        {
            return _context.Colleges.Any(e => e.CollegeId == id);
        }
    }
}
