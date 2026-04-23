using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/ap-settings")]
    public class ApSettingsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ApSettingsController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET
        [HttpGet("{companyId}")]
        public async Task<IActionResult> Get(string companyId)
        {
            var data = await _context.ApSettings.FindAsync(companyId);
            if (data == null) return NotFound();

            return Ok(data);
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.ApSettings.ToListAsync());
        }

        // ✅ UPSERT (Single record per company)
        [HttpPost("save")]
        public async Task<IActionResult> Save(ApSettings input)
        {
            var existing = await _context.ApSettings
                .FirstOrDefaultAsync(x => x.CompanyId == input.CompanyId);

            if (existing == null)
            {
                input.TimeStamp = DateTime.UtcNow;
                await _context.ApSettings.AddAsync(input);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(input);
                existing.TimeStamp = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok("Saved successfully");
        }

        // ✅ DELETE
        [HttpDelete("{companyId}")]
        public async Task<IActionResult> Delete(string companyId)
        {
            var entity = await _context.ApSettings.FindAsync(companyId);
            if (entity == null) return NotFound();

            _context.ApSettings.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
    }
}
