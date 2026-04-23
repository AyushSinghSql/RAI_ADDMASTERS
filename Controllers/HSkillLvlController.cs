using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HSkillLvlController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public HSkillLvlController(MydatabaseContext context)
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
            var query = _context.HSkillLvls.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.SkillLvlCd.Contains(search) ||
                    x.SkillLvlDesc.Contains(search));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.SkillLvlCd)
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
            var data = await _context.HSkillLvls.FindAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(HSkillLvl model)
        {
            if (await _context.HSkillLvls.AnyAsync(x => x.SkillLvlCd == model.SkillLvlCd))
                return BadRequest("Skill Level already exists");

            model.TimeStamp = DateTime.UtcNow;

            // Validate FL fields
            if (!string.IsNullOrEmpty(model.Misc1Fl))
                model.Misc1Fl = model.Misc1Fl == "Y" ? "Y" : "N";

            _context.HSkillLvls.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, HSkillLvl model)
        {
            var db = await _context.HSkillLvls.FindAsync(id);
            if (db == null)
                return NotFound();

            db.SkillLvlDesc = model.SkillLvlDesc;
            db.Misc1Fld = model.Misc1Fld;
            db.Misc1Dt = model.Misc1Dt;
            db.Misc1Fl = string.IsNullOrEmpty(model.Misc1Fl)
                ? null
                : (model.Misc1Fl == "Y" ? "Y" : "N");

            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE (with validation)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var db = await _context.HSkillLvls.FindAsync(id);
            if (db == null)
                return NotFound();

            // 🔥 VALIDATION: prevent delete if used in SUBC_SKILLS
            var isUsed = await _context.VendorEmployeeSkills
                .AnyAsync(x => x.SkillLvlCd == id);

            if (isUsed)
            {
                return BadRequest(new
                {
                    message = "Cannot delete. Skill Level is used in subcontractor skills."
                });
            }

            _context.HSkillLvls.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ DROPDOWN
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var data = await _context.HSkillLvls
                .Select(x => new
                {
                    value = x.SkillLvlCd,
                    label = x.SkillLvlDesc
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
