using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Route("api/vendor-settings")]
    [ApiController]
    public class VendorSettingsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VendorSettingsController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET ALL (pagination)
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            var query = _context.VendorSettings.AsQueryable();

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.CompanyId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // ✅ GET BY COMPANY
        [HttpGet("{companyId}")]
        public async Task<IActionResult> Get(string companyId)
        {
            var entity = await _context.VendorSettings.FindAsync(companyId);

            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        // ✅ UPSERT (IMPORTANT)
        [HttpPost]
        public async Task<IActionResult> Upsert(VendorSettings dto)
        {
            if (string.IsNullOrEmpty(dto.CompanyId))
                return BadRequest("CompanyId required");

            var existing = await _context.VendorSettings
                .FindAsync(dto.CompanyId);

            var now = DateTime.UtcNow;

            if (existing != null)
            {
                // 🔄 UPDATE
                _context.Entry(existing).CurrentValues.SetValues(dto);
                existing.TimeStamp = now;
            }
            else
            {
                // ➕ INSERT
                dto.TimeStamp = now;
                await _context.VendorSettings.AddAsync(dto);
            }

            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // ✅ UPDATE (explicit)
        [HttpPut("{companyId}")]
        public async Task<IActionResult> Update(string companyId, VendorSettings dto)
        {
            var entity = await _context.VendorSettings.FindAsync(companyId);

            if (entity == null)
                return NotFound();

            _context.Entry(entity).CurrentValues.SetValues(dto);
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // ✅ DELETE
        [HttpDelete("{companyId}")]
        public async Task<IActionResult> Delete(string companyId)
        {
            var entity = await _context.VendorSettings.FindAsync(companyId);

            if (entity == null)
                return NotFound();

            _context.VendorSettings.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}
