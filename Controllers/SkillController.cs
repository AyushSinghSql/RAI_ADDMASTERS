using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public SkillController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (Pagination + Filtering)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            string? activeFl,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Skills.AsQueryable();

            // 🔍 Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.SkillId.Contains(search) ||
                    x.SkillDesc.Contains(search));
            }

            // 🔍 Active filter
            if (!string.IsNullOrEmpty(activeFl))
            {
                query = query.Where(x => x.ActiveFl == activeFl);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.SkillId)
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
            var data = await _context.Skills.FindAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(Skill model)
        {
            if (await _context.Skills.AnyAsync(x => x.SkillId == model.SkillId))
                return BadRequest("Skill already exists");

            model.TimeStamp = DateTime.UtcNow;

            // Validate flag
            model.ActiveFl = (model.ActiveFl == "N") ? "N" : "Y";

            _context.Skills.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Skill model)
        {
            var db = await _context.Skills.FindAsync(id);
            if (db == null)
                return NotFound();

            db.SkillDesc = model.SkillDesc;
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;
            db.ActiveFl = (model.ActiveFl == "N") ? "N" : "Y";

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE (Protected)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var db = await _context.Skills.FindAsync(id);
            if (db == null)
                return NotFound();

            // 🔥 VALIDATION: prevent delete if used in SUBC_SKILLS
            var isUsed = await _context.VendorEmployeeSkills
                .AnyAsync(x => x.SkillId == id);

            if (isUsed)
            {
                return BadRequest(new
                {
                    message = "Cannot delete. Skill is used in subcontractor skills."
                });
            }

            _context.Skills.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ DROPDOWN (Active only)
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var data = await _context.Skills
                .Where(x => x.ActiveFl == "Y")
                .Select(x => new
                {
                    value = x.SkillId,
                    label = x.SkillDesc
                })
                .ToListAsync();

            return Ok(data);
        }

        // ✅ SOFT DELETE (Recommended alternative)
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(string id)
        {
            var db = await _context.Skills.FindAsync(id);
            if (db == null)
                return NotFound();

            db.ActiveFl = "N";
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Skill deactivated");
        }
    }
}
