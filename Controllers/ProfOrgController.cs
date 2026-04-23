using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfOrgController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ProfOrgController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (Pagination + Filtering)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.ProfOrgs.AsQueryable();

            // 🔍 Filtering
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.ProfOrgId.Contains(search) ||
                    x.ProfOrgDesc.Contains(search));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.ProfOrgId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data
            });
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await _context.ProfOrgs.FindAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(ProfOrg model)
        {
            if (string.IsNullOrWhiteSpace(model.ProfOrgId))
                return BadRequest("ProfOrgId is required");

            if (await _context.ProfOrgs.AnyAsync(x => x.ProfOrgId == model.ProfOrgId))
                return BadRequest("ProfOrg already exists");

            model.TimeStamp = DateTime.UtcNow;

            _context.ProfOrgs.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, ProfOrg model)
        {
            var db = await _context.ProfOrgs.FindAsync(id);
            if (db == null)
                return NotFound();

            db.ProfOrgDesc = model.ProfOrgDesc;
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var db = await _context.ProfOrgs.FindAsync(id);
            if (db == null)
                return NotFound();

            _context.ProfOrgs.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
