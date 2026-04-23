using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyTypeController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public PolicyTypeController(MydatabaseContext context)
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
            var query = _context.PolicyTypes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(x =>
                    x.PolicyTypeCode.Contains(search) ||
                    x.PolicyTypeDescription.Contains(search));

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.PolicyTypeCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET SINGLE
        [HttpGet("{code}")]
        public async Task<IActionResult> Get(string code)
        {
            var data = await _context.PolicyTypes.FindAsync(code);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(PolicyType model)
        {
            var exists = await _context.PolicyTypes
                .AnyAsync(x => x.PolicyTypeCode == model.PolicyTypeCode);

            if (exists)
                return BadRequest("Policy Type already exists");

            model.TimeStamp = DateTime.UtcNow;

            _context.PolicyTypes.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{code}")]
        public async Task<IActionResult> Update(string code, PolicyType model)
        {
            var db = await _context.PolicyTypes.FindAsync(code);

            if (db == null)
                return NotFound();

            db.PolicyTypeDescription = model.PolicyTypeDescription;
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE (with validation placeholder)
        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var db = await _context.PolicyTypes.FindAsync(code);

            if (db == null)
                return NotFound();

            //// 🔒 Example: prevent delete if used in vendor insurance table
            //var isUsed = await _context.Set<dynamic>() // replace with actual table
            //    .AnyAsync(x => x.PolicyType == code);

            //if (isUsed)
            //    return BadRequest("Cannot delete. Policy Type is in use.");

            _context.PolicyTypes.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ DROPDOWN
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var data = await _context.PolicyTypes
                .Select(x => new
                {
                    value = x.PolicyTypeCode,
                    label = x.PolicyTypeDescription
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
