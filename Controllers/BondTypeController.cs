using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BondTypeController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public BondTypeController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.BondTypes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(x =>
                    x.BondTypeCode.Contains(search) ||
                    x.BondTypeDescription.Contains(search));

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.BondTypeCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET SINGLE
        [HttpGet("{code}")]
        public async Task<IActionResult> Get(string code)
        {
            var data = await _context.BondTypes.FindAsync(code);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(BondType model)
        {
            var exists = await _context.BondTypes
                .AnyAsync(x => x.BondTypeCode == model.BondTypeCode);

            if (exists)
                return BadRequest("Bond Type already exists");

            model.TimeStamp = DateTime.UtcNow;

            _context.BondTypes.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{code}")]
        public async Task<IActionResult> Update(string code, BondType model)
        {
            var db = await _context.BondTypes.FindAsync(code);

            if (db == null)
                return NotFound();

            db.BondTypeDescription = model.BondTypeDescription;
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE (with validation)
        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var db = await _context.BondTypes.FindAsync(code);

            if (db == null)
                return NotFound();

            //// 🔒 Example: prevent delete if used in vendor bonds table
            //var isUsed = await _context.Set<dynamic>() // replace with actual DbSet
            //    .AnyAsync(x => x.BondType == code);

            //if (isUsed)
            //    return BadRequest("Cannot delete. Bond Type is in use.");

            _context.BondTypes.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ DROPDOWN
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var data = await _context.BondTypes
                .Select(x => new
                {
                    value = x.BondTypeCode,
                    label = x.BondTypeDescription
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
