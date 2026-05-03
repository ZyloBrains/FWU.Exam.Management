using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Web.Controllers
{
    public class CollegesController : Controller
    {
        private readonly AppDbContext _context;

        public CollegesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Colleges with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "DisplayOrder", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.Colleges
                .Include(c => c.CollegeType)
                .Include(c => c.Address)
                .ThenInclude(a => a.LocalLevel)
                .ThenInclude(ll => ll.District)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Code.ToString().Contains(search) ||
                    c.Name.Contains(search) ||
                    c.CollegeNameNepali.Contains(search) ||
                    c.ShortName.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.Phone1.Contains(search) ||
                    c.Phone2.Contains(search) ||
                    c.PrincipalName.Contains(search) ||
                    c.Remarks.Contains(search) ||
                    (c.Address != null && c.Address.LocalLevel != null && c.Address.LocalLevel.District != null && c.Address.LocalLevel.District.DistrictName.Contains(search)) ||
                    (c.CollegeType != null && c.CollegeType.Code.Contains(search))
                );
            }

            // Apply sorting
            query = sortDir.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sort))
                : query.OrderBy(GetSortProperty(sort));

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.SortDir = sortDir;

            return View(items);
        }

        private static System.Linq.Expressions.Expression<Func<College, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "code" => c => c.Code,
                "name" => c => c.Name,
                "shortname" => c => c.ShortName,
                "district" => c => c.Address.LocalLevel.District.DistrictName,
                "collegetype" => c => c.CollegeType.Code,
                "displayorder" => c => c.DisplayOrder,
                "isactive" => c => c.IsActive,
                _ => c => c.DisplayOrder
            };
        }

        // Helper to get filtered items for export
        private async Task<List<College>> GetFilteredItems(string search, string sort = "DisplayOrder", string sortDir = "asc")
        {
            var query = _context.Colleges
                .Include(c => c.CollegeType)
                .Include(c => c.Address)
                .ThenInclude(a => a.LocalLevel)
                .ThenInclude(ll => ll.District)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Code.ToString().Contains(search) ||
                    c.Name.Contains(search) ||
                    c.CollegeNameNepali.Contains(search) ||
                    c.ShortName.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.Phone1.Contains(search) ||
                    c.Phone2.Contains(search) ||
                    c.PrincipalName.Contains(search) ||
                    c.Remarks.Contains(search) ||
                    (c.Address != null && c.Address.LocalLevel != null && c.Address.LocalLevel.District != null && c.Address.LocalLevel.District.DistrictName.Contains(search)) ||
                    (c.CollegeType != null && c.CollegeType.Code.Contains(search))
                );
            }

            // Apply sorting
            query = sortDir.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sort))
                : query.OrderBy(GetSortProperty(sort));

            return await query.ToListAsync();
        }

        // Helper method to escape CSV fields
        private string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
                return $"\"{field.Replace("\"", "\"\"")}\"";
            return field;
        }

        // ONLY ONE ExportToCsv method - this is the one to keep
        public async Task<IActionResult> ExportToCsv(string search = null, string sort = "DisplayOrder", string sortDir = "asc")
        {
            var items = await GetFilteredItems(search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("College Code,College Name,College Name (Nepali),Short Name,District,Municipality/VDC,Ward No.,House No.,Website,Email,Phone 1,Phone 2,Principal Name,Principal Contact,Fax,Remarks,Is Exam Center Only,Is Active,College Type,Allocated Amount,Area,Display Order,Established Date,Closed Date");

            foreach (var c in items)
            {
                sb.AppendLine($"{EscapeCsv(c.Code.ToString())}," +
                              $"{EscapeCsv(c.Name)}," +
                              $"{EscapeCsv(c.CollegeNameNepali)}," +
                              $"{EscapeCsv(c.ShortName)}," +
                              $"{EscapeCsv(c.Address?.LocalLevel?.District?.DistrictName)}," +
                              $"{EscapeCsv(c.Address?.LocalLevel?.LocalLevelName)}," +
                              $"{c.Address?.WardNumber}," +
                              $"{EscapeCsv(c.Address?.HouseNumber)}," +
                              $"{EscapeCsv(c.Website)}," +
                              $"{EscapeCsv(c.Email)}," +
                              $"{EscapeCsv(c.Phone1)}," +
                              $"{EscapeCsv(c.Phone2)}," +
                              $"{EscapeCsv(c.PrincipalName)}," +
                              $"{EscapeCsv(c.PrincipalContactNumber)}," +
                              $"{EscapeCsv(c.Fax)}," +
                              $"{EscapeCsv(c.Remarks)}," +
                              $"{(c.IsExamCenterOnly ? "Yes" : "No")}," +
                              $"{(c.IsActive ? "Active" : "Inactive")}," +
                              $"{EscapeCsv(c.CollegeType?.Code)}," +
                              $"{c.AllocatedAmount}," +
                              $"{EscapeCsv(c.Address?.ToleStreet)}," +
                              $"{c.DisplayOrder}," +
                              $"{c.EstablishedDate?.ToString("yyyy-MM-dd")}," +
                              $"{c.ClosedDate?.ToString("yyyy-MM-dd")}");
            }

            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", $"Colleges_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        // ONLY ONE ExportToPdf method - this is the one to keep
        public async Task<IActionResult> ExportToPdf(string search = null, string sort = "DisplayOrder", string sortDir = "asc")
        {
            var items = await GetFilteredItems(search, sort, sortDir);
            return View("PrintPdf", items);
        }

        // GET: Colleges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var college = await _context.Colleges
                .Include(c => c.CollegeType)
                .Include(c => c.Address)
                .ThenInclude(a => a.LocalLevel)
                .ThenInclude(ll => ll.District)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (college == null)
            {
                return NotFound();
            }

            return View(college);
        }

        // GET: Colleges/Create
        public IActionResult Create()
        {
            ViewData["CollegeTypeId"] = new SelectList(_context.CollegeTypes, "Id", "Code");
            return View();
        }

        // POST: Colleges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Name,CollegeNameNepali,ShortName,EstablishedDate,ClosedDate,Website,Email,Phone1,Phone2,PrincipalName,PrincipalContactNumber,Fax,Remarks,IsExamCenterOnly,IsActive,AllocatedAmount,DisplayOrder,CollegeTypeId,CollegeProfileId")] College college)
        {
            // Create Address from form data
            var provinceId = Request.Form["ProvinceId"].ToString();
            var districtId = Request.Form["DistrictId"].ToString();
            var localLevelId = Request.Form["LocalLevelId"].ToString();
            var wardNumber = Request.Form["WardNumber"].ToString();
            var toleStreet = Request.Form["ToleStreet"].ToString();
            var houseNumber = Request.Form["HouseNumber"].ToString();

            if (!string.IsNullOrEmpty(localLevelId))
            {
                var address = new Address
                {
                    LocalLevelId = int.Parse(localLevelId),
                    WardNumber = string.IsNullOrEmpty(wardNumber) ? null : int.Parse(wardNumber),
                    ToleStreet = toleStreet,
                    HouseNumber = houseNumber,
                    AddressType = AddressType.Current,
                    IsActive = true
                };
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                college.AddressId = address.Id;
            }

            if (ModelState.IsValid)
            {
                _context.Add(college);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CollegeTypeId"] = new SelectList(_context.CollegeTypes, "Id", "Code", college.CollegeTypeId);
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
            ViewData["CollegeTypeId"] = new SelectList(_context.CollegeTypes, "Id", "Code", college.CollegeTypeId);
            return View(college);
        }

        // POST: Colleges/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,CollegeNameNepali,ShortName,EstablishedDate,ClosedDate,Website,Email,Phone1,Phone2,PrincipalName,PrincipalContactNumber,Fax,Remarks,IsExamCenterOnly,IsActive,AllocatedAmount,DisplayOrder,CollegeTypeId,CollegeProfileId,AddressId")] College college)
        {
            if (id != college.Id)
            {
                return NotFound();
            }

            // Update Address from form data
            var localLevelId = Request.Form["LocalLevelId"].ToString();
            var wardNumber = Request.Form["WardNumber"].ToString();
            var toleStreet = Request.Form["ToleStreet"].ToString();
            var houseNumber = Request.Form["HouseNumber"].ToString();

            if (!string.IsNullOrEmpty(localLevelId))
            {
                var address = await _context.Addresses.FindAsync(college.AddressId);
                if (address == null)
                {
                    address = new Address
                    {
                        LocalLevelId = int.Parse(localLevelId),
                        WardNumber = string.IsNullOrEmpty(wardNumber) ? null : int.Parse(wardNumber),
                        ToleStreet = toleStreet,
                        HouseNumber = houseNumber,
                        AddressType = AddressType.Current,
                        IsActive = true
                    };
                    _context.Addresses.Add(address);
                    await _context.SaveChangesAsync();
                    college.AddressId = address.Id;
                }
                else
                {
                    address.LocalLevelId = int.Parse(localLevelId);
                    address.WardNumber = string.IsNullOrEmpty(wardNumber) ? null : int.Parse(wardNumber);
                    address.ToleStreet = toleStreet;
                    address.HouseNumber = houseNumber;
                    _context.Update(address);
                }
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
                    if (!CollegeExists(college.Id))
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
            ViewData["CollegeTypeId"] = new SelectList(_context.CollegeTypes, "Id", "Code", college.CollegeTypeId);
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
                .Include(c => c.CollegeType)
                .Include(c => c.Address)
                .ThenInclude(a => a.LocalLevel)
                .ThenInclude(ll => ll.District)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            return _context.Colleges.Any(e => e.Id == id);
        }
    }
}