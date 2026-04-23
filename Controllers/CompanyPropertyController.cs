using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyPropertyController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public CompanyPropertyController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (Pagination + Filtering)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            string? companyId,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.CompanyProperties.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.PropId.Contains(search) ||
                    x.PropDesc.Contains(search) ||
                    x.ManufName.Contains(search));
            }

            if (!string.IsNullOrEmpty(companyId))
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.PropId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await _context.CompanyProperties.FindAsync(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CompanyProperty model)
        {
            if (await _context.CompanyProperties.AnyAsync(x => x.PropId == model.PropId))
                return BadRequest("Property already exists");

            model.TimeStamp = DateTime.UtcNow;

            _context.CompanyProperties.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, CompanyProperty model)
        {
            var db = await _context.CompanyProperties.FindAsync(id);
            if (db == null)
                return NotFound();

            db.PropDesc = model.PropDesc;
            db.ManufName = model.ManufName;
            db.SerialId = model.SerialId;
            db.CompanyId = model.CompanyId;
            db.ModifiedBy = model.ModifiedBy;
            db.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(db);
        }

        // ✅ DELETE (Protected)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var db = await _context.CompanyProperties.FindAsync(id);
            if (db == null)
                return NotFound();

            // 🔥 VALIDATION: prevent delete if used in SUBC_PROPERTY
            var isUsed = await _context.SubcProperties
                .AnyAsync(x => x.PropId == id);

            if (isUsed)
            {
                return BadRequest(new
                {
                    message = "Cannot delete. Property is assigned to subcontractor."
                });
            }

            _context.CompanyProperties.Remove(db);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }

        // ✅ DROPDOWN
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown(string? companyId)
        {
            var query = _context.CompanyProperties.AsQueryable();

            if (!string.IsNullOrEmpty(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            var data = await query
                .Select(x => new
                {
                    value = x.PropId,
                    label = x.PropDesc
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
