using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReasonUsageCodeController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ReasonUsageCodeController(MydatabaseContext context)
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
            var query = _context.ReasonUsageCodes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(x =>
                    x.UsageCode.Contains(search) ||
                    x.UsageDescription.Contains(search));

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.UsageCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET SINGLE
        [HttpGet("{code}")]
        public async Task<IActionResult> Get(string code)
        {
            var data = await _context.ReasonUsageCodes.FindAsync(code);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(ReasonUsageCode model)
        {
            var exists = await _context.ReasonUsageCodes
                .AnyAsync(x => x.UsageCode == model.UsageCode);

            if (exists)
                return BadRequest("Usage Value already exists");

            model.TimeStamp = DateTime.UtcNow;

            _context.ReasonUsageCodes.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{code}")]
        public async Task<IActionResult> Update(string code, ReasonUsageCode model)
        {
            var db = await _context.ReasonUsageCodes.FindAsync(code);

            if (db == null)
                return NotFound();

            db.UsageDescription = model.UsageDescription;
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE (with validation)
        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var db = await _context.ReasonUsageCodes.FindAsync(code);

            if (db == null)
                return NotFound();

            // 🔒 Check if used in Reason Codes
            var isUsed = await _context.ReasonCodes
                .AnyAsync(x => x.SRsnWhUsedCd == code);

            if (isUsed)
                return BadRequest("Cannot delete. Usage Value is used in Reason Codes.");

            _context.ReasonUsageCodes.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ DROPDOWN
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var data = await _context.ReasonUsageCodes
                .Select(x => new
                {
                    value = x.UsageCode,
                    label = x.UsageDescription
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
