using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("api/voucher-settings")]
    public class VoucherSettingsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public VoucherSettingsController(MydatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET
        [HttpGet("{companyId}")]
        public async Task<IActionResult> Get(string companyId)
        {
            var data = await _context.VoucherSettings.FindAsync(companyId);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ✅ UPSERT (best for settings)
        [HttpPost]
        public async Task<IActionResult> Upsert(VoucherSettings model)
        {
            var existing = await _context.VoucherSettings
                .FirstOrDefaultAsync(x => x.CompanyId == model.CompanyId);

            if (existing == null)
            {
                model.ModifiedAt = DateTime.UtcNow;
                _context.VoucherSettings.Add(model);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(model);
                existing.ModifiedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(model);
        }
    }
}
